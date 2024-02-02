using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Services;
using NodaTime;

namespace Tests;

public class EventDownloaderTest {

    private readonly EventDownloader eventDownloader;

    private readonly IGdqClient gdq   = A.Fake<IGdqClient>();
    private readonly IClock     clock = A.Fake<IClock>();

    public EventDownloaderTest() {
        eventDownloader = new EventDownloader(gdq, clock);
    }

    [Fact]
    public async Task downloadSchedule() {
        GdqEvent gdqEvent = new(46, "AGDQ2024", "Awesome Games Done Quick 2024", "", new DateTimeOffset(2024, 1, 14, 11, 30, 0, TimeSpan.FromHours(-5)), "US/Eastern", false);
        A.CallTo(() => gdq.getCurrentEvent()).Returns(gdqEvent);

        A.CallTo(() => clock.GetCurrentInstant()).Returns(new LocalDateTime(2024, 1, 14, 12, 30).WithOffset(Offset.FromHours(-5)).ToInstant());

        GameRun tunic = new(new LocalDateTime(2024, 1, 14, 12, 12, 0).WithOffset(Offset.FromHours(-5)),
            Duration.FromMinutes(36),
            "TUNIC", "Any% Unrestricted — PC",
            new[] { new Person(1, "Radicoon") },
            new[] { new Person(2, "kevinregamey"), new Person(3, "silentdestroyer") },
            new[] { new Person(4, "AttyJoe") });
        A.CallTo(() => gdq.getEventRuns(gdqEvent)).Returns([tunic]);

        Event? actual = await eventDownloader.downloadSchedule();

        actual.Should().NotBeNull();
        actual!.shortTitle.Should().Be("AGDQ2024");
        actual.longTitle.Should().Be("Awesome Games Done Quick 2024");
        actual.runs.Should().Equal(tunic);
    }

    [Fact]
    public async Task downloadScheduleEmpty() {
        GdqEvent gdqEvent = new(46, "AGDQ2024", "Awesome Games Done Quick 2024", "", new DateTimeOffset(2024, 1, 14, 11, 30, 0, TimeSpan.FromHours(-5)), "US/Eastern", false);
        A.CallTo(() => gdq.getCurrentEvent()).Returns(gdqEvent);

        A.CallTo(() => clock.GetCurrentInstant()).Returns(new LocalDateTime(2024, 1, 28, 12, 30).WithOffset(Offset.FromHours(-5)).ToInstant());

        GameRun tunic = new(new LocalDateTime(2024, 1, 14, 12, 12, 0).WithOffset(Offset.FromHours(-5)),
            Duration.FromMinutes(36),
            "TUNIC", "Any% Unrestricted — PC",
            new[] { new Person(1, "Radicoon") },
            new[] { new Person(2, "kevinregamey"), new Person(3, "silentdestroyer") },
            new[] { new Person(4, "AttyJoe") });
        A.CallTo(() => gdq.getEventRuns(gdqEvent)).Returns([tunic]);

        Event? actual = await eventDownloader.downloadSchedule();

        actual.Should().BeNull();
    }

    [Fact]
    public async Task ignoreSleepRuns() {
        GdqEvent gdqEvent = new(46, "AGDQ2024", "Awesome Games Done Quick 2024", "", new DateTimeOffset(2024, 1, 14, 11, 30, 0, TimeSpan.FromHours(-5)), "US/Eastern", false);
        A.CallTo(() => gdq.getCurrentEvent()).Returns(gdqEvent);

        OffsetDateTime now = SystemClock.Instance.GetCurrentInstant().WithOffset(Offset.Zero);
        A.CallTo(() => clock.GetCurrentInstant()).Returns(now.ToInstant());

        IReadOnlyList<GameRun> mockRuns = new List<GameRun> {
            // This is the only run that should be returned
            new(now,
                Duration.FromMinutes(10),
                "Real run",
                "To show the test works",
                new[] { new Person(1, "Runner") },
                Enumerable.Empty<Person>(),
                Enumerable.Empty<Person>()),

            // Sleep event
            new(now,
                Duration.FromHours(12) + Duration.FromMinutes(53),
                "Sleep",
                "Pillow Fight Boss Rush — GDQ Studio",
                new[] { new Person(1, "Faith") }, // Faith is actually 1884, but that case is tested separately below
                Enumerable.Empty<Person>(),
                new[] { new Person(2, "Velocity") }),

            // Long event
            new(now,
                Duration.FromHours(14) + Duration.FromMinutes(48),
                "Day 1 Intermission",
                "Intermission — Offline",
                new[] { new Person(1, "Twitchcon") },
                Enumerable.Empty<Person>(),
                Enumerable.Empty<Person>()),

            // Short sleep
            new(now,
                Duration.FromSeconds(15),
                "Sleep",
                "get-some-rest-too% — GDQ Studio",
                new[] { new Person(1, "GDQ Studio") },
                Enumerable.Empty<Person>(),
                new[] { new Person(2, "Studio Workers") }),

            // Tech Crew
            new(now, Duration.FromMinutes(70),
                "The Checkpoint",
                "Day 1 - Sunday — Live",
                new[] { new Person(367, "Tech Crew") },
                Enumerable.Empty<Person>(),
                new[] { new Person(205, "TheKingsPride") }),

            // Interview Crew
            new(now, Duration.FromMinutes(42),
                "AGDQ 2024 Pre-Show",
                "Pre-Show — GDQ",
                new[] { new Person(1434, "Interview Crew") },
                Enumerable.Empty<Person>(),
                Enumerable.Empty<Person>()),

            // Faith
            new(now, Duration.FromMinutes(1), // actually longer, but long events are tested separately above
                "Not Sleep",                  // Sleep name is tested separately above
                "Sound Machine TAS — GDQ Studio",
                new[] { new Person(1884, "Faith") },
                Enumerable.Empty<Person>(),
                Enumerable.Empty<Person>()),

            // Everyone
            new(now, Duration.FromMinutes(15),
                "Finale!",
                "Finale% — GDQ",
                new[] { new Person(1885, "Everyone!") },
                Enumerable.Empty<Person>(),
                Enumerable.Empty<Person>()),

            // Frame Fatales Interstitial Team
            new(now, Duration.FromMinutes(30),
                "Preshow",
                "Preshow — GDQ",
                new[] { new Person(2071, "Frame Fatales Interstitial Team") },
                Enumerable.Empty<Person>(),
                Enumerable.Empty<Person>()),
        };

        A.CallTo(() => gdq.getEventRuns(gdqEvent)).Returns(mockRuns);

        Event? actual = await eventDownloader.downloadSchedule();

        actual.Should().NotBeNull();
        actual!.runs.Should().HaveCount(1);
        actual.runs[0].name.Should().Be("Real run");
    }

}