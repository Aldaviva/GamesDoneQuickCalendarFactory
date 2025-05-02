using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Data.Marshal;
using NodaTime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Unfucked;
using Unfucked.HTTP;
using Unfucked.HTTP.Config;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IGdqClient {

    Task<int> getCurrentEventId();

    Task<GdqEvent> getEvent(int eventId);

    Task<GdqEvent> getCurrentEvent();

    Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent);

    Task<IEnumerable<GameRun>> getEventRuns(int eventId);

}

public class GdqClient(HttpClient httpClient): IGdqClient {

    private static readonly Uri        SCHEDULE_URL   = new("https://gamesdonequick.com/schedule");
    private static readonly UrlBuilder EVENTS_API_URL = new("https://tracker.gamesdonequick.com/tracker/api/v2/events");
    private static readonly Duration   MAX_SETUP_TIME = Duration.FromHours(17);

    internal static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new(JsonSerializerDefaults.Web) {
        Converters = {
            EmptyToNullUriConverter.INSTANCE,
            OffsetDateTimeConverter.INSTANCE,
            DurationConverter.INSTANCE,
            new JsonStringEnumConverter()
        }
    };

    private readonly HttpClient httpClient = httpClient.Property(PropertyKey.JsonSerializerOptions, JSON_SERIALIZER_OPTIONS);

    public async Task<int> getCurrentEventId() {
        using HttpResponseMessage eventIdResponse = await httpClient.Target(SCHEDULE_URL).Head();
        return Convert.ToInt32(eventIdResponse.RequestMessage!.RequestUri!.Segments[2]);
    }

    public async Task<GdqEvent> getEvent(int eventId) => await httpClient.Target(EVENTS_API_URL).Path(eventId).Get<GdqEvent>();

    public async Task<GdqEvent> getCurrentEvent() => await getEvent(await getCurrentEventId());

    public Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent) => getEventRuns(gdqEvent.id);

    public async Task<IEnumerable<GameRun>> getEventRuns(int eventId) {
        IList<GameRun>? runs         = null;
        Uri             runsUrl      = EVENTS_API_URL.Path(eventId).Path("runs");
        var             resultsCount = new ValueHolderStruct<int>();
        OffsetDateTime? tailStart    = null;
        bool            sorted       = true;

        await foreach (GdqRun run in downloadAllPages<GdqRun>(runsUrl, resultsCount)) {
            runs ??= new List<GameRun>(resultsCount.value!.Value);
            if (run is { startTime: { } startTime, endTime: { } endTime }) {
                GameRun gameRun = new(
                    start: startTime,
                    // PAX East 2025 had each last run of the day as a 20-hour overnight run with an 18-hour setup time, instead of just ending at the correct time
                    duration: run.setupTime > MAX_SETUP_TIME ? run.actualRunTime : endTime - startTime,
                    name: run.gameName,
                    description: ((List<string?>) [run.category.Replace(" - ", " \u2014 "), run.console.EmptyToNull(), run.gameReleaseYear?.ToString()]).Compact().Join(" \u2014 "),
                    runners: run.runners.Select(getPerson),
                    commentators: run.commentators.Select(getPerson),
                    hosts: run.hosts.Select(getPerson),
                    tags: run.tags.Select(s => s.ToLowerInvariant()));
                runs.Add(gameRun);

                // The API returns runs sorted in ascending start time order, but guarantee it here so the faster equality check in CalendarPoller is correct
                if (sorted) {
                    sorted    = !tailStart?.IsAfter(gameRun.start) ?? sorted;
                    tailStart = gameRun.start;
                }
            }
        }

        return ((sorted ? runs?.AsEnumerable() : runs?.OrderBy(run => run.start)) ?? []).ToList().AsReadOnly();
    }

    private static Person getPerson(GdqPerson person) => new(person.id, person.name);

    private async IAsyncEnumerable<T> downloadAllPages<T>(Uri firstPageUrl, ValueHolderStruct<int>? resultsCount = null, [EnumeratorCancellation] CancellationToken ct = default) {
        JsonObject? page;
        for (Uri? nextPageToDownload = firstPageUrl; nextPageToDownload != null; nextPageToDownload = page["next"]?.GetValue<Uri?>()) {
            page = await httpClient.Target(nextPageToDownload).Get<JsonObject>(ct);

            if (resultsCount is { value: null }) {
                resultsCount.value = page["count"]!.GetValue<int>();
            }

            foreach (T result in page["results"]!.Deserialize<IEnumerable<T>>(JSON_SERIALIZER_OPTIONS)!) {
                yield return result;
            }
        }
    }

}