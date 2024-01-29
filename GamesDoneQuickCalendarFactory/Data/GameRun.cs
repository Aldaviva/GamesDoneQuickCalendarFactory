using NodaTime;

namespace GamesDoneQuickCalendarFactory.Data;

public record GameRun(
    OffsetDateTime      start,
    Duration            duration,
    string              name,
    string              description,
    IEnumerable<string> runners,
    IEnumerable<string> commentators,
    IEnumerable<string> hosts
);