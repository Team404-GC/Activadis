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

                IEnumerable<Rating> ratings = [
                    // Tafeltennis
                    new Rating() { UserId = daan, CategoryId = tafeltennis, CurrentRating = 858.66, PeakRating = 858.66, MatchCount = 3, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = emma, CategoryId = tafeltennis, CurrentRating = 761.15, PeakRating = 800.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = fleur, CategoryId = tafeltennis, CurrentRating = 800.00, PeakRating = 820.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = lotte, CategoryId = tafeltennis, CurrentRating = 782.23, PeakRating = 800.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = milan, CategoryId = tafeltennis, CurrentRating = 797.97, PeakRating = 818.85, MatchCount = 3, CreatedAt = DateTime.UtcNow },

                    // Tafelvoetbal
                    new Rating() { UserId = daan, CategoryId = tafelvoetbal, CurrentRating = 780.00, PeakRating = 800.00, MatchCount = 1, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = emma, CategoryId = tafelvoetbal, CurrentRating = 840.00, PeakRating = 840.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = fleur, CategoryId = tafelvoetbal, CurrentRating = 761.15, PeakRating = 800.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = lotte, CategoryId = tafelvoetbal, CurrentRating = 800.00, PeakRating = 820.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = milan, CategoryId = tafelvoetbal, CurrentRating = 818.85, PeakRating = 818.85, MatchCount = 1, CreatedAt = DateTime.UtcNow },

                    // Darts
                    new Rating() { UserId = daan, CategoryId = darts, CurrentRating = 782.29, PeakRating = 800.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = emma, CategoryId = darts, CurrentRating = 838.85, PeakRating = 838.85, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = fleur, CategoryId = darts, CurrentRating = 817.71, PeakRating = 820.00, MatchCount = 2, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = lotte, CategoryId = darts, CurrentRating = 781.15, PeakRating = 800.00, MatchCount = 1, CreatedAt = DateTime.UtcNow },
                    new Rating() { UserId = milan, CategoryId = darts, CurrentRating = 780.00, PeakRating = 800.00, MatchCount = 1, CreatedAt = DateTime.UtcNow }
                ];

                set.AddRange(ratings);
                context.SaveChanges();
            }
        }
    }
}