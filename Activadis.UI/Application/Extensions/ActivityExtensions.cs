using Activadis.Shared.DTOs.Activity;
using Activadis.UI.Application.DTOs.Activity;

namespace Activadis.UI.Application.Extensions
{
    public static class ActivityExtensions
    {
        public static EditActivityRequest ToEditActivityRequest(this ActivityDetailResponse activity)
            => new EditActivityRequest()
            {
                Name = activity.Name,
                Description = activity.Description,
                CostPerPerson = activity.CostPerPerson <= 0 ? null : activity.CostPerPerson,
                Location = activity.Location,
                
                MinParticipants = activity.MinParticipants <= 0 ? null : activity.MinParticipants,
                MaxParticipants = activity.MaxParticipants <= 0 ? null : activity.MaxParticipants,

                FoodIncluded = activity.FoodIncluded,
                IsDraft = activity.IsDraft,

                StartDate = activity.StartDate,
                EndDate = activity.EndDate,

                SignUpDeadline = activity.SignUpDeadline,
                SignOutDeadline = activity.SignOutDeadline,
                ActivityId = activity.Id.ToString()
            };
    }
}
