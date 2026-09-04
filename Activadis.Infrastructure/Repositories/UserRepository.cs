using Activadis.Domain.Entities;
using Activadis.Domain.Interfaces;
using Activadis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }
        public Task<User?> GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefaultAsync(e => e.Email == email);

        }
    }
}
