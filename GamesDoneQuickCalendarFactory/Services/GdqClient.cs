using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Data.Marshal;
using NodaTime;
using System.Runtime.CompilerServices;
using Unfucked;
using Unfucked.DateTime;
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

public class GdqClient(HttpClient httpClient, ILogger<GdqClient> logger): IGdqClient {

    private static readonly Uri        CURRENT_EVENT_REDIRECTOR = new("https://tracker.gamesdonequick.com/tracker/donate/");
    private static readonly UrlBuilder EVENTS_API_URL           = new("https://tracker.gamesdonequick.com/tracker/api/v2/events");
    private static readonly Duration   MAX_SETUP_TIME           = (Hours) 17;

    private readonly HttpClient httpClient = httpClient.Property(PropertyKey.JsonSerializerOptions, JsonSerializerGlobalOptions.JSON_SERIALIZER_OPTIONS);

    public async Task<int> getCurrentEventId() {
        using HttpResponseMessage eventIdResponse          = await httpClient.Target(CURRENT_EVENT_REDIRECTOR).Head();
        Uri?                      eventSpecificDonationUrl = eventIdResponse.RequestMessage?.RequestUri;
        logger.Trace("Current event ID comes from {url}", eventSpecificDonationUrl);
        return Convert.ToInt32(eventSpecificDonationUrl!.Segments[4].TrimEnd('/'));
    }

    public async Task<GdqEvent> getEvent(int eventId) => await httpClient.Target(EVENTS_API_URL).Path(eventId).Get<GdqEvent>();

    public async Task<GdqEvent> getCurrentEvent() => await getEvent(await getCurrentEventId());

    public Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent) => getEventRuns(gdqEvent.id);

    public async Task<IEnumerable<GameRun>> getEventRuns(int eventId) {
        IList<GameRun>? runs         = null;
        Uri             runsUrl      = EVENTS_API_URL.Path(eventId).Path("runs/");
        var             resultsCount = new ValueHolderStruct<long>();
        OffsetDateTime? tailStart    = null;
        bool            sorted       = true;

        await foreach (GdqRun run in downloadAllPages<GdqRun>(runsUrl, resultsCount)) {
            runs ??= new List<GameRun>((int) resultsCount.value!.Value);
            if (run is { startTime: {} startTime, endTime: {} endTime }) {
                GameRun gameRun = new(
                    id: run.id,
                    start: startTime,
                    // PAX East 2025 had the last appointment of each day as a fake 20-hour overnight run with an 18-hour setup time, instead of just ending at the correct time
                    duration: run.setupTime > MAX_SETUP_TIME ? run.actualRunTime : endTime - startTime,
                    name: run.gameName.EmptyToNull() ?? run.runName,
                    category: run.category.Replace(" - ", " \u2014 "),
                    console: run.console,
                    gameReleaseYear: run.gameReleaseYear,
                    runners: run.runners.Select(getPerson),
                    commentators: run.commentators.Select(getPerson),
                    hosts: run.hosts.Select(getPerson),
                    tags: run.tags.Select(s => s.ToLowerInvariant()).ToHashSet());

                if (gameRun.tags.Remove("new_highlighted")) {
                    gameRun.tags.AddAll("highlight", "new_addition");
                }

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

    private async IAsyncEnumerable<T> downloadAllPages<T>(Uri firstPageUrl, ValueHolderStruct<long>? resultsCount = null, [EnumeratorCancellation] CancellationToken ct = default) {
        ResponseEnvelope<T>? page;
        for (Uri? nextPageToDownload = firstPageUrl; nextPageToDownload != null; nextPageToDownload = page.next) {
            page = await httpClient.Target(nextPageToDownload).Get<ResponseEnvelope<T>>(ct);

            if (resultsCount is { value: null }) {
                resultsCount.value = page.count;
            }

            foreach (T result in page.results) {
                yield return result;
            }
        }
    }

}