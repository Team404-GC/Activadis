using Activadis.Domain.Entities;

namespace Activadis.Application.Extensions
{
    public static class ActivitySignUpExtensions
    {
        public static int? AvailableSpots(this Activity activity)
            => activity.MaxParticipants > 0
                ? Math.Max(activity.MaxParticipants - activity.TotalSignUps, 0)
                : null;

        public static bool HasTakenPlace(this Activity activity)
            => DateTime.UtcNow > activity.EndDate;

        public static bool IsFull(this Activity activity)
            => activity.AvailableSpots() == 0;

        public static bool SignUpDeadlinePassed(this Activity activity)
            => DateTime.UtcNow > activity.SignUpDeadline;

        public static bool IsOpenForSignUp(this Activity activity)
            => !activity.HasTakenPlace()
                && !activity.IsFull()
                && !activity.SignUpDeadlinePassed();
    }
}
