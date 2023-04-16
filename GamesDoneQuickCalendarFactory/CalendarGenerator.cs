using AngleSharp;
using AngleSharp.Dom;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace GamesDoneQuickCalendarFactory;

public interface ICalendarGenerator: IDisposable {

    Task<Calendar> generateCalendar();

}

public sealed class CalendarGenerator: ICalendarGenerator {

    private static readonly Url TWITCH_STREAM_URL = Url.Create("https://www.twitch.tv/gamesdonequick");
    private static readonly Url SCHEDULE_URL      = Url.Create("https://gamesdonequick.com/schedule");

    private readonly IBrowsingContext browser;

    private readonly ILogger<CalendarGenerator> logger;

    public CalendarGenerator(IBrowsingContext browser, ILogger<CalendarGenerator> logger) {
        this.browser = browser;
        this.logger  = logger;
    }

    public async Task<Calendar> generateCalendar() {
        logger.LogDebug("Downloading schedule from Games Done Quick website");
        using IDocument doc = await browser.OpenAsync(SCHEDULE_URL);

        string eventTitle = doc.QuerySelector(".text-gdq-red")?.TextContent.Replace(" Schedule", "") ?? "Games Done Quick";
        IEnumerable<GameRun> runs = doc.QuerySelectorAll("tbody tr:not(.second-row, .day-split)").Select(firstRow => {
            IElement secondRow = firstRow.NextElementSibling!;

            return new GameRun(
                start: DateTimeOffset.Parse(firstRow.QuerySelector(".start-time")!.TextContent),
                duration: secondRow.QuerySelector(".text-right")!.TextContent is var duration && !string.IsNullOrWhiteSpace(duration) ? TimeSpan.Parse(duration) : TimeSpan.Zero,
                name: firstRow.QuerySelector("td:nth-child(2)")!.TextContent.Trim(),
                description: secondRow.QuerySelector("td:nth-child(2)")!.TextContent.Trim(),
                runners: firstRow.QuerySelector("td:nth-child(3)")!.TextContent.Split(", "),
                host: secondRow.QuerySelector("td:nth-child(3)")!.TextContent.Trim() is var host && !string.IsNullOrWhiteSpace(host) ? host : null,
                setupDuration: firstRow.QuerySelector("td.visible-lg")!.TextContent is var setupDuration && !string.IsNullOrWhiteSpace(setupDuration) ? TimeSpan.Parse(setupDuration) : null
            );
        });

        Calendar  calendar  = new() { Method     = CalendarMethods.Publish };
        Organizer organizer = new() { CommonName = "Games Done Quick" };
        calendar.Events.AddRange(runs.Select((run, runIndex) => new CalendarEvent {
            Start       = run.start.toIDateTime(),
            Duration    = run.duration,
            IsAllDay    = false, // needed because iCal.NET assumes all events that start at midnight are always all-day events, even if they have a duration that isn't 24 hours
            Summary     = run.name,
            Organizer   = organizer,
            Description = $"{run.description}\n\nRun by {run.runners.joinHumanized()}{(run.host is not null ? $"\nHosted by {run.host}" : string.Empty)}",
            Location    = TWITCH_STREAM_URL.ToString(),
            Alarms = {
                runIndex == 0 ? new Alarm {
                    Action      = AlarmAction.Display,
                    Trigger     = new Trigger(TimeSpan.FromDays(7)),
                    Description = $"{eventTitle} is coming up next week"
                } : null,
                runIndex == 0 ? new Alarm {
                    Action      = AlarmAction.Display,
                    Trigger     = new Trigger(TimeSpan.FromDays(1)),
                    Description = $"{eventTitle} is starting tomorrow"
                } : null,
                runIndex == 0 ? new Alarm {
                    Action      = AlarmAction.Display,
                    Trigger     = new Trigger(TimeSpan.FromMinutes(15)),
                    Description = $"{eventTitle} will be starting soon"
                } : null
            }
        }));

        return calendar;
    }

    public void Dispose() {
        browser.Dispose();
    }

}