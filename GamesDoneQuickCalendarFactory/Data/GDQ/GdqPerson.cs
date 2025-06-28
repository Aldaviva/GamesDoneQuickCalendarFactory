using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global - these are instantiated by deserializers

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

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
    [property: JsonPropertyName("platform")] StreamingPlatform streamingPlatform,
    string twitter,
    string youtube,
    string pronouns
): GdqPerson(id, name, pronouns);

/*
 * Source: https://github.com/GamesDoneQuick/donation-tracker/blob/094007fac93c76b335217e50327c26c04df16751/tracker/models/event.py#L807-L817
 */
public enum StreamingPlatform {

    TWITCH,

    /// <summary>
    /// Only one person in GDQ history streams on YouTube Live:
    /// Bar0ti (https://www.youtube.com/@maeveskora), who showed an inspiring Katana Zero TAS during Frost Fatales 2023
    /// </summary>
    YOUTUBE,

    /// <summary>
    /// Facebook Live is not used as any runner's primary streaming platform in GDQ history
    /// </summary>
    FACEBOOK,

    /// <summary>
    /// Shut down by Microsoft in 2020
    /// </summary>
    MIXER

}