using NodaTime;
using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global - these are instantiated by deserializers

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

/// <param name="name">Usually the same as <paramref name="gameName"/>, but if this is a bonus game, <paramref name="name"/> will have a <c>BONUS GAME 1- </c> prefix.</param>
/// <param name="gameName">Usually the same as <paramref name="name"/>, but if this is a bonus game, <paramref name="gameName"/> won't have the <c>BONUS GAME 1- </c> prefix.</param>
/// <param name="category">The type or rule set of the run, such as 100% or Any%.</param>
/// <param name="console">The hardware the game is running on, such as PC or PS5.</param>
/// <param name="order">The sequence number of this run in its containing event, starting at <c>1</c> for the first run of the even and increasing by <c>1</c> for each run in the event</param>
/// <param name="runTime">Before a run ends, this is the estimated duration, but after a run ends, this changes to the actual duration. To get the original estimated duration even after the run ends, use <paramref name="endTime"/><c>-</c><paramref name="startTime"/>.</param>
public record GdqRun(
    int id,
    [property: JsonPropertyName("name")] string name,
    [property: JsonPropertyName("display_name")] string gameName,
    string category,
    string console,
    IReadOnlyList<Runner> runners,
    IReadOnlyList<GdqPerson> hosts,
    IReadOnlyList<GdqPerson> commentators,
    [property: JsonPropertyName("starttime")] OffsetDateTime? startTime,
    [property: JsonPropertyName("endtime")] OffsetDateTime? endTime,
    int? order,
    [property: JsonPropertyName("run_time")] Period runTime,
    [property: JsonPropertyName("setup_time")] Period setupTime,
    [property: JsonPropertyName("anchor_time")] OffsetDateTime? anchorTime,
    [property: JsonPropertyName("video_links")] IReadOnlyList<Video> recordings
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
    /// Only one person in GDQ history streams primarily on YouTube Live:
    /// Bar0ti (https://www.youtube.com/@maeveskora) who showed a very cool Katana Zero TAS during Frost Fatales 2023
    /// </summary>
    YOUTUBE

}

public record Video(
    int id,
    [property: JsonPropertyName("link_type")] VideoHost host,
    Uri url
);

public enum VideoHost {

    YOUTUBE

}