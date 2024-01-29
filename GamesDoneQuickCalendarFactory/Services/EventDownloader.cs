using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using NodaTime;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IEventDownloader {

    Task<Event?> downloadSchedule();

}

public class EventDownloader(IGdqClient gdq, IClock clock): IEventDownloader {

    /// <summary>
    /// If there are no calendar events ending in the last 1 day, and no upcoming events, hide all those old past events.
    /// </summary>
    private static readonly Duration MAX_EVENT_END_CLEANUP_DELAY = Duration.FromDays(1);

    public async Task<Event?> downloadSchedule() {
        GdqEvent               currentEvent = await gdq.getCurrentEvent();
        IReadOnlyList<GameRun> runs         = await gdq.getEventRuns(currentEvent);

        Instant now = clock.GetCurrentInstant();
        if (runs.Any(run => (run.start + run.duration).ToInstant() > now - MAX_EVENT_END_CLEANUP_DELAY)) {
            return new Event(currentEvent.longName, currentEvent.shortName, runs);
        } else {
            return null;
        }

    }

}