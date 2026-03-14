using Ical.Net;
using Microsoft.Net.Http.Headers;
using NodaTime;

namespace GamesDoneQuickCalendarFactory.Data;

public sealed record CalendarResponse(Calendar calendar, Instant dateModified) {

    public EntityTagHeaderValue etag { get; } = new($"\"{dateModified.ToUnixTimeMilliseconds()}\"");

}