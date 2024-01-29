using NodaTime;
using NodaTime.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.Marshal;

public class PeriodConverter: JsonConverter<Period> {

    public static readonly  PeriodConverter  INSTANCE      = new();
    private static readonly IPattern<Period> COLON_PATTERN = new ColonDelimitedPeriodPattern();

    public override Period Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String && reader.GetString() is { } jsonString) {
            ParseResult<Period> parsed = COLON_PATTERN.Parse(jsonString);
            if (parsed.Success) {
                return parsed.Value;
            } else {
                throw new FormatException($"Could not parse {jsonString} as an ISO-8601 period", parsed.Exception);
            }
        } else {
            throw new InvalidOperationException($"JSON token type must be {nameof(JsonTokenType.String)} to parse as {nameof(Period)}, but was {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, Period value, JsonSerializerOptions options) {
        writer.WriteStringValue(COLON_PATTERN.Format(value));
    }

}

public class ColonDelimitedPeriodPattern: IPattern<Period> {

    private const char SEPARATOR = ':';

    public ParseResult<Period> Parse(string text) {
        long[] split;
        try {
            split = text.Split(SEPARATOR).Select(long.Parse).ToArray();
        } catch (FormatException e) {
            return ParseResult<Period>.ForException(() => e);
        }

        Period? result = split.Length switch {
            1 => Period.FromSeconds(split[0]),
            2 => Period.FromMinutes(split[0]) + Period.FromSeconds(split[1]),
            3 => Period.FromHours(split[0]) + Period.FromMinutes(split[1]) + Period.FromSeconds(split[2]),
            _ => null
        };

        return result != null ? ParseResult<Period>.ForValue(result) : ParseResult<Period>.ForException(() => new FormatException($"Could not parse {text} as a time period of the form h:m:s"));
    }

    public StringBuilder AppendFormat(Period value, StringBuilder builder) {
        Period normalized = value.Normalize();
        builder.Append((long) Math.Floor(normalized.ToDuration().TotalHours))
            .Append(SEPARATOR)
            .Append(normalized.Minutes)
            .Append(SEPARATOR)
            .Append(normalized.Seconds);
        return builder;
    }

    public string Format(Period value) {
        StringBuilder stringBuilder = new();
        AppendFormat(value, stringBuilder);
        return stringBuilder.ToString();
    }

}