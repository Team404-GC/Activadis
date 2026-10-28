using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Repositories
{
    public class MatchRepository : Repository<Match>, IMatchRepository
    {
        private readonly ApplicationDBContext _context;

        public MatchRepository(ApplicationDBContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Match>> GetCategoryMatchesAsync(Guid categoryId)
            => await _context.Matches
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Category)
                .Where(m => m.CategoryId == categoryId && m.DeletedAt == null)
                .OrderByDescending(m => m.MatchDate)
                .ToListAsync();

        public async Task<Match?> GetByIdWithParticipantsAsync(Guid matchId)
            => await _context.Matches
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == matchId && m.DeletedAt == null);

        public async Task<IEnumerable<Match>> GetUserMatchesAsync(Guid userId)
            => await _context.Matches
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Category)
                .Where(m => m.Participants.Any(p => p.UserId == userId) && m.DeletedAt == null)
                .OrderByDescending(m => m.MatchDate)
                .ToListAsync();

        public async Task<IEnumerable<Match>> GetRecentMatchesAsync(Guid? categoryId, int take)
            => await _context.Matches
                .AsNoTracking()
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Category)
                .Where(m => (categoryId == null || m.CategoryId == categoryId) && m.IsFinalized && m.DeletedAt == null)
                .OrderByDescending(m => m.MatchDate)
                .ThenByDescending(m => m.CreatedAt)
                .Take(take)
                .AsSplitQuery()
                .ToListAsync();

        public async Task<IEnumerable<Match>> GetUnlinkedMatchesAsync(IEnumerable<Guid> categoryIds, DateTime from, DateTime to)
            => await _context.Matches
                .AsNoTracking()
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Category)
                .Where(m => categoryIds.Contains(m.CategoryId) && m.ActivityId == null && m.IsFinalized && m.DeletedAt == null)
                .Where(m => m.MatchDate >= from && m.MatchDate < to)
                .OrderByDescending(m => m.MatchDate)
                .AsSplitQuery()
                .ToListAsync();
    }
}
