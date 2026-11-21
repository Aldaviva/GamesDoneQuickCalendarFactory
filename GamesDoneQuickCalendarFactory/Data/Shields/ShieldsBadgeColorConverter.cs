using System.Drawing;

namespace GamesDoneQuickCalendarFactory.Data.Shields;

internal sealed class ShieldsBadgeColorConverter: JsonConverter<ShieldsBadgeColor> {

    public override void Write(Utf8JsonWriter writer, ShieldsBadgeColor value, JsonSerializerOptions options) {
        if (formatColor(value) is {} jsonValue) {
            writer.WriteStringValue(jsonValue);
        } else {
            writer.WriteNullValue();
        }
    }

    private static string? formatColor(ShieldsBadgeColor value) => value switch {
        ShieldsBadgeColorKeyword keyword           => Enum.GetName(keyword)!.ToLowerInvariant().Replace("_", string.Empty),
        Color color                                => color.asRgbHexString,
        KnownColor { isCssColor: true } knownColor => Enum.GetName(knownColor)!.ToLowerInvariant(),
        KnownColor knownColor                      => formatColor(Color.FromKnownColor(knownColor)),
        string cssColor                            => cssColor,
        _                                          => null
    };

    public override ShieldsBadgeColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new InvalidOperationException("No deserialization");

}