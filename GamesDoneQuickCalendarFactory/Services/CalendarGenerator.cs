using GamesDoneQuickCalendarFactory.Data;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NodaTime;

namespace GamesDoneQuickCalendarFactory.Services;

public interface ICalendarGenerator {

    Task<Calendar> generateCalendar();

}

public sealed class CalendarGenerator(IEventDownloader eventDownloader, ILogger<CalendarGenerator> logger): ICalendarGenerator {

    private static readonly Uri      TWITCH_STREAM_URL = new("https://www.twitch.tv/gamesdonequick");
    private static readonly Duration MAX_RUN_DURATION  = Duration.FromHours(11);

    public async Task<Calendar> generateCalendar() {
        logger.LogDebug("Downloading schedule from Games Done Quick website");
        Event?   gdqEvent = await eventDownloader.downloadSchedule();
        Calendar calendar = new() { Method = CalendarMethods.Publish };

        if (gdqEvent != null) {
            calendar.Events.AddRange(gdqEvent.runs
                // #8: exclude break or filler events like sleep and intermissions
                .Where(isNotSleep)
                .Select((run, runIndex) => new CalendarEvent {
                    Uid = $"aldaviva.com/{gdqEvent.shortTitle}/{run.name}",
                    // UTC works better than trying to coerce the OffsetDateTime into a ZonedDateTime, because NodaTime will pick a zone like UTC-5 instead of America/New_York (which makes sense), but Vivaldi doesn't apply zones like UTC-5 correctly and render the times as if they were local time, leading to events starting 3 hours too early for subscribers in America/Los_Angeles. Alternatively, we could map offsets and dates to more well-known zones like America/New_York, or use the zone specified in the GdqEvent.timezone property except I don't know if Vivaldi handles US/Eastern
                    Start    = run.start.toIDateTimeUtc(),
                    Duration = run.duration.ToTimeSpan(),
                    IsAllDay = false, // needed because iCal.NET assumes all events that start at midnight are always all-day events, even if they have a duration that isn't 24 hours
                    Summary  = run.name,
                    // having an Organizer makes Outlook show "this event has not been accepted"
                    Description =
                        $"{run.description}\nRun by {run.runners.joinHumanized()}{(run.commentators.Any() ? $"\nCommentary by {run.commentators.joinHumanized()}" : string.Empty)}{(run.hosts.Any() ? $"\nHosted by {run.hosts.joinHumanized()}" : string.Empty)}",
                    Location = TWITCH_STREAM_URL.ToString(),
                    Alarms = {
                        runIndex == 0 ? new Alarm {
                            Action      = AlarmAction.Display,
                            Trigger     = new Trigger(TimeSpan.FromDays(7)),
                            Description = $"{gdqEvent.longTitle} is coming up next week"
                        } : null,
                        runIndex == 0 ? new Alarm {
                            Action      = AlarmAction.Display,
                            Trigger     = new Trigger(TimeSpan.FromDays(1)),
                            Description = $"{gdqEvent.longTitle} is starting tomorrow"
                        } : null,
                        runIndex == 0 ? new Alarm {
                            Action      = AlarmAction.Display,
                            Trigger     = new Trigger(TimeSpan.FromMinutes(15)),
                            Description = $"{gdqEvent.longTitle} will be starting soon"
                        } : null
                    }
                }));
        }

        return calendar;
    }

    private static bool isNotSleep(GameRun run) {
        return run.name != "Sleep" && run.duration < MAX_RUN_DURATION;
    }

}