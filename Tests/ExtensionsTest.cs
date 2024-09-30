using Google.Apis.Calendar.v3.Data;
using Ical.Net.DataTypes;

namespace Tests;

public class ExtensionsTest {

    [Fact]
    public void toEventDateTime() {
        EventDateTime actual = new CalDateTime(2024, 7, 1, 11, 27, 0, "America/Los_Angeles").toGoogleEventDateTime();
        actual.DateTimeDateTimeOffset.Should().Be(new DateTimeOffset(2024, 7, 1, 11, 27, 0, TimeSpan.FromHours(-7)));
        actual.TimeZone.Should().Be("America/Los_Angeles");
    }

}