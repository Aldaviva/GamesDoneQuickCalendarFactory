using AngleSharp;
using AngleSharp.Browser;
using AngleSharp.Dom;
using AngleSharp.Io;
using GamesDoneQuickCalendarFactory;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests;

public class CalendarGeneratorTest: IDisposable {

    private readonly IBrowsingContext   browser = A.Fake<IBrowsingContext>();
    private readonly CalendarGenerator  calendarGenerator;
    private readonly INavigationHandler navigationHandler = A.Fake<INavigationHandler>();

    public CalendarGeneratorTest() {
        calendarGenerator = new CalendarGenerator(browser, new NullLogger<CalendarGenerator>());

        A.CallTo(() => navigationHandler.SupportsProtocol(A<string>._)).Returns(true);
        A.CallTo(() => browser.GetServices<INavigationHandler>()).Returns(new[] { navigationHandler });
    }

    public void Dispose() {
        calendarGenerator.Dispose();
    }

    [Fact]
    public async Task generateCalendar() {
        await using Stream htmlStream = File.OpenRead("Data/schedule-2023-sgdq.html");
        using IDocument    page       = await BrowsingContext.New().OpenAsync(response => response.Content(htmlStream).Address("https://gamesdonequick.com/schedule/43"));

        A.CallTo(() => navigationHandler.NavigateAsync(A<DocumentRequest>._, A<CancellationToken>._)).Returns(page);

        Calendar actual = await calendarGenerator.generateCalendar();

        actual.Events.Should().HaveCount(141);

        CalendarEvent actualEvent = actual.Events[0];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T16:30:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(30));
        actualEvent.Summary.Should().Be("Pre-Show");
        actualEvent.Uid.Should().Be("aldaviva.com/Summer Games Done Quick 2023/Pre-Show");
        actualEvent.Description.Should().Be("before the marathon% — Live!\nRun by Interview Crew\nHosted by Interview team");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().HaveCount(3);
        actualEvent.Alarms[0].Action.Should().Be("DISPLAY");
        actualEvent.Alarms[0].Description.Should().Be("Summer Games Done Quick 2023 is coming up next week");
        actualEvent.Alarms[0].Trigger.Duration.Should().Be(TimeSpan.FromDays(7));
        actualEvent.Alarms[0].Trigger.IsRelative.Should().BeTrue();
        actualEvent.Alarms[1].Action.Should().Be("DISPLAY");
        actualEvent.Alarms[1].Description.Should().Be("Summer Games Done Quick 2023 is starting tomorrow");
        actualEvent.Alarms[1].Trigger.Duration.Should().Be(TimeSpan.FromDays(1));
        actualEvent.Alarms[1].Trigger.IsRelative.Should().BeTrue();
        actualEvent.Alarms[2].Action.Should().Be("DISPLAY");
        actualEvent.Alarms[2].Description.Should().Be("Summer Games Done Quick 2023 will be starting soon");
        actualEvent.Alarms[2].Trigger.Duration.Should().Be(TimeSpan.FromMinutes(15));
        actualEvent.Alarms[2].Trigger.IsRelative.Should().BeTrue();

        actualEvent = actual.Events[1];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T17:00:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(77));
        actualEvent.Summary.Should().Be("Sonic Frontiers");
        actualEvent.Uid.Should().Be("aldaviva.com/Summer Games Done Quick 2023/Sonic Frontiers");
        actualEvent.Description.Should().Be("Any% (No DLC) — PC\nRun by AlphaDolphin");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[2];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T18:37:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(45));
        actualEvent.Summary.Should().Be("Bugsnax");
        actualEvent.Uid.Should().Be("aldaviva.com/Summer Games Done Quick 2023/Bugsnax");
        actualEvent.Description.Should().Be("All Bosses Co-op — PlayStation 5\nRun by Konception and limy");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[3];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T19:37:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(30));
        actualEvent.Summary.Should().Be("Mega Man Maker");
        actualEvent.Uid.Should().Be("aldaviva.com/Summer Games Done Quick 2023/Mega Man Maker");
        actualEvent.Description.Should().Be("any% — PC\nRun by megamarino");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();

        actualEvent = actual.Events[140];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-06-04T05:29:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(20));
        actualEvent.Summary.Should().Be("Finale!");
        actualEvent.Uid.Should().Be("aldaviva.com/Summer Games Done Quick 2023/Finale!");
        actualEvent.Description.Should().Be("The End% — GDQ Stage\nRun by Tech Crew");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.Should().BeNull();
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
        actualEvent.Alarms.Should().BeEmpty();
    }

    [Fact]
    public async Task ignoreSleepRuns() {
        await using Stream htmlStream = File.OpenRead("Data/schedule-sleep.html");
        using IDocument    page       = await BrowsingContext.New().OpenAsync(response => response.Content(htmlStream).Address("https://gamesdonequick.com/schedule/45"));
        A.CallTo(() => navigationHandler.NavigateAsync(A<DocumentRequest>._, A<CancellationToken>._)).Returns(page);

        Calendar actual = await calendarGenerator.generateCalendar();

        actual.Events.Should().BeEmpty();
    }

}