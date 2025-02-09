using GamesDoneQuickCalendarFactory.Data;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NodaTime;
using Unfucked;

namespace GamesDoneQuickCalendarFactory.Services;

public interface ICalendarGenerator {

    Task<Calendar> generateCalendar();

}

public sealed class CalendarGenerator(IEventDownloader eventDownloader, ILogger<CalendarGenerator> logger): ICalendarGenerator {

    private const int SCHEMA_VERSION = 3;

    private static readonly Duration MIN_RUN_GAP = Duration.FromMinutes(1);

    public async Task<Calendar> generateCalendar() {
        logger.LogTrace("Downloading schedule from Games Done Quick website");
        Event?   gdqEvent = await eventDownloader.downloadSchedule();
        Calendar calendar = new() { Method = CalendarMethods.Publish };

        if (gdqEvent != null) {
            calendar.Events.AddRange(gdqEvent.runs.Select((run, runIndex) => new CalendarEvent {
                Uid = $"{SCHEMA_VERSION}/{gdqEvent.shortTitle}/{run.name}/{run.description}",
                // UTC works better than trying to coerce the OffsetDateTime into a ZonedDateTime, because NodaTime will pick a zone like UTC-5 instead of America/New_York (which makes sense), but Vivaldi doesn't apply zones like UTC-5 correctly and render the times as if they were local time, leading to events starting 3 hours too early for subscribers in America/Los_Angeles. Alternatively, we could map offsets and dates to more well-known zones like America/New_York, or use the zone specified in the GdqEvent.timezone property except I don't know if Vivaldi handles US/Eastern
                Start = run.start.ToIcalDateTimeUtc(),
                // ensure at least 1 minute gap between runs, to make calendars look nicer
                Duration = ((Duration?[]) [run.duration, gdqEvent.runs.ElementAtOrDefault(runIndex + 1) is { } nextRun ? nextRun.start - run.start - MIN_RUN_GAP : null]).Compact().Min().ToTimeSpan(),
                IsAllDay = false, // needed because iCal.NET assumes all events that start at midnight are always all-day events, even if they have a duration that isn't 24 hours
                Summary  = run.name,
                // having an Organizer makes Outlook show "this event has not been accepted"
                Description =
                    $"{run.description}\nRun by {run.runners.Select(getName).JoinHumanized()}{(run.commentators.Any() ? $"\nCommentary by {run.commentators.Select(getName).JoinHumanized()}" : string.Empty)}{(run.hosts.Any() ? $"\nHosted by {run.hosts.Select(getName).JoinHumanized()}" : string.Empty)}",
                // Location = TWITCH_STREAM_URL.ToString(),
                Alarms = {
                    runIndex == 0 ? new Alarm {
                        Action = AlarmAction.Display,
                        // RFC-5545 valarm documentation is wrong, trigger specifies a duration AFTER the start (by default) of the event to display the alarm, not BEFORE, so the timespan must be negative to trigger before (https://icalendar.org/iCalendar-RFC-5545/3-6-6-alarm-component.html)
                        Trigger     = new Trigger(TimeSpan.FromDays(-7)),
                        Description = $"{gdqEvent.longTitle} is coming up next week"
                    } : null,
                    runIndex == 0 ? new Alarm {
                        Action      = AlarmAction.Display,
                        Trigger     = new Trigger(TimeSpan.FromDays(-1)),
                        Description = $"{gdqEvent.longTitle} is starting tomorrow"
                    } : null,
                    runIndex == 0 ? new Alarm {
                        Action      = AlarmAction.Display,
                        Trigger     = new Trigger(TimeSpan.FromMinutes(-15)),
                        Description = $"{gdqEvent.longTitle} will be starting soon"
                    } : null
                }
            }));
        }

        return calendar;
    }

    private static string getName(Person person) => person.name;

}