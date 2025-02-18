using Ical.Net;
using Microsoft.Net.Http.Headers;

namespace GamesDoneQuickCalendarFactory.Data;

public record CalendarResponse(Calendar calendar, DateTimeOffset dateModified) {

    public EntityTagHeaderValue etag { get; } = new($"\"{dateModified.ToUnixTimeMilliseconds()}\"");

}