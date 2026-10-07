using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IMatchRepository : IRepository<Match>
    {
        Task<IEnumerable<Match>> GetCategoryMatchesAsync(Guid categoryId);

        Task<Match?> GetByIdWithParticipantsAsync(Guid matchId);

        Task<IEnumerable<Match>> GetUserMatchesAsync(Guid userId);

        Task<IEnumerable<Match>> GetRecentMatchesAsync(Guid categoryId, int limit = 10);
    }
}
