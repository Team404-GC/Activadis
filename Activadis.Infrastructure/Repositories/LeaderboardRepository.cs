using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Repositories
{
    public class LeaderboardRepository : Repository<Leaderboard>, ILeaderboardRepository
    {
        private readonly ApplicationDBContext _context;

        public LeaderboardRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Leaderboard?> GetLatestCategoryLeaderboardAsync(Guid categoryId)
        {
            return await _context.Leaderboards
                .Where(l => l.CategoryId == categoryId && !l.IsOverall && l.DeletedAt == null)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Leaderboard?> GetLatestOverallLeaderboardAsync()
        {
            return await _context.Leaderboards
                .Where(l => l.IsOverall && l.DeletedAt == null)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Leaderboard> SaveLeaderboardAsync(Leaderboard leaderboard)
        {
            leaderboard.CreatedAt = DateTime.UtcNow;
            _context.Leaderboards.Add(leaderboard);
            await _context.SaveChangesAsync();
            return leaderboard;
        }

        public async Task CleanupOldLeaderboardsAsync(int keepCount = 1)
        {
            var categoryLeaderboards = await _context.Leaderboards
                .Where(l => !l.IsOverall && l.DeletedAt == null)
                .GroupBy(l => l.CategoryId)
                .ToListAsync();

            foreach (var group in categoryLeaderboards)
            {
                var toDelete = group
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip(keepCount)
                    .ToList();

                foreach (var leaderboard in toDelete)
                {
                    leaderboard.DeletedAt = DateTime.UtcNow;
                }
            }

            var overallLeaderboards = await _context.Leaderboards
                .Where(l => l.IsOverall && l.DeletedAt == null)
                .OrderByDescending(l => l.CreatedAt)
                .Skip(keepCount)
                .ToListAsync();

            foreach (var leaderboard in overallLeaderboards)
            {
                leaderboard.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
