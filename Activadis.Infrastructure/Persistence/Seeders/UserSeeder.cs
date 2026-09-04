using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;
using Activadis.Domain.Enums;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class UserSeeder
    {
        public static void UseUserSeeder(this DbContext context)
        {
            DbSet<User> set = context.Set<User>();

            if (!set.Any())
            {
                IEnumerable<User> users = [
                    new User()
                    {
                        Email = "beheerder1@covadis.nl",
                        FullName = "Beheerder 1",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.Admin,
                        CreatedAt = DateTime.UtcNow
                    }
                ];

                set.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}
