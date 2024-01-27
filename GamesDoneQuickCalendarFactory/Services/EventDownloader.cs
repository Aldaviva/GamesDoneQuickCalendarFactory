using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Data.Marshal;
using jaytwo.FluentUri;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IEventDownloader {

    Task<Event> downloadSchedule();

}

public class EventDownloader(HttpClient httpClient): IEventDownloader {

    private static readonly Uri SCHEDULE_URL        = new("https://gamesdonequick.com/schedule");
    private static readonly Uri EVENTS_API_LOCATION = new("https://gamesdonequick.com/tracker/api/v2/events");

    private static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
        Converters = {
            ZeroTolerantTimeSpanConverter.INSTANCE,
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)
        }
    };

    public async Task<Event> downloadSchedule() {
        using HttpResponseMessage eventIdResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, SCHEDULE_URL));

        int eventId       = Convert.ToInt32(eventIdResponse.RequestMessage!.RequestUri!.GetPathSegment(1));
        Uri eventLocation = EVENTS_API_LOCATION.WithPath(eventId.ToString());

        GdqEvent eventResponse = (await httpClient.GetFromJsonAsync<GdqEvent>(eventLocation, JSON_SERIALIZER_OPTIONS))!;
        GdqRuns  runResponse   = (await httpClient.GetFromJsonAsync<GdqRuns>(eventLocation.WithPath("runs"), JSON_SERIALIZER_OPTIONS))!;

        IEnumerable<GameRun> runs = runResponse.results.Select(run => new GameRun(
            start: run.startTime,
            duration: run.endTime - run.startTime,
            name: run.name,
            description: $"{run.category} \u2014 {run.console}",
            runners: run.runners.Select(getName),
            commentators: run.commentators.Select(getName),
            hosts: run.hosts.Select(getName),
            setupDuration: run.setupTime));

        return new Event(eventResponse.name, eventResponse.@short, runs);

    }

    private static string getName(Person person) => person.name;

}