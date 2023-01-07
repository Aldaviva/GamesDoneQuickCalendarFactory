using System.Text;
using AngleSharp;
using AngleSharp.Dom;
using GamesDoneQuickCalendarFactory;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

const string OUTPUT_FILENAME   = "gdq.ics";
const string TWITCH_STREAM_URL = "https://www.twitch.tv/gamesdonequick";
Url          scheduleUrl       = Url.Create("https://gamesdonequick.com/schedule");

using IBrowsingContext browser = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
using IDocument        doc     = await browser.OpenAsync(scheduleUrl);

IEnumerable<GameRun> runs = doc.QuerySelectorAll("tbody tr:not(.second-row, .day-split)").Select(firstRow => {
    IElement secondRow = firstRow.NextElementSibling!;

    return new GameRun(
        start: DateTimeOffset.Parse(firstRow.QuerySelector(".start-time")!.TextContent),
        duration: secondRow.QuerySelector(".text-right")!.TextContent is var duration && !string.IsNullOrWhiteSpace(duration) ? TimeSpan.Parse(duration) : TimeSpan.Zero,
        name: firstRow.QuerySelector("td:nth-child(2)")!.TextContent.Trim(),
        description: secondRow.QuerySelector("td:nth-child(2)")!.TextContent.Trim(),
        runners: firstRow.QuerySelector("td:nth-child(3)")!.TextContent.Split(", "),
        host: secondRow.QuerySelector("td:nth-child(3)")!.TextContent.Trim(),
        setupDuration: firstRow.QuerySelector("td.visible-lg")!.TextContent is var setupDuration && !string.IsNullOrWhiteSpace(setupDuration) ? TimeSpan.Parse(setupDuration) : null
    );
});

Calendar  calendar  = new() { Method     = CalendarMethods.Publish };
Organizer organizer = new() { CommonName = "Games Done Quick" };
calendar.Events.AddRange(runs.Select(run => new CalendarEvent {
    Start       = run.start.toIDateTime(),
    Duration    = run.duration,
    IsAllDay    = false, // needed because iCal.NET assumes all events that start at midnight are always all-day events, even if they have a duration that isn't 24 hours
    Summary     = run.name,
    Organizer   = organizer,
    Description = $"{run.description}\n\nRun by {run.runners.joinHumanized()}\nHosted by {run.host}",
    Location    = TWITCH_STREAM_URL
}));

await using FileStream fileStream = File.Create(OUTPUT_FILENAME);
new CalendarSerializer().Serialize(calendar, fileStream, new UTF8Encoding(false, true)); //BOM will mess up Google Calendar's URL subscription feature, but not the upload import feature
Console.WriteLine($"Wrote iCalendar to file {Path.GetFullPath(OUTPUT_FILENAME)}");