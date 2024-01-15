using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using jaytwo.FluentUri;
using System.Text.Json.Nodes;

namespace GamesDoneQuickCalendarFactory;

public interface ICalendarGenerator: IDisposable {

    Task<Calendar> generateCalendar();

}

public sealed class CalendarGenerator: ICalendarGenerator {

    private static readonly Uri      TWITCH_STREAM_URL   = new("https://www.twitch.tv/gamesdonequick");
    private static readonly Uri      SCHEDULE_URL        = new("https://gamesdonequick.com/schedule");
    private static readonly Uri      EVENTS_API_LOCATION = new("https://gamesdonequick.com/tracker/api/v2/events");
    private static readonly TimeSpan MAX_RUN_DURATION    = new(11, 0, 0);

    private readonly HttpClient                 httpClient;
    private readonly ILogger<CalendarGenerator> logger;

    public CalendarGenerator(HttpClient httpClient, ILogger<CalendarGenerator> logger) {
        this.httpClient = httpClient;
        this.logger     = logger;
    }

    public async Task<Calendar> generateCalendar() {
        logger.LogDebug("Downloading schedule from Games Done Quick website");
        (string eventTitle, IEnumerable<GameRun> runs) = await downloadSchedule();

        Calendar calendar = new() { Method = CalendarMethods.Publish };
        calendar.Events.AddRange(runs
            // #8: exclude break or filler events like sleep and intermissions
            .Where(run => run.name != "Sleep" && run.duration < MAX_RUN_DURATION)
            .Select((run, runIndex) => new CalendarEvent {
                Uid      = $"aldaviva.com/{eventTitle}/{run.name}",
                Start    = run.start.ToUniversalTime().toIDateTime(),
                Duration = run.duration,
                IsAllDay = false, // needed because iCal.NET assumes all events that start at midnight are always all-day events, even if they have a duration that isn't 24 hours
                Summary  = run.name,
                // having an Organizer makes Outlook show "this event has not been accepted"
                Description =
                    $"{run.description}\r\nRun by {run.runners.joinHumanized()}{(run.commentators.Any() ? $"\nCommentary by {run.commentators.joinHumanized()}" : string.Empty)}{(run.hosts.Any() ? $"\nHosted by {run.hosts.joinHumanized()}" : string.Empty)}",
                Location = TWITCH_STREAM_URL.ToString(),
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

    private async Task<(string eventTitle, IEnumerable<GameRun> runs)> downloadSchedule() {
        using HttpResponseMessage eventIdResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, SCHEDULE_URL));

        int eventId       = Convert.ToInt32(eventIdResponse.RequestMessage!.RequestUri!.GetPathSegment(1));
        Uri eventLocation = EVENTS_API_LOCATION.WithPath(eventId.ToString());

        JsonNode eventResponse = (await httpClient.GetFromJsonAsync<JsonNode>(eventLocation))!;
        string   eventTitle    = eventResponse["short"]!.GetValue<string>();

        JsonNode runResponse = (await httpClient.GetFromJsonAsync<JsonNode>(eventLocation.WithPath("runs")))!;

        IEnumerable<GameRun> runs = runResponse["results"]!.AsArray().Select(result => new GameRun(
            start: DateTimeOffset.Parse(result!["starttime"]!.GetValue<string>()),
            duration: DateTimeOffset.Parse(result["endtime"]!.GetValue<string>()) - DateTimeOffset.Parse(result["starttime"]!.GetValue<string>()),
            name: result["name"]!.GetValue<string>(),
            description: result["category"]!.GetValue<string>() + " — " + result["console"]!.GetValue<string>(),
            runners: result["runners"]!.AsArray().Select(person => person!["name"]!.GetValue<string>()),
            hosts: result["hosts"]!.AsArray().Select(person => person!["name"]!.GetValue<string>()),
            commentators: result["commentators"]!.AsArray().Select(person => person!["name"]!.GetValue<string>()),
            setupDuration: TimeSpan.Parse(result["setup_time"]!.GetValue<string>())
        ));

        return (eventTitle, runs);
    }

    public void Dispose() { }

}