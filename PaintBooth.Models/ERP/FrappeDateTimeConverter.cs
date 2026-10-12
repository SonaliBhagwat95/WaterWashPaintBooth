using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaintBooth.Models.ERP
{
    public class FrappeDateTimeConverter : JsonConverter<DateTime?>
    {
        private static readonly string[] Formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss.ffffff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd"
        };

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParseExact(value, Formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var result))
            {
                return result;
            }

            // Fallback — let .NET try its best guess if none of the exact formats hit
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var fallback))
            {
                return fallback;
            }

            return null; // give up gracefully instead of throwing and killing the whole list
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            else
                writer.WriteNullValue();
        }
    }
}
