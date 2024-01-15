using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global - these are instantiated by deserializers

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

public record GdqRuns(
    int                count,
    object?            next,
    object?            previous,
    IReadOnlyList<Run> results
);

/// <param name="displayName">always the same as <paramref name="name"/></param>
/// <param name="description">always <c>""</c></param>
/// <param name="runTime">before a run ends, this is the estimated duration, but after a run ends, this changes to the actual duration</param>
public record Run(
    string                                                           type,
    int                                                              id,
    string                                                           name,
    [property: JsonPropertyName("display_name")] string              displayName,
    string                                                           description,
    string                                                           category,
    string                                                           console,
    IReadOnlyList<Runner>                                            runners,
    IReadOnlyList<Person>                                            hosts,
    IReadOnlyList<Person>                                            commentators,
    [property: JsonPropertyName("starttime")] DateTimeOffset         startTime,
    [property: JsonPropertyName("endtime")]   DateTimeOffset         endTime,
    int                                                              order,
    [property: JsonPropertyName("run_time")]    string               runTime,
    [property: JsonPropertyName("setup_time")]  TimeSpan             setupTime,
    [property: JsonPropertyName("anchor_time")] DateTimeOffset?      anchorTime,
    [property: JsonPropertyName("video_links")] IReadOnlyList<Video> videos
);

public record Person(
    string type,
    int    id,
    string name,
    string pronouns
);

/// <param name="twitter">Handle/username on Twitter</param>
/// <param name="youtube">Handle on YouTube</param>
/// <param name="streamingPlatform">The service that <paramref name="stream"/> is hosted on</param>
public record Runner(
    string                                                     type,
    int                                                        id,
    string                                                     name,
    Uri                                                        stream,
    string                                                     twitter,
    string                                                     youtube,
    [property: JsonPropertyName("platform")] StreamingPlatform streamingPlatform,
    string                                                     pronouns
): Person(type, id, name, pronouns);

public enum StreamingPlatform {

    TWITCH

}

public record Video(
    int                                                 id,
    [property: JsonPropertyName("link_type")] VideoType type,
    Uri                                                 url
);

public enum VideoType {

    TWITCH,
    YOUTUBE

}