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
            if (jsonString == "0") {
                return Duration.Zero;
            } else {
                ParseResult<Duration> parsed = COLON_PATTERN.Parse(jsonString);
                if (parsed.Success) {
                    return parsed.Value;
                } else {
                    throw new FormatException($"Could not parse {jsonString} as an h:m:s duration", parsed.Exception);
                }
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

    private static readonly char[] SEPARATORS = [':', '.'];

    public ParseResult<Duration> Parse(string text) {
        long[] split;
        try {
            split = text.Split(SEPARATORS, 4).Select(long.Parse).ToArray();
        } catch (FormatException e) {
            return ParseResult<Duration>.ForException(() => e);
        }

        Duration? result = split.Length switch {
            1 => Duration.FromSeconds(split[0]),
            2 => Duration.FromMinutes(split[0]) + Duration.FromSeconds(split[1]),
            3 => Duration.FromHours(split[0]) + Duration.FromMinutes(split[1]) + Duration.FromSeconds(split[2]),
            4 => Duration.FromHours(split[0]) + Duration.FromMinutes(split[1]) + Duration.FromSeconds(split[2]) + Duration.FromMilliseconds(split[3]),
            _ => null
        };

        return result != null ? ParseResult<Duration>.ForValue(result.Value)
            : ParseResult<Duration>.ForException(() => new FormatException($"Could not parse {text} as a Noda Duration of the form h:m:s"));
    }

    public StringBuilder AppendFormat(Duration value, StringBuilder builder) {
        builder.Append((long) Math.Floor(value.TotalHours))
            .Append(value.Minutes.ToString(":00"))
            .Append(value.Seconds.ToString(":00"));

        if (value.Milliseconds != 0) {
            builder.Append(value.Milliseconds.ToString(".000"));
        }

        return builder;
    }

    public string Format(Duration value) => AppendFormat(value, new StringBuilder()).ToString();

}