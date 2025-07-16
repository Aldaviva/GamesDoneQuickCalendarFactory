using NodaTime;
using NodaTime.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.Marshal;

public class DurationConverter: JsonConverter<Duration> {

    public static readonly  DurationConverter  INSTANCE      = new();
    private static readonly IPattern<Duration> COLON_PATTERN = new ColonDelimitedDurationPattern();

    public override Duration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } jsonString) {
            ParseResult<Duration> parsed = COLON_PATTERN.Parse(jsonString);
            if (parsed.Success) {
                return parsed.Value;
            } else {
                throw new FormatException($"Could not parse {jsonString} as an ISO-8601 Period", parsed.Exception);
            }
        } else {
            throw new InvalidOperationException($"JSON token type must be {nameof(JsonTokenType.String)} to parse as {nameof(Duration)}, but was {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, Duration value, JsonSerializerOptions options) {
        writer.WriteStringValue(COLON_PATTERN.Format(value));
    }

}

public class ColonDelimitedDurationPattern: IPattern<Duration> {

    private const char SEPARATOR = ':';

    public ParseResult<Duration> Parse(string text) {
        long[] split;
        try {
            split = text.Split(SEPARATOR, 3).Select(long.Parse).ToArray();
        } catch (FormatException e) {
            return ParseResult<Duration>.ForException(() => e);
        }

        Duration? result = split.Length switch {
            1 => Duration.FromSeconds(split[0]),
            2 => Duration.FromMinutes(split[0]) + Duration.FromSeconds(split[1]),
            3 => Duration.FromHours(split[0]) + Duration.FromMinutes(split[1]) + Duration.FromSeconds(split[2]),
            _ => null
        };

        return result != null ? ParseResult<Duration>.ForValue(result.Value)
            : ParseResult<Duration>.ForException(() => new FormatException($"Could not parse {text} as a time Duration of the form h:m:s"));
    }

    public StringBuilder AppendFormat(Duration value, StringBuilder builder) => builder
        .Append((long) Math.Floor(value.TotalHours))
        .Append(SEPARATOR)
        .Append(value.Minutes)
        .Append(SEPARATOR)
        .Append(value.Seconds);

    public string Format(Duration value) => AppendFormat(value, new StringBuilder()).ToString();

}