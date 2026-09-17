namespace Activadis.UI.Application.DTOs
{
    public class Notification
    {
        public Guid Id { get; set; }
        public bool HasErrored { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public int DurationInMs { get; set; }
        public DateTime SendOn { get; set; }
    }

    public static class Notifications
    {
        public static event Action<Notification>? OnNotification;

        public static void AddNotification(string? message, bool hasErrored = false, int durationInMs = 2500)
        {
            OnNotification?.Invoke(new Notification()
            {
                Id = Guid.NewGuid(),
                Message = message ?? "Er is een onverwachte fout opgetreden.",
                HasErrored = hasErrored,
                DurationInMs = durationInMs,
                SendOn = DateTime.UtcNow
            });
        }

        public static void AddNotification(string? message, int durationInMs = 2500, bool hasErrored = false)
            => AddNotification(message, hasErrored, durationInMs);
    }
}
