using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NodaTime;
using System.Reflection;
using System.Text;

namespace GamesDoneQuickCalendarFactory;

public static class Extensions {

    private static readonly Type       ENCODINGSTACK_TYPE = typeof(SerializerBase).Assembly.GetType("Ical.Net.Serialization.EncodingStack")!;
    private static readonly MethodInfo ENCODINGSTACK_PUSH = ENCODINGSTACK_TYPE.GetMethod("Push", [typeof(Encoding)])!;
    private static readonly MethodInfo ENCODINGSTACK_POP  = ENCODINGSTACK_TYPE.GetMethod("Pop")!;

    public static IDateTime toIDateTimeUtc(this OffsetDateTime input) => new CalDateTime(input.ToInstant().ToDateTimeUtc(), DateTimeZone.Utc.Id);

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

}