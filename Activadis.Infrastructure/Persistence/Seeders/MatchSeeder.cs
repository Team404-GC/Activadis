using Microsoft.EntityFrameworkCore;
using Activadis.Domain.Entities;

namespace Activadis.Infrastructure.Persistence.Seeders
{
    public static class MatchSeeder
    {
        public static void UseMatchSeeder(this DbContext context)
        {
            DbSet<Match> set = context.Set<Match>();

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

                // Participant: (user, placement, actual score, expected score, rating change)
                IEnumerable<Match> matches = [
                    CreateMatch(tafeltennis, 30, (daan, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 29, (fleur, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 27, (emma, 2, 0.0, 0.4712, -18.85), (milan, 1, 1.0, 0.5288, 18.85)),
                    CreateMatch(tafelvoetbal, 26, (emma, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 25, (daan, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 24, (daan, 2, 0.0, 0.5000, -20.00), (lotte, 1, 1.0, 0.5000, 20.00)),
                    CreateMatch(tafeltennis, 22, (lotte, 1, 0.5, 0.4443, 2.23), (milan, 1, 0.5, 0.5557, -2.23)),
                    CreateMatch(darts, 21, (daan, 2, 0.0, 0.5000, -20.00), (emma, 1, 1.0, 0.5000, 20.00)),
                    CreateMatch(tafelvoetbal, 20, (milan, 1, 1.0, 0.5288, 18.85), (fleur, 2, 0.0, 0.4712, -18.85)),
                    CreateMatch(darts, 19, (fleur, 1, 1.0, 0.5000, 20.00), (milan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 18, (daan, 1, 1.0, 0.5336, 18.66), (milan, 2, 0.0, 0.4664, -18.66)),
                    CreateMatch(tafelvoetbal, 15, (lotte, 2, 0.0, 0.5000, -20.00), (emma, 1, 1.0, 0.5000, 20.00)),
                    CreateMatch(darts, 12, (emma, 1, 1.0, 0.5288, 18.85), (lotte, 2, 0.0, 0.4712, -18.85)),
                    CreateMatch(darts, 5, (daan, 1, 0.5, 0.4427, 2.29), (fleur, 1, 0.5, 0.5573, -2.29))
                ];

                set.AddRange(matches);
                context.SaveChanges();
            }
        }

        private static Match CreateMatch(
            Guid categoryId,
            int daysAgo,
            (Guid UserId, int Placement, double ActualScore, double ExpectedScore, double RatingChange) playerA,
            (Guid UserId, int Placement, double ActualScore, double ExpectedScore, double RatingChange) playerB)
        {
            DateTime matchDate = DateTime.UtcNow.AddDays(-daysAgo);

            return new Match()
            {
                CategoryId = categoryId,
                MatchDate = matchDate,
                Notes = null,
                IsFinalized = true,
                CreatedAt = matchDate,
                Participants = [
                    new MatchParticipant()
                    {
                        UserId = playerA.UserId,
                        Placement = playerA.Placement,
                        ActualScore = playerA.ActualScore,
                        ExpectedScore = playerA.ExpectedScore,
                        RatingChange = playerA.RatingChange,
                        CreatedAt = matchDate
                    },
                    new MatchParticipant()
                    {
                        UserId = playerB.UserId,
                        Placement = playerB.Placement,
                        ActualScore = playerB.ActualScore,
                        ExpectedScore = playerB.ExpectedScore,
                        RatingChange = playerB.RatingChange,
                        CreatedAt = matchDate
                    }
                ]
            };
        }
    }
}