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
            new[] { "Radicoon" },
            new[] { "kevinregamey", "silentdestroyer" },
            new[] { "AttyJoe" });
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
            new[] { "Radicoon" },
            new[] { "kevinregamey", "silentdestroyer" },
            new[] { "AttyJoe" });
        A.CallTo(() => gdq.getEventRuns(gdqEvent)).Returns([tunic]);

        Event? actual = await eventDownloader.downloadSchedule();

        actual.Should().BeNull();
    }

}