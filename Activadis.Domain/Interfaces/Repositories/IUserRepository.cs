using Activadis.Domain.Entities;
using Activadis.Domain.Enums;

namespace Activadis.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPasswordSetupTokenHashAsync(string tokenHash);
        Task<int> CountByRoleAsync(UserRole role);
    }
}
