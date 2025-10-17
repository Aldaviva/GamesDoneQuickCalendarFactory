using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using NodaTime;
using System.Collections.Frozen;
using Unfucked.HTTP.Exceptions;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IEventDownloader {

    Task<Event?> downloadSchedule();

}

public class EventDownloader(IGdqClient gdq, IClock clock): IEventDownloader {

    private static readonly Duration MAX_RUN_DURATION = Duration.FromHours(11);

    /// <summary>
    /// If there are no calendar events ending in the last 1 day, and no upcoming events, hide all those old past events.
    /// </summary>
    private static readonly Duration MAX_EVENT_END_CLEANUP_DELAY = Duration.FromDays(1);

    private static readonly IReadOnlySet<int> RUNNER_BLACKLIST = new HashSet<int> {
        367,  // Tech Crew
        885,  // GDQ Staff
        1434, // Interview Crew
        1884, // Faith (the Frame Fatales saber-toothed tiger mascot)
        1885, // Everyone!
        2071, // Frame Fatales Interstitial Team
        2171, // Everyone
    }.ToFrozenSet();

    /// <summary>
    /// <para>If a run has any of these tags, it will not appear in the calendar.</para>
    /// <para>To compute the set of all tags in a given event, run this JSON Query on the /runs JSON object:</para>
    /// <para><c>.results | map(.tags) | flatten() | uniq() | sort()</c></para>
    /// </summary>
    private static readonly IReadOnlySet<string> TAG_BLACKLIST = new HashSet<string> {
        "kickoff", "flame_kickoff", "frost_kickoff",
        "preshow",
        "checkpoint",
        "chomp",
        "recap", "daily_recap",
        "sleep",

        // #34: Frame Fatales inconsistently uses "opener" and "finale" to tag the first and last runs of an event, not the first and last interstitials like GDQ and BTB events do, so fall back to runner ID blocking to avoid hiding real runs
        // "opener", "finale"
    }.Select(s => s.ToLowerInvariant()).ToFrozenSet();

    /// <summary>
    /// GDQ Express at TwitchCon 2025 doesn't mark their daily openers or event finale with useful tags or runners (they do use the finale tag, but we can't use that because Frame Fatales uses that for their last real run).
    /// </summary>
    private static readonly IReadOnlySet<string> CONSOLE_BLACKLIST = new HashSet<string> {
        "TwitchCon"
    }.Select(s => s.ToLowerInvariant()).ToFrozenSet();

    public async Task<Event?> downloadSchedule() {
        try {
            GdqEvent currentEvent = await gdq.getCurrentEvent();

            IReadOnlyList<GameRun> runs = (await gdq.getEventRuns(currentEvent))
                .Where(run =>
                    !run.runners.IntersectBy(RUNNER_BLACKLIST, runner => runner.id).Any() &&
                    !run.tags.Intersect(TAG_BLACKLIST).Any() &&
                    !CONSOLE_BLACKLIST.Contains(run.console.ToLowerInvariant()) &&
                    !"Sleep".Equals(run.name, StringComparison.CurrentCultureIgnoreCase) &&
                    run.duration < MAX_RUN_DURATION)
                .ToList().AsReadOnly();

            Instant latestRunEndTimeToInclude = clock.GetCurrentInstant() - MAX_EVENT_END_CLEANUP_DELAY;
            if (runs.Any(run => (run.start + run.duration).ToInstant() > latestRunEndTimeToInclude)) {
                return new Event(currentEvent.longName, currentEvent.shortName, runs);
            } else {
                // All runs ended too far in the past
                return null;
            }
        } catch (NotFoundException) {
            /*
             * No schedule has been yet published for the next event.
             * Either the official schedule URL redirects to a 404 (no upcoming event), or it has a valid event ID but no runs share that event ID (upcoming event with no runs).
             */
            return null;
        }
    }

}