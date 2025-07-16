using Google.Apis.Calendar.v3.Data;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Proxies;
using System.Diagnostics.Contracts;
using Unfucked;
using Calendar = Ical.Net.Calendar;

namespace GamesDoneQuickCalendarFactory;

public static class Extensions {

    [Pure]
    public static EventDateTime toGoogleEventDateTime(this CalDateTime dateTime) => new() { DateTimeDateTimeOffset = dateTime.ToZonedDateTime().ToDateTimeOffset(), TimeZone = dateTime.TimeZoneName };

    [Pure]
    public static Event toGoogleEvent(this CalendarEvent calendarEvent) => new() {
        ICalUID      = calendarEvent.Uid,
        Start        = calendarEvent.Start!.toGoogleEventDateTime(),
        End          = calendarEvent.Start!.Add(calendarEvent.Duration!.Value).toGoogleEventDateTime(),
        Summary      = calendarEvent.Summary,
        Description  = calendarEvent.Description,
        Location     = calendarEvent.Location,
        Visibility   = "public",
        Transparency = "transparent" // show me as available
    };

    /// <summary>
    /// Checks if two calendars have equal lists of events. Both calendars' event lists must already be sorted the same (such as ascending start time) for vastly improved CPU and memory usage compared to <see cref="Calendar.Equals(Ical.Net.CalendarObject)"/>.
    /// </summary>
    /// <param name="a">a <see cref="Calendar"/></param>
    /// <param name="b">another <see cref="Calendar"/></param>
    /// <returns><c>true</c> if <paramref name="a"/> and <paramref name="b"/> have the same events in the same order, or <c>false</c> otherwise</returns>
    [Pure]
    public static bool EqualsPresorted(this Calendar a, Calendar? b) {
        if (b is null) {
            return false;
        }

        IUniqueComponentList<CalendarEvent> eventsA = a.Events;
        IUniqueComponentList<CalendarEvent> eventsB = b.Events;

        if (eventsA.Count == eventsB.Count) {
            return !eventsA.Where((eventA, i) => !eventA.EqualsFast(eventsB[i])).Any();
        } else {
            return false;
        }
    }

    [Pure]
    private static bool EqualsFast(this CalendarEvent a, CalendarEvent? b) =>
        b != null &&
        a.Uid == b.Uid &&
        a.Summary == b.Summary &&
        a.Location == b.Location &&
        a.Description == b.Description &&
        a.DtStart!.Value.Equals(b.DtStart!.Value) &&
        a.Duration!.Value.Equals(b.Duration!.Value, fucked: false);

}