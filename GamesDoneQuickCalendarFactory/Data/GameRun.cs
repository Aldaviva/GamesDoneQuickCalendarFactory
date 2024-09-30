using NodaTime;

namespace GamesDoneQuickCalendarFactory.Data;

public record GameRun(
    OffsetDateTime      start,
    Duration            duration,
    string              name,
    string              description,
    IEnumerable<Person> runners,
    IEnumerable<Person> commentators,
    IEnumerable<Person> hosts
);

public record Person(int id, string name);