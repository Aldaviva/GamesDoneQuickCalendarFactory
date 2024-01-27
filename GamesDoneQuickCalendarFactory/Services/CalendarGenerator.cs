using GamesDoneQuickCalendarFactory.Data;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace GamesDoneQuickCalendarFactory.Services;

public interface ICalendarGenerator {

    Task<Calendar> generateCalendar();

}

public sealed class CalendarGenerator(IEventDownloader eventDownloader, ILogger<CalendarGenerator> logger): ICalendarGenerator {

    private static readonly Uri      TWITCH_STREAM_URL = new("https://www.twitch.tv/gamesdonequick");
    private static readonly TimeSpan MAX_RUN_DURATION  = new(11, 0, 0);

    public async Task<Calendar> generateCalendar() {
        logger.LogDebug("Downloading schedule from Games Done Quick website");
        Event gdqEvent = await eventDownloader.downloadSchedule();

        Calendar calendar = new() { Method = CalendarMethods.Publish };
        calendar.Events.AddRange(gdqEvent.runs
            // #8: exclude break or filler events like sleep and intermissions
            .Where(run => run.name != "Sleep" && run.duration < MAX_RUN_DURATION)
            .Select((run, runIndex) => new CalendarEvent {
                Uid      = $"aldaviva.com/{gdqEvent.longTitle}/{run.name}",
                Start    = run.start.ToUniversalTime().toIDateTime(),
                Duration = run.duration,
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

        return calendar;
    }

}