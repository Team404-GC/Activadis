using System.Text.Json.Serialization;
using System.Text.Json;

namespace Activadis.API.Converters
{
    public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTime datetime = reader.GetDateTime();
            return DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            DateTime datetime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            writer.WriteStringValue(datetime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}
