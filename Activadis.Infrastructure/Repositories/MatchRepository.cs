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

        public async Task<IEnumerable<Match>> GetRecentMatchesAsync(Guid categoryId, int limit = 10)
            => await _context.Matches
                .Include(m => m.Participants)
                    .ThenInclude(p => p.User)
                .Include(m => m.Category)
                .Where(m => m.CategoryId == categoryId && m.DeletedAt == null)
                .OrderByDescending(m => m.MatchDate)
                .Take(limit)
                .ToListAsync();
    }
}
