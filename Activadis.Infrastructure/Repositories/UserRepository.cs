using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;

namespace Activadis.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDBContext Context;

        public UserRepository(ApplicationDBContext context)
            : base(context)
        {
            Context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
            => await Context.Users.FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null);

        public async Task<User?> GetByPasswordSetupTokenHashAsync(string tokenHash)
            => await Context.Users.FirstOrDefaultAsync(x => x.PasswordSetupTokenHash == tokenHash && x.DeletedAt == null);

        public async Task<int> CountActiveByRoleAsync(UserRole role)
            => await Context.Users.CountAsync(x => x.Role == role && x.IsActive && x.DeletedAt == null);
    }
}
