using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using NodaTime.Text;
using Unfucked;

namespace Tests;

public class CalendarGeneratorTest {

    private readonly CalendarGenerator calendarGenerator;
    private readonly IEventDownloader  eventDownloader = A.Fake<IEventDownloader>();

    public CalendarGeneratorTest() {
        calendarGenerator = new CalendarGenerator(eventDownloader, new NullLogger<CalendarGenerator>());
    }

    [Fact]
    public async Task generateCalendar() {
        Event @event = new("Awesome Games Done Quick 2024", "AGDQ2024", [
            new GameRun(
                OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:12:00-05:00").GetValueOrThrow(),
                Duration.FromMinutes(36),
                "TUNIC",
                "Any% Unrestricted — PC",
                [new Person(1, "Radicoon")],
                [new Person(2, "kevinregamey"), new Person(3, "silentdestroyer")],
                [new Person(4, "AttyJoe")],
                []),

            new GameRun(
                OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:48:00-05:00").GetValueOrThrow(),
                Duration.FromMinutes(33),
                "Super Monkey Ball",
                "Master — Wii",
                [new Person(1, "Helix")],
                [new Person(2, "limy"), new Person(3, "PeasSMB")],
                [new Person(4, "AttyJoe")],
                []),

            new GameRun(
                OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T13:21:00-05:00").GetValueOrThrow(),
                Duration.FromHours(1) + Duration.FromMinutes(13),
                "Donkey Kong Country",
                "101% — SNES",
                [new Person(1, "Tonkotsu")],
                [new Person(2, "Glan"), new Person(3, "V0oid")],
                [new Person(4, "AttyJoe")],
                []),

            new GameRun(
                OffsetDateTimePattern.GeneralIso.Parse("2024-01-20T21:04:00-05:00").GetValueOrThrow(),
                Duration.FromHours(2) + Duration.FromMinutes(56),
                "Final Fantasy V Pixel Remaster",
                "Any% Cutscene Remover — PC",
                [new Person(1, "Zic3")],
                [new Person(2, "FoxyJira"), new Person(3, "WoadyB")],
                [new Person(4, "Prolix")],
                [])
        ]);

        A.CallTo(() => eventDownloader.downloadSchedule()).Returns(@event);

        Calendar actual = await calendarGenerator.generateCalendar();

        actual.Events.Should().HaveCount(4);

        CalendarEvent actualEvent = actual.Events[0];
        actualEvent.Start.Should().Be(OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:12:00-05:00").GetValueOrThrow().ToIcalDateTimeUtc());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(35), "min run gap");
        actualEvent.Summary.Should().Be("TUNIC");
        actualEvent.Uid.Should().Be("3/AGDQ2024/TUNIC/Any% Unrestricted — PC");
        actualEvent.Description.Should().Be("Any% Unrestricted — PC\nRun by Radicoon\nCommentary by kevinregamey and silentdestroyer\nHosted by AttyJoe");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().BeNull("I removed the Twitch stream URL because it was repetitive and cluttering up my calendar UI, making it hard to read any events");
        actualEvent.Alarms[0].Action.Should().Be("DISPLAY");
        actualEvent.Alarms[0].Description.Should().Be("Awesome Games Done Quick 2024 is coming up next week");
        actualEvent.Alarms[0].Trigger.Duration.Should().Be(TimeSpan.FromDays(7));
        actualEvent.Alarms[0].Trigger.IsRelative.Should().BeTrue();
        actualEvent.Alarms[1].Action.Should().Be("DISPLAY");
        actualEvent.Alarms[1].Description.Should().Be("Awesome Games Done Quick 2024 is starting tomorrow");
        actualEvent.Alarms[1].Trigger.Duration.Should().Be(TimeSpan.FromDays(1));
        actualEvent.Alarms[1].Trigger.IsRelative.Should().BeTrue();
        actualEvent.Alarms[2].Action.Should().Be("DISPLAY");
        actualEvent.Alarms[2].Description.Should().Be("Awesome Games Done Quick 2024 will be starting soon");
        actualEvent.Alarms[2].Trigger.Duration.Should().Be(TimeSpan.FromMinutes(15));
        actualEvent.Alarms[2].Trigger.IsRelative.Should().BeTrue();

        actualEvent = actual.Events[1];
        actualEvent.Start.Should().Be(OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:48:00-05:00").GetValueOrThrow().ToIcalDateTimeUtc());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(32), "min run gap");
        actualEvent.Summary.Should().Be("Super Monkey Ball");
        actualEvent.Uid.Should().Be("3/AGDQ2024/Super Monkey Ball/Master — Wii");
        actualEvent.Description.Should().Be("Master — Wii\nRun by Helix\nCommentary by limy and PeasSMB\nHosted by AttyJoe");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().BeNull("I removed the Twitch stream URL because it was repetitive and cluttering up my calendar UI, making it hard to read any events");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[2];
        actualEvent.Start.Should().Be(OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T13:21:00-05:00").GetValueOrThrow().ToIcalDateTimeUtc());
        actualEvent.Duration.Should().Be(TimeSpan.Parse("1:13:00"));
        actualEvent.Summary.Should().Be("Donkey Kong Country");
        actualEvent.Uid.Should().Be("3/AGDQ2024/Donkey Kong Country/101% — SNES");
        actualEvent.Description.Should().Be("101% — SNES\nRun by Tonkotsu\nCommentary by Glan and V0oid\nHosted by AttyJoe");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().BeNull("I removed the Twitch stream URL because it was repetitive and cluttering up my calendar UI, making it hard to read any events");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[3];
        actualEvent.Start.Should().Be(OffsetDateTimePattern.GeneralIso.Parse("2024-01-20T21:04:00-05:00").GetValueOrThrow().ToIcalDateTimeUtc());
        actualEvent.Duration.Should().Be(TimeSpan.Parse("2:56:0"));
        actualEvent.Summary.Should().Be("Final Fantasy V Pixel Remaster");
        actualEvent.Uid.Should().Be("3/AGDQ2024/Final Fantasy V Pixel Remaster/Any% Cutscene Remover — PC");
        actualEvent.Description.Should().Be("Any% Cutscene Remover — PC\nRun by Zic3\nCommentary by FoxyJira and WoadyB\nHosted by Prolix");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().BeNull("I removed the Twitch stream URL because it was repetitive and cluttering up my calendar UI, making it hard to read any events");
        actualEvent.Alarms.Should().BeEmpty();
    }

}