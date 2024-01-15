using Bom.Squad;
using GamesDoneQuickCalendarFactory;
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
    .AddSingleton<ICalendarGenerator, CalendarGenerator>();

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

webApp.MapGet("/", [OutputCache(Duration = CACHE_DURATION_MINUTES * 60)] async (request) => {
    ICalendarGenerator calendarGenerator = request.RequestServices.GetRequiredService<ICalendarGenerator>();
    Calendar           calendar          = await calendarGenerator.generateCalendar();
    request.Response.ContentType = ICALENDAR_MIME_TYPE;
    await new CalendarSerializer().serializeAsync(calendar, request.Response.Body, Encoding.UTF8);
});

webApp.Run();