namespace Tests;

public class EventDownloaderTest {

    private readonly FakeHttpMessageHandler httpMessageHandler = A.Fake<FakeHttpMessageHandler>();

    private readonly EventDownloader eventDownloader;

    public EventDownloaderTest() {
        eventDownloader = new EventDownloader(new HttpClient(httpMessageHandler));
    }

    [Fact]
    public async Task downloadSchedule() {
        A.CallTo(() => httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://gamesdonequick.com/schedule")))
            .Returns(new HttpResponseMessage { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://gamesdonequick.com/schedule/46") });

        await using Stream eventStream = File.OpenRead("Data/event.json");
        A.CallTo(() => httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://gamesdonequick.com/tracker/api/v2/events/46")))
            .Returns(new HttpResponseMessage { Content = new StreamContent(eventStream) });

        await using Stream runsStream = File.OpenRead("Data/runs.json");
        A.CallTo(() => httpMessageHandler.SendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://gamesdonequick.com/tracker/api/v2/events/46/runs")))
            .Returns(new HttpResponseMessage { Content = new StreamContent(runsStream) });

        GdqEvent actual = await eventDownloader.downloadSchedule();

        actual.title.Should().Be("Awesome Games Done Quick 2024");

        actual.runs.Should().HaveCount(140);

        actual.runs.ElementAt(0).Should().BeEquivalentTo(new GameRun(
            DateTimeOffset.Parse("2024-01-14T11:30:00-05:00"),
            TimeSpan.FromMinutes(42),
            "AGDQ 2024 Pre-Show",
            "Pre-Show — GDQ",
            new[] { "Interview Crew" },
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            TimeSpan.Zero));

        actual.runs.ElementAt(1).Should().BeEquivalentTo(new GameRun(
            DateTimeOffset.Parse("2024-01-14T12:12:00-05:00"),
            TimeSpan.FromMinutes(36),
            "TUNIC",
            "Any% Unrestricted — PC",
            new[] { "Radicoon" },
            new[] { "kevinregamey", "silentdestroyer" },
            new[] { "AttyJoe" },
            TimeSpan.Parse("0:14:18")));

        actual.runs.ElementAt(2).Should().BeEquivalentTo(new GameRun(
            DateTimeOffset.Parse("2024-01-14T12:48:00-05:00"),
            TimeSpan.FromMinutes(33),
            "Super Monkey Ball",
            "Master — Wii",
            new[] { "Helix" },
            new[] { "limy", "PeasSMB" },
            new[] { "AttyJoe" },
            TimeSpan.Parse("0:13:44")));

        actual.runs.ElementAt(3).Should().BeEquivalentTo(new GameRun(
            DateTimeOffset.Parse("2024-01-14T13:21:00-05:00"),
            TimeSpan.Parse("1:13:00"),
            "Donkey Kong Country",
            "101% — SNES",
            new[] { "Tonkotsu" },
            new[] { "Glan", "V0oid" },
            new[] { "AttyJoe" },
            TimeSpan.Parse("0:19:33")));

        actual.runs.Last().Should().BeEquivalentTo(new GameRun(
            DateTimeOffset.Parse("2024-01-21T00:00:00-05:00"),
            TimeSpan.FromMinutes(20),
            "Finale!",
            "The End% — Live",
            new[] { "Tech Crew" },
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            TimeSpan.Zero));
    }

}