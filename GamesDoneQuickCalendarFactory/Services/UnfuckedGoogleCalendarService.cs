using Google.Apis.Calendar.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util;
using System.Net;

namespace GamesDoneQuickCalendarFactory.Services;

/// <summary>
/// <para>Like <see cref="CalendarService"/>, but with application-specific logic for retrying rate-limited requests, unlike the default generic <see cref="BaseClientService"/> that only handles 503 because it's not specific to Google Calendar.</para>
/// <para>Documentation:</para>
/// <para><see href="https://developers.google.com/workspace/calendar/api/guides/quota"/></para>
/// <para><see href="https://cloud.google.com/storage/docs/retry-strategy"/></para>
/// </summary>
public class UnfuckedGoogleCalendarService: CalendarService {

    public UnfuckedGoogleCalendarService() { }
    public UnfuckedGoogleCalendarService(Initializer initializer): base(initializer) { }

    protected override BackOffHandler CreateBackOffHandler() => new(new BackOffHandler.Initializer(new ExponentialBackOff()) {
        HandleUnsuccessfulResponseFunc = response => BackOffHandler.Initializer.DefaultHandleUnsuccessfulResponseFunc(response)
            || response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout
    });

}