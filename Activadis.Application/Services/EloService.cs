using Activadis.Application.DTOs.Elo;
using Activadis.Application.Interfaces;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Match;

namespace Activadis.Application.Services
{
    public class EloService : IEloService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRatingRepository _ratingRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchParticipantRepository _participantRepository;
        private readonly IUserRepository _userRepository;
        private readonly LeaderboardService _leaderboardService;

        public EloService(
            ICategoryRepository categoryRepository,
            IRatingRepository ratingRepository,
            IMatchRepository matchRepository,
            IMatchParticipantRepository participantRepository,
            IUserRepository userRepository,
            LeaderboardService leaderboardService)
        {
            _categoryRepository = categoryRepository;
            _ratingRepository = ratingRepository;
            _matchRepository = matchRepository;
            _participantRepository = participantRepository;
            _userRepository = userRepository;
            _leaderboardService = leaderboardService;
        }

        public async Task<MatchResultDto> RecordMatchAsync(RecordMatchRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                throw new InvalidOperationException($"Category {request.CategoryId} not found");

            var match = new Match
            {
                Id = Guid.NewGuid(),
                CategoryId = request.CategoryId,
                MatchDate = request.MatchDate,
                Notes = request.Notes,
                IsFinalized = false,
                CreatedAt = DateTime.UtcNow
            };

            var results = request.MatchType?.ToLower() == "team"
                ? await ProcessTeamMatchAsync(match, request)
                : await ProcessIndividualMatchAsync(match, request);

            match.IsFinalized = true;
            await _matchRepository.UpdateAsync(match);

            _leaderboardService.InvalidateCache(request.CategoryId);

            return new MatchResultDto
            {
                MatchId = match.Id,
                CategoryId = match.CategoryId,
                MatchDate = match.MatchDate,
                Notes = match.Notes,
                MatchType = request.MatchType ?? "Individual",
                Results = results
            };
        }

        private async Task<List<ParticipantResultDto>> ProcessIndividualMatchAsync(
            Match match,
            RecordMatchRequest request)
        {
            var results = new List<ParticipantResultDto>();
            var matchParticipants = new List<MatchParticipant>();

            var participantData = new List<(MatchParticipantInput Input, Rating Rating, User User)>();

            foreach (var participant in request.Participants)
            {
                var rating = await _ratingRepository.GetOrCreateRatingAsync(
                    participant.UserId,
                    match.CategoryId);

                var user = await _userRepository.GetByIdAsync(participant.UserId);
                if (user == null)
                    throw new InvalidOperationException($"User {participant.UserId} not found");

                participantData.Add((participant, rating, user));
            }

            var totalPlayers = request.Participants.Count;

            for (int i = 0; i < participantData.Count; i++)
            {
                var (input, rating, user) = participantData[i];
                var actualScore = EloCalculationService.ConvertPlacementToScore(
                    input.Placement,
                    totalPlayers,
                    input.IsTie);

                double expectedScore = CalculateIndividualExpectedScore(
                    rating.CurrentRating,
                    participantData,
                    i);

                var newRating = EloCalculationService.CalculateNewRating(
                    rating.CurrentRating,
                    actualScore,
                    expectedScore,
                    rating.MatchCount);

                var ratingChange = newRating - rating.CurrentRating;

                rating.CurrentRating = newRating;
                rating.MatchCount++;
                if (newRating > rating.PeakRating)
                    rating.PeakRating = newRating;
                await _ratingRepository.UpdateAsync(rating);

                var matchParticipant = new MatchParticipant
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    UserId = user.Id,
                    Placement = input.Placement,
                    ActualScore = actualScore,
                    ExpectedScore = expectedScore,
                    RatingChange = ratingChange,
                    CreatedAt = DateTime.UtcNow
                };

                await _participantRepository.AddAsync(matchParticipant);

                results.Add(new ParticipantResultDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Placement = input.Placement,
                    OldRating = rating.CurrentRating - ratingChange,
                    NewRating = newRating,
                    RatingChange = ratingChange,
                    ActualScore = actualScore,
                    ExpectedScore = expectedScore,
                    TeamId = input.TeamId
                });
            }

            return results;
        }

        private async Task<List<ParticipantResultDto>> ProcessTeamMatchAsync(
            Match match,
            RecordMatchRequest request)
        {
            var results = new List<ParticipantResultDto>();

            var teamGroups = request.Participants
                .GroupBy(p => p.TeamId ?? Guid.NewGuid())
                .ToList();

            if (teamGroups.Count < 2)
                throw new InvalidOperationException("De teamwedstrijd moet uit minstens 2 teams bestaan");

            var allTeamData = new List<(Guid TeamId, List<(MatchParticipantInput Input, Rating Rating, User User)> Members)>();

            foreach (var team in teamGroups)
            {
                var teamMembers = new List<(MatchParticipantInput, Rating, User)>();

                foreach (var participant in team)
                {
                    var rating = await _ratingRepository.GetOrCreateRatingAsync(
                        participant.UserId,
                        match.CategoryId);

                    var user = await _userRepository.GetByIdAsync(participant.UserId);
                    if (user == null)
                        throw new InvalidOperationException($"User {participant.UserId} not found");

                    teamMembers.Add((participant, rating, user));
                }

                allTeamData.Add((team.Key, teamMembers));
            }

            var teamPlacements = allTeamData
                .Select(t => new
                {
                    TeamId = t.TeamId,
                    Placement = t.Members.Min(m => m.Input.Placement),
                    Members = t.Members
                })
                .OrderBy(t => t.Placement)
                .ToList();

            var team1Data = teamPlacements[0];
            var team2Data = teamPlacements.Count > 1 ? teamPlacements[1] : null;

            var team1Ratings = team1Data.Members.Select(m => m.Rating.CurrentRating).ToList();
            var team2Ratings = team2Data?.Members.Select(m => m.Rating.CurrentRating).ToList() ?? [];

            double team1ExpectedScore = EloCalculationService.CalculateTeamExpectedScore(
                team1Ratings,
                team2Ratings);
            double team2ExpectedScore = 1.0 - team1ExpectedScore;

            double team1ActualScore = team1Data.Placement == 1 ? 1.0 : 0.0;
            double team2ActualScore = team2Data?.Placement == 1 ? 1.0 : 0.0;

            if (team1Data.Placement == team2Data?.Placement)
            {
                team1ActualScore = 0.5;
                team2ActualScore = 0.5;
            }

            var team1MatchCounts = team1Data.Members.Select(m => m.Rating.MatchCount).ToList();
            var team1NewRatings = EloCalculationService.CalculateTeamNewRatings(
                team1Ratings,
                team1ActualScore,
                team1ExpectedScore,
                team1MatchCounts);

            for (int i = 0; i < team1Data.Members.Count; i++)
            {
                var (input, rating, user) = team1Data.Members[i];
                var newRating = team1NewRatings[i];
                var ratingChange = newRating - rating.CurrentRating;

                rating.CurrentRating = newRating;
                rating.MatchCount++;
                if (newRating > rating.PeakRating)
                    rating.PeakRating = newRating;
                await _ratingRepository.UpdateAsync(rating);

                var matchParticipant = new MatchParticipant
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    UserId = user.Id,
                    Placement = team1Data.Placement,
                    ActualScore = team1ActualScore,
                    ExpectedScore = team1ExpectedScore,
                    RatingChange = ratingChange,
                    CreatedAt = DateTime.UtcNow
                };

                await _participantRepository.AddAsync(matchParticipant);

                results.Add(new ParticipantResultDto
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Placement = team1Data.Placement,
                    OldRating = rating.CurrentRating - ratingChange,
                    NewRating = newRating,
                    RatingChange = ratingChange,
                    ActualScore = team1ActualScore,
                    ExpectedScore = team1ExpectedScore,
                    TeamId = team1Data.TeamId
                });
            }

            if (team2Data != null)
            {
                var team2MatchCounts = team2Data.Members.Select(m => m.Rating.MatchCount).ToList();
                var team2NewRatings = EloCalculationService.CalculateTeamNewRatings(
                    team2Ratings,
                    team2ActualScore,
                    team2ExpectedScore,
                    team2MatchCounts);

                for (int i = 0; i < team2Data.Members.Count; i++)
                {
                    var (input, rating, user) = team2Data.Members[i];
                    var newRating = team2NewRatings[i];
                    var ratingChange = newRating - rating.CurrentRating;

                    rating.CurrentRating = newRating;
                    rating.MatchCount++;
                    if (newRating > rating.PeakRating)
                        rating.PeakRating = newRating;
                    await _ratingRepository.UpdateAsync(rating);

                    var matchParticipant = new MatchParticipant
                    {
                        Id = Guid.NewGuid(),
                        MatchId = match.Id,
                        UserId = user.Id,
                        Placement = team2Data.Placement,
                        ActualScore = team2ActualScore,
                        ExpectedScore = team2ExpectedScore,
                        RatingChange = ratingChange,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _participantRepository.AddAsync(matchParticipant);

                    results.Add(new ParticipantResultDto
                    {
                        UserId = user.Id,
                        FullName = user.FullName,
                        Placement = team2Data.Placement,
                        OldRating = rating.CurrentRating - ratingChange,
                        NewRating = newRating,
                        RatingChange = ratingChange,
                        ActualScore = team2ActualScore,
                        ExpectedScore = team2ExpectedScore,
                        TeamId = team2Data.TeamId
                    });
                }
            }

            return results;
        }

        private static double CalculateIndividualExpectedScore(
            double playerRating,
            List<(MatchParticipantInput Input, Rating Rating, User User)> allPlayers,
            int playerIndex)
        {
            var otherPlayerRatings = allPlayers
                .Where((p, i) => i != playerIndex)
                .Select(p => p.Rating.CurrentRating)
                .ToList();

            if (otherPlayerRatings.Count == 0)
                return 0.5;

            double totalExpected = 0;
            foreach (var opponentRating in otherPlayerRatings)
            {
                totalExpected += EloCalculationService.CalculateExpectedScore(playerRating, opponentRating);
            }

            return totalExpected / otherPlayerRatings.Count;
        }

        public async Task<PlayerProfileDto?> GetPlayerProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            var ratings = await _ratingRepository.GetUserRatingsAsync(userId);
            var ratingList = ratings.ToList();

            var categoryRatings = ratingList.Select(r => new RatingDto
            {
                UserId = r.UserId,
                CategoryId = r.CategoryId,
                CategoryName = r.Category?.Name ?? "Onbekend",
                CurrentRating = r.CurrentRating,
                MatchCount = r.MatchCount,
                PeakRating = r.PeakRating
            }).ToList();

            var categoryData = ratingList.ToDictionary(
                r => r.CategoryId.ToString(),
                r => (r.CurrentRating, r.MatchCount));

            var overallRating = EloCalculationService.CalculateOverallRating(categoryData);

            var participations = await _participantRepository.GetUserParticipationsAsync(userId);
            var matchHistory = new List<MatchHistoryDto>();
            foreach (var p in participations.Take(20))
            {
                var rating = p.Match != null
                    ? await _ratingRepository.GetUserCategoryRatingAsync(userId, p.Match.CategoryId)
                    : null;

                matchHistory.Add(new MatchHistoryDto
                {
                    MatchId = p.MatchId,
                    CategoryName = p.Match?.Category?.Name ?? "Onbekend",
                    MatchDate = p.Match?.MatchDate ?? DateTime.MinValue,
                    Placement = p.Placement,
                    RatingBefore = (rating?.CurrentRating ?? 0) - p.RatingChange,
                    RatingAfter = rating?.CurrentRating ?? 0,
                    RatingChange = p.RatingChange
                });
            }

            return new PlayerProfileDto
            {
                UserId = userId,
                FullName = user.FullName,
                JobTitle = user.JobTitle,
                Email = user.Email,
                OverallRating = overallRating,
                TotalMatches = ratingList.Sum(r => r.MatchCount),
                CategoryRatings = categoryRatings,
                RecentMatches = matchHistory
            };
        }

        public async Task<List<LeaderboardEntryDto>> GetCategoryLeaderboardAsync(Guid categoryId)
        {
            var ratings = await _ratingRepository.GetCategoryRatingsAsync(categoryId);
            var entries = _leaderboardService.GetCategoryLeaderboard(
                categoryId,
                ratings.ToList(),
                cacheDurationMinutes: 5);

            return entries.Select(e => new LeaderboardEntryDto
            {
                UserId = e.UserId,
                FullName = e.FullName,
                JobTitle = e.JobTitle,
                Rating = e.Rating,
                MatchCount = e.MatchCount,
                PeakRating = e.PeakRating,
                Rank = e.Rank
            }).ToList();
        }

        public async Task<List<LeaderboardEntryDto>> GetOverallLeaderboardAsync()
        {
            var allRatings = await _ratingRepository.GetAllAsync();
            var entries = _leaderboardService.GetOverallLeaderboard(
                allRatings.ToList(),
                cacheDurationMinutes: 5);

            return entries.Select(e => new LeaderboardEntryDto
            {
                UserId = e.UserId,
                FullName = e.FullName,
                JobTitle = e.JobTitle,
                Rating = e.Rating,
                MatchCount = e.MatchCount,
                PeakRating = e.PeakRating,
                Rank = e.Rank
            }).ToList();
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category == null)
                return null;

            if (!string.IsNullOrEmpty(request.Name))
                category.Name = request.Name;

            if (request.Description != null)
                category.Description = request.Description;

            if (request.DisplayOrder.HasValue)
                category.DisplayOrder = request.DisplayOrder.Value;

            if (request.IsActive.HasValue)
                category.IsActive = request.IsActive.Value;

            await _categoryRepository.UpdateAsync(category);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };
        }
    }
}
