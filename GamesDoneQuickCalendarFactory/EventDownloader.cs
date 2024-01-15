using jaytwo.FluentUri;
using System.Text.Json.Nodes;

namespace GamesDoneQuickCalendarFactory;

public interface IEventDownloader {

    Task<GdqEvent> downloadSchedule();

}

public class EventDownloader(HttpClient httpClient): IEventDownloader {

    private static readonly Uri SCHEDULE_URL        = new("https://gamesdonequick.com/schedule");
    private static readonly Uri EVENTS_API_LOCATION = new("https://gamesdonequick.com/tracker/api/v2/events");

    public async Task<GdqEvent> downloadSchedule() {
        using HttpResponseMessage eventIdResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, SCHEDULE_URL));

        int eventId       = Convert.ToInt32(eventIdResponse.RequestMessage!.RequestUri!.GetPathSegment(1));
        Uri eventLocation = EVENTS_API_LOCATION.WithPath(eventId.ToString());

        JsonNode eventResponse = (await httpClient.GetFromJsonAsync<JsonNode>(eventLocation))!;
        string   eventTitle    = eventResponse["name"]!.GetValue<string>();

        JsonNode runResponse = (await httpClient.GetFromJsonAsync<JsonNode>(eventLocation.WithPath("runs")))!;

        IEnumerable<GameRun> runs = runResponse["results"]!.AsArray().Select(result => new GameRun(
            start: DateTimeOffset.Parse(result!["starttime"]!.GetValue<string>()),
            duration: DateTimeOffset.Parse(result["endtime"]!.GetValue<string>()) - DateTimeOffset.Parse(result["starttime"]!.GetValue<string>()),
            name: result["name"]!.GetValue<string>(),
            description: result["category"]!.GetValue<string>() + " — " + result["console"]!.GetValue<string>(),
            runners: result["runners"]!.AsArray().Select(person => person!["name"]!.GetValue<string>()),
            commentators: result["commentators"]!.AsArray().Select(person => person!["name"]!.GetValue<string>()),
            hosts: result["hosts"]!.AsArray().Select(person => person!["name"]!.GetValue<string>()), setupDuration: TimeSpan.Parse(result["setup_time"]!.GetValue<string>())));

        return new GdqEvent(eventTitle, runs);
    }

}