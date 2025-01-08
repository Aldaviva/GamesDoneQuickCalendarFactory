﻿using GamesDoneQuickCalendarFactory.Data;
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
            [new Person(1, "Radicoon")],
            [new Person(2, "kevinregamey"), new Person(3, "silentdestroyer")],
            [new Person(4, "AttyJoe")],
            []);
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
            [new Person(1, "Radicoon")],
            [new Person(2, "kevinregamey"), new Person(3, "silentdestroyer")],
            [new Person(4, "AttyJoe")],
            []);
        A.CallTo(() => gdq.getEventRuns(gdqEvent)).Returns([tunic]);

        Event? actual = await eventDownloader.downloadSchedule();

        actual.Should().BeNull();
    }

    [Fact]
    public async Task ignoreRuns() {
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
                [new Person(1, "Runner")],
                [],
                [],
                []),

            // Sleep event
            new(now,
                Duration.FromHours(12) + Duration.FromMinutes(53),
                "Sleep",
                "Pillow Fight Boss Rush — GDQ Studio",
                [new Person(1, "Faith")], // Faith is actually 1884, but that case is tested separately below
                [],
                [new Person(2, "Velocity")],
                []),

            // Long event
            new(now,
                Duration.FromHours(14) + Duration.FromMinutes(48),
                "Day 1 Intermission",
                "Intermission — Offline",
                [new Person(1, "Twitchcon")],
                [],
                [],
                []),

            // Short sleep
            new(now,
                Duration.FromSeconds(15),
                "Sleep",
                "get-some-rest-too% — GDQ Studio",
                [new Person(1, "GDQ Studio")],
                [],
                [new Person(2, "Studio Workers")],
                []),

            // Tech Crew
            new(now, Duration.FromMinutes(70),
                "The Checkpoint",
                "Day 1 - Sunday — Live",
                [new Person(367, "Tech Crew")],
                [],
                [new Person(205, "TheKingsPride")],
                []),

            // Interview Crew
            new(now, Duration.FromMinutes(42),
                "AGDQ 2024 Pre-Show",
                "Pre-Show — GDQ",
                [new Person(1434, "Interview Crew")],
                [],
                [],
                []),

            // Faith
            new(now, Duration.FromMinutes(1), // actually longer, but long events are tested separately above
                "Not Sleep",                  // Sleep name is tested separately above
                "Sound Machine TAS — GDQ Studio",
                [new Person(1884, "Faith")],
                [],
                [],
                []),

            // Everyone
            new(now, Duration.FromMinutes(15),
                "Finale!",
                "Finale% — GDQ",
                [new Person(1885, "Everyone!")],
                [],
                [],
                []),

            // Frame Fatales Interstitial Team
            new(now, Duration.FromMinutes(30),
                "Preshow",
                "Preshow — GDQ",
                [new Person(2071, "Frame Fatales Interstitial Team")],
                [],
                [],
                []),

            // recap tag
            new(now, Duration.FromMinutes(30),
                "The Red Bull Daily Recap",
                "The Red Bull Daily Recap",
                [
                    new Person(60, "spikevegeta"),
                    new Person(884, "Kungfufruitcup"),
                    new Person(2758, "Melo Acevedo"),
                ],
                [new Person(2750, "THEKyleThomas")],
                [],
                ["recap"])
        };

        A.CallTo(() => gdq.getEventRuns(gdqEvent)).Returns(mockRuns);

        Event? actual = await eventDownloader.downloadSchedule();

        actual.Should().NotBeNull();
        actual!.runs.Should().HaveCount(1);
        actual.runs[0].name.Should().Be("Real run");
    }

}