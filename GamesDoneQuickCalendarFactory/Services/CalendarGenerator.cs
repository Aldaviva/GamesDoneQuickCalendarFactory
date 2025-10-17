using GamesDoneQuickCalendarFactory.Data;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Collections.Frozen;
using Unfucked;
using Duration = NodaTime.Duration;

namespace GamesDoneQuickCalendarFactory.Services;

public interface ICalendarGenerator {

    Task<Calendar> generateCalendar();

}

public sealed class CalendarGenerator(IEventDownloader eventDownloader, State state, ILogger<CalendarGenerator> logger): ICalendarGenerator {

    private static readonly Duration     MIN_RUN_GAP = Duration.FromMinutes(1);
    private static readonly ISet<string> HIDDEN_TAGS = new HashSet<string> { "online", "studio" }.ToFrozenSet();

    public async Task<Calendar> generateCalendar() {
        logger.LogTrace("Downloading schedule from Games Done Quick website");
        Event?   gdqEvent = await eventDownloader.downloadSchedule();
        Calendar calendar = new() { Method = CalendarMethods.Publish };

        if (gdqEvent != null) {
            calendar.Events.AddRange(gdqEvent.runs.Select((run, runIndex) => {
                CalendarEvent calendarEvent = new() {
                    Uid = $"{state.googleCalendarUidCounter}/{run.id}",
                    // UTC works better than trying to coerce the OffsetDateTime into a ZonedDateTime, because NodaTime will pick a zone like UTC-5 instead of America/New_York (which makes sense), but UTC-5 is a fake, non-Olsen zone ID made up by NodaTime, which Vivaldi doesn't apply correctly and render the times as if they were local time, leading to events starting 3 hours too early for subscribers in America/Los_Angeles. Alternatively, we could map offsets and dates to more well-known zones like America/New_York, or use the zone specified in the GdqEvent.timezone property except I don't know if Vivaldi handles US/Eastern
                    Start = run.start.ToIcalDateTimeUtc(),
                    // ensure at least 1 minute gap between runs, to make calendars look nicer
                    Duration = ((IEnumerable<Duration?>) [run.duration, gdqEvent.runs.ElementAtOrDefault(runIndex + 1) is { } nextRun ? nextRun.start - run.start - MIN_RUN_GAP : null])
                        .Compact().Min().ToIcalDuration(),
                    Summary = run.name,
                    // having an Organizer makes Outlook show "this event has not been accepted"
                    Description = ((IEnumerable<string?>) [run.category, run.console.EmptyToNull(), run.gameReleaseYear?.ToString()]).Compact().Join(" \u2014 ") +
                        $"\nRun by {run.runners.Select(getName).JoinHumanized()}" +
                        $"{(run.commentators.Any() ? $"\nCommentary by {run.commentators.Select(getName).JoinHumanized()}" : string.Empty)}" +
                        $"{(run.hosts.Any() ? $"\nHosted by {run.hosts.Select(getName).JoinHumanized()}" : string.Empty)}" +
                        $"{(run.tags.Except(HIDDEN_TAGS).ToList() is { Count: not 0 } tags ? $"\nTagged {tags.Select(formatTag).Order(StringComparer.CurrentCultureIgnoreCase).Join(", ")}" : string.Empty)}"
                };

                if (runIndex == 0) {
                    calendarEvent.Alarms.AddAll(
                        new Alarm {
                            Action = AlarmAction.Display,
                            // RFC-5545 valarm documentation is wrong, trigger actually specifies a duration AFTER the start (by default) of the event to display the alarm, not BEFORE, so the timespan must be negative to trigger before (https://icalendar.org/iCalendar-RFC-5545/3-6-6-alarm-component.html)
                            Trigger     = new Trigger(Duration.FromDays(-7).ToIcalDuration()),
                            Description = $"{gdqEvent.longTitle} is coming up next week"
                        }, new Alarm {
                            Action      = AlarmAction.Display,
                            Trigger     = new Trigger(Duration.FromDays(-1).ToIcalDuration()),
                            Description = $"{gdqEvent.longTitle} is starting tomorrow"
                        }, new Alarm {
                            Action      = AlarmAction.Display,
                            Trigger     = new Trigger(Duration.FromMinutes(-15).ToIcalDuration()),
                            Description = $"{gdqEvent.longTitle} will be starting soon"
                        });
                }

                return calendarEvent;
            }));
        }

        return calendar;
    }

    private static string getName(Person person) => person.name;

    private static string formatTag(string rawTag) => rawTag switch {
        "checkpoint_run" => "Checkpoint run",
        "coop"           => "co-op",
        "kaizo"          => "Kaizo",
        "tas"            => "tool-assisted",
        _                => rawTag.Replace('_', ' ')
    };

}