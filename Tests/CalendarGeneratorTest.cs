using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests;

public class CalendarGeneratorTest {

    private readonly CalendarGenerator calendarGenerator;
    private readonly IEventDownloader  eventDownloader = A.Fake<IEventDownloader>();

    public CalendarGeneratorTest() {
        calendarGenerator = new CalendarGenerator(eventDownloader, new NullLogger<CalendarGenerator>());
    }

    [Fact]
    public async Task generateCalendar() {
        Event @event = new("Awesome Games Done Quick 2024", "AGDQ2024", new[] {
            new GameRun(
                DateTimeOffset.Parse("2024-01-14T11:30:00-05:00"),
                TimeSpan.FromMinutes(42),
                "AGDQ 2024 Pre-Show",
                "Pre-Show — GDQ",
                new[] { "Interview Crew" },
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                TimeSpan.Zero),

            new GameRun(
                DateTimeOffset.Parse("2024-01-14T12:12:00-05:00"),
                TimeSpan.FromMinutes(36),
                "TUNIC",
                "Any% Unrestricted — PC",
                new[] { "Radicoon" },
                new[] { "kevinregamey", "silentdestroyer" },
                new[] { "AttyJoe" },
                TimeSpan.Parse("0:14:18")),

            new GameRun(
                DateTimeOffset.Parse("2024-01-14T12:48:00-05:00"),
                TimeSpan.FromMinutes(33),
                "Super Monkey Ball",
                "Master — Wii",
                new[] { "Helix" },
                new[] { "limy", "PeasSMB" },
                new[] { "AttyJoe" },
                TimeSpan.Parse("0:13:44")),

            new GameRun(
                DateTimeOffset.Parse("2024-01-14T13:21:00-05:00"),
                TimeSpan.Parse("1:13:00"),
                "Donkey Kong Country",
                "101% — SNES",
                new[] { "Tonkotsu" },
                new[] { "Glan", "V0oid" },
                new[] { "AttyJoe" },
                TimeSpan.Parse("0:19:33")),

            new GameRun(
                DateTimeOffset.Parse("2024-01-21T00:00:00-05:00"),
                TimeSpan.FromMinutes(20),
                "Finale!",
                "The End% — Live",
                new[] { "Tech Crew" },
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                TimeSpan.Zero)
        });

        A.CallTo(() => eventDownloader.downloadSchedule()).Returns(@event);

        Calendar actual = await calendarGenerator.generateCalendar();

        actual.Events.Should().HaveCount(5);

        CalendarEvent actualEvent = actual.Events[0];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2024-01-14T11:30:00-05:00").ToUniversalTime().toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(42));
        actualEvent.Summary.Should().Be("AGDQ 2024 Pre-Show");
        actualEvent.Uid.Should().Be("aldaviva.com/Awesome Games Done Quick 2024/AGDQ 2024 Pre-Show");
        actualEvent.Description.Should().Be("Pre-Show — GDQ\nRun by Interview Crew");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().HaveCount(3);
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
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2024-01-14T12:12:00-05:00").ToUniversalTime().toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(36));
        actualEvent.Summary.Should().Be("TUNIC");
        actualEvent.Uid.Should().Be("aldaviva.com/Awesome Games Done Quick 2024/TUNIC");
        actualEvent.Description.Should().Be("Any% Unrestricted — PC\nRun by Radicoon\nCommentary by kevinregamey and silentdestroyer\nHosted by AttyJoe");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[2];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2024-01-14T12:48:00-05:00").ToUniversalTime().toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(33));
        actualEvent.Summary.Should().Be("Super Monkey Ball");
        actualEvent.Uid.Should().Be("aldaviva.com/Awesome Games Done Quick 2024/Super Monkey Ball");
        actualEvent.Description.Should().Be("Master — Wii\nRun by Helix\nCommentary by limy and PeasSMB\nHosted by AttyJoe");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[3];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2024-01-14T13:21:00-05:00").ToUniversalTime().toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.Parse("1:13:00"));
        actualEvent.Summary.Should().Be("Donkey Kong Country");
        actualEvent.Uid.Should().Be("aldaviva.com/Awesome Games Done Quick 2024/Donkey Kong Country");
        actualEvent.Description.Should().Be("101% — SNES\nRun by Tonkotsu\nCommentary by Glan and V0oid\nHosted by AttyJoe");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[4];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2024-01-21T00:00:00-05:00").ToUniversalTime().toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.Parse("0:20:00"));
        actualEvent.Summary.Should().Be("Finale!");
        actualEvent.Uid.Should().Be("aldaviva.com/Awesome Games Done Quick 2024/Finale!");
        actualEvent.Description.Should().Be("The End% — Live\nRun by Tech Crew");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();
    }

    [Fact]
    public async Task ignoreSleepRuns() {
        Event @event = new("test", "t", new[] {
            // Sleep event
            new GameRun(
                DateTimeOffset.Now,
                new TimeSpan(12, 53, 0),
                "Sleep",
                "Pillow Fight Boss Rush — GDQ Studio",
                new[] { "Faith" },
                Enumerable.Empty<string>(),
                new[] { "Velocity" }, null),

            // Long event
            new GameRun(
                DateTimeOffset.Now,
                new TimeSpan(14, 48, 0),
                "Day 1 Intermission",
                "Intermission — Offline",
                new[] { "Twitchcon" },
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(), null),

            // Short sleep
            new GameRun(
                DateTimeOffset.Now,
                TimeSpan.FromSeconds(15),
                "Sleep",
                "get-some-rest-too%— GDQ Studio",
                new[] { "GDQ Studio" },
                Enumerable.Empty<string>(),
                new[] { "Studio Workers" }, null)
        });

        A.CallTo(() => eventDownloader.downloadSchedule()).Returns(@event);

        Calendar actual = await calendarGenerator.generateCalendar();

        actual.Events.Should().BeEmpty();
    }

}