using Activadis.Domain.Entities;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByIdentifierAsync(string identifier);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPasswordSetupTokenHashAsync(string tokenHash);
    }
}
