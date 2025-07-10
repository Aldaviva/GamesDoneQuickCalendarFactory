using Bom.Squad;
using GamesDoneQuickCalendarFactory;
using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Net.Http.Headers;
using NodaTime;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using Unfucked;
using Unfucked.HTTP;

BomSquad.DefuseUtf8Bom();

Encoding             responseEncoding     = Encoding.UTF8;
MediaTypeHeaderValue icalendarContentType = new("text/calendar") { Charset = responseEncoding.WebName };
string[]             varyHeaderValue      = [HeaderNames.AcceptEncoding];

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host
    .UseWindowsService()
    .UseSystemd();

builder.Logging.AddUnfuckedConsole();

builder.Configuration.AlsoSearchForJsonFilesInExecutableDirectory();

// GZIP response compression is handled by Apache httpd, not Kestrel, per https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-8.0#when-to-use-response-compression-middleware
builder.Services
    .Configure<Configuration>(builder.Configuration)
    .AddOutputCache()
    .AddSingleton<ICalendarGenerator, CalendarGenerator>()
    .AddSingleton<IEventDownloader, EventDownloader>()
    .AddSingleton<IGdqClient, GdqClient>()
    .AddSingleton<ICalendarPoller, CalendarPoller>()
    .AddSingleton<IGoogleCalendarSynchronizer, GoogleCalendarSynchronizer>()
    .AddSingleton<IClock>(SystemClock.Instance)
    .AddSingleton<HttpClient>(new UnfuckedHttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromHours(1) }) { Timeout = TimeSpan.FromSeconds(30) });

builder.Services.AddSingleton(await State.load("state.json"));

await using WebApplication webApp = builder.Build();

webApp
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
    .UseOutputCache()
    .Use(async (context, next) => {
        ICalendarPoller calendarPoller  = webApp.Services.GetRequiredService<ICalendarPoller>();
        ResponseHeaders responseHeaders = context.Response.GetTypedHeaders();
        responseHeaders.CacheControl               = new CacheControlHeaderValue { Public = true, MaxAge = calendarPoller.getPollingInterval() }; // longer cache when no event running
        context.Response.Headers[HeaderNames.Vary] = varyHeaderValue;

        if (await calendarPoller.mostRecentlyPolledCalendar.ResultOrNullForException() is { } mostRecentlyPolledCalendar) {
            responseHeaders.ETag         = mostRecentlyPolledCalendar.etag;
            responseHeaders.LastModified = mostRecentlyPolledCalendar.dateModified;
        }
        await next();
    });

webApp.MapGet("/", [OutputCache] async Task ([FromServices] ICalendarPoller calendarPoller, HttpResponse response) => {
    try {
        if (await calendarPoller.mostRecentlyPolledCalendar is { } mostRecentlyPolledCalendar) {
            response.GetTypedHeaders().ContentType = icalendarContentType;
            await new CalendarSerializer().SerializeAsync(mostRecentlyPolledCalendar.calendar, response.Body, responseEncoding);
        } else {
            response.StatusCode = StatusCodes.Status204NoContent;
        }
    } catch (Exception e) when (e is not OutOfMemoryException) {
        response.StatusCode  = StatusCodes.Status500InternalServerError;
        response.ContentType = MediaTypeNames.Text.Plain;
        await using StreamWriter bodyWriter = new(response.Body, responseEncoding);
        await bodyWriter.WriteAsync(e.ToString());
    }
});

webApp.MapGet("/badge.json", [OutputCache] async ([FromServices] IEventDownloader eventDownloader) =>
await eventDownloader.downloadSchedule() is { } schedule
    ? new ShieldsBadgeResponse(
        label: shortNamePattern().Replace(schedule.shortTitle, " ").ToLower(), // add spaces to abbreviation
        message: $"{schedule.runs.Count} {(schedule.runs.Count == 1 ? "run" : "runs")}",
        color: "success",
        logoSvg: Resources.gdqDpadBadgeLogo)
    : new ShieldsBadgeResponse("gdq", "no event now", "inactive", false, Resources.gdqDpadBadgeLogo));

await webApp.Services.GetRequiredService<IGoogleCalendarSynchronizer>().start();

await webApp.RunAsync();

internal partial class Program {

    [GeneratedRegex(@"(?<=\D)(?=\d)|(?<=[a-z])(?=[A-Z])")]
    private static partial Regex shortNamePattern();

}