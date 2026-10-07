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
                        Email = "peter.hoekstra@covadis.nl",
                        FullName = "Peter Hoekstra",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.Admin,
                        JobTitle = "Directeur",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "sanne.devries@covadis.nl",
                        FullName = "Sanne de Vries",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "Projectmanager",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "daan.bakker@covadis.nl",
                        FullName = "Daan Bakker",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "Softwareontwikkelaar",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "lotte.jansen@covadis.nl",
                        FullName = "Lotte Jansen",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "UX-designer",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "thijs.visser@covadis.nl",
                        FullName = "Thijs Visser",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "Backend developer",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "emma.smit@covadis.nl",
                        FullName = "Emma Smit",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "HR-medewerker",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "milan.vandijk@covadis.nl",
                        FullName = "Milan van Dijk",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "Testengineer",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "fleur.mulder@covadis.nl",
                        FullName = "Fleur Mulder",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "Office manager",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User()
                    {
                        Email = "ruben.bos@covadis.nl",
                        FullName = "Ruben Bos",
                        HashedPassword = "$2a$12$OaQw61Dqu1N8ufUzAcVYT.mnAur1KXHqwMm/9fOl4PXmGscAKKAMK", //StrongPassword1!
                        Role = UserRole.User,
                        JobTitle = "Stagiair softwareontwikkeling",
                        CreatedAt = DateTime.UtcNow
                    },
                ];

                set.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}