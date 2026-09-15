using Activadis.Application.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Activadis.UnitTests
{
    public class EloCalculationServiceTests
    {
        [Fact]
        public void GetKFactor_NewPlayer_Returns40()
        {
            var result = EloCalculationService.GetKFactor(5);

            Assert.Equal(40, result);
        }

        [Fact]
        public void GetKFactor_IntermediatePlayer_Returns20()
        {
            var result = EloCalculationService.GetKFactor(15);

            Assert.Equal(20, result);
        }

        [Fact]
        public void GetKFactor_ExperiencedPlayer_Returns10()
        {
            var result = EloCalculationService.GetKFactor(50);

            Assert.Equal(10, result);
        }

        [Fact]
        public void GetKFactor_BoundaryAtNine_Returns40()
        {
            var result = EloCalculationService.GetKFactor(9);

            Assert.Equal(40, result);
        }

        [Fact]
        public void GetKFactor_BoundaryAtTen_Returns20()
        {
            var result = EloCalculationService.GetKFactor(10);

            Assert.Equal(20, result);
        }

        [Fact]
        public void CalculateExpectedScore_EqualRatings_ReturnsHalf()
        {
            var result = EloCalculationService.CalculateExpectedScore(1200, 1200);

            Assert.Equal(0.5, result, 3);
        }

        [Fact]
        public void CalculateExpectedScore_HigherRating_ReturnsGreaterThanHalf()
        {
            var result = EloCalculationService.CalculateExpectedScore(1400, 1200);

            Assert.True(result > 0.5);
        }

        [Fact]
        public void CalculateExpectedScore_LowerRating_ReturnsLessThanHalf()
        {
            var result = EloCalculationService.CalculateExpectedScore(1200, 1400);

            Assert.True(result < 0.5);
        }

        [Fact]
        public void CalculateNewRating_Win_IncreasesRating()
        {
            var result = EloCalculationService.CalculateNewRating(1200, 1.0, 0.5, 5);

            Assert.True(result > 1200);
        }

        [Fact]
        public void CalculateNewRating_Loss_DecreasesRating()
        {
            var result = EloCalculationService.CalculateNewRating(1200, 0.0, 0.5, 5);

            Assert.True(result < 1200);
        }

        [Fact]
        public void CalculateNewRating_ExceedsMax_ClampedToMax()
        {
            var result = EloCalculationService.CalculateNewRating(1995, 1.0, 0.0, 5, maxRating: 2000);

            Assert.Equal(2000, result);
        }

        [Fact]
        public void CalculateNewRating_BelowFloor_ClampedToFloor()
        {
            var result = EloCalculationService.CalculateNewRating(105, 0.0, 1.0, 5);

            Assert.Equal(EloCalculationService.MinRatingFloor, result);
        }

        [Fact]
        public void CalculateOverallRating_NoCategories_ReturnsInitialRating()
        {
            var result = EloCalculationService.CalculateOverallRating(new Dictionary<string, (double rating, int matchCount)>());

            Assert.Equal(EloCalculationService.InitialRating, result);
        }

        [Fact]
        public void CalculateOverallRating_NullDictionary_ReturnsInitialRating()
        {
            var result = EloCalculationService.CalculateOverallRating(null);

            Assert.Equal(EloCalculationService.InitialRating, result);
        }

        [Fact]
        public void CalculateOverallRating_WeightedAverage_ReturnsCorrectValue()
        {
            var categoryRatings = new Dictionary<string, (double rating, int matchCount)>
            {
                { "chess", (1400, 10) },
                { "darts", (1200, 30) },
            };

            var result = EloCalculationService.CalculateOverallRating(categoryRatings);

            Assert.Equal(1250, result, 3);
        }

        [Fact]
        public void ConvertPlacementToScore_Tie_ReturnsHalf()
        {
            var result = EloCalculationService.ConvertPlacementToScore(1, 2, isTie: true);

            Assert.Equal(0.5, result);
        }

        [Fact]
        public void ConvertPlacementToScore_OneVOneWin_ReturnsOne()
        {
            var result = EloCalculationService.ConvertPlacementToScore(1, 2);

            Assert.Equal(1.0, result);
        }

        [Fact]
        public void ConvertPlacementToScore_OneVOneLoss_ReturnsZero()
        {
            var result = EloCalculationService.ConvertPlacementToScore(2, 2);

            Assert.Equal(0.0, result);
        }

        [Fact]
        public void ConvertPlacementToScore_GroupMatchFirstPlace_ReturnsOne()
        {
            var result = EloCalculationService.ConvertPlacementToScore(1, 4);

            Assert.Equal(1.0, result);
        }

        [Fact]
        public void ConvertPlacementToScore_GroupMatchLastPlace_ReturnsZero()
        {
            var result = EloCalculationService.ConvertPlacementToScore(4, 4);

            Assert.Equal(0.0, result);
        }

        [Fact]
        public void ConvertPlacementToScore_GroupMatchMiddlePlace_ReturnsInterpolatedScore()
        {
            var result = EloCalculationService.ConvertPlacementToScore(2, 4);

            Assert.Equal(0.6667, result, 3);
        }

        [Fact]
        public void CalculateTeamAverageRating_EmptyList_ReturnsInitialRating()
        {
            var result = EloCalculationService.CalculateTeamAverageRating(new List<double>());

            Assert.Equal(EloCalculationService.InitialRating, result);
        }

        [Fact]
        public void CalculateTeamAverageRating_NullList_ReturnsInitialRating()
        {
            var result = EloCalculationService.CalculateTeamAverageRating(null);

            Assert.Equal(EloCalculationService.InitialRating, result);
        }

        [Fact]
        public void CalculateTeamAverageRating_ValidList_ReturnsAverage()
        {
            var ratings = new List<double> { 1200, 1400, 1300, 1250, 1350 };

            var result = EloCalculationService.CalculateTeamAverageRating(ratings);

            Assert.Equal(1300, result, 3);
        }

        [Fact]
        public void CalculateTeamExpectedScore_EqualTeams_ReturnsHalf()
        {
            var teamA = new List<double> { 1200, 1200 };
            var teamB = new List<double> { 1200, 1200 };

            var result = EloCalculationService.CalculateTeamExpectedScore(teamA, teamB);

            Assert.Equal(0.5, result, 3);
        }

        [Fact]
        public void CalculateTeamExpectedScore_StrongerTeam_ReturnsGreaterThanHalf()
        {
            var teamA = new List<double> { 1400, 1400 };
            var teamB = new List<double> { 1200, 1200 };

            var result = EloCalculationService.CalculateTeamExpectedScore(teamA, teamB);

            Assert.True(result > 0.5);
        }

        [Fact]
        public void CalculateTeamNewRatings_MismatchedLengths_ThrowsArgumentException()
        {
            var ratings = new List<double> { 1200, 1300 };
            var matchCounts = new List<int> { 5 };

            Assert.Throws<ArgumentException>(() =>
                EloCalculationService.CalculateTeamNewRatings(ratings, 1.0, 0.5, matchCounts));
        }

        [Fact]
        public void CalculateTeamNewRatings_ValidInput_ReturnsUpdatedRatings()
        {
            var ratings = new List<double> { 1200, 1300 };
            var matchCounts = new List<int> { 5, 40 };

            var result = EloCalculationService.CalculateTeamNewRatings(ratings, 1.0, 0.5, matchCounts);

            Assert.Equal(2, result.Count);
            Assert.True(result[0] > ratings[0]);
            Assert.True(result[1] > ratings[1]);
            // New player (K=40) should move more than experienced player (K=10) on the same outcome
            Assert.True((result[0] - ratings[0]) > (result[1] - ratings[1]));
        }

        [Theory]
        [InlineData(0.0, true)]
        [InlineData(0.5, true)]
        [InlineData(1.0, true)]
        [InlineData(-0.1, false)]
        [InlineData(1.1, false)]
        public void IsValidActualScore_VariousValues_ReturnsExpected(double score, bool expected)
        {
            var result = EloCalculationService.IsValidActualScore(score);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(100, true)]
        [InlineData(1200, true)]
        [InlineData(2000, true)]
        [InlineData(99, false)]
        [InlineData(2001, false)]
        public void IsValidRating_VariousValues_ReturnsExpected(double rating, bool expected)
        {
            var result = EloCalculationService.IsValidRating(rating);

            Assert.Equal(expected, result);
        }
    }
}