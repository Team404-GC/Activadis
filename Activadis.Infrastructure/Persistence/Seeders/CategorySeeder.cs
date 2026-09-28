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
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Tafelvoetbal",
                        Description = "Wedstrijden tafelvoetbal in de pauzeruimte.",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category()
                    {
                        Name = "Darts",
                        Description = "Darttoernooi tijdens de vrijdagmiddagborrel.",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                ];

                set.AddRange(categories);
                context.SaveChanges();
            }
        }
    }
}