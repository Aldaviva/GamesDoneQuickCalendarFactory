using System.Runtime.ExceptionServices;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IGdqClient {

    /// <exception cref="CalendarFactoryException.CurrentEventFetchError" />
    Task<int?> getCurrentEventId();

    Task<GdqEvent> getEvent(int eventId);

    /// <exception cref="CalendarFactoryException.CurrentEventFetchError">timeout or other network IO error from GDQ servers</exception>
    Task<GdqEvent?> getCurrentEvent();

    Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent);

    Task<IEnumerable<GameRun>> getEventRuns(int eventId);

}

public sealed class GdqClient: IGdqClient {

    internal static readonly UrlBuilder EVENT_URL      = new("https://private.gamesdonequick.com/tracker/api/v2/events/");
    private static readonly  Duration   MAX_SETUP_TIME = (Hours) 17;

    private readonly HttpClient                   httpClient;
    private readonly IList<CurrentEventIdFetcher> currentEventIdFetchers;

    public GdqClient(HttpClient httpClient, IEnumerable<CurrentEventIdFetcher> currentEventIdFetchers) {
        this.httpClient             = httpClient;
        this.currentEventIdFetchers = currentEventIdFetchers.OrderBy(implementation => implementation.priority).ToList();
    }

    /// <inheritdoc />
    public async Task<int?> getCurrentEventId() {
        ExceptionDispatchInfo? latestException = null;
        foreach (CurrentEventIdFetcher currentEventIdFetcher in currentEventIdFetchers) {
            try {
                if (await currentEventIdFetcher.getCurrentEventId() is {} currentEventId) {
                    return currentEventId;
                }
            } catch (CalendarFactoryException.CurrentEventFetchError e) {
                latestException = ExceptionDispatchInfo.Capture(e);
            }
        }

        latestException?.Throw();
        return null;
    }

    public async Task<GdqEvent> getEvent(int eventId) => await httpClient.Target(EVENT_URL).Path("{eventId}/").ResolveTemplate("eventId", eventId).Get<GdqEvent>();

    /// <inheritdoc />
    public async Task<GdqEvent?> getCurrentEvent() => await getCurrentEventId() is {} currentEventId ? await getEvent(currentEventId) : null;

    public Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent) => getEventRuns(gdqEvent.id);

    public async Task<IEnumerable<GameRun>> getEventRuns(int eventId) {
        List<GameRun>?  runs         = null;
        Uri             runsUrl      = EVENT_URL.Path("{eventId}/runs/").ResolveTemplate("eventId", eventId);
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
                    name: (run.runName.EmptyToNull ?? run.runNameWithBadFormatting).Replace("\\n", " "),
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

#pragma warning disable Ex0105 // undocumentable due to limits of static analyzer on async generator methods

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

#pragma warning restore Ex0105

}