using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using NodaTime;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IEventDownloader {

    Task<Event?> downloadSchedule();

}

public class EventDownloader(IGdqClient gdq, IClock clock): IEventDownloader {

    private static readonly Duration          MAX_RUN_DURATION    = Duration.FromHours(11);
    private static readonly IReadOnlySet<int> RUNNER_ID_BLACKLIST = new HashSet<int> { 367, 1434, 1884, 1885, 2071, 6154 };

    /// <summary>
    /// If there are no calendar events ending in the last 1 day, and no upcoming events, hide all those old past events.
    /// </summary>
    private static readonly Duration MAX_EVENT_END_CLEANUP_DELAY = Duration.FromDays(1);

    public async Task<Event?> downloadSchedule() {
        GdqEvent currentEvent = await gdq.getCurrentEvent();

        IReadOnlyList<GameRun> runs = (await gdq.getEventRuns(currentEvent))
            .Where(run => !run.runners.IntersectBy(RUNNER_ID_BLACKLIST, runner => runner.id).Any() && !isSleep(run))
            .ToList().AsReadOnly();

        Instant latestRunEndTimeToInclude = clock.GetCurrentInstant() - MAX_EVENT_END_CLEANUP_DELAY;
        if (runs.Any(run => (run.start + run.duration).ToInstant() > latestRunEndTimeToInclude)) {
            return new Event(currentEvent.longName, currentEvent.shortName, runs);
        } else {
            return null;
        }
    }

    private static bool isSleep(GameRun run) => run.name == "Sleep" || run.duration >= MAX_RUN_DURATION;

}