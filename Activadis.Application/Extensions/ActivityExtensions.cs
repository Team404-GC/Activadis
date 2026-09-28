using Activadis.Application.DTOs.Activity;
using Activadis.Shared.DTOs.Activity;
using Activadis.Domain.Entities;

namespace Activadis.Application.Extensions
{
    public static class ActivityExtensions
    {
        public static Activity ToActivity(this CreateActivityRequest request, byte[] image)
        {
            return new Activity()
            {
                Name = request.Name,
                Description = request.Description,
                CostPerPerson = request.CostPerPerson,

                Location = request.Location,
                Image = image,
                ImageContentType = request.Image.ContentType,

                MinParticipants = request.MinParticipants,
                MaxParticipants = request.MaxParticipants,

                FoodIncluded = request.FoodIncluded,
                ExternalAllowed = request.ExternalAllowed,
                PlusOneAllowed = request.PlusOneAllowed,

                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PublishedOn = request.IsDraft ? null : DateTime.UtcNow,

                SignUpDeadline = request.SignUpDeadline,
                SignOutDeadline = request.SignOutDeadline
            };
        }

        public static ActivityOverviewResponse ToOverviewResponse(this Activity activity, bool hasSignedIn)
        {
            return new ActivityOverviewResponse()
            {
                Id = activity.Id,
                Name = activity.Name,

                Location = activity.Location,
                IsDraft = activity.PublishedOn is null,
                IsDeleted = activity.DeletedAt is not null,
                HasTakenPlace = DateTime.UtcNow >= activity.StartDate,
                HasSignedIn = hasSignedIn,

                StartDate = activity.StartDate,
                EndDate = activity.EndDate
            };
        }

        public static ActivityDetailResponse ToDetailResponse(this Activity activity, bool hasSignedUp)
        {
            return new ActivityDetailResponse()
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                CostPerPerson = activity.CostPerPerson,

                Location = activity.Location,
                Image = activity.Image,
                ImageContentType = activity.ImageContentType,

                MinParticipants = activity.MinParticipants,
                MaxParticipants = activity.MaxParticipants,
                HasParticipantLimit = activity.HasParticipantLimit(),
                TotalSignUps = activity.TotalSignUps,
                AvailableSpots = activity.AvailableSpots(),

                FoodIncluded = activity.FoodIncluded,
                ExternalAllowed = activity.ExternalAllowed,
                PlusOneAllowed = activity.PlusOneAllowed,

                StartDate = activity.StartDate,
                EndDate = activity.EndDate,

                SignUpDeadline = activity.SignUpDeadline,
                SignOutDeadline = activity.SignOutDeadline,

                HasTakenPlace = activity.HasTakenPlace(),
                IsFull = activity.IsFull(),
                SignUpDeadlinePassed = activity.SignUpDeadlinePassed(),
                IsOpenForSignUp = activity.IsOpenForSignUp(),
                HasSignedUp = hasSignedUp
            };
        }
    }
}
