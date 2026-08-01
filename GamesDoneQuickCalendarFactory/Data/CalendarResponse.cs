using Microsoft.Net.Http.Headers;

namespace GamesDoneQuickCalendarFactory.Data;

public sealed record CalendarResponse(Calendar calendar, Instant dateModified) {

    public EntityTagHeaderValue etag { get; } = new($"\"{dateModified.ToUnixTimeMilliseconds()}\"");

}