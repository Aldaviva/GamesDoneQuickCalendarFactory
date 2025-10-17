using NodaTime;

namespace GamesDoneQuickCalendarFactory.Data;

public record GameRun(
    int id,
    OffsetDateTime start,
    Duration duration,
    string name,
    string category,
    string console,
    int? gameReleaseYear,
    IEnumerable<Person> runners,
    IEnumerable<Person> commentators,
    IEnumerable<Person> hosts,
    ISet<string> tags
);

public record Person(int id, string name);