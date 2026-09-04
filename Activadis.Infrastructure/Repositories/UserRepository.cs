using Activadis.Domain.Interfaces.Repositories;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

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
    }
}
