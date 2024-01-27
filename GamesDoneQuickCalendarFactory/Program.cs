using Bom.Squad;
using GamesDoneQuickCalendarFactory;
using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Net.Http.Headers;
using System.Text;

const string ICALENDAR_MIME_TYPE    = "text/calendar;charset=UTF-8";
const int    CACHE_DURATION_MINUTES = 1;

BomSquad.DefuseUtf8Bom();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host
    .UseWindowsService()
    .UseSystemd();
builder.Services
    .AddOutputCache()
    .AddResponseCaching()
    .AddHttpClient()
    .AddSingleton<ICalendarGenerator, CalendarGenerator>()
    .AddSingleton<IEventDownloader, EventDownloader>();

WebApplication webApp = builder.Build();
webApp
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
    .UseOutputCache()
    .UseResponseCaching()
    .Use(async (context, next) => {
        context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES) };
        context.Response.Headers[HeaderNames.Vary]      = new[] { HeaderNames.AcceptEncoding };
        await next();
    });

webApp.MapGet("/", [OutputCache(Duration = CACHE_DURATION_MINUTES * 60)] async (ICalendarGenerator calendarGenerator, HttpResponse response) => {
    Calendar calendar = await calendarGenerator.generateCalendar();
    response.ContentType = ICALENDAR_MIME_TYPE;
    await new CalendarSerializer().serializeAsync(calendar, response.Body, Encoding.UTF8);
});

webApp.MapGet("/badge.json", [OutputCache(Duration = CACHE_DURATION_MINUTES * 60)] async (IEventDownloader eventDownloader) => {
    const string BADGE_LOGO = // language=xml
        """<svg xmlns="http://www.w3.org/2000/svg" version="1.1" viewBox="0 0 62.2 54.3"><style>.s{fill:#fff}</style><path d="M62.2 19.6H49c-2.5 0-4.9.7-6.9 2.1l-7.5 5.4 6.1 7.6h13.5c2 0 3.9-1.3 4.4-3.1l3.6-12zM16.4 49.2l-1.3 5.1h12.2c4.8 0 4.7-2.9 5.4-4.8l3.8-12.8-14.1-17.1H8c-2 0-3.9 1.3-4.4 3.1L0 34.7h7.8c8.8 0 16.5-2 19.3-4.4-1.3 4.1-7.9 7.7-7.9 7.7l-2.8 11.2zM47.4 0H34.5c-2 0-3.8 1.3-4.3 3.2l-4 13.5 5.7 6.9L42.6 16l1.2-4 3.6-12z" class="s"/></svg>""";

    Event schedule = await eventDownloader.downloadSchedule();
    return new ShieldsBadgeResponse(label: schedule.shortTitle, message: $"{schedule.runs.Count()} runs", color: "success", logoSvg: BADGE_LOGO);
});

webApp.Run();