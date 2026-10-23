using Activadis.Domain.Entities;
using Activadis.Application.Services;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Stats;

namespace Activadis.Application.Extensions
{
    public static class EloMappingExtensions
    {
        public static CategoryDto ToDto(this Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Slug = category.Slug,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                RankTitles = category.RankTitles,
            };
        }

        public static RatingDto ToDto(this Rating rating)
        {
            return new RatingDto
            {
                UserId = rating.UserId,
                CategoryId = rating.CategoryId,
                CategoryName = rating.Category?.Name ?? "Onbekend",
                CurrentRating = rating.CurrentRating,
                MatchCount = rating.MatchCount,
                PeakRating = rating.PeakRating,
            };
        }

        public static LeaderboardEntryDto ToDto(this LeaderboardService.LeaderboardEntry entry)
        {
            return new LeaderboardEntryDto
            {
                UserId = entry.UserId,
                FullName = entry.FullName,
                JobTitle = entry.JobTitle,
                Rating = entry.Rating,
                MatchCount = entry.MatchCount,
                PeakRating = entry.PeakRating,
                Rank = entry.Rank,
                RankTitle = entry.RankTitle,
            };
        }

        public static RecentMatchDto ToRecentMatchDto(this Match match)
        {
            return new RecentMatchDto
            {
                MatchId = match.Id,
                CategoryId = match.CategoryId,
                CategorySlug = match.Category?.Slug,
                CategoryName = match.Category?.Name ?? "Onbekend",
                MatchDate = match.MatchDate,
                Notes = match.Notes,
                Participants = match.Participants
                    .OrderBy(p => p.Placement)
                    .Select(p => new RecentMatchParticipantDto
                    {
                        UserId = p.UserId,
                        UserSlug = p.User?.Slug,
                        FullName = p.User?.FullName ?? "Onbekend",
                        Placement = p.Placement,
                        RatingChange = p.RatingChange
                    }).ToList()
            };
        }

        public static ParticipantResultDto ToDto(this MatchParticipant participant)
        {
            return new ParticipantResultDto
            {
                UserId = participant.UserId,
                UserSlug = participant.User?.Slug,
                FullName = participant.User?.FullName ?? "Onbekend",
                Placement = participant.Placement,
                OldRating = participant.Match != null ? 
                    (participant.Match.Category?.Name == null ? 0 : 
                     participant.RatingChange + (int)participant.ActualScore) : 0,
                NewRating = (int)(participant.RatingChange + (int)participant.ActualScore),
                RatingChange = participant.RatingChange,
                ActualScore = participant.ActualScore,
                ExpectedScore = participant.ExpectedScore,
                TeamId = null,
            };
        }
    }
}
