using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Google.Apis.Calendar.v3;
using Google.Apis.Http;
using Google.Apis.Services;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text;
using Calendar = Ical.Net.Calendar;
using Event = Google.Apis.Calendar.v3.Data.Event;
using IHttpClientFactory = Google.Apis.Http.IHttpClientFactory;

namespace Tests;

public class GoogleCalendarSynchronizerTest: IDisposable {

    private readonly ICalendarPoller        calendarPoller = A.Fake<ICalendarPoller>();
    private readonly CalendarService        googleCalendar = A.Fake<CalendarService>();
    private readonly FakeHttpMessageHandler httpHandler    = A.Fake<FakeHttpMessageHandler>();
    private readonly EventsResource         eventsResource = A.Fake<EventsResource>();
    private readonly State                  state          = new() { googleCalendarUidCounter = 3 };
    private readonly ConfigurableHttpClient httpClient;

    private readonly IOptions<Configuration> configuration = new OptionsWrapper<Configuration>(new Configuration {
        googleCalendarId                 = "test@group.calendar.google.com",
        googleServiceAccountEmailAddress = "test@gamesdonequickcalendarfactory.iam.gserviceaccount.com",
        googleServiceAccountPrivateKey = """
            -----BEGIN PRIVATE KEY-----
            test
            -----END PRIVATE KEY-----

            """
    });

    private GoogleCalendarSynchronizer synchronizer;

    public GoogleCalendarSynchronizerTest() {
        httpClient = new ConfigurableHttpClient(new ConfigurableMessageHandler(httpHandler));
        IClientService clientService = new CalendarService(new TestClient(httpClient));
        synchronizer = new GoogleCalendarSynchronizer(calendarPoller, googleCalendar, state, configuration, NullLogger<GoogleCalendarSynchronizer>.Instance);

        A.CallTo(() => googleCalendar.Events).Returns(eventsResource);
        A.CallTo(() => eventsResource.List(A<string>._)).ReturnsLazily((string calendarId) => new EventsResource.ListRequest(clientService, calendarId));
        A.CallTo(() => eventsResource.Insert(An<Event>._, A<string>._)).ReturnsLazily((Event body, string calendarId) => new EventsResource.InsertRequest(clientService, body, calendarId));
        A.CallTo(() => eventsResource.Update(An<Event>._, A<string>._, A<string>._))
            .ReturnsLazily((Event body, string calendarId, string eventId) => new EventsResource.UpdateRequest(clientService, body, calendarId, eventId));
        A.CallTo(() => eventsResource.Delete(A<string>._, A<string>._)).ReturnsLazily((string calendarId, string eventId) => new EventsResource.DeleteRequest(clientService, calendarId, eventId));
    }

    [Fact]
    public async Task notConfigured() {
        synchronizer = new GoogleCalendarSynchronizer(calendarPoller, null, state, configuration, NullLogger<GoogleCalendarSynchronizer>.Instance);

        await synchronizer.start();

        A.CallTo(() => eventsResource.List(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task insertOnly() {
        A.CallTo(() => httpHandler.SendAsync(An<HttpRequestMessage>._)).ReturnsNextFromSequence(
            /* list */ jsonHttpResponse("""
                {
                    "kind": "calendar#events",
                    "items": []
                }
                """),
            /* insert */ jsonHttpResponse("""
                {
                    "kind": "calendar#event",
                    "iCalUID": "00001"
                }
                """));

        await synchronizer.start();

        calendarPoller.calendarChanged += Raise.With(new Calendar {
            Events = {
                new CalendarEvent {
                    Uid      = "00001",
                    Start    = new CalDateTime(2026, 7, 14, 2, 21, 0, "America/Los_Angeles"),
                    Duration = new Duration(hours: 1),
                    Summary  = "Run 1"
                }
            }
        });

        A.CallTo(() => eventsResource.List(configuration.Value.googleCalendarId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => eventsResource.Insert(An<Event>.That.Matches(e => e.ICalUID == "00001"), configuration.Value.googleCalendarId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => eventsResource.Update(An<Event>._, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => eventsResource.Delete(A<string>._, A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task fullDiff() {
        A.CallTo(() => httpHandler.SendAsync(An<HttpRequestMessage>._)).ReturnsNextFromSequence(
            /* list */ jsonHttpResponse("""
                {
                    "kind": "calendar#events",
                    "items": [
                        {
                            "kind": "calendar#event",
                            "id": "00001",
                            "iCalUID": "00001",
                            "summary": "To delete",
                            "start": {
                                "dateTime": "2026-07-14T01:21:00-07:00"
                            },
                            "end": {
                                "dateTime": "2026-07-14T02:21:00-07:00"
                            }
                        },
                        {
                            "kind": "calendar#event",
                            "id": "00002",
                            "iCalUID": "00002",
                            "summary": "To update",
                            "description": "Old description",
                            "start": {
                                "dateTime": "2026-07-14T02:21:00-07:00"
                            },
                            "end": {
                                "dateTime": "2026-07-14T03:21:00-07:00"
                            }
                        },
                        {
                            "kind": "calendar#event",
                            "id": "00003",
                            "iCalUID": "00003",
                            "summary": "To ignore",
                            "start": {
                                "dateTime": "2026-07-14T03:21:00-07:00"
                            },
                            "end": {
                                "dateTime": "2026-07-14T04:21:00-07:00"
                            }
                        }
                    ]
                }
                """),
            /* delete */ jsonHttpResponse("""
                {
                    "kind": "calendar#event",
                    "id": "00001",
                    "iCalUID": "00001"
                }
                """),
            /* insert */ jsonHttpResponse("""
                {
                    "kind": "calendar#event",
                    "id": "00004",
                    "iCalUID": "00004"
                }
                """),
            /* update */ jsonHttpResponse("""
                {
                    "kind": "calendar#event",
                    "id": "00002",
                    "iCalUID": "00002"
                }
                """));

        await synchronizer.start();

        calendarPoller.calendarChanged += Raise.With(new Calendar {
            Events = {
                new CalendarEvent {
                    Uid         = "00002",
                    Start       = new CalDateTime(2026, 7, 14, 2, 21, 0, "America/Los_Angeles"),
                    Duration    = new Duration(hours: 1),
                    Summary     = "To update",
                    Description = "New description"
                },
                new CalendarEvent {
                    Uid      = "00003",
                    Start    = new CalDateTime(2026, 7, 14, 3, 21, 0, "America/Los_Angeles"),
                    Duration = new Duration(hours: 1),
                    Summary  = "To ignore"
                },
                new CalendarEvent {
                    Uid      = "00004",
                    Start    = new CalDateTime(2026, 7, 14, 5, 21, 0, "America/Los_Angeles"),
                    Duration = new Duration(hours: 1),
                    Summary  = "To insert"
                }
            }
        });

        A.CallTo(() => eventsResource.List(configuration.Value.googleCalendarId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => eventsResource.Insert(An<Event>.That.Matches(e => e.ICalUID == "00004"), configuration.Value.googleCalendarId)).MustHaveHappenedOnceExactly();
        A.CallTo(() => eventsResource.Update(An<Event>.That.Matches(e => e.ICalUID == "00002"), configuration.Value.googleCalendarId, "00002")).MustHaveHappenedOnceExactly();
        A.CallTo(() => eventsResource.Delete(configuration.Value.googleCalendarId, "00001")).MustHaveHappenedOnceExactly();
    }

    private static HttpResponseMessage jsonHttpResponse([StringSyntax("json")] string responseBody) =>
        new() { Content = new StringContent(responseBody, Encoding.UTF8, MediaTypeNames.Application.Json) };

    private class TestClient: BaseClientService.Initializer, IHttpClientFactory {

        private readonly ConfigurableHttpClient httpClient;

        public TestClient(ConfigurableHttpClient httpClient) {
            this.httpClient   = httpClient;
            BaseUri           = "https://test.googleapis.com/calendar/v3/";
            HttpClientFactory = this;
        }

        public ConfigurableHttpClient CreateHttpClient(CreateHttpClientArgs args) => httpClient;

    }

    public void Dispose() {
        httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

}