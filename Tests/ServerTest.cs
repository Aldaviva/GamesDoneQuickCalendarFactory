using AngleSharp;
using AngleSharp.Io;
using GamesDoneQuickCalendarFactory;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Tests;

public class ServerTest: IDisposable {

    private readonly ICalendarGenerator             calendarGenerator = A.Fake<ICalendarGenerator>();
    private readonly HttpClient                     client;
    private readonly WebApplicationFactory<Program> webapp;

    public ServerTest() {
        webapp = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => {
                builder.UseTestServer(options => options.AllowSynchronousIO = true);
                builder.ConfigureTestServices(collection => { collection.AddSingleton(calendarGenerator); });
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
    public void browsingContext() {
        IBrowsingContext? browsingContext = webapp.Services.GetService<IBrowsingContext>();

        browsingContext.Should().NotBeNull();
        browsingContext!.GetService<DefaultHttpRequester>().Should().NotBeNull();
        browsingContext.GetService<IDocumentLoader>().Should().NotBeNull();
    }

    [Fact]
    public async Task noUtf8Bom() {
        using HttpResponseMessage response = await client.GetAsync("/");

        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
        responseBytes[..3].Should().NotEqual(new byte[] { 0xEF, 0xBB, 0xBF },
            "ICS response bytes should not start with a UTF-8 BOM, since Google Calendar's URL subscription client cannot parse them and throws an error");
    }

    public void Dispose() {
        client.Dispose();
        webapp.Dispose();
    }

}