using System.Net;

namespace GamesDoneQuickCalendarFactory.Services;

/// <summary>Fetches the ID of the current or upcoming GDQ event.</summary>
public interface CurrentEventIdFetcher {

    /// <summary>Smaller numbers (closest to <see cref="int.MinValue"/>) are higher priority.</summary>
    int priority { get; }

    /// <summary>Fetch the ID of the current or upcoming GDQ event.</summary>
    /// <param name="cancellationToken"></param>
    /// <returns>The primary key ID number of the current event, or <c>null</c> if there is no event currently either in progress or upcoming.</returns>
    /// <exception cref="CalendarFactoryException.CurrentEventFetchError">An error occurred while fetching the event ID, besides there not being a current event.</exception>
    Task<int?> getCurrentEventId(CancellationToken cancellationToken = default);

}

public sealed class DonationCurrentEventIdFetcher(HttpClient httpClient): CurrentEventIdFetcher {

    public int priority => 1;

    public async Task<int?> getCurrentEventId(CancellationToken cancellationToken = default) {
        try {
            using HttpResponseMessage headResponse = await httpClient.Target("https://tracker.gamesdonequick.com/tracker/donate/").Head(cancellationToken);
            return headResponse.StatusCode switch {
                HttpStatusCode.OK when int.TryParse(headResponse.RequestMessage?.RequestUri!.Path.ElementAtOrDefault(3), out int eventId) => eventId,
                HttpStatusCode.NotFound => null, // No event currently running, and the upcoming event has no published schedule yet
                HttpStatusCode.Forbidden => throw new CalendarFactoryException.CurrentEventFetchError("Tracker donation redirection blocked by CAPTCHA"),
                var status => throw new CalendarFactoryException.CurrentEventFetchError($"Tracker donation {status} response did not contain a Location header with an event ID path segment")
            };
        } catch (ProcessingException e) {
            throw new CalendarFactoryException.CurrentEventFetchError("Error while fetching current event ID from tracker donation redirection", e);
        }
    }

}

public sealed class ScheduleWebPageCurrentEventIdFetcher(HttpClient httpClient): CurrentEventIdFetcher {

    public int priority => 2;

    public async Task<int?> getCurrentEventId(CancellationToken cancellationToken = default) {
        try {
            using HttpResponseMessage headResponse = await httpClient.Target("https://gamesdonequick.com/schedule/").Head(cancellationToken);
            return headResponse.StatusCode switch {
                HttpStatusCode.OK when int.TryParse(headResponse.RequestMessage?.RequestUri!.Path.ElementAtOrDefault(1), out int eventId) => eventId,
                HttpStatusCode.NotFound => null, // No event currently running, and the upcoming event has no published schedule yet
                var status => throw new CalendarFactoryException.CurrentEventFetchError($"Schedule page {status} response did not contain a Location header with an event ID path segment")
            };
        } catch (ProcessingException e) {
            throw new CalendarFactoryException.CurrentEventFetchError("Error while fetching current event ID from schedule web page redirection", e);
        }
    }

}

public sealed class EventsApiCurrentEventIdFetcher(HttpClient httpClient, IClock clock): CurrentEventIdFetcher {

    /// <summary>The longest non-outlier GDQ event duration was 6d 18h 46m for SGDQ 2023.</summary>
    private static readonly Duration MAX_EVENT_DURATION = (Days) 7;

    public int priority => 3;

    public async Task<int?> getCurrentEventId(CancellationToken cancellationToken = default) {
        Instant now = clock.GetCurrentInstant();
        try {
            var notEndedYet = (await httpClient
                    .Target(GdqClient.EVENT_URL)
                    .Get<ResponseEnvelope<GdqEvent>>(cancellationToken))
                .results
                .Where(gdqEvent => now < (gdqEvent.expectedStartTime + MAX_EVENT_DURATION).ToInstant());
            return notEndedYet
                .MinBy(static gdqEvent => gdqEvent.expectedStartTime.ToInstant())?
                .id;
        } catch (HttpException e) {
            throw new CalendarFactoryException.CurrentEventFetchError("Error while fetching current event ID from Events V2 API", e);
        }
    }

}