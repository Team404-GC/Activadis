using Activadis.Domain.Entities;

namespace Activadis.Application.Validations.Activity
{
    public static class LinkMatchValidation
    {
        public static void ValidateLink(this Match match, Domain.Entities.Activity activity)
        {
            if (match.ActivityId == activity.Id)
                throw new ArgumentException("Deze wedstrijd is al aan deze activiteit gekoppeld.");

            if (match.ActivityId is not null)
                throw new ArgumentException("Deze wedstrijd is al aan een andere activiteit gekoppeld.");

            activity.ValidateCategory(match.CategoryId);
        }

        public static void ValidateUnlink(this Match match, Guid activityId)
        {
            if (match.ActivityId != activityId)
                throw new ArgumentException("Deze wedstrijd is niet aan deze activiteit gekoppeld.");
        }
    }
}
