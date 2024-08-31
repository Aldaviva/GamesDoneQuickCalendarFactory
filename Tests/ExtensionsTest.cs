using Google.Apis.Calendar.v3.Data;
using Ical.Net.DataTypes;
using NodaTime.Text;

// ReSharper disable SuggestVarOrType_Elsewhere

namespace Tests;

public class ExtensionsTest {

    [Fact]
    public void toIDateTime() {
        IDateTime actual = OffsetDateTimePattern.GeneralIso.Parse("2023-05-28T16:30:00Z").GetValueOrThrow().toIcsDateTimeUtc();
        actual.Year.Should().Be(2023);
        actual.Month.Should().Be(5);
        actual.Day.Should().Be(28);
        actual.Hour.Should().Be(16);
        actual.Minute.Should().Be(30);
        actual.Second.Should().Be(0);
        actual.Millisecond.Should().Be(0);
        actual.IsUtc.Should().BeTrue();
        actual.TzId.Should().Be("UTC");
    }

    [Fact]
    public void toEventDateTime() {
        EventDateTime actual = new CalDateTime(2024, 7, 1, 11, 27, 0, "America/Los_Angeles").toGoogleEventDateTime();
        actual.DateTimeDateTimeOffset.Should().Be(new DateTimeOffset(2024, 7, 1, 11, 27, 0, TimeSpan.FromHours(-7)));
        actual.TimeZone.Should().Be("America/Los_Angeles");
    }

    [Theory]
    [InlineData(new object[0], "")]
    [InlineData(new[] { "AlphaDolphin" }, "AlphaDolphin")]
    [InlineData(new[] { "Konception", "limy" }, "Konception and limy")]
    [InlineData(new[] { "ZephyrGlaze", "WDRM", "chezmix" }, "ZephyrGlaze, WDRM, and chezmix")]
    [InlineData(new[] { "SpacebarS", "shovelclaws", "Themimik", "Yoshipuff", "AlphaDolphin", "Allegro" }, "SpacebarS, shovelclaws, Themimik, Yoshipuff, AlphaDolphin, and Allegro")]
    [InlineData(new[] { "Aurateur", "PangaeaPanga", "TanukiDan", "Shoujo", "Caspur", "Aldwulf", "LilKirbs", "Thabeast721" },
        "Aurateur, PangaeaPanga, TanukiDan, Shoujo, Caspur, Aldwulf, LilKirbs, and Thabeast721")]
    public void joinHumanized(IEnumerable<object> input, string expected) {
        input.joinHumanized().Should().Be(expected);
    }

    [Fact]
    public void deltaWithPrimitives() {
        List<int> existing = [1, 2, 3];
        List<int> @new     = [1, 2, 4];

        var actual = existing.DeltaWith(@new);

        actual.created.Should().Equal(4);
        actual.updated.Should().BeEmpty("ints are immutable");
        actual.deleted.Should().Equal(3);
        actual.unmodified.Should().Equal(1, 2);
    }

    [Fact]
    public void deltaWithObjects() {
        List<Person> existing = [new Person("A", 1), new Person("B", 2), new Person("C", 3)];
        List<Human>  @new     = [new Human("A", 1), new Human("B", 4), new Human("D", 5)];

        var actual = existing.DeltaWith(@new, p => p.name, h => h.name, (person, human) => person.name.Equals(human.name, StringComparison.CurrentCulture) && person.age == human.age);

        actual.created.Should().Equal(new Human("D", 5));
        actual.updated.Should().Equal(new Human("B", 4));
        actual.deleted.Should().Equal(new Person("C", 3));
        actual.unmodified.Should().Equal(new Person("A", 1));
    }

    private record Person(string name, int age);
    private record Human(string  name, int age);

}