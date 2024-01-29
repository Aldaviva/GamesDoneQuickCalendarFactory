using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Tests;

public class ServerTest: IDisposable {

    private const string EXPECTED_LOGO_SVG = // language=xml
        """<svg xmlns="http://www.w3.org/2000/svg" version="1.1" viewBox="0 0 62.2 54.3"><style>.s{fill:#fff}</style><path d="M62.2 19.6H49c-2.5 0-4.9.7-6.9 2.1l-7.5 5.4 6.1 7.6h13.5c2 0 3.9-1.3 4.4-3.1l3.6-12zM16.4 49.2l-1.3 5.1h12.2c4.8 0 4.7-2.9 5.4-4.8l3.8-12.8-14.1-17.1H8c-2 0-3.9 1.3-4.4 3.1L0 34.7h7.8c8.8 0 16.5-2 19.3-4.4-1.3 4.1-7.9 7.7-7.9 7.7l-2.8 11.2zM47.4 0H34.5c-2 0-3.8 1.3-4.3 3.2l-4 13.5 5.7 6.9L42.6 16l1.2-4 3.6-12z" class="s"/></svg>""";

    private readonly ICalendarGenerator             calendarGenerator = A.Fake<ICalendarGenerator>();
    private readonly IEventDownloader               eventDownloader   = A.Fake<IEventDownloader>();
    private readonly HttpClient                     client;
    private readonly WebApplicationFactory<Program> webapp;

    public ServerTest() {
        webapp = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => {
                builder.UseTestServer();
                builder.ConfigureTestServices(collection => collection
                    .AddSingleton(calendarGenerator)
                    .AddSingleton(eventDownloader));
            });
        client = webapp.CreateClient();
    }

    [Fact]
    public async Task getCalendar() {
        Calendar      calendar      = new();
        CalendarEvent calendarEvent = new();
        calendar.Events.Add(calendarEvent);
        calendarEvent.Summary     = "My Event";
        calendarEvent.Start       = new CalDateTime(2023, 4, 16, 1, 18, 0, "America/Los_Angeles");
        calendarEvent.Duration    = TimeSpan.FromHours(1);
        calendarEvent.IsAllDay    = false;
        calendarEvent.Organizer   = new Organizer { CommonName = "Ben Hutchison" };
        calendarEvent.Location    = "My location";
        calendarEvent.Description = "My description";
        calendarEvent.DtStamp     = new CalDateTime("20230416T082040Z");
        calendarEvent.Uid         = "c9e08bcf-773a-4291-b0a4-dd7459ed13ba";

        A.CallTo(() => calendarGenerator.generateCalendar()).Returns(calendar);

        using HttpResponseMessage response = await client.GetAsync("/");

        string responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Be(Regex.Replace("""
                                               BEGIN:VCALENDAR
                                               PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
                                               VERSION:2.0
                                               BEGIN:VEVENT
                                               DESCRIPTION:My description
                                               DTEND;TZID=America/Los_Angeles:20230416T021800
                                               DTSTAMP:20230416T082040Z
                                               DTSTART;TZID=America/Los_Angeles:20230416T011800
                                               LOCATION:My location
                                               ORGANIZER;CN=Ben Hutchison:
                                               SEQUENCE:0
                                               SUMMARY:My Event
                                               UID:c9e08bcf-773a-4291-b0a4-dd7459ed13ba
                                               END:VEVENT
                                               END:VCALENDAR

                                               """, @"(?<!\r)\n", "\r\n"));

        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;
        contentType.Should().NotBeNull();
        contentType!.MediaType.Should().Be("text/calendar");
        contentType.CharSet.Should().Be("UTF-8");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        A.CallTo(() => calendarGenerator.generateCalendar()).MustHaveHappenedOnceExactly();

    }

    [Fact]
    public async Task noUtf8Bom() {
        using HttpResponseMessage response = await client.GetAsync("/");

        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
        responseBytes[..3].Should().NotEqual(new byte[] { 0xEF, 0xBB, 0xBF },
            "ICS response bytes should not start with a UTF-8 BOM, since Google Calendar's URL subscription client cannot parse them and throws an error");
    }

    [Fact]
    public async Task badgeJson() {
        A.CallTo(() => eventDownloader.downloadSchedule()).Returns(new Event("Awesome Games Done Quick 2024", "AGDQ2024", new GameRun[145]));

        JsonObject? response = await client.GetFromJsonAsync<JsonObject>("/badge.json");

        response.Should().NotBeNull();
        response!["schemaVersion"]!.GetValue<int>().Should().Be(1);
        response["label"]!.GetValue<string>().Should().Be("AGDQ 2024");
        response["message"]!.GetValue<string>().Should().Be("145 runs");
        response["color"]!.GetValue<string>().Should().Be("success");
        response["isError"]!.GetValue<bool>().Should().BeFalse();
        response["logoSvg"]!.GetValue<string>().Should().Be(EXPECTED_LOGO_SVG);
        response.Should().HaveCount(6);
    }

    [Fact]
    public async Task badgeJsonEmpty() {
        A.CallTo(() => eventDownloader.downloadSchedule()).Returns((Event?) null);

        JsonObject? response = await client.GetFromJsonAsync<JsonObject>("/badge.json");

        response.Should().NotBeNull();
        response!["schemaVersion"]!.GetValue<int>().Should().Be(1);
        response["label"]!.GetValue<string>().Should().Be("GDQ");
        response["message"]!.GetValue<string>().Should().Be("no event now");
        response["color"]!.GetValue<string>().Should().Be("important");
        response["isError"]!.GetValue<bool>().Should().BeFalse();
        response["logoSvg"]!.GetValue<string>().Should().Be(EXPECTED_LOGO_SVG);
        response.Should().HaveCount(6);
    }

    public void Dispose() {
        client.Dispose();
        webapp.Dispose();
    }

}