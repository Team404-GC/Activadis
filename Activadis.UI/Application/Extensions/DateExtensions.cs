using System.Globalization;

namespace Activadis.UI.Application.Extensions
{
    public static class DateExtensions
    {
        private static readonly CultureInfo Dutch = CultureInfo.GetCultureInfo("nl-NL");

        private static readonly string[] InputFormats =
        [
            "d MMMM yyyy, H:mm", "d MMMM yyyy H:mm", "d MMMM yyyy",
            "d-M-yyyy H:mm", "d-M-yyyy", "d/M/yyyy H:mm", "d/M/yyyy"
        ];

        public static string ToDisplayDate(this DateTime date)
            => date.ToLocalTime().ToString("d MMMM yyyy", Dutch);

        public static string ToDisplayTime(this DateTime date)
            => date.ToLocalTime().ToString("HH:mm", Dutch);

        public static string ToDisplayDateTime(this DateTime date)
            => $"{date.ToDisplayDate()}, {date.ToDisplayTime()}";

        public static string ToDisplayPeriod(DateTime start, DateTime end)
            => start.ToLocalTime().Date == end.ToLocalTime().Date
                ? $"{start.ToDisplayDateTime()} - {end.ToDisplayTime()}"
                : $"{start.ToDisplayDateTime()} - {end.ToDisplayDateTime()}";

        public static bool TryParseInput(string? text, out DateTime date)
            => DateTime.TryParseExact(text?.Trim(), InputFormats, Dutch, DateTimeStyles.AllowWhiteSpaces, out date);
    }
}
