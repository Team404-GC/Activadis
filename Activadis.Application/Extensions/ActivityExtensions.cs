using Activadis.Application.DTOs.Activity;
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

                MinParticipants = request.MinParticipants,
                MaxParticipants = request.MaxParticipants,

                FoodIncluded = request.FoodIncluded,
                ExternalAllowed = request.ExternalAllowed,
                PlusOneAllowed = request.PlusOneAllowed,

                StartDate = request.StartDate,
                EndDate = request.EndDate,

                SignUpDeadline = request.SignUpDeadline,
                SignOutDeadline = request.SignOutDeadline
            };
        }
    }
}
