using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces.Repositories;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IMatchParticipantRepository : IRepository<MatchParticipant>
    {
        Task<IEnumerable<MatchParticipant>> GetMatchParticipantsAsync(Guid matchId);

        Task<IEnumerable<MatchParticipant>> GetUserParticipationsAsync(Guid userId);
    }
}
