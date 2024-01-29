namespace GamesDoneQuickCalendarFactory.Data;

public record Event(string longTitle, string shortTitle, IReadOnlyList<GameRun> runs);