using System.Text.Json.Serialization;

// ReSharper disable ClassNeverInstantiated.Global - these are instantiated by deserializers

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

/// <param name="twitter">Handle/username on Twitter</param>
/// <param name="youtube">Handle on YouTube</param>
/// <param name="videoPlatform">The service that <paramref name="videoLocation"/> is hosted on, defaults to <see cref="VideoPlatform.TWITCH"/> even if <paramref name="videoLocation"/> is <c>null</c>.</param>
public record GdqPerson(
    int id,
    string name,
    [property: JsonPropertyName("stream")] Uri? videoLocation,
    [property: JsonPropertyName("platform")] VideoPlatform videoPlatform,
    string twitter,
    string youtube,
    string pronouns
);

/*
 * Source: https://github.com/GamesDoneQuick/donation-tracker/blob/094007fac93c76b335217e50327c26c04df16751/tracker/models/event.py#L807-L817
 */
public enum VideoPlatform {

    TWITCH,

    /// <summary>
    /// <para>Only one person in GDQ history streams on YouTube Live:</para>
    /// <list type="number"><item><description>kurushiidrive (https://youtube.com/@kurushiidrive/live), who ran Afterimage at SGDQ2024 and BTB2025</description></item></list>
    /// <para> </para>
    /// <para>Two other people have their <see cref="GdqPerson.videoPlatform"/> set to <see cref="YOUTUBE"/>:</para>
    /// <list type="number"><item><description>Bar0ti (https://www.youtube.com/@maeveskora), who showed an inspiring Katana Zero TAS during Frost Fatales 2023, but who doesn't stream and who uploads videos to YouTube, which is supposed to only be reflected in <see cref="GdqPerson.youtube"/> and not <see cref="GdqPerson.videoPlatform"/>.</description></item>
    /// <item><description>Shockwve, who streams on Twitch (https://twitch.tv/shockwve) but whose database entry incorrectly has <see cref="GdqPerson.videoPlatform"/> set to <see cref="YOUTUBE"/> instead of <see cref="TWITCH"/></description></item></list>
    /// </summary>
    YOUTUBE,

    /// <summary>
    /// Facebook Live is not used as any runner's primary video streaming platform in GDQ history
    /// </summary>
    FACEBOOK,

    /// <summary>
    /// Mixer only existed for 4 years and was shut down by Microsoft in 2020, not used as any runner's primary video streaming platform in GDQ history
    /// </summary>
    MIXER

}