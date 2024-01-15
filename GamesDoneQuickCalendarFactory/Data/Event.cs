namespace GamesDoneQuickCalendarFactory.Data;

public record Event(string title, IEnumerable<GameRun> runs);