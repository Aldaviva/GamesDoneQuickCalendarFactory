namespace GamesDoneQuickCalendarFactory.Data;

public record Event(string longTitle, string shortTitle, IEnumerable<GameRun> runs);