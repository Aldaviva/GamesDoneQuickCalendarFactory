using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.Marshal;

/// <summary>
/// Copy of <see cref="System.Text.Json.Serialization.Converters.TimeSpanConverter"/> that can handle <c>"0"</c> inputs
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Copied third-party code")]
public class ZeroTolerantTimeSpanConverter: JsonConverter<TimeSpan> {

    public static readonly ZeroTolerantTimeSpanConverter INSTANCE = new();

    private const int MINIMUM_TIME_SPAN_FORMAT_LENGTH         = 1;  // 0, changed from 8 by Ben
    private const int MAXIMUM_TIME_SPAN_FORMAT_LENGTH         = 26; // -dddddddd.hh:mm:ss.fffffff
    private const int MAXIMUM_ESCAPED_TIME_SPAN_FORMAT_LENGTH = 6 * MAXIMUM_TIME_SPAN_FORMAT_LENGTH;

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.String) {
            throw new InvalidOperationException($"Cannot get the value of a token type '{reader.TokenType}' as a string.");
        }

        return readCore(ref reader);
    }

    private static TimeSpan readCore(ref Utf8JsonReader reader) {
        Debug.Assert(reader.TokenType is JsonTokenType.String or JsonTokenType.PropertyName);

        int valueLength = reader.HasValueSequence ? checked((int) reader.ValueSequence.Length) : reader.ValueSpan.Length;
        if (valueLength is < MINIMUM_TIME_SPAN_FORMAT_LENGTH or > MAXIMUM_ESCAPED_TIME_SPAN_FORMAT_LENGTH) {
            throw new FormatException($"The JSON value is not in a supported {nameof(TimeSpan)} format.");
        }

        scoped ReadOnlySpan<byte> source;
        if (!reader.HasValueSequence && !reader.ValueIsEscaped) {
            source = reader.ValueSpan;
        } else {
            Span<byte> stackSpan    = stackalloc byte[MAXIMUM_ESCAPED_TIME_SPAN_FORMAT_LENGTH];
            int        bytesWritten = reader.CopyString(stackSpan);
            source = stackSpan[..bytesWritten];
        }

        char firstChar = (char) source[0];
        if (firstChar is < '0' or > '9' && firstChar != '-') {
            throw new FormatException($"The JSON value is not in a supported {nameof(TimeSpan)} format.");
        }

        bool result = Utf8Parser.TryParse(source, out TimeSpan tmpValue, out int bytesConsumed, 'c');

        if (!result || source.Length != bytesConsumed) {
            throw new FormatException($"The JSON value is not in a supported {nameof(TimeSpan)} format.");
        }

        return tmpValue;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) {
        Span<byte> output = stackalloc byte[MAXIMUM_TIME_SPAN_FORMAT_LENGTH];

        bool result = Utf8Formatter.TryFormat(value, output, out int bytesWritten, 'c');
        Debug.Assert(result);

        writer.WriteStringValue(output[..bytesWritten]);
    }

}