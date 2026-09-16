using Activadis.Domain.Entities;

namespace Activadis.Application.Extensions
{
    /// <summary>
    /// The rules that decide whether an employee can still sign up for an activity.
    /// The sign-ups of the activity have to be loaded for the spots to be correct.
    /// </summary>
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
