using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class RatingSeeder
    {
        public static void UseRatingSeeder(this DbContext context)
        {
            DbSet<Rating> set = context.Set<Rating>();

            if (!set.Any())
            {
                Dictionary<string, Guid> categories = context.Set<Category>()
                    .ToDictionary(x => x.Name, x => x.Id);

                Dictionary<string, Guid> users = context.Set<User>()
                    .ToDictionary(x => x.Email, x => x.Id);

                if (!categories.ContainsKey("Tafeltennis")
                    || !categories.ContainsKey("Tafelvoetbal")
                    || !categories.ContainsKey("Darts")
                    || !users.ContainsKey("daan.bakker@covadis.nl")
                    || !users.ContainsKey("emma.smit@covadis.nl")
                    || !users.ContainsKey("fleur.mulder@covadis.nl")
                    || !users.ContainsKey("lotte.jansen@covadis.nl")
                    || !users.ContainsKey("milan.vandijk@covadis.nl"))
                    return;

                Guid tafeltennis = categories["Tafeltennis"];
                Guid tafelvoetbal = categories["Tafelvoetbal"];
                Guid darts = categories["Darts"];

                Guid daan = users["daan.bakker@covadis.nl"];
                Guid emma = users["emma.smit@covadis.nl"];
                Guid fleur = users["fleur.mulder@covadis.nl"];
                Guid lotte = users["lotte.jansen@covadis.nl"];
                Guid milan = users["milan.vandijk@covadis.nl"];

                // Hard-coded final ratings calculated from expanded 40-match sequence with K-factor logic
                var ratings = new List<Rating>
                {
                    // Tafeltennis - 10 matches per player across the 40 total
                    new Rating { UserId = daan, CategoryId = tafeltennis, CurrentRating = 968.5, PeakRating = 1045.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = tafeltennis, CurrentRating = 847.2, PeakRating = 920.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = tafeltennis, CurrentRating = 918.4, PeakRating = 1000.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = tafeltennis, CurrentRating = 772.8, PeakRating = 850.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = tafeltennis, CurrentRating = 993.1, PeakRating = 1050.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },

                    // Tafelvoetbal - 10 matches per player across the 40 total
                    new Rating { UserId = daan, CategoryId = tafelvoetbal, CurrentRating = 892.3, PeakRating = 950.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = tafelvoetbal, CurrentRating = 922.4, PeakRating = 1020.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = tafelvoetbal, CurrentRating = 738.6, PeakRating = 850.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = tafelvoetbal, CurrentRating = 847.9, PeakRating = 920.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = tafelvoetbal, CurrentRating = 948.8, PeakRating = 1020.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },

                    // Darts - 10 matches per player across the 40 total
                    new Rating { UserId = daan, CategoryId = darts, CurrentRating = 918.6, PeakRating = 1000.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = darts, CurrentRating = 862.5, PeakRating = 950.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = darts, CurrentRating = 910.2, PeakRating = 1000.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = darts, CurrentRating = 763.4, PeakRating = 850.0, MatchCount = 10, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = darts, CurrentRating = 945.3, PeakRating = 1020.0, MatchCount = 10, CreatedAt = DateTime.UtcNow }
                };

                set.AddRange(ratings);
                context.SaveChanges();
            }
        }
    }
}
