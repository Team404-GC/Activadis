using Activadis.Domain.Entities;

namespace Activadis.Application.Services
{
    public class LeaderboardService
    {
        private readonly Dictionary<Guid, LeaderboardCache> _leaderboardCache = [];
        private readonly object _cacheLock = new();

        public class LeaderboardEntry
        {
            public Guid UserId { get; set; }
            public required string FullName { get; set; }
            public string? JobTitle { get; set; }
            public double Rating { get; set; }
            public int MatchCount { get; set; }
            public double PeakRating { get; set; }
            public int Rank { get; set; }
        }

        private class LeaderboardCache
        {
            public DateTime CachedAt { get; set; }
            public List<LeaderboardEntry> Entries { get; set; } = [];
        }

        public List<LeaderboardEntry> GetCategoryLeaderboard(
            Guid categoryId,
            IEnumerable<Rating> ratings,
            int cacheDurationMinutes = 5)
        {
            lock (_cacheLock)
            {
                if (_leaderboardCache.TryGetValue(categoryId, out var cache))
                {
                    if (DateTime.UtcNow - cache.CachedAt < TimeSpan.FromMinutes(cacheDurationMinutes))
                    {
                        return cache.Entries;
                    }
                }

                var entries = BuildLeaderboardEntries(ratings);
                _leaderboardCache[categoryId] = new LeaderboardCache
                {
                    CachedAt = DateTime.UtcNow,
                    Entries = entries
                };

                return entries;
            }
        }

        public List<LeaderboardEntry> GetOverallLeaderboard(
            IEnumerable<Rating> allRatings,
            int cacheDurationMinutes = 5)
        {
            Guid overallKey = Guid.Empty;

            lock (_cacheLock)
            {
                if (_leaderboardCache.TryGetValue(overallKey, out var cache))
                {
                    if (DateTime.UtcNow - cache.CachedAt < TimeSpan.FromMinutes(cacheDurationMinutes))
                    {
                        return cache.Entries;
                    }
                }

                var userRatings = allRatings
                    .Where(r => r.User != null)
                    .GroupBy(r => r.UserId)
                    .Select(group => new
                    {
                        UserId = group.Key,
                        User = group.First().User!,
                        CategoryRatings = group.ToDictionary(
                            r => r.CategoryId.ToString(),
                            r => (r.CurrentRating, r.MatchCount))
                    })
                    .ToList();

                var entries = userRatings
                    .Select((data, index) => new LeaderboardEntry
                    {
                        UserId = data.UserId,
                        FullName = data.User.FullName,
                        JobTitle = data.User.JobTitle,
                        Rating = EloCalculationService.CalculateOverallRating(data.CategoryRatings),
                        MatchCount = data.CategoryRatings.Values.Sum(x => x.MatchCount),
                        PeakRating = data.CategoryRatings.Values.Max(x => x.CurrentRating),
                        Rank = 0 
                    })
                    .OrderByDescending(e => e.Rating)
                    .Select((entry, index) =>
                    {
                        entry.Rank = index + 1;
                        return entry;
                    })
                    .ToList();

                _leaderboardCache[overallKey] = new LeaderboardCache
                {
                    CachedAt = DateTime.UtcNow,
                    Entries = entries
                };

                return entries;
            }
        }

        public void InvalidateCache(Guid categoryId)
        {
            lock (_cacheLock)
            {
                _leaderboardCache.Remove(categoryId);
                _leaderboardCache.Remove(Guid.Empty);
            }
        }

        public void ClearAllCache()
        {
            lock (_cacheLock)
            {
                _leaderboardCache.Clear();
            }
        }

        private static List<LeaderboardEntry> BuildLeaderboardEntries(IEnumerable<Rating> ratings)
        {
            return ratings
                .Where(r => r.User != null)
                .OrderByDescending(r => r.CurrentRating)
                .Select((rating, index) => new LeaderboardEntry
                {
                    UserId = rating.UserId,
                    FullName = rating.User!.FullName,
                    JobTitle = rating.User.JobTitle,
                    Rating = rating.CurrentRating,
                    MatchCount = rating.MatchCount,
                    PeakRating = rating.PeakRating,
                    Rank = index + 1
                })
                .ToList();
        }
    }
}
