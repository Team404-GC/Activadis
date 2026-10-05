using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;

namespace Activadis.Application.Services
{
    public class LeaderboardService
    {
        private readonly Dictionary<Guid, LeaderboardCache> _leaderboardCache = [];
        private readonly object _cacheLock = new();
        private readonly ILeaderboardRepository _leaderboardRepository;

        private const string HighestOverallTitle = "Ultime Covadiaan";
        private const string LowestOverallTitle = "Subsidie Pakker";

        private static readonly Dictionary<double, string> DefaultOverallRankTitles = new()
        {
            [0] = "Nieuweling",
            [800] = "Groentje",
            [1200] = "Strijder",
            [1500] = "Kampioen",
            [1800] = "Opperbaas",
            [2000] = "Legende",
        };

        public class LeaderboardEntry
        {
            public Guid UserId { get; set; }
            public required string FullName { get; set; }
            public string? JobTitle { get; set; }
            public double Rating { get; set; }
            public int MatchCount { get; set; }
            public double PeakRating { get; set; }
            public int Rank { get; set; }
            public string? RankTitle { get; set; }
        }

        private class LeaderboardCache
        {
            public DateTime CachedAt { get; set; }
            public List<LeaderboardEntry> Entries { get; set; } = [];
        }

        public LeaderboardService(ILeaderboardRepository leaderboardRepository)
        {
            _leaderboardRepository = leaderboardRepository;
        }

        public async Task<List<LeaderboardEntry>> GetCategoryLeaderboardAsync(
            Guid categoryId,
            IEnumerable<Rating> ratings,
            Category? category = null,
            int cacheDurationMinutes = 5,
            bool forceRebuild = false)
        {
            if (!forceRebuild)
            {
                lock (_cacheLock)
                {
                    if (_leaderboardCache.TryGetValue(categoryId, out var cache) &&
                        DateTime.UtcNow - cache.CachedAt < TimeSpan.FromMinutes(cacheDurationMinutes))
                    {
                        return cache.Entries;
                    }
                }

                var dbLeaderboard = await _leaderboardRepository.GetLatestCategoryLeaderboardAsync(categoryId);
                if (dbLeaderboard != null)
                {
                    var entries = ConvertToLeaderboardEntries(dbLeaderboard.Entries);
                    lock (_cacheLock)
                    {
                        _leaderboardCache[categoryId] = new LeaderboardCache
                        {
                            CachedAt = DateTime.UtcNow,
                            Entries = entries
                        };
                    }
                    return entries;
                }
            }

            var newEntries = BuildLeaderboardEntries(ratings, category?.RankTitles);
            lock (_cacheLock)
            {
                _leaderboardCache[categoryId] = new LeaderboardCache
                {
                    CachedAt = DateTime.UtcNow,
                    Entries = newEntries
                };
            }

            return newEntries;
        }

        public async Task<List<LeaderboardEntry>> GetOverallLeaderboardAsync(
            IEnumerable<Rating> allRatings,
            Dictionary<double, string>? overallRankTitles = null,
            int cacheDurationMinutes = 5,
            bool forceRebuild = false)
        {
            overallRankTitles ??= DefaultOverallRankTitles;
            Guid overallKey = Guid.Empty;

            if (!forceRebuild)
            {
                lock (_cacheLock)
                {
                    if (_leaderboardCache.TryGetValue(overallKey, out var cache) &&
                        DateTime.UtcNow - cache.CachedAt < TimeSpan.FromMinutes(cacheDurationMinutes))
                    {
                        return cache.Entries;
                    }
                }

                var dbLeaderboard = await _leaderboardRepository.GetLatestOverallLeaderboardAsync();
                if (dbLeaderboard != null)
                {
                    var entries = ConvertToLeaderboardEntries(dbLeaderboard.Entries);
                    lock (_cacheLock)
                    {
                        _leaderboardCache[overallKey] = new LeaderboardCache
                        {
                            CachedAt = DateTime.UtcNow,
                            Entries = entries
                        };
                    }
                    return entries;
                }
            }

            var newEntries = BuildOverallLeaderboardEntries(allRatings, overallRankTitles);
            lock (_cacheLock)
            {
                _leaderboardCache[overallKey] = new LeaderboardCache
                {
                    CachedAt = DateTime.UtcNow,
                    Entries = newEntries
                };
            }

            return newEntries;
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

                var entries = BuildLeaderboardEntries(ratings, null);
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

                var entries = BuildOverallLeaderboardEntries(allRatings, DefaultOverallRankTitles);
                _leaderboardCache[overallKey] = new LeaderboardCache
                {
                    CachedAt = DateTime.UtcNow,
                    Entries = entries
                };

                return entries;
            }
        }

        public async Task SaveCategoryLeaderboardAsync(
            Guid categoryId,
            List<LeaderboardEntry> entries,
            Category? category = null)
        {
            var leaderboardEntries = entries.Select(e => new Domain.Entities.LeaderboardEntry
            {
                UserId = e.UserId,
                FullName = e.FullName,
                JobTitle = e.JobTitle,
                Rating = e.Rating,
                MatchCount = e.MatchCount,
                PeakRating = e.PeakRating,
                Rank = e.Rank,
                RankTitle = e.RankTitle
            }).ToList();

            var leaderboard = new Leaderboard
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                IsOverall = false,
                Entries = leaderboardEntries,
                CreatedAt = DateTime.UtcNow
            };

            await _leaderboardRepository.SaveLeaderboardAsync(leaderboard);
        }

        public async Task SaveOverallLeaderboardAsync(List<LeaderboardEntry> entries)
        {
            var leaderboardEntries = entries.Select(e => new Domain.Entities.LeaderboardEntry
            {
                UserId = e.UserId,
                FullName = e.FullName,
                JobTitle = e.JobTitle,
                Rating = e.Rating,
                MatchCount = e.MatchCount,
                PeakRating = e.PeakRating,
                Rank = e.Rank,
                RankTitle = e.RankTitle
            }).ToList();

            var leaderboard = new Leaderboard
            {
                Id = Guid.NewGuid(),
                CategoryId = null,
                IsOverall = true,
                Entries = leaderboardEntries,
                CreatedAt = DateTime.UtcNow
            };

            await _leaderboardRepository.SaveLeaderboardAsync(leaderboard);
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

        private static List<LeaderboardEntry> BuildLeaderboardEntries(
            IEnumerable<Rating> ratings,
            Dictionary<double, string>? rankTitles = null)
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
                    Rank = index + 1,
                    RankTitle = GetRankTitle(rating.CurrentRating, rankTitles)
                })
                .ToList();
        }

        private static List<LeaderboardEntry> BuildOverallLeaderboardEntries(
            IEnumerable<Rating> allRatings,
            Dictionary<double, string>? rankTitles = null)
        {
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
                    entry.RankTitle = GetRankTitle(entry.Rating, rankTitles);
                    return entry;
                })
                .ToList();

            if (entries.Count > 1)
                entries[^1].RankTitle = LowestOverallTitle;

            if (entries.Count > 0)
                entries[0].RankTitle = HighestOverallTitle;

            return entries;
        }

        private static string? GetRankTitle(double rating, Dictionary<double, string>? rankTitles)
        {
            if (rankTitles == null || rankTitles.Count == 0)
                return null;

            var applicableTitle = rankTitles
                .Where(kvp => rating >= kvp.Key)
                .OrderByDescending(kvp => kvp.Key)
                .FirstOrDefault();

            return applicableTitle.Value;
        }

        private static List<LeaderboardEntry> ConvertToLeaderboardEntries(
            List<Domain.Entities.LeaderboardEntry> dbEntries)
        {
            return dbEntries
                .Select(e => new LeaderboardEntry
                {
                    UserId = e.UserId,
                    FullName = e.FullName,
                    JobTitle = e.JobTitle,
                    Rating = e.Rating,
                    MatchCount = e.MatchCount,
                    PeakRating = e.PeakRating,
                    Rank = e.Rank,
                    RankTitle = e.RankTitle
                })
                .ToList();
        }
    }
}