namespace Activadis.Application.Services
{
    public class EloCalculationService
    {
        public const double DefaultMaxRating = 2000;

        public const double LeagueTierMaxRating = 2500;

        public const double MinRatingFloor = 100;

        public const double InitialRating = 1200;

        public static double GetKFactor(int matchCount)
        {
            return matchCount switch
            {
                < 10 => 40, 
                < 30 => 20, 
                _ => 10  
            };
        }

        public static double CalculateExpectedScore(double playerRating, double opponentRating)
        {
            return 1.0 / (1.0 + Math.Pow(10, (opponentRating - playerRating) / 400.0));
        }

        public static double CalculateNewRating(
            double currentRating,
            double actualScore,
            double expectedScore,
            int matchCount,
            double maxRating = DefaultMaxRating)
        {
            double kFactor = GetKFactor(matchCount);
            double ratingChange = kFactor * (actualScore - expectedScore);
            double newRating = currentRating + ratingChange;

            return Math.Clamp(newRating, MinRatingFloor, maxRating);
        }

        public static double CalculateOverallRating(Dictionary<string, (double rating, int matchCount)> categoryRatingsWithCounts)
        {
            if (categoryRatingsWithCounts == null || categoryRatingsWithCounts.Count == 0)
            {
                return InitialRating;
            }

            int totalMatches = categoryRatingsWithCounts.Values.Sum(x => x.matchCount);
            if (totalMatches == 0)
            {
                return InitialRating;
            }

            double weightedSum = categoryRatingsWithCounts.Values.Sum(x => x.rating * x.matchCount);
            return weightedSum / totalMatches;
        }

        public static double ConvertPlacementToScore(int placement, int totalParticipants, bool isTie = false)
        {
            if (isTie)
            {
                return 0.5;
            }

            if (totalParticipants <= 2)
            {
                return placement == 1 ? 1.0 : 0.0;
            }

            return Math.Max(0.0, 1.0 - ((placement - 1.0) / (totalParticipants - 1.0)));
        }

        public static double CalculateTeamAverageRating(List<double> teamMemberRatings)
        {
            if (teamMemberRatings == null || teamMemberRatings.Count == 0)
                return InitialRating;

            return teamMemberRatings.Average();
        }

        public static double CalculateTeamExpectedScore(List<double> teamRatings, List<double> opponentTeamRatings)
        {
            double teamAvg = CalculateTeamAverageRating(teamRatings);
            double opponentAvg = CalculateTeamAverageRating(opponentTeamRatings);

            return CalculateExpectedScore(teamAvg, opponentAvg);
        }

        public static List<double> CalculateTeamNewRatings(
            List<double> teamMemberRatings,
            double teamActualScore,
            double teamExpectedScore,
            List<int> matchCounts,
            double maxRating = DefaultMaxRating)
        {
            if (teamMemberRatings.Count != matchCounts.Count)
                throw new ArgumentException("Teamleden-ratings en wedstrijdaantallen moeten even lang zijn");

            var newRatings = new List<double>();

            for (int i = 0; i < teamMemberRatings.Count; i++)
            {
                double newRating = CalculateNewRating(
                    teamMemberRatings[i],
                    teamActualScore,
                    teamExpectedScore,
                    matchCounts[i],
                    maxRating
                );
                newRatings.Add(newRating);
            }

            return newRatings;
        }

        public static bool IsValidActualScore(double actualScore)
        {
            return actualScore >= 0.0 && actualScore <= 1.0;
        }

        public static bool IsValidRating(double rating, double maxRating = DefaultMaxRating)
        {
            return rating >= MinRatingFloor && rating <= maxRating;
        }
    }
}
