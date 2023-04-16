namespace GamesDoneQuickCalendarFactory;

public record GameRun(DateTimeOffset start, TimeSpan duration, string name, string description, IEnumerable<string> runners, string? host, TimeSpan? setupDuration);