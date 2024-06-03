﻿using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Data.Marshal;
using jaytwo.FluentUri;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IGdqClient {

    Task<int> getCurrentEventId();

    Task<GdqEvent> getEvent(int eventId);

    Task<GdqEvent> getCurrentEvent();

    Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent);

    Task<IEnumerable<GameRun>> getEventRuns(int eventId);

}

public class GdqClient(HttpClient httpClient): IGdqClient {

    private static readonly Uri SCHEDULE_URL   = new("https://gamesdonequick.com/schedule");
    private static readonly Uri EVENTS_API_URL = new("https://tracker.gamesdonequick.com/tracker/api/v2/events");

    internal static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new() {
        Converters = {
            EmptyToNullUriConverter.INSTANCE,
            OffsetDateTimeConverter.INSTANCE,
            PeriodConverter.INSTANCE,
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)
        }
    };

    public async Task<int> getCurrentEventId() {
        using HttpResponseMessage eventIdResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, SCHEDULE_URL));
        return Convert.ToInt32(eventIdResponse.RequestMessage!.RequestUri!.GetPathSegment(1));
    }

    public async Task<GdqEvent> getEvent(int eventId) {
        Uri eventUrl = EVENTS_API_URL.WithPath(eventId.ToString());
        return (await httpClient.GetFromJsonAsync<GdqEvent>(eventUrl, JSON_SERIALIZER_OPTIONS))!;
    }

    public async Task<GdqEvent> getCurrentEvent() => await getEvent(await getCurrentEventId());

    public Task<IEnumerable<GameRun>> getEventRuns(GdqEvent gdqEvent) => getEventRuns(gdqEvent.id);

    public async Task<IEnumerable<GameRun>> getEventRuns(int eventId) {
        IList<GameRun>? runs         = null;
        Uri             runsUrl      = EVENTS_API_URL.WithPath(eventId.ToString()).WithPath("runs");
        var             resultsCount = new ValueHolderStruct<int>();

        await foreach (GdqRun run in downloadAllPages<GdqRun>(runsUrl, resultsCount)) {
            runs ??= new List<GameRun>(resultsCount.value!.Value);
            runs.Add(new GameRun(
                start: run.startTime,
                duration: run.endTime - run.startTime,
                name: run.gameName,
                description: $"{run.category} \u2014 {run.console}",
                runners: run.runners.Select(getPerson),
                commentators: run.commentators.Select(getPerson),
                hosts: run.hosts.Select(getPerson)));
        }

        return runs?.AsReadOnly() ?? Enumerable.Empty<GameRun>();
    }

    private static Person getPerson(GdqPerson person) => new(person.id, person.name);

    private async IAsyncEnumerable<T> downloadAllPages<T>(Uri firstPageUrl, ValueHolderStruct<int>? resultsCount = default, [EnumeratorCancellation] CancellationToken c = default) {
        JsonObject? page;
        for (Uri? nextPageToDownload = firstPageUrl; nextPageToDownload != null; nextPageToDownload = page?["next"]?.GetValue<Uri?>()) {
            page = await httpClient.GetFromJsonAsync<JsonObject>(nextPageToDownload, JSON_SERIALIZER_OPTIONS, c);

            if (page != null) {
                if (resultsCount is { value: null }) {
                    resultsCount.value = page["count"]!.GetValue<int>();
                }

                foreach (T result in page["results"]!.Deserialize<IEnumerable<T>>(JSON_SERIALIZER_OPTIONS)!) {
                    yield return result;
                }
            }
        }
    }

}