using Activadis.Domain.Entities;
using Activadis.Application.Services;
using Activadis.Shared.DTOs.Elo;
using Activadis.Shared.DTOs.Elo.Leaderboard;
using Activadis.Shared.DTOs.Elo.Category;

namespace Activadis.Application.Extensions
{
    public static class EloMappingExtensions
    {
        public static CategoryDto ToDto(this Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };
        }

        public static RatingDto ToDto(this Rating rating)
        {
            return new RatingDto
            {
                UserId = rating.UserId,
                CategoryId = rating.CategoryId,
                CategoryName = rating.Category?.Name ?? "Onbekend",
                CurrentRating = rating.CurrentRating,
                MatchCount = rating.MatchCount,
                PeakRating = rating.PeakRating
            };
        }

        public static LeaderboardEntryDto ToDto(this LeaderboardService.LeaderboardEntry entry)
        {
            return new LeaderboardEntryDto
            {
                UserId = entry.UserId,
                FullName = entry.FullName,
                JobTitle = entry.JobTitle,
                Rating = entry.Rating,
                MatchCount = entry.MatchCount,
                PeakRating = entry.PeakRating,
                Rank = entry.Rank
            };
        }

        public static ParticipantResultDto ToDto(this MatchParticipant participant)
        {
            return new ParticipantResultDto
            {
                UserId = participant.UserId,
                FullName = participant.User?.FullName ?? "Onbekend",
                Placement = participant.Placement,
                OldRating = participant.Match != null ? 
                    (participant.Match.Category?.Name == null ? 0 : 
                     participant.RatingChange + (int)participant.ActualScore) : 0,
                NewRating = (int)(participant.RatingChange + (int)participant.ActualScore),
                RatingChange = participant.RatingChange,
                ActualScore = participant.ActualScore,
                ExpectedScore = participant.ExpectedScore,
                TeamId = null
            };
        }
    }
}
