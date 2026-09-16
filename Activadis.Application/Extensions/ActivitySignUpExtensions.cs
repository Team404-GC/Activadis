using Activadis.Domain.Entities;

namespace Activadis.Application.Extensions
{
    public static class ActivitySignUpExtensions
    {
        public static bool HasParticipantLimit(this Activity activity)
            => activity.MaxParticipants > 0;

        public static int AvailableSpots(this Activity activity)
            => activity.MaxParticipants - activity.TotalSignUps;

        public static bool IsFull(this Activity activity)
            => activity.HasParticipantLimit()
                && activity.TotalSignUps >= activity.MaxParticipants;

        public static bool HasTakenPlace(this Activity activity)
            => DateTime.UtcNow > activity.EndDate;

        public static bool SignUpDeadlinePassed(this Activity activity)
            => DateTime.UtcNow > activity.SignUpDeadline;

        public static bool IsOpenForSignUp(this Activity activity)
            => !activity.HasTakenPlace()
                && !activity.SignUpDeadlinePassed()
                && !activity.IsFull();
    }
}
