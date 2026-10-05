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
                    || !categories.ContainsKey("Pool")
                    || !categories.ContainsKey("Schaken")
                    || !categories.ContainsKey("Mario Kart")
                    || !users.ContainsKey("daan.bakker@covadis.nl")
                    || !users.ContainsKey("milan.vandijk@covadis.nl")
                    || !users.ContainsKey("thijs.visser@covadis.nl")
                    || !users.ContainsKey("sanne.devries@covadis.nl")
                    || !users.ContainsKey("emma.smit@covadis.nl")
                    || !users.ContainsKey("peter.hoekstra@covadis.nl")
                    || !users.ContainsKey("lotte.jansen@covadis.nl")
                    || !users.ContainsKey("fleur.mulder@covadis.nl")
                    || !users.ContainsKey("ruben.bos@covadis.nl"))
                    return;

                Guid tafeltennis = categories["Tafeltennis"];
                Guid tafelvoetbal = categories["Tafelvoetbal"];
                Guid darts = categories["Darts"];
                Guid pool = categories["Pool"];
                Guid schaken = categories["Schaken"];
                Guid marioKart = categories["Mario Kart"];

                Guid daan = users["daan.bakker@covadis.nl"];
                Guid milan = users["milan.vandijk@covadis.nl"];
                Guid thijs = users["thijs.visser@covadis.nl"];
                Guid sanne = users["sanne.devries@covadis.nl"];
                Guid emma = users["emma.smit@covadis.nl"];
                Guid peter = users["peter.hoekstra@covadis.nl"];
                Guid lotte = users["lotte.jansen@covadis.nl"];
                Guid fleur = users["fleur.mulder@covadis.nl"];
                Guid ruben = users["ruben.bos@covadis.nl"];

                // Hard-coded final ratings calculated from the 4000-match sequence with K-factor logic
                var ratings = new List<Rating>
                {
                    // Tafeltennis
                    new Rating { UserId = daan, CategoryId = tafeltennis, CurrentRating = 1149.4, PeakRating = 1155.0, MatchCount = 253, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = tafeltennis, CurrentRating = 1201.1, PeakRating = 1230.3, MatchCount = 165, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = thijs, CategoryId = tafeltennis, CurrentRating = 948.2, PeakRating = 1070.5, MatchCount = 209, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = sanne, CategoryId = tafeltennis, CurrentRating = 845.4, PeakRating = 995.6, MatchCount = 209, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = tafeltennis, CurrentRating = 657.2, PeakRating = 879.0, MatchCount = 119, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = peter, CategoryId = tafeltennis, CurrentRating = 600.2, PeakRating = 837.9, MatchCount = 141, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = tafeltennis, CurrentRating = 623.0, PeakRating = 850.0, MatchCount = 96, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = tafeltennis, CurrentRating = 437.8, PeakRating = 800.0, MatchCount = 104, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = ruben, CategoryId = tafeltennis, CurrentRating = 413.0, PeakRating = 800.0, MatchCount = 62, CreatedAt = DateTime.UtcNow },

                    // Tafelvoetbal
                    new Rating { UserId = daan, CategoryId = tafelvoetbal, CurrentRating = 1162.9, PeakRating = 1213.4, MatchCount = 206, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = tafelvoetbal, CurrentRating = 933.9, PeakRating = 1055.6, MatchCount = 239, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = thijs, CategoryId = tafelvoetbal, CurrentRating = 1065.0, PeakRating = 1092.9, MatchCount = 264, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = sanne, CategoryId = tafelvoetbal, CurrentRating = 986.8, PeakRating = 1028.2, MatchCount = 171, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = tafelvoetbal, CurrentRating = 820.0, PeakRating = 851.3, MatchCount = 113, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = peter, CategoryId = tafelvoetbal, CurrentRating = 543.3, PeakRating = 800.0, MatchCount = 118, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = tafelvoetbal, CurrentRating = 528.1, PeakRating = 838.4, MatchCount = 89, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = tafelvoetbal, CurrentRating = 519.4, PeakRating = 807.1, MatchCount = 92, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = ruben, CategoryId = tafelvoetbal, CurrentRating = 382.2, PeakRating = 800.0, MatchCount = 52, CreatedAt = DateTime.UtcNow },

                    // Darts
                    new Rating { UserId = daan, CategoryId = darts, CurrentRating = 1133.8, PeakRating = 1141.7, MatchCount = 315, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = darts, CurrentRating = 1037.8, PeakRating = 1156.2, MatchCount = 147, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = thijs, CategoryId = darts, CurrentRating = 859.5, PeakRating = 1075.0, MatchCount = 145, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = sanne, CategoryId = darts, CurrentRating = 946.3, PeakRating = 1005.2, MatchCount = 233, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = darts, CurrentRating = 744.3, PeakRating = 882.9, MatchCount = 140, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = peter, CategoryId = darts, CurrentRating = 651.0, PeakRating = 845.6, MatchCount = 139, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = darts, CurrentRating = 580.6, PeakRating = 800.0, MatchCount = 81, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = darts, CurrentRating = 462.0, PeakRating = 800.0, MatchCount = 52, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = ruben, CategoryId = darts, CurrentRating = 521.4, PeakRating = 800.0, MatchCount = 76, CreatedAt = DateTime.UtcNow },

                    // Pool
                    new Rating { UserId = daan, CategoryId = pool, CurrentRating = 1112.9, PeakRating = 1259.7, MatchCount = 193, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = pool, CurrentRating = 1122.0, PeakRating = 1146.7, MatchCount = 239, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = thijs, CategoryId = pool, CurrentRating = 978.0, PeakRating = 1030.2, MatchCount = 174, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = sanne, CategoryId = pool, CurrentRating = 904.6, PeakRating = 935.3, MatchCount = 105, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = pool, CurrentRating = 623.0, PeakRating = 824.3, MatchCount = 203, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = peter, CategoryId = pool, CurrentRating = 623.2, PeakRating = 961.3, MatchCount = 99, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = pool, CurrentRating = 681.0, PeakRating = 800.0, MatchCount = 131, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = pool, CurrentRating = 395.7, PeakRating = 800.0, MatchCount = 86, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = ruben, CategoryId = pool, CurrentRating = 437.2, PeakRating = 800.0, MatchCount = 40, CreatedAt = DateTime.UtcNow },

                    // Schaken
                    new Rating { UserId = daan, CategoryId = schaken, CurrentRating = 1221.0, PeakRating = 1268.7, MatchCount = 292, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = schaken, CurrentRating = 1136.5, PeakRating = 1224.5, MatchCount = 152, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = thijs, CategoryId = schaken, CurrentRating = 962.5, PeakRating = 1017.1, MatchCount = 224, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = sanne, CategoryId = schaken, CurrentRating = 923.8, PeakRating = 970.8, MatchCount = 239, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = schaken, CurrentRating = 807.1, PeakRating = 853.0, MatchCount = 113, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = peter, CategoryId = schaken, CurrentRating = 652.2, PeakRating = 868.4, MatchCount = 83, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = schaken, CurrentRating = 516.0, PeakRating = 850.0, MatchCount = 96, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = schaken, CurrentRating = 495.8, PeakRating = 800.0, MatchCount = 59, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = ruben, CategoryId = schaken, CurrentRating = 375.6, PeakRating = 800.0, MatchCount = 62, CreatedAt = DateTime.UtcNow },

                    // Mario Kart
                    new Rating { UserId = daan, CategoryId = marioKart, CurrentRating = 1169.0, PeakRating = 1246.8, MatchCount = 250, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = milan, CategoryId = marioKart, CurrentRating = 1062.1, PeakRating = 1109.0, MatchCount = 165, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = thijs, CategoryId = marioKart, CurrentRating = 983.0, PeakRating = 1107.9, MatchCount = 275, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = sanne, CategoryId = marioKart, CurrentRating = 799.8, PeakRating = 965.8, MatchCount = 164, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = emma, CategoryId = marioKart, CurrentRating = 750.7, PeakRating = 941.5, MatchCount = 164, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = peter, CategoryId = marioKart, CurrentRating = 642.2, PeakRating = 800.0, MatchCount = 118, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = lotte, CategoryId = marioKart, CurrentRating = 529.9, PeakRating = 845.1, MatchCount = 91, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = fleur, CategoryId = marioKart, CurrentRating = 462.3, PeakRating = 858.8, MatchCount = 93, CreatedAt = DateTime.UtcNow },
                    new Rating { UserId = ruben, CategoryId = marioKart, CurrentRating = 455.6, PeakRating = 862.1, MatchCount = 60, CreatedAt = DateTime.UtcNow }
                };

                set.AddRange(ratings);
                context.SaveChanges();
            }
        }
    }
}