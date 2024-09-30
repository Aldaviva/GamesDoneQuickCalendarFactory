using Ical.Net;

namespace GamesDoneQuickCalendarFactory.Data;

public record CalendarResponse(Calendar calendar, DateTimeOffset dateModified) {

    public string etag => $"\"{dateModified.ToUnixTimeMilliseconds()}\"";

}