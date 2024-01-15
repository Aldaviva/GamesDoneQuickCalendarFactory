using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Data.GDQ;

public record GdqEvent(
    string         type,
    int            id,
    string         @short,
    string         name,
    string         hashtag,
    DateTimeOffset datetime,
    string         timezone,
    [property: JsonPropertyName("use_one_step_screening")]
    bool useOneStepScreening
);