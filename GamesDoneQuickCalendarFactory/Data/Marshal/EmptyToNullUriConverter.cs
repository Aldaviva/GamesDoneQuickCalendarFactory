using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.Marshal;

/// <summary>
/// Like <see cref="UriTypeConverter"/>, but empty strings turn into null values instead of malformed "" URIs
/// </summary>
public class EmptyToNullUriConverter: JsonConverter<Uri> {

    public static readonly EmptyToNullUriConverter INSTANCE = new();

    public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { } jsonString) {
            throw new InvalidOperationException($"JSON token type must be {nameof(JsonTokenType.String)} to parse as {nameof(Uri)}, but was {reader.TokenType}.");
        } else if (string.IsNullOrWhiteSpace(jsonString)) {
            return null;
        }

        try {
            return new Uri(jsonString, UriKind.RelativeOrAbsolute);
        } catch (UriFormatException e) {
            throw new FormatException($"Could not parse {jsonString} as a URI", e);
        }
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.OriginalString);
    }

}