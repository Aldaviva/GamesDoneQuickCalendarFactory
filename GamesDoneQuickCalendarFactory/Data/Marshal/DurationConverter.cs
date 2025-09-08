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
                throw new FormatException($"Could not parse {jsonString} as an h:m:s duration", parsed.Exception);
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

    public ParseResult<Duration> Parse(string text) {
        ReadOnlySpan<char> remaining = text.AsSpan();
        Duration           result    = Duration.AdditiveIdentity;
        for (int groupIndex = 0; groupIndex <= 3; groupIndex++) {
            Index groupEnd = groupIndex switch {
                <= 2 => remaining.IndexOf(groupIndex == 2 ? '.' : ':') is var i && i != -1 ? i : ^0,
                > 2  => ^0
            };

            int parsedNumber = int.Parse(remaining[..groupEnd]);

            result += groupIndex switch {
                0 => Duration.FromHours(parsedNumber),
                1 => Duration.FromMinutes(parsedNumber),
                2 => Duration.FromSeconds(parsedNumber),
                3 => Duration.FromMilliseconds(parsedNumber),
                _ => Duration.AdditiveIdentity
            };

            if (groupEnd.Equals(^0) || remaining.Length <= 1) {
                break;
            }

            remaining = remaining[groupEnd..][1..];
        }

        return ParseResult<Duration>.ForValue(result);
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