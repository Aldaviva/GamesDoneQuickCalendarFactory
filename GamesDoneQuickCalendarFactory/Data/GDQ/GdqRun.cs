using NodaTime;
using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global - these are instantiated by deserializers

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

/// <summary>A playthrough of a game</summary>
/// <param name="id">unique numeric identifier for this run</param>
/// <param name="runName">Usually the same as <paramref name="gameName"/>, but if this is a bonus game, <paramref name="runName"/> will have a <c>BONUS GAME 1- </c> prefix.</param>
/// <param name="gameName">Usually the same as <paramref name="runName"/>, but if this is a bonus game, <paramref name="gameName"/> won't have the <c>BONUS GAME 1- </c> prefix.</param>
/// <param name="category">The type or rule set of the run, such as 100% or Any%.</param>
/// <param name="console">The hardware the game is running on, such as PC or PS5, or the empty string.</param>
/// <param name="gameReleaseYear">The year the game came out, or null.</param>
/// <param name="actualRunTime">Before a run ends, this is the estimated duration, but after a run ends, this changes to the actual duration. To get the original estimated duration even after the run ends, use <paramref name="endTime"/><c>-</c><paramref name="startTime"/>.</param>
/// <param name="tags"><para>Zero or more of <c>awful</c>, <c>bingo</c>, <c>bonus</c>, <c>checkpoint</c>, <c>checkpoint_run</c>, <c>coop</c>, <c>finale</c>, <c>horror</c>, <c>kaizo</c>, <c>kickoff</c>, <c>new_addition</c>, <c>online</c>, <c>opener</c>, <c>preshow</c>, <c>race</c>, <c>randomizer</c>, <c>recap</c>, <c>relay</c>, <c>rhythm</c>, <c>showcase</c>, <c>sleep</c>, <c>tas</c>, or <c>tournament</c> (as of BTB2025, there can be more in the future). Can be empty, but never null.</para><para>JSON Query for an array of runs: <c>map(.tags) | flatten() | uniq() | sort()</c></para></param>
public record GdqRun(
    int id,
    [property: JsonPropertyName("name")] string runName,
    [property: JsonPropertyName("display_name")] string gameName,
    string category,
    string console,
    [property: JsonPropertyName("release_year")] int? gameReleaseYear,
    IReadOnlyList<Runner> runners,
    IReadOnlyList<GdqPerson> hosts,
    IReadOnlyList<GdqPerson> commentators,
    [property: JsonPropertyName("starttime")] OffsetDateTime? startTime,
    [property: JsonPropertyName("endtime")] OffsetDateTime? endTime,
    [property: JsonPropertyName("setup_time")] Duration setupTime,
    [property: JsonPropertyName("run_time")] Duration actualRunTime,
    IReadOnlyList<string> tags
);

public record GdqPerson(
    int id,
    string name,
    string pronouns
);

/// <param name="twitter">Handle/username on Twitter</param>
/// <param name="youtube">Handle on YouTube</param>
/// <param name="streamingPlatform">The service that <paramref name="stream"/> is hosted on, defaults to <see cref="StreamingPlatform.TWITCH"/> even if <paramref name="stream"/> is <c>null</c>.</param>
public record Runner(
    int id,
    string name,
    Uri? stream,
    string twitter,
    string youtube,
    [property: JsonPropertyName("platform")] StreamingPlatform streamingPlatform,
    string pronouns
): GdqPerson(id, name, pronouns);

public enum StreamingPlatform {

    TWITCH,

    /// <summary>
    /// Only one person in GDQ history streams on YouTube Live:
    /// Bar0ti (https://www.youtube.com/@maeveskora), who showed an inspiring Katana Zero TAS during Frost Fatales 2023
    /// </summary>
    YOUTUBE

}