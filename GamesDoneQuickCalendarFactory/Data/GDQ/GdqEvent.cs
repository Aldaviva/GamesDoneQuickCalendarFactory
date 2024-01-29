using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

/// <summary>
/// A marathon week, like AGDQ 2024.
/// </summary>
/// <param name="id">unique numeric identifier, e.g. <c>47</c></param>
/// <param name="shortName">e.g. <c>AGDQ2024</c></param>
/// <param name="longName">e.g. <c>Awesome Games Done Quick 2024</c></param>
/// <param name="hashtag"></param>
/// <param name="datetime"></param>
/// <param name="timezone">e.g. <c>US/Eastern</c>, which are valid Olsen/IANA zones, however they're merely links to canonical zones like <c>America/New_York</c></param>
/// <param name="useOneStepScreening"></param>
public record GdqEvent(
    int                                                         id,
    [property: JsonPropertyName("short")] string                shortName,
    [property: JsonPropertyName("name")]  string                longName,
    string                                                      hashtag,
    DateTimeOffset                                              datetime,
    string                                                      timezone,
    [property: JsonPropertyName("use_one_step_screening")] bool useOneStepScreening
);