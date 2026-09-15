using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Activadis.Infrastructure.Repositories
{
    public class MatchParticipantRepository : Repository<MatchParticipant>, IMatchParticipantRepository
    {
        private readonly ApplicationDBContext _context;

        public MatchParticipantRepository(ApplicationDBContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MatchParticipant>> GetMatchParticipantsAsync(Guid matchId)
            => await _context.MatchParticipants
                .Include(mp => mp.User)
                .Include(mp => mp.Match)
                    .ThenInclude(m => m!.Category)
                .Where(mp => mp.MatchId == matchId && mp.DeletedAt == null)
                .ToListAsync();

        public async Task<IEnumerable<MatchParticipant>> GetUserParticipationsAsync(Guid userId)
            => await _context.MatchParticipants
                .Include(mp => mp.User)
                .Include(mp => mp.Match)
                    .ThenInclude(m => m!.Category)
                .Where(mp => mp.UserId == userId && mp.DeletedAt == null)
                .OrderByDescending(mp => mp.Match!.MatchDate)
                .ToListAsync();
    }
}
