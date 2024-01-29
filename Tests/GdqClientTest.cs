using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Services;
using NodaTime;
using NodaTime.Text;

namespace Tests;

public class GdqClientTest {

    private readonly FakeHttpMessageHandler httpMessageHandler = A.Fake<FakeHttpMessageHandler>();

    private readonly GdqClient gdq;

    public GdqClientTest() {
        gdq = new GdqClient(new HttpClient(httpMessageHandler));
    }

    [Fact]
    public async Task getCurrentEvent() {
        A.CallTo(() => httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://gamesdonequick.com/schedule"))).Returns(
            new HttpResponseMessage { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://gamesdonequick.com/schedule/46") });

        await using Stream eventStream = File.OpenRead("Data/event.json");
        A.CallTo(() => httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://gamesdonequick.com/tracker/api/v2/events/46"))).Returns(
            new HttpResponseMessage { Content = new StreamContent(eventStream) });

        GdqEvent actual = await gdq.getCurrentEvent();

        actual.id.Should().Be(46);
        actual.shortName.Should().Be("AGDQ2024");
        actual.longName.Should().Be("Awesome Games Done Quick 2024");
        actual.timezone.Should().Be("US/Eastern");
    }

    [Fact]
    public async Task getEventRuns() {
        await using Stream runsStream = File.OpenRead("Data/runs.json");
        A.CallTo(() => httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://gamesdonequick.com/tracker/api/v2/events/46/runs"))).Returns(
            new HttpResponseMessage { Content = new StreamContent(runsStream) });

        GdqEvent gdqEvent = new(46, "AGDQ2024", "Awesome Games Done Quick 2024", "", DateTimeOffset.Parse("2024-01-14T11:30:00-05:00"), "US/Eastern", false);

        IReadOnlyList<GameRun> actual = await gdq.getEventRuns(gdqEvent);

        actual.Should().HaveCount(140);

        actual[0].Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T11:30:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(42),
            "AGDQ 2024 Pre-Show",
            "Pre-Show — GDQ",
            new[] { "Interview Crew" },
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>()));

        actual[1].Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:12:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(36),
            "TUNIC",
            "Any% Unrestricted — PC",
            new[] { "Radicoon" },
            new[] { "kevinregamey", "silentdestroyer" },
            new[] { "AttyJoe" }));

        actual[2].Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:48:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(33),
            "Super Monkey Ball",
            "Master — Wii",
            new[] { "Helix" },
            new[] { "limy", "PeasSMB" },
            new[] { "AttyJoe" }));

        actual[3].Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T13:21:00-05:00").GetValueOrThrow(),
            Duration.FromHours(1) + Duration.FromMinutes(13),
            "Donkey Kong Country",
            "101% — SNES",
            new[] { "Tonkotsu" },
            new[] { "Glan", "V0oid" },
            new[] { "AttyJoe" }));

        actual[139].Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-21T00:00:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(20),
            "Finale!",
            "The End% — Live",
            new[] { "Tech Crew" },
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>()));
    }

}