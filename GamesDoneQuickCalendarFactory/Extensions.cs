using System.Text;
using Ical.Net.DataTypes;
using NodaTime.Extensions;

namespace GamesDoneQuickCalendarFactory;

public static class Extensions {

    public static IDateTime toIDateTime(this DateTimeOffset dateTimeOffset) => new CalDateTime(dateTimeOffset.DateTime, dateTimeOffset.ToZonedDateTime().Zone.Id);

    public static string joinHumanized(this IEnumerable<string> enumerable, string comma = ",", string conjunction = "and", bool oxfordComma = true) {
        List<string> elements = enumerable.ToList();

        switch (elements.Count) {
            case 0:
                return string.Empty;
            case 1:
                return elements[0];
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

}