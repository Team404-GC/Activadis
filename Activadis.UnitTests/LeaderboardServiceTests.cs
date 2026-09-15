using Activadis.Application.Services;
using Activadis.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Activadis.UnitTests
{
    public class LeaderboardServiceTests
    {
        private readonly LeaderboardService _leaderboardServiceMock;

        public LeaderboardServiceTests()
        {
            _leaderboardServiceMock = new LeaderboardService();
        }

        private static Rating CreateRating(User user, Guid categoryId, double currentRating, int matchCount, double peakRating)
        {
            return new Rating
            {
                UserId = user.Id,
                CategoryId = categoryId,
                CurrentRating = currentRating,
                MatchCount = matchCount,
                PeakRating = peakRating,
                User = user,
            };
        }

        [Fact]
        public void GetCategoryLeaderboard_FirstCall_ReturnsEntriesRankedByRating()
        {
            var categoryId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var userB = new User { Id = Guid.NewGuid(), FullName = "Bob" };

            var ratings = new List<Rating>
            {
                CreateRating(userA, categoryId, 1300, 10, 1350),
                CreateRating(userB, categoryId, 1500, 12, 1500),
            };

            var result = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, ratings);

            Assert.Equal(2, result.Count);
            Assert.Equal("Bob", result[0].FullName);
            Assert.Equal(1, result[0].Rank);
            Assert.Equal("Alice", result[1].FullName);
            Assert.Equal(2, result[1].Rank);
        }

        [Fact]
        public void GetCategoryLeaderboard_UserWithNullUserNavigation_IsExcluded()
        {
            var categoryId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };

            var ratings = new List<Rating>
            {
                CreateRating(userA, categoryId, 1300, 10, 1350),
                new Rating { UserId = Guid.NewGuid(), CategoryId = categoryId, CurrentRating = 1600, MatchCount = 5, User = null! },
            };

            var result = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, ratings);

            Assert.Single(result);
            Assert.Equal("Alice", result[0].FullName);
        }

        [Fact]
        public void GetCategoryLeaderboard_CalledWithinCacheDuration_ReturnsCachedEntries()
        {
            var categoryId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };

            var firstBatch = new List<Rating> { CreateRating(userA, categoryId, 1300, 10, 1350) };
            var secondBatch = new List<Rating>
            {
                CreateRating(userA, categoryId, 1300, 10, 1350),
                CreateRating(new User { Id = Guid.NewGuid(), FullName = "Bob" }, categoryId, 1600, 8, 1600),
            };

            var firstResult = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, firstBatch, cacheDurationMinutes: 5);
            var secondResult = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, secondBatch, cacheDurationMinutes: 5);

            Assert.Single(secondResult);
            Assert.Equal(firstResult, secondResult);
        }

        [Fact]
        public void GetCategoryLeaderboard_CacheExpired_RebuildsFromNewData()
        {
            var categoryId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var userB = new User { Id = Guid.NewGuid(), FullName = "Bob" };

            var firstBatch = new List<Rating> { CreateRating(userA, categoryId, 1300, 10, 1350) };
            var secondBatch = new List<Rating>
            {
                CreateRating(userA, categoryId, 1300, 10, 1350),
                CreateRating(userB, categoryId, 1600, 8, 1600),
            };

            _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, firstBatch, cacheDurationMinutes: 0);
            var secondResult = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, secondBatch, cacheDurationMinutes: 0);

            Assert.Equal(2, secondResult.Count);
        }

        [Fact]
        public void InvalidateCache_ClearsCategoryCache_NextCallRebuilds()
        {
            var categoryId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var userB = new User { Id = Guid.NewGuid(), FullName = "Bob" };

            var firstBatch = new List<Rating> { CreateRating(userA, categoryId, 1300, 10, 1350) };
            var secondBatch = new List<Rating>
            {
                CreateRating(userA, categoryId, 1300, 10, 1350),
                CreateRating(userB, categoryId, 1600, 8, 1600),
            };

            _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, firstBatch, cacheDurationMinutes: 10);
            _leaderboardServiceMock.InvalidateCache(categoryId);
            var result = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, secondBatch, cacheDurationMinutes: 10);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetOverallLeaderboard_MultipleUsers_RanksByWeightedOverallRating()
        {
            var categoryA = Guid.NewGuid();
            var categoryB = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var userB = new User { Id = Guid.NewGuid(), FullName = "Bob" };

            var allRatings = new List<Rating>
            {
                CreateRating(userA, categoryA, 1200, 10, 1250),
                CreateRating(userA, categoryB, 1400, 10, 1400),
                CreateRating(userB, categoryA, 1800, 20, 1800),
            };

            var result = _leaderboardServiceMock.GetOverallLeaderboard(allRatings);

            Assert.Equal(2, result.Count);
            Assert.Equal("Bob", result[0].FullName);
            Assert.Equal(1, result[0].Rank);
            Assert.Equal("Alice", result[1].FullName);
            Assert.Equal(1300, result[1].Rating, 3);
        }

        [Fact]
        public void ClearAllCache_RemovesCachedCategoryAndOverallEntries()
        {
            var categoryId = Guid.NewGuid();
            var userA = new User { Id = Guid.NewGuid(), FullName = "Alice" };
            var userB = new User { Id = Guid.NewGuid(), FullName = "Bob" };

            var firstBatch = new List<Rating> { CreateRating(userA, categoryId, 1300, 10, 1350) };
            var secondBatch = new List<Rating>
            {
                CreateRating(userA, categoryId, 1300, 10, 1350),
                CreateRating(userB, categoryId, 1600, 8, 1600),
            };

            _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, firstBatch, cacheDurationMinutes: 10);
            _leaderboardServiceMock.ClearAllCache();
            var result = _leaderboardServiceMock.GetCategoryLeaderboard(categoryId, secondBatch, cacheDurationMinutes: 10);

            Assert.Equal(2, result.Count);
        }
    }
}