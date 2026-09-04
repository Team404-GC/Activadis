using Activadis.Domain.Entities;
using Activadis.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class UserSeeder
    {
        public static void UseUserSeeder(this DbContext context)
        {
            if (!context.Set<User>().Any())
            {
                IEnumerable<User> Users = new List<User>()
                {
                    new User()
                    {
                        Email = "beheerder1@covadis.nl",
                        FullName = "Beheerder 1",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        UserRole = UserRoles.Admin
                    }
                };
                context.Set<User>().AddRange(Users);
                context.SaveChanges();
            }
        }
    }
}
