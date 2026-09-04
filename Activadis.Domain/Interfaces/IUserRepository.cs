using Activadis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByEmail(string email);
    }
}
