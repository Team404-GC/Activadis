using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class CategorySeeder
    {
        public static void UseCategorySeeder(this DbContext context)
        {
            DbSet<Category> set = context.Set<Category>();

            if (!set.Any())
            {
                IEnumerable<Category> categories = [
                    new Category()
                    {
                        Name = "Tafeltennis",
                        Description = "Wedstrijden tafeltennis op kantoor.",
                        DisplayOrder = 1,
                        IsActive = true,
                        RankTitles = DefaultRankTitles(),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Tafelvoetbal",
                        Description = "Wedstrijden tafelvoetbal in de pauzeruimte.",
                        DisplayOrder = 2,
                        IsActive = true,
                        RankTitles = DefaultRankTitles(),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Darts",
                        Description = "Darttoernooi tijdens de vrijdagmiddagborrel.",
                        DisplayOrder = 3,
                        IsActive = true,
                        RankTitles = DefaultRankTitles(),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Pool",
                        Description = "Potjes pool aan de pooltafel in de kantine.",
                        DisplayOrder = 4,
                        IsActive = true,
                        RankTitles = DefaultRankTitles(),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Schaken",
                        Description = "Schaakpartijtjes tijdens de lunchpauze.",
                        DisplayOrder = 5,
                        IsActive = true,
                        RankTitles = DefaultRankTitles(),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Mario Kart",
                        Description = "Races op de spelcomputer na het werk.",
                        DisplayOrder = 6,
                        IsActive = true,
                        RankTitles = DefaultRankTitles(),
                        CreatedAt = DateTime.UtcNow
                    }
                ];

                set.AddRange(categories);
                context.SaveChanges();
            }
        }

        private static Dictionary<double, string> DefaultRankTitles() => new()
        {
            [0] = "Beginner",
            [550] = "Amateur",
            [750] = "Gevorderde",
            [950] = "Expert",
            [1100] = "Meester",
        };
    }
}