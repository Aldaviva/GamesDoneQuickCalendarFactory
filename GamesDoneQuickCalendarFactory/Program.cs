﻿using Bom.Squad;
using GamesDoneQuickCalendarFactory;
using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NodaTime;
using System.Text;
using System.Text.RegularExpressions;

const string         CACHE_POLICY         = "Cache policy";
Encoding             calendarEncoding     = Encoding.UTF8;
MediaTypeHeaderValue icalendarContentType = new("text/calendar") { Charset = calendarEncoding.WebName };

BomSquad.DefuseUtf8Bom();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host
    .UseWindowsService()
    .UseSystemd();

builder.Logging.addMyCustomFormatter();

// GZIP response compression is handled by Apache httpd, not Kestrel, per https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression?view=aspnetcore-8.0#when-to-use-response-compression-middleware
builder.Services
    .Configure<Configuration>(builder.Configuration)
    .AddOutputCache(options => options.AddPolicy(CACHE_POLICY, policyBuilder => policyBuilder.Expire(builder.Configuration.Get<Configuration>()!.cacheDuration)))
    .AddSingleton<ICalendarGenerator, CalendarGenerator>()
    .AddSingleton<IEventDownloader, EventDownloader>()
    .AddSingleton<IGdqClient, GdqClient>()
    .AddSingleton<ICalendarPoller, CalendarPoller>()
    .AddSingleton<IGoogleCalendarSynchronizer, GoogleCalendarSynchronizer>()
    .AddSingleton<IClock>(SystemClock.Instance);
builder.Services.AddHttpClient("http", client => { client.Timeout = TimeSpan.FromSeconds(30); }).SetHandlerLifetime(TimeSpan.FromHours(1));

WebApplication webApp = builder.Build();
webApp
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
    .UseOutputCache()
    .Use(async (context, next) => {
        Configuration config = webApp.Services.GetRequiredService<IOptions<Configuration>>().Value;
        context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = config.cacheDuration };
        context.Response.Headers[HeaderNames.Vary]      = new[] { HeaderNames.AcceptEncoding };
        await next();
    });

webApp.MapGet("/", async Task ([FromServices] ICalendarPoller calendarPoller, HttpResponse response) => {
    CalendarResponse? mostRecentlyPolledCalendar = calendarPoller.mostRecentlyPolledCalendar;
    if (mostRecentlyPolledCalendar != null) {
        ResponseHeaders responseHeaders = response.GetTypedHeaders();
        responseHeaders.ContentType  = icalendarContentType;
        responseHeaders.ETag         = new EntityTagHeaderValue(mostRecentlyPolledCalendar.etag);
        responseHeaders.LastModified = mostRecentlyPolledCalendar.dateModified;
        await new CalendarSerializer().serializeAsync(mostRecentlyPolledCalendar.calendar, response.Body, calendarEncoding);
    }
}).CacheOutput(CACHE_POLICY);

webApp.MapGet("/badge.json", async ([FromServices] IEventDownloader eventDownloader) =>
await eventDownloader.downloadSchedule() is { } schedule
    ? new ShieldsBadgeResponse(
        label: Regex.Replace(schedule.shortTitle, @"(?<=\D)(?=\d)|(?<=[a-z])(?=[A-Z])", " ").ToLower(), // add spaces to abbreviation
        message: $"{schedule.runs.Count} {(schedule.runs.Count == 1 ? "run" : "runs")}",
        color: "success",
        logoSvg: Resources.gdqDpadBadgeLogo)
    : new ShieldsBadgeResponse("gdq", "no event now", "important", false, Resources.gdqDpadBadgeLogo)).CacheOutput(CACHE_POLICY);

await webApp.Services.GetRequiredService<IGoogleCalendarSynchronizer>().start();

await webApp.RunAsync();