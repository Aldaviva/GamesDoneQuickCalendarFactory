using Bom.Squad;
using GamesDoneQuickCalendarFactory;
using GamesDoneQuickCalendarFactory.Data;
using GamesDoneQuickCalendarFactory.Services;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Net.Http.Headers;
using NodaTime;
using System.Text;
using System.Text.RegularExpressions;

const string ICALENDAR_MIME_TYPE      = "text/calendar;charset=UTF-8";
const int    CACHE_DURATION_MINUTES   = 3;
const string QUERY_PARAM_CACHE_POLICY = "Vary by query param";

BomSquad.DefuseUtf8Bom();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host
    .UseWindowsService()
    .UseSystemd();

builder.Services
    .AddOutputCache(options => options.AddPolicy(QUERY_PARAM_CACHE_POLICY, policyBuilder => policyBuilder.SetVaryByQuery("includeAnnoyingPeople")))
    .AddHttpClient()
    .AddSingleton<ICalendarGenerator, CalendarGenerator>()
    .AddSingleton<IEventDownloader, EventDownloader>()
    .AddSingleton<IGdqClient, GdqClient>()
    .AddSingleton<IClock>(SystemClock.Instance);

WebApplication webApp = builder.Build();
webApp
    .UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto })
    .UseOutputCache()
    .Use(async (context, next) => {
        context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue { Public = true, MaxAge = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES) };
        context.Response.Headers[HeaderNames.Vary]      = new[] { HeaderNames.AcceptEncoding };
        await next();
    });

webApp.MapGet("/", [OutputCache(Duration = CACHE_DURATION_MINUTES * 60, PolicyName = QUERY_PARAM_CACHE_POLICY)]
    async Task (ICalendarGenerator calendarGenerator, HttpResponse response, [FromQuery] bool includeAnnoyingPeople = false) => {
        Calendar calendar = await calendarGenerator.generateCalendar(includeAnnoyingPeople);
        response.ContentType = ICALENDAR_MIME_TYPE;
        await new CalendarSerializer().serializeAsync(calendar, response.Body, Encoding.UTF8);
    });

webApp.MapGet("/badge.json", [OutputCache(Duration = CACHE_DURATION_MINUTES * 60)] async (IEventDownloader eventDownloader) =>
    await eventDownloader.downloadSchedule() is { } schedule
        ? new ShieldsBadgeResponse(
            label: Regex.Replace(schedule.shortTitle, @"(?<=\D)(?=\d)|(?<=[a-z])(?=[A-Z])", " "), // add spaces to abbreviation
            message: $"{schedule.runs.Count} {(schedule.runs.Count == 1 ? "run" : "runs")}",
            color: "success",
            logoSvg: Resources.gdqDpadBadgeLogo)
        : new ShieldsBadgeResponse("GDQ", "no event now", "important", false, Resources.gdqDpadBadgeLogo));

webApp.Run();