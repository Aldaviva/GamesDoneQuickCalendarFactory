using NodaTime;
using NodaTime.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.Marshal;

public class OffsetDateTimeConverter: JsonConverter<OffsetDateTime> {

    public static readonly  OffsetDateTimeConverter INSTANCE = new();
    private static readonly OffsetDateTimePattern   PATTERN  = OffsetDateTimePattern.GeneralIso;

    public override OffsetDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } jsonString) {
            ParseResult<OffsetDateTime> parsed = PATTERN.Parse(jsonString);
            if (parsed.Success) {
                return parsed.Value;
            } else {
                throw new FormatException($"Could not parse {jsonString} as an ISO-8601 date, time, and UTC offset", parsed.Exception);
            }
        } else {
            throw new InvalidOperationException($"JSON token type must be {nameof(JsonTokenType.String)} to parse as {nameof(OffsetDateTime)}, but was {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, OffsetDateTime value, JsonSerializerOptions options) {
        writer.WriteStringValue(PATTERN.Format(value));
    }

}