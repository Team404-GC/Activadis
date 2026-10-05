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
                    // Early matches - establishing baseline
                    CreateMatch(tafeltennis, 90, (daan, 1, 1.0, 0.5000, 100.00), (emma, 2, 0.0, 0.5000, -100.00)),
                    CreateMatch(tafeltennis, 89, (fleur, 1, 1.0, 0.5000, 100.00), (lotte, 2, 0.0, 0.5000, -100.00)),
                    CreateMatch(tafeltennis, 88, (emma, 2, 0.0, 0.4712, -47.12), (milan, 1, 1.0, 0.5288, 52.88)),
                    CreateMatch(tafelvoetbal, 87, (emma, 1, 1.0, 0.5000, 100.00), (fleur, 2, 0.0, 0.5000, -100.00)),
                    CreateMatch(tafeltennis, 86, (daan, 1, 1.0, 0.5000, 100.00), (fleur, 2, 0.0, 0.5000, -100.00)),
                    CreateMatch(tafelvoetbal, 85, (daan, 2, 0.0, 0.5000, -100.00), (lotte, 1, 1.0, 0.5000, 100.00)),
                    CreateMatch(darts, 84, (daan, 2, 0.0, 0.5000, -100.00), (emma, 1, 1.0, 0.5000, 100.00)),
                    CreateMatch(tafelvoetbal, 83, (milan, 1, 1.0, 0.5288, 52.88), (fleur, 2, 0.0, 0.4712, -52.88)),
                    CreateMatch(darts, 82, (fleur, 1, 1.0, 0.5000, 100.00), (milan, 2, 0.0, 0.5000, -100.00)),
                    CreateMatch(tafeltennis, 81, (daan, 1, 1.0, 0.5336, 86.78), (milan, 2, 0.0, 0.4664, -86.78)),

                    // Mid-tier matches - more activity
                    CreateMatch(tafelvoetbal, 80, (lotte, 2, 0.0, 0.5000, -100.00), (emma, 1, 1.0, 0.5000, 100.00)),
                    CreateMatch(darts, 79, (emma, 1, 1.0, 0.5288, 52.88), (lotte, 2, 0.0, 0.4712, -52.88)),
                    CreateMatch(tafeltennis, 78, (daan, 1, 0.5, 0.5168, 27.08), (lotte, 1, 0.5, 0.4832, -48.32)),
                    CreateMatch(tafelvoetbal, 77, (fleur, 1, 1.0, 0.4664, 86.78), (daan, 2, 0.0, 0.5336, -53.36)),
                    CreateMatch(darts, 76, (milan, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 75, (emma, 1, 1.0, 0.5288, 18.85), (fleur, 2, 0.0, 0.4712, -18.85)),
                    CreateMatch(tafelvoetbal, 74, (daan, 1, 1.0, 0.5000, 20.00), (milan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 73, (lotte, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 72, (fleur, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 71, (emma, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),

                    // Recent matches - more competitive
                    CreateMatch(darts, 70, (daan, 1, 1.0, 0.5000, 20.00), (milan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 69, (milan, 1, 1.0, 0.5287, 18.85), (daan, 2, 0.0, 0.4713, -18.85)),
                    CreateMatch(tafelvoetbal, 68, (fleur, 1, 1.0, 0.5168, 27.08), (emma, 2, 0.0, 0.4832, -19.28)),
                    CreateMatch(darts, 67, (emma, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 66, (lotte, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 65, (daan, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 64, (milan, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 63, (fleur, 1, 1.0, 0.5288, 18.85), (daan, 2, 0.0, 0.4712, -18.85)),
                    CreateMatch(tafelvoetbal, 62, (lotte, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 61, (emma, 1, 1.0, 0.5336, 18.66), (daan, 2, 0.0, 0.4664, -18.66)),

                    // Latest matches - high frequency
                    CreateMatch(tafeltennis, 60, (daan, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 59, (milan, 1, 1.0, 0.5288, 18.85), (daan, 2, 0.0, 0.4712, -18.85)),
                    CreateMatch(darts, 58, (fleur, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 57, (emma, 1, 1.0, 0.5000, 20.00), (milan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 56, (fleur, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 55, (lotte, 1, 1.0, 0.5000, 20.00), (daan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 54, (milan, 1, 1.0, 0.5287, 18.85), (fleur, 2, 0.0, 0.4713, -18.85)),
                    CreateMatch(tafelvoetbal, 53, (emma, 1, 1.0, 0.5000, 20.00), (daan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 52, (daan, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 51, (lotte, 1, 0.5, 0.4713, 5.74), (fleur, 1, 0.5, 0.5287, -9.36)),

                    // Final recent matches
                    CreateMatch(tafelvoetbal, 50, (daan, 1, 1.0, 0.5000, 20.00), (fleur, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 49, (milan, 1, 1.0, 0.5336, 18.66), (lotte, 2, 0.0, 0.4664, -18.66)),
                    CreateMatch(tafeltennis, 48, (emma, 1, 1.0, 0.5288, 18.85), (daan, 2, 0.0, 0.4712, -18.85)),
                    CreateMatch(tafelvoetbal, 47, (lotte, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(darts, 46, (fleur, 1, 1.0, 0.5000, 20.00), (milan, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 45, (daan, 1, 1.0, 0.5000, 20.00), (emma, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 44, (milan, 1, 1.0, 0.5287, 18.85), (fleur, 2, 0.0, 0.4713, -18.85)),
                    CreateMatch(darts, 43, (emma, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafeltennis, 42, (fleur, 1, 1.0, 0.5000, 20.00), (lotte, 2, 0.0, 0.5000, -20.00)),
                    CreateMatch(tafelvoetbal, 41, (daan, 1, 0.5, 0.5168, 0.16), (lotte, 1, 0.5, 0.4832, 1.34))
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