using AngleSharp;
using AngleSharp.Browser;
using AngleSharp.Dom;
using AngleSharp.Io;
using GamesDoneQuickCalendarFactory;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests;

public class CalendarGeneratorTest {

    [Fact]
    public async Task generateCalendar() {
        Stream                  htmlStream        = File.OpenRead("Data/schedule-2023-sgdq.html");
        IBrowsingContext        browser           = A.Fake<IBrowsingContext>();
        using CalendarGenerator calendarGenerator = new(browser, new NullLogger<CalendarGenerator>());
        using IDocument         page              = await BrowsingContext.New().OpenAsync(response => response.Content(htmlStream).Address("https://gamesdonequick.com/schedule/43"));
        INavigationHandler      navigationHandler = A.Fake<INavigationHandler>();

        A.CallTo(() => navigationHandler.SupportsProtocol(A<string>._)).Returns(true);
        A.CallTo(() => navigationHandler.NavigateAsync(A<DocumentRequest>._, A<CancellationToken>._)).Returns(page);
        A.CallTo(() => browser.GetServices<INavigationHandler>()).Returns(new[] { navigationHandler });

        Calendar actual = await calendarGenerator.generateCalendar();

        actual.Events.Should().HaveCount(141);

        CalendarEvent actualEvent = actual.Events[0];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T16:30:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(30));
        actualEvent.Summary.Should().Be("Pre-Show");
        actualEvent.Description.Should().Be("before the marathon% — Live!\n\nRun by Interview Crew\nHosted by Interview team");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.CommonName.Should().Be("Games Done Quick");
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");

        actualEvent = actual.Events[1];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T17:00:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(77));
        actualEvent.Summary.Should().Be("Sonic Frontiers");
        actualEvent.Description.Should().Be("Any% (No DLC) — PC\n\nRun by AlphaDolphin");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.CommonName.Should().Be("Games Done Quick");
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");

        actualEvent = actual.Events[2];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T18:37:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(45));
        actualEvent.Summary.Should().Be("Bugsnax");
        actualEvent.Description.Should().Be("All Bosses Co-op — PlayStation 5\n\nRun by Konception and limy");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.CommonName.Should().Be("Games Done Quick");
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");

        actualEvent = actual.Events[3];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-05-28T19:37:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(30));
        actualEvent.Summary.Should().Be("Mega Man Maker");
        actualEvent.Description.Should().Be("any% — PC\n\nRun by megamarino");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.CommonName.Should().Be("Games Done Quick");
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");

        actualEvent = actual.Events[140];
        actualEvent.Start.Should().Be(DateTimeOffset.Parse("2023-06-04T05:29:00Z").toIDateTime());
        actualEvent.Duration.Should().Be(TimeSpan.FromMinutes(20));
        actualEvent.Summary.Should().Be("Finale!");
        actualEvent.Description.Should().Be("The End% — GDQ Stage\n\nRun by Tech Crew");
        actualEvent.IsAllDay.Should().BeFalse();
        actualEvent.Organizer.CommonName.Should().Be("Games Done Quick");
        actualEvent.Location.Should().Be("https://www.twitch.tv/gamesdonequick");
    }

}