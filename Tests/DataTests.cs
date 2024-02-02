using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Data.GDQ;
using GamesDoneQuickCalendarFactory.Services;
using NodaTime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tests;

public class DataTests {

    [Fact]
    public void gdqEventTest() {
        using Stream jsonStream = File.OpenRead("Data/event.json");
        GdqEvent?    actual     = JsonSerializer.Deserialize<GdqEvent>(jsonStream, GdqClient.JSON_SERIALIZER_OPTIONS);
        GdqEvent     expected   = new(46, "AGDQ2024", "Awesome Games Done Quick 2024", "", new DateTimeOffset(2024, 1, 14, 11, 30, 0, TimeSpan.FromHours(-5)), "US/Eastern", false);

        actual.Should().NotBeNull();
        actual!.id.Should().Be(expected.id);
        actual.shortName.Should().Be(expected.shortName);
        actual.longName.Should().Be(expected.longName);
        actual.hashtag.Should().Be(expected.hashtag);
        actual.datetime.Should().Be(expected.datetime);
        actual.timezone.Should().Be(expected.timezone);
        actual.useOneStepScreening.Should().Be(expected.useOneStepScreening);
    }

    [Fact]
    public void gdqRunsTest() {
        using Stream  jsonStream = File.OpenRead("Data/runs.json");
        JsonNode      jsonRoot   = JsonNode.Parse(jsonStream)!;
        IList<GdqRun> results    = jsonRoot["results"].Deserialize<IEnumerable<GdqRun>>(GdqClient.JSON_SERIALIZER_OPTIONS)!.ToList();

        results[0].runners[0].stream.Should().BeNull();

        GdqRun run = results[1];
        run.id.Should().Be(5971);
        run.name.Should().Be("TUNIC");
        run.gameName.Should().Be("TUNIC");
        run.category.Should().Be("Any% Unrestricted");
        run.console.Should().Be("PC");
        run.startTime.Should().Be(new OffsetDateTime(new LocalDateTime(2024, 1, 14, 12, 12, 0), Offset.FromHours(-5)));
        run.endTime.Should().Be(new OffsetDateTime(new LocalDateTime(2024, 1, 14, 12, 48, 0), Offset.FromHours(-5)));
        run.order.Should().Be(2);
        run.runTime.Should().Be(Period.FromMinutes(21) + Period.FromSeconds(42));
        run.setupTime.Should().Be(Period.FromMinutes(14) + Period.FromSeconds(18));
        run.anchorTime.Should().BeNull();

        run.recordings.Should().HaveCount(1);
        Video video = run.recordings[0];
        video.id.Should().Be(3);
        video.host.Should().Be(VideoHost.YOUTUBE);
        video.url.Should().Be("https://youtu.be/KvN_XNPFToA");

        run.runners.Should().HaveCount(1);
        Runner runner = run.runners[0];
        runner.id.Should().Be(2042);
        runner.name.Should().Be("Radicoon");
        runner.stream.Should().Be("https://www.twitch.tv/radicoon");
        runner.twitter.Should().Be("radicoon");
        runner.youtube.Should().BeEmpty();
        runner.streamingPlatform.Should().Be(StreamingPlatform.TWITCH);
        runner.pronouns.Should().BeEmpty();

        run.hosts.Should().HaveCount(1);
        GdqPerson host = run.hosts[0];
        host.Should().NotBeNull();
        host.id.Should().Be(190);
        host.name.Should().Be("AttyJoe");
        host.pronouns.Should().BeEmpty();

        run.commentators.Should().HaveCount(2);
        GdqPerson commentator = run.commentators[0];
        commentator.id.Should().Be(307);
        commentator.name.Should().Be("kevinregamey");
        commentator.pronouns.Should().BeEmpty();

        commentator = run.commentators[1];
        commentator.id.Should().Be(306);
        commentator.name.Should().Be("silentdestroyer");
        commentator.pronouns.Should().BeEmpty();
    }

    [Fact]
    public void valueHolderStruct() {
        var valueHolder = new ValueHolderStruct<int>();
        valueHolder.value.Should().BeNull();

        valueHolder.value = 8;
        valueHolder.value.Should().Be(8);
    }

    [Fact]
    public void valueHolderRef() {
        var valueHolder = new ValueHolderRef<string>();
        valueHolder.value.Should().BeNull();

        valueHolder.value = "hi";
        valueHolder.value.Should().Be("hi");
    }

}