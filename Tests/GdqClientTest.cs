using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Services;
using NodaTime;
using NodaTime.Text;
using System.Net.Http.Headers;
using System.Net.Mime;
using Unfucked.HTTP;

namespace Tests;

public class GdqClientTest {

    private readonly UnfuckedHttpHandler httpMessageHandler = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    private readonly GdqClient           gdq;

    public GdqClientTest() {
        gdq = new GdqClient(new HttpClient(httpMessageHandler));
    }

    [Fact]
    public async Task getCurrentEvent() {
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://gamesdonequick.com/schedule"), A<CancellationToken>._)).Returns(
            new HttpResponseMessage { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://gamesdonequick.com/schedule/46") });

        await using Stream eventStream = File.OpenRead("Data/event.json");
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://tracker.gamesdonequick.com/tracker/api/v2/events/46"), A<CancellationToken>._))
            .Returns(new HttpResponseMessage {
                Content = new StreamContent(eventStream) {
                    Headers = {
                        ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json)
                    }
                }
            });

        GdqEvent actual = await gdq.getCurrentEvent();

        actual.id.Should().Be(46);
        actual.shortName.Should().Be("AGDQ2024");
        actual.longName.Should().Be("Awesome Games Done Quick 2024");
    }

    [Fact]
    public async Task getEventRuns() {
        await using Stream runsStream = File.OpenRead("Data/runs.json");
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://tracker.gamesdonequick.com/tracker/api/v2/events/46/runs"),
            A<CancellationToken>._)).Returns(new HttpResponseMessage { Content = new StreamContent(runsStream) });

        GdqEvent gdqEvent = new(46, "AGDQ2024", "Awesome Games Done Quick 2024");

        IList<GameRun> actual = (await gdq.getEventRuns(gdqEvent)).ToList();

        actual.Should().HaveCount(140);

        actual.ElementAt(0).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T11:30:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(42),
            "AGDQ 2024 Pre-Show",
            "Pre-Show — GDQ",
            [new Person(1434, "Interview Crew")],
            [],
            [],
            []));

        actual.ElementAt(1).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:12:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(36),
            "TUNIC",
            "Any% Unrestricted — PC — 2022",
            [new Person(2042, "Radicoon")],
            [new Person(307, "kevinregamey"), new Person(306, "silentdestroyer")],
            [new Person(190, "AttyJoe")],
            []));

        actual.ElementAt(2).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T12:48:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(33),
            "Super Monkey Ball",
            "Master — Wii",
            [new Person(1023, "Helix")],
            [new Person(308, "limy"), new Person(469, "PeasSMB")],
            [new Person(190, "AttyJoe")],
            []));

        actual.ElementAt(3).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-14T13:21:00-05:00").GetValueOrThrow(),
            Duration.FromHours(1) + Duration.FromMinutes(13),
            "Donkey Kong Country",
            "101% — SNES",
            [new Person(1240, "Tonkotsu")],
            [new Person(310, "Glan"), new Person(309, "V0oid")],
            [new Person(190, "AttyJoe")],
            []));

        actual.ElementAt(138).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-20T21:04:00-05:00").GetValueOrThrow(),
            Duration.FromHours(2) + Duration.FromMinutes(56),
            "Final Fantasy V Pixel Remaster",
            "Any% Cutscene Remover — PC",
            [new Person(432, "Zic3")],
            [new Person(448, "FoxyJira"), new Person(449, "WoadyB")],
            [new Person(2, "Prolix")],
            []));

        actual.Last().Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2024-01-21T00:00:00-05:00").GetValueOrThrow(),
            Duration.FromMinutes(20),
            "Finale!",
            "The End% — Live",
            [new Person(367, "Tech Crew")],
            [],
            [],
            []));
    }

    [Fact]
    public async Task getEventRunsWithOvernightSetupTimes() {
        await using Stream runsStream = File.OpenRead("Data/runs-with-overnight-setup-times.json");
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://tracker.gamesdonequick.com/tracker/api/v2/events/57/runs"),
            A<CancellationToken>._)).Returns(new HttpResponseMessage { Content = new StreamContent(runsStream) });

        GdqEvent gdqEvent = new(57, "SpeedAtPAXEast25", "Speedrun Stage @ PAX East 25");

        IList<GameRun> actual = (await gdq.getEventRuns(gdqEvent)).ToList();

        actual.Should().HaveCount(24);

        actual.ElementAt(6).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2025-05-08T14:46:00-04:00").GetValueOrThrow(),
            Duration.FromHours(2),
            "Donkey Kong 64 Randomizer",
            "Beat K Rool — N64",
            [new Person(363, "altabiscuit")],
            [],
            [],
            []));

        actual.ElementAt(12).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2025-05-09T16:37:00-04:00").GetValueOrThrow(),
            Duration.FromMinutes(20),
            "Portal",
            "Glitchless — PC",
            [new Person(924, "Msushi")],
            [],
            [],
            []));

        actual.ElementAt(18).Should().BeEquivalentTo(new GameRun(
            OffsetDateTimePattern.GeneralIso.Parse("2025-05-10T16:14:00-04:00").GetValueOrThrow(),
            Duration.FromMinutes(15),
            "Super Mario World",
            "11 Exit Orb — SNES",
            [new Person(3958, "ThePresidentNoir")],
            [],
            [],
            []));
    }

}