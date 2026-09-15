using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Shared.DTOs.Elo.Category;
using Activadis.Shared.DTOs.Elo.Match;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Activadis.UnitTests
{
    public class EloServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IRatingRepository> _ratingRepositoryMock;
        private readonly Mock<IMatchRepository> _matchRepositoryMock;
        private readonly Mock<IMatchParticipantRepository> _participantRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly LeaderboardService _leaderboardService;
        private readonly EloService _eloServiceMock;

        public EloServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _ratingRepositoryMock = new Mock<IRatingRepository>();
            _matchRepositoryMock = new Mock<IMatchRepository>();
            _participantRepositoryMock = new Mock<IMatchParticipantRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();

            // LeaderboardService has no interface and is a lightweight in-memory cache,
            // so a real instance is used rather than a mock.
            _leaderboardService = new LeaderboardService();

            _eloServiceMock = new EloService(
                _categoryRepositoryMock.Object,
                _ratingRepositoryMock.Object,
                _matchRepositoryMock.Object,
                _participantRepositoryMock.Object,
                _userRepositoryMock.Object,
                _leaderboardService);
        }

        private static Rating CreateRating(Guid userId, Guid categoryId, double currentRating, int matchCount)
        {
            return new Rating
            {
                UserId = userId,
                CategoryId = categoryId,
                CurrentRating = currentRating,
                MatchCount = matchCount,
                PeakRating = currentRating,
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task RecordMatchAsync_CategoryNotFound_ThrowsInvalidOperationException()
        {
            var request = new RecordMatchRequest
            {
                CategoryId = Guid.NewGuid(),
                MatchType = "Individual",
                MatchDate = DateTime.UtcNow,
                Participants = new List<MatchParticipantInput>(),
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(request.CategoryId))
                .ReturnsAsync((Category?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _eloServiceMock.RecordMatchAsync(request));
        }

        [Fact]
        public async System.Threading.Tasks.Task RecordMatchAsync_IndividualMatch_UpdatesBothPlayersAndReturnsResults()
        {
            var category = new Category { Id = Guid.NewGuid(), Name = "Chess" };
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var userB = new User { Id = Guid.NewGuid(), FullName = "Bob" };
            var ratingA = CreateRating(userA.Id, category.Id, 1200, 5);
            var ratingB = CreateRating(userB.Id, category.Id, 1200, 5);

            var request = new RecordMatchRequest
            {
                CategoryId = category.Id,
                MatchDate = DateTime.UtcNow,
                MatchType = "Individual",
                Participants = new List<MatchParticipantInput>
                {
                    new MatchParticipantInput { UserId = userA.Id, Placement = 1 },
                    new MatchParticipantInput { UserId = userB.Id, Placement = 2 },
                },
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(category.Id)).ReturnsAsync(category);
            _ratingRepositoryMock.Setup(repo => repo.GetOrCreateRatingAsync(userA.Id, category.Id)).ReturnsAsync(ratingA);
            _ratingRepositoryMock.Setup(repo => repo.GetOrCreateRatingAsync(userB.Id, category.Id)).ReturnsAsync(ratingB);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userA.Id)).ReturnsAsync(userA);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userB.Id)).ReturnsAsync(userB);

            var result = await _eloServiceMock.RecordMatchAsync(request);

            Assert.Equal(2, result.Results.Count);
            Assert.True(result.Results.Single(r => r.UserId == userA.Id).RatingChange > 0);
            Assert.True(result.Results.Single(r => r.UserId == userB.Id).RatingChange < 0);
            _ratingRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Rating>()), Times.Exactly(2));
            _participantRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<MatchParticipant>()), Times.Exactly(2));
            _matchRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Domain.Entities.Match>(m => m.IsFinalized)), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task RecordMatchAsync_IndividualMatch_UserNotFound_ThrowsInvalidOperationException()
        {
            var category = new Category { Id = Guid.NewGuid(), Name = "Chess" };
            var missingUserId = Guid.NewGuid();
            var rating = CreateRating(missingUserId, category.Id, 1200, 5);

            var request = new RecordMatchRequest
            {
                CategoryId = category.Id,
                MatchDate = DateTime.UtcNow,
                MatchType = "Individual",
                Participants = new List<MatchParticipantInput>
                {
                    new MatchParticipantInput { UserId = missingUserId, Placement = 1 },
                },
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(category.Id)).ReturnsAsync(category);
            _ratingRepositoryMock.Setup(repo => repo.GetOrCreateRatingAsync(missingUserId, category.Id)).ReturnsAsync(rating);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(missingUserId)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _eloServiceMock.RecordMatchAsync(request));
        }

        [Fact]
        public async System.Threading.Tasks.Task RecordMatchAsync_TeamMatchWithOneTeam_ThrowsInvalidOperationException()
        {
            var category = new Category { Id = Guid.NewGuid(), Name = "Soccer" };
            var teamId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var ratingA = CreateRating(userA.Id, category.Id, 1200, 5);

            var request = new RecordMatchRequest
            {
                CategoryId = category.Id,
                MatchDate = DateTime.UtcNow,
                MatchType = "team",
                Participants = new List<MatchParticipantInput>
                {
                    new MatchParticipantInput { UserId = userA.Id, Placement = 1, TeamId = teamId },
                },
            };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(category.Id)).ReturnsAsync(category);
            _ratingRepositoryMock.Setup(repo => repo.GetOrCreateRatingAsync(userA.Id, category.Id)).ReturnsAsync(ratingA);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userA.Id)).ReturnsAsync(userA);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _eloServiceMock.RecordMatchAsync(request));
        }

        [Fact]
        public async System.Threading.Tasks.Task RecordMatchAsync_TeamMatch_UpdatesEveryMemberOnBothTeams()
        {
            var category = new Category { Id = Guid.NewGuid(), Name = "Soccer" };
            var teamOneId = Guid.NewGuid();
            var teamTwoId = Guid.NewGuid();

            var winners = new List<User>
            {
                new User { Id = Guid.NewGuid(), FullName = "Alice" },
                new User { Id = Guid.NewGuid(), FullName = "Bob" },
            };
            var losers = new List<User>
            {
                new User { Id = Guid.NewGuid(), FullName = "Carol" },
                new User { Id = Guid.NewGuid(), FullName = "Dave" },
            };

            var request = new RecordMatchRequest
            {
                CategoryId = category.Id,
                MatchDate = DateTime.UtcNow,
                MatchType = "team",
                Participants = new List<MatchParticipantInput>(),
            };

            foreach (var user in winners)
            {
                request.Participants.Add(new MatchParticipantInput { UserId = user.Id, Placement = 1, TeamId = teamOneId });
                var rating = CreateRating(user.Id, category.Id, 1200, 5);
                _ratingRepositoryMock.Setup(repo => repo.GetOrCreateRatingAsync(user.Id, category.Id)).ReturnsAsync(rating);
                _userRepositoryMock.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
            }

            foreach (var user in losers)
            {
                request.Participants.Add(new MatchParticipantInput { UserId = user.Id, Placement = 2, TeamId = teamTwoId });
                var rating = CreateRating(user.Id, category.Id, 1200, 5);
                _ratingRepositoryMock.Setup(repo => repo.GetOrCreateRatingAsync(user.Id, category.Id)).ReturnsAsync(rating);
                _userRepositoryMock.Setup(repo => repo.GetByIdAsync(user.Id)).ReturnsAsync(user);
            }

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(category.Id)).ReturnsAsync(category);

            var result = await _eloServiceMock.RecordMatchAsync(request);

            Assert.Equal(4, result.Results.Count);
            Assert.All(result.Results.Where(r => r.TeamId == teamOneId), r => Assert.True(r.RatingChange > 0));
            Assert.All(result.Results.Where(r => r.TeamId == teamTwoId), r => Assert.True(r.RatingChange < 0));
            _ratingRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Rating>()), Times.Exactly(4));
            _participantRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<MatchParticipant>()), Times.Exactly(4));
        }

        [Fact]
        public async System.Threading.Tasks.Task GetPlayerProfileAsync_UserNotFound_ReturnsNull()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var result = await _eloServiceMock.GetPlayerProfileAsync(userId);

            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetPlayerProfileAsync_UserFound_ReturnsProfileWithOverallRating()
        {
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var user = new User { Id = userId, FullName = "Alice", JobTitle = "Engineer", Email = "alice@test.com" };
            var ratings = new List<Rating>
            {
                new Rating
                {
                    UserId = userId,
                    CategoryId = categoryId,
                    CurrentRating = 1400,
                    MatchCount = 10,
                    PeakRating = 1420,
                    Category = new Category { Id = categoryId, Name = "Chess" },
                },
            };

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _ratingRepositoryMock.Setup(repo => repo.GetUserRatingsAsync(userId)).ReturnsAsync(ratings);
            _participantRepositoryMock.Setup(repo => repo.GetUserParticipationsAsync(userId))
                .ReturnsAsync(new List<MatchParticipant>());

            var result = await _eloServiceMock.GetPlayerProfileAsync(userId);

            Assert.NotNull(result);
            Assert.Equal("Alice", result!.FullName);
            Assert.Equal(1400, result.OverallRating, 3);
            Assert.Equal(10, result.TotalMatches);
            Assert.Single(result.CategoryRatings);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetCategoryLeaderboardAsync_ReturnsRankedEntries()
        {
            var categoryId = Guid.NewGuid();
            var ratings = new List<Rating>
            {
                new Rating { UserId = Guid.NewGuid(), CategoryId = categoryId, CurrentRating = 1200, MatchCount = 5, PeakRating = 1200, User = new User { FullName = "Alice" } },
                new Rating { UserId = Guid.NewGuid(), CategoryId = categoryId, CurrentRating = 1500, MatchCount = 8, PeakRating = 1500, User = new User { FullName = "Bob" } },
            };

            _ratingRepositoryMock.Setup(repo => repo.GetCategoryRatingsAsync(categoryId)).ReturnsAsync(ratings);

            var result = await _eloServiceMock.GetCategoryLeaderboardAsync(categoryId);

            Assert.Equal(2, result.Count);
            Assert.Equal("Bob", result[0].FullName);
            Assert.Equal(1, result[0].Rank);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetOverallLeaderboardAsync_ReturnsRankedEntries()
        {
            var ratings = new List<Rating>
            {
                new Rating { UserId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), CurrentRating = 1200, MatchCount = 5, PeakRating = 1200, User = new User { FullName = "Alice" } },
                new Rating { UserId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), CurrentRating = 1700, MatchCount = 12, PeakRating = 1700, User = new User { FullName = "Bob" } },
            };

            _ratingRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(ratings);

            var result = await _eloServiceMock.GetOverallLeaderboardAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Bob", result[0].FullName);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetCategoriesAsync_ReturnsCategoryDtoList()
        {
            var categories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Chess", IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Darts", IsActive = true },
            };

            _categoryRepositoryMock.Setup(repo => repo.GetActiveCategoriesAsync()).ReturnsAsync(categories);

            var result = await _eloServiceMock.GetCategoriesAsync();

            Assert.Equal(2, result.Count);
            Assert.IsType<List<CategoryDto>>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateCategoryAsync_ValidRequest_ReturnsCategoryDtoAndPersists()
        {
            var request = new CreateCategoryRequest { Name = "Chess", Description = "1v1 strategy", DisplayOrder = 1 };

            var result = await _eloServiceMock.CreateCategoryAsync(request);

            Assert.Equal("Chess", result.Name);
            Assert.True(result.IsActive);
            _categoryRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Category>(c => c.Name == "Chess")), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateCategoryAsync_CategoryNotFound_ReturnsNull()
        {
            var request = new UpdateCategoryRequest { Id = Guid.NewGuid(), Name = "Chess" };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(request.Id)).ReturnsAsync((Category?)null);

            var result = await _eloServiceMock.UpdateCategoryAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateCategoryAsync_ValidRequest_UpdatesAndReturnsCategoryDto()
        {
            var category = new Category { Id = Guid.NewGuid(), Name = "Chess", DisplayOrder = 1, IsActive = true };
            var request = new UpdateCategoryRequest { Id = category.Id, Name = "Chess Classic", IsActive = false };

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(category.Id)).ReturnsAsync(category);

            var result = await _eloServiceMock.UpdateCategoryAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Chess Classic", result!.Name);
            Assert.False(result.IsActive);
            _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Category>()), Times.Once);
        }
    }
}