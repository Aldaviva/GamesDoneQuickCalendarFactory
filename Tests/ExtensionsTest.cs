using Google.Apis.Calendar.v3.Data;
using Ical.Net.DataTypes;

namespace Tests;

public class ExtensionsTest {

    [Fact]
    public void toGoogleEventDateTime() {
        EventDateTime  actual   = new CalDateTime(2024, 7, 1, 11, 27, 0, "America/New_York").toGoogleEventDateTime();
        DateTimeOffset expected = new(2024, 7, 1, 11, 27, 0, TimeSpan.FromHours(-4));
        actual.DateTimeDateTimeOffset.Should().Be(expected);
        actual.TimeZone.Should().Be("America/New_York");
    }

    [Fact]
    public void toGoogleEventDateTimeUtc() {
        EventDateTime  actual   = new CalDateTime(2025, 9, 7, 17, 0, 0, "UTC").toGoogleEventDateTime();
        DateTimeOffset expected = new(2025, 9, 7, 17, 0, 0, TimeSpan.Zero);
        actual.DateTimeDateTimeOffset.Should().Be(expected);
        actual.TimeZone.Should().Be("UTC");
    }

}