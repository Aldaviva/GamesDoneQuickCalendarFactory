namespace GamesDoneQuickCalendarFactory.Data;

public sealed record Event(string longTitle, string shortTitle, IReadOnlyList<GameRun> runs);