﻿using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NodaTime.Extensions;
using System.Text;

namespace GamesDoneQuickCalendarFactory;

public static class Extensions {

    public static IDateTime toIDateTime(this DateTimeOffset dateTimeOffset) => new CalDateTime(dateTimeOffset.DateTime, dateTimeOffset.ToZonedDateTime().Zone.Id);

    public static string joinHumanized(this IEnumerable<object> enumerable, string comma = ",", string conjunction = "and", bool oxfordComma = true) {
        List<object> elements = enumerable.ToList();

        switch (elements.Count) {
            case 0:
                return string.Empty;
            case 1:
                return elements[0].ToString() ?? string.Empty;
            case 2:
                return $"{elements[0]} {conjunction} {elements[1]}";
            default:
                StringBuilder stringBuilder = new();
                for (int index = 0; index < elements.Count; index++) {
                    bool isLast = index == elements.Count - 1;
                    if (index > 0 && (!isLast || oxfordComma)) {
                        stringBuilder.Append(comma).Append(' ');
                    }

                    if (isLast) {
                        stringBuilder.Append(conjunction).Append(' ');
                    }

                    stringBuilder.Append(elements[index]);
                }

                return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Without this, you would have to manually configure your web server (Kestrel and IIS) to allow synchronous writes:
    /// <code>
    /// webappBuilder.WebHost.ConfigureKestrel(options =&gt; options.AllowSynchronousIO = true);
    /// webappBuilder.Services.Configure&lt;IISServerOptions&gt;(options =&gt; options.AllowSynchronousIO = true);
    /// </code>
    /// </summary>
    public static async Task serializeAsync(this SerializerBase serializerBase, object obj, Stream stream, Encoding encoding) {
        await using StreamWriter streamWriter = new(stream, encoding, 1024, true);

        serializerBase.SerializationContext.Push(obj);
        await streamWriter.WriteAsync(serializerBase.SerializeToString(obj));
        serializerBase.SerializationContext.Pop();
    }

}