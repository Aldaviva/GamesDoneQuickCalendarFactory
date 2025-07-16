using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

/// <summary>
/// A marathon week, like AGDQ 2024.
/// </summary>
/// <param name="id">unique numeric identifier, e.g. <c>47</c></param>
/// <param name="shortName">e.g. <c>AGDQ2024</c></param>
/// <param name="longName">e.g. <c>Awesome Games Done Quick 2024</c></param>
public record GdqEvent(
    int id,
    [property: JsonPropertyName("short")] string shortName,
    [property: JsonPropertyName("name")] string longName
);