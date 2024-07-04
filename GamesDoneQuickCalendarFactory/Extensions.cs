using Google.Apis.Calendar.v3.Data;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Proxies;
using Ical.Net.Serialization;
using NodaTime;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;
using Calendar = Ical.Net.Calendar;

namespace GamesDoneQuickCalendarFactory;

public static class Extensions {

    private static readonly Type       ENCODINGSTACK_TYPE = typeof(SerializerBase).Assembly.GetType("Ical.Net.Serialization.EncodingStack")!;
    private static readonly MethodInfo ENCODINGSTACK_PUSH = ENCODINGSTACK_TYPE.GetMethod("Push", [typeof(Encoding)])!;
    private static readonly MethodInfo ENCODINGSTACK_POP  = ENCODINGSTACK_TYPE.GetMethod("Pop")!;

    [Pure]
    public static IDateTime toIcsDateTimeUtc(this OffsetDateTime input) => new CalDateTime(input.ToInstant().ToDateTimeUtc(), DateTimeZone.Utc.Id);

    [Pure]
    public static EventDateTime toGoogleEventDateTime(this IDateTime dateTime) => new() { DateTimeDateTimeOffset = dateTime.AsDateTimeOffset, TimeZone = dateTime.TimeZoneName };

    [Pure]
    public static Event toGoogleEvent(this CalendarEvent calendarEvent) => new() {
        ICalUID      = calendarEvent.Uid,
        Start        = calendarEvent.Start.toGoogleEventDateTime(),
        End          = calendarEvent.End.toGoogleEventDateTime(),
        Summary      = calendarEvent.Summary,
        Description  = calendarEvent.Description,
        Location     = calendarEvent.Location,
        Visibility   = "public",
        Transparency = "transparent" // show me as available
    };

    [Pure]
    public static string joinHumanized(this IEnumerable<object> enumerable, string comma = ",", string conjunction = "and", bool oxfordComma = true) {
        using IEnumerator<object> enumerator = enumerable.GetEnumerator();

        if (!enumerator.MoveNext()) {
            return string.Empty;
        }

        object first = enumerator.Current;
        if (!enumerator.MoveNext()) {
            return first.ToString() ?? string.Empty;
        }

        object second = enumerator.Current;
        if (!enumerator.MoveNext()) {
            return $"{first} {conjunction} {second}";
        }

        object        third         = enumerator.Current;
        const char    SPACE         = ' ';
        StringBuilder stringBuilder = new StringBuilder().Append(first).Append(comma).Append(SPACE);

        while (enumerator.MoveNext()) {
            first  = second;
            second = third;
            third  = enumerator.Current;
            stringBuilder.Append(first).Append(comma).Append(SPACE);
        }

        stringBuilder.Append(second);
        if (oxfordComma) {
            stringBuilder.Append(comma);
        }
        stringBuilder.Append(SPACE).Append(conjunction).Append(SPACE).Append(third);

        return stringBuilder.ToString();
    }

    /// <summary>
    /// <para>Asynchronous version of <see cref="SerializerBase.Serialize" />.</para>
    /// <para> </para>
    /// <para>Alternatively, you can manually configure your web server (Kestrel or IIS) to allow synchronous writes:</para>
    /// <code>webappBuilder.WebHost.ConfigureKestrel(options =&gt; options.AllowSynchronousIO = true);
    /// webappBuilder.Services.Configure&lt;IISServerOptions&gt;(options =&gt; options.AllowSynchronousIO = true);
    /// </code>
    /// </summary>
    public static async Task serializeAsync(this SerializerBase serializer, object dataToSerialize, Stream destinationStream, Encoding destinationEncoding) {
        await using StreamWriter streamWriter = new(destinationStream, destinationEncoding, 1024, true);

        serializer.SerializationContext.Push(dataToSerialize);
        object encodingStack = serializer.GetService(ENCODINGSTACK_TYPE);
        ENCODINGSTACK_PUSH.Invoke(encodingStack, [destinationEncoding]);

        await streamWriter.WriteAsync(serializer.SerializeToString(dataToSerialize));

        ENCODINGSTACK_POP.Invoke(encodingStack, []);
        serializer.SerializationContext.Pop();
    }

    /// <summary>
    /// Checks if two calendars have equal lists of events. Both calendars' event lists must already be sorted the same (such as ascending start time) for vastly improved CPU and memory usage compared to <see cref="Calendar.Equals(Ical.Net.Calendar)"/>.
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
    public static bool EqualsFast(this CalendarEvent a, CalendarEvent? b) =>
        b != null &&
        a.Uid == b.Uid &&
        a.Summary == b.Summary &&
        a.Location == b.Location &&
        a.Description == b.Description &&
        a.DtStart.Equals(b.DtStart) &&
        a.DtEnd.Equals(b.DtEnd);

    /// <summary>
    /// Is this time before another?
    /// </summary>
    /// <param name="time">a time</param>
    /// <param name="other">another time</param>
    /// <returns><c>true</c> if this <paramref name="time"/> happens before <paramref name="other"/>, or <c>false</c> if it happens on or after <paramref name="other"/>.</returns>
    [Pure]
    public static bool IsBefore(this OffsetDateTime time, OffsetDateTime other) {
        return time.ToInstant() < other.ToInstant();
    }

    /// <summary>
    /// Is this time after another?
    /// </summary>
    /// <param name="time">a time</param>
    /// <param name="other">another time</param>
    /// <returns><c>true</c> if this <paramref name="time"/> happens after <paramref name="other"/>, or <c>false</c> if it happens on or before <paramref name="other"/>.</returns>
    [Pure]
    public static bool IsAfter(this OffsetDateTime time, OffsetDateTime other) {
        return time.ToInstant() > other.ToInstant();
    }

}