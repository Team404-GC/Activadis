using System.Globalization;

namespace Activadis.Application.Extensions
{
    public static class DateExtensions
    {
        private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-NL");
        private static readonly TimeZoneInfo DutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

        // Dates are stored in UTC, emails are read in Dutch time no matter where the server runs.
        public static DateTime ToDutchTime(this DateTime date)
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(date, DateTimeKind.Utc), DutchTimeZone);

        public static string ToDisplayDateTime(this DateTime date)
            => date.ToDutchTime().ToString("d MMMM yyyy, HH:mm", Dutch);

        public static string ToDisplayPeriod(DateTime start, DateTime end)
            => start.ToDutchTime().Date == end.ToDutchTime().Date
                ? $"{start.ToDisplayDateTime()} - {end.ToDutchTime():HH:mm}"
                : $"{start.ToDisplayDateTime()} - {end.ToDisplayDateTime()}";
    }
}
