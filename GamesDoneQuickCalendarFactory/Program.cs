using System.Text;
using GamesDoneQuickCalendarFactory;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Net.Http.Headers;

const string ICALENDAR_MIME_TYPE    = "text/calendar;charset=UTF-8";
const int    CACHE_DURATION_MINUTES = 5;

Encoding utf8 = new UTF8Encoding(false, true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options => options.AllowSynchronousIO = true);
builder.Host
    .UseWindowsService()
    .UseSystemd();
builder.Services
    .AddOutputCache()
    .AddResponseCaching()
    .AddSingleton<ICalendarGenerator, CalendarGenerator>();

WebApplication webApp = builder.Build();
webApp
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
    .UseOutputCache()
    .UseResponseCaching()
    .Use(async (context, next) => {
        context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES) };
        context.Response.Headers[HeaderNames.Vary]      = new[] { "Accept-Encoding" };
        await next();
    });

webApp.MapGet("/", [OutputCache(Duration = CACHE_DURATION_MINUTES * 60)] async (request) => {
    ICalendarGenerator calendarGenerator = request.RequestServices.GetRequiredService<ICalendarGenerator>();
    Calendar           calendar          = await calendarGenerator.generateCalendar();
    request.Response.ContentType = ICALENDAR_MIME_TYPE;
    new CalendarSerializer().Serialize(calendar, request.Response.Body, utf8);
});

webApp.Run();