using GamesDoneQuickCalendarFactory.Data.Marshal;
using GamesDoneQuickCalendarFactory.Services;
using NodaTime;
using NodaTime.Text;
using System.Net;
using System.Net.Mime;
using System.Text;
using Unfucked.HTTP;
using Unfucked.HTTP.Config;

namespace Tests;

public class CurrentEventIdFetcherTest: IDisposable {

    private readonly UnfuckedHttpHandler httpMessageHandler = A.Fake<UnfuckedHttpHandler>(options => options.CallsBaseMethods());
    private readonly UnfuckedHttpClient  http;

    public CurrentEventIdFetcherTest() {
        http = new UnfuckedHttpClient(httpMessageHandler).Property(PropertyKey.JsonSerializerOptions, JsonSerializerGlobalOptions.JSON_SERIALIZER_OPTIONS);
    }

    public void Dispose() {
        http.Dispose();
    }

    [Fact]
    public async Task donationRedirection() {
        DonationCurrentEventIdFetcher currentEventIdFetcher = new(http);
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://tracker.gamesdonequick.com/tracker/donate/"), A<CancellationToken>._))
            .Returns(new HttpResponseMessage { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://tracker.gamesdonequick.com/tracker/ui/events/123/donate") });

        int? actual = await currentEventIdFetcher.getCurrentEventId();

        actual.Should().Be(123);
        currentEventIdFetcher.priority.Should().Be(1);
    }

    [Fact]
    public async Task donationRedirectionCaptcha() {
        DonationCurrentEventIdFetcher currentEventIdFetcher = new(http);
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://tracker.gamesdonequick.com/tracker/donate/"), A<CancellationToken>._))
            .Returns(new HttpResponseMessage(HttpStatusCode.Forbidden) { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://tracker.gamesdonequick.com/tracker/donate/") });

        Func<Task<int?>> thrower = async () => await currentEventIdFetcher.getCurrentEventId();
        await thrower.Should().ThrowAsync<CalendarFactoryException.CurrentEventFetchError>();
    }

    [Fact]
    public async Task scheduleRedirection() {
        ScheduleWebPageCurrentEventIdFetcher currentEventIdFetcher = new(http);
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://gamesdonequick.com/schedule/"), A<CancellationToken>._))
            .Returns(new HttpResponseMessage { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://gamesdonequick.com/schedule/123") });

        int? actual = await currentEventIdFetcher.getCurrentEventId();

        actual.Should().Be(123);
        currentEventIdFetcher.priority.Should().Be(2);
    }

    [Fact]
    public async Task scheduleRedirectionNoRuns() {
        ScheduleWebPageCurrentEventIdFetcher currentEventIdFetcher = new(http);
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Head, "https://gamesdonequick.com/schedule/"), A<CancellationToken>._))
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = new HttpRequestMessage(HttpMethod.Head, "https://gamesdonequick.com/schedule/") });

        int? actual = await currentEventIdFetcher.getCurrentEventId();

        actual.Should().BeNull();
    }

    [Theory]
    [InlineData("2026-09-04T12:00:00-05:00", 68, "before paxwest starts")]
    [InlineData("2026-09-04T14:00:00-05:00", 68, "while paxwest runs")]
    [InlineData("2026-09-11T12:00:00-05:00", 68, "a little after paxwest ended")]
    [InlineData("2026-09-13T00:00:00-05:00", 69, "longer after paxwest ended")]
    public async Task eventsApiQuery(string now, int expected, string description) {
        IClock                         clock                 = A.Fake<IClock>();
        EventsApiCurrentEventIdFetcher currentEventIdFetcher = new(http, clock);

        A.CallTo(() => clock.GetCurrentInstant()).Returns(OffsetDateTimePattern.GeneralIso.Parse(now).Value.ToInstant());
        A.CallTo(() => httpMessageHandler.TestableSendAsync(An<HttpRequestMessage>.That.Matches(HttpMethod.Get, "https://private.gamesdonequick.com/tracker/api/v2/events/"), A<CancellationToken>._))
            .Returns(new HttpResponseMessage {
                Content = new StringContent( /*lang=json*/ """
                    {
                        "count": 66,
                        "next": null,
                        "previous": null,
                        "results": [
                            {
                                "type": "event",
                                "id": 69,
                                "short": "flamefatales2026",
                                "name": "Flame Fatales 2026",
                                "paypalcurrency": "USD",
                                "hashtag": "FlameFatales",
                                "datetime": "2026-09-13T11:30:00-05:00",
                                "timezone": "US/Eastern",
                                "receivername": "Malala Fund",
                                "receiver_short": "",
                                "receiver_solicitation_text": "I want to receive emails from Malala Fund",
                                "receiver_logo": "https://gdq-assets.gamesdonequick.com/assets/ff/MalalaPrimaryLogo_Large_500px.png",
                                "receiver_privacy_policy": "https://malala.org/privacy-policy",
                                "use_one_step_screening": false,
                                "screening_mode": "two_pass",
                                "locked": false,
                                "archived": false,
                                "draft": false,
                                "allow_donations": false,
                                "minimumdonation": 5,
                                "maximum_paypal_donation": null
                            },
                            {
                                "type": "event",
                                "id": 68,
                                "short": "SpeedAtPAXWest26",
                                "name": "Speedrun Stage @ PAX West 2026",
                                "paypalcurrency": "USD",
                                "hashtag": "",
                                "datetime": "2026-09-04T13:00:00-05:00",
                                "timezone": "US/Pacific",
                                "receivername": "",
                                "receiver_short": "",
                                "receiver_solicitation_text": "",
                                "receiver_logo": "",
                                "receiver_privacy_policy": "",
                                "use_one_step_screening": false,
                                "screening_mode": "two_pass",
                                "locked": false,
                                "archived": false,
                                "draft": true,
                                "allow_donations": false,
                                "minimumdonation": 1,
                                "maximum_paypal_donation": null
                            },
                            {
                                "type": "event",
                                "id": 67,
                                "short": "gamescomgdq",
                                "name": "gamescom GDQ",
                                "paypalcurrency": "EUR",
                                "hashtag": "gamescomgdq",
                                "datetime": "2026-08-28T03:00:00-05:00",
                                "timezone": "Europe/Berlin",
                                "receivername": "Gaming for Democracy",
                                "receiver_short": "G4D",
                                "receiver_solicitation_text": "I want to receive emails from Gaming for Democracy",
                                "receiver_logo": "",
                                "receiver_privacy_policy": "https://www.stiftung-digitale-spielekultur.de/en/privacy/",
                                "use_one_step_screening": true,
                                "screening_mode": "one_pass",
                                "locked": false,
                                "archived": false,
                                "draft": false,
                                "allow_donations": false,
                                "minimumdonation": 1,
                                "maximum_paypal_donation": null
                            }
                        ]
                    }
                    """, Encoding.UTF8, MediaTypeNames.Application.Json)
            });

        int? actual = await currentEventIdFetcher.getCurrentEventId();

        actual.Should().Be(expected, description);
        currentEventIdFetcher.priority.Should().Be(3);
    }

}