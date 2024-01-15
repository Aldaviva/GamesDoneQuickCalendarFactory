namespace GamesDoneQuickCalendarFactory;

public record GameRun(
    DateTimeOffset      start,
    TimeSpan            duration,
    string              name,
    string              description,
    IEnumerable<string> runners,
    IEnumerable<string> commentators,
    IEnumerable<string> hosts,
    TimeSpan?           setupDuration);