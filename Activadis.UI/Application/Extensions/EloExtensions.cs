using System.Globalization;

namespace Activadis.UI.Application.Extensions
{
    public static class EloExtensions
    {
        public static string ToRating(this double rating)
            => rating.ToString("0.00", CultureInfo.InvariantCulture);

        public static string ToRatingChange(this double change)
            => change > 0
                ? $"+{change.ToRating()}"
                : change.ToRating();
    }
}
