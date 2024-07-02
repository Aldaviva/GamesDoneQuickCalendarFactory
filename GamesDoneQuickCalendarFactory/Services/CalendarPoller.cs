using GamesDoneQuickCalendarFactory.Data;
using Ical.Net;
using Microsoft.Extensions.Options;

namespace GamesDoneQuickCalendarFactory.Services;

public interface ICalendarPoller: IDisposable, IAsyncDisposable {

    CalendarResponse? mostRecentlyPolledCalendar { get; }

    event EventHandler<Calendar>? calendarChanged;

}

public class CalendarPoller: ICalendarPoller {

    private static readonly TimeSpan OUT_OF_EVENT_POLLING_INTERVAL = TimeSpan.FromHours(1);

    private readonly ICalendarGenerator      calendarGenerator;
    private readonly IOptions<Configuration> config;
    private readonly ILogger<CalendarPoller> logger;
    private readonly Timer                   pollingTimer;
    private readonly SemaphoreSlim           pollingLock = new(1);

    public CalendarResponse? mostRecentlyPolledCalendar { get; private set; }

    public event EventHandler<Calendar>? calendarChanged;

    private bool wasEventRunning;

    public CalendarPoller(ICalendarGenerator calendarGenerator, IOptions<Configuration> config, ILogger<CalendarPoller> logger) {
        this.calendarGenerator = calendarGenerator;
        this.config            = config;
        this.logger            = logger;

        pollingTimer = new Timer(pollCalendar, null, TimeSpan.Zero, OUT_OF_EVENT_POLLING_INTERVAL);
    }

    private async void pollCalendar(object? state = null) {
        if (await pollingLock.WaitAsync(0)) { // don't allow parallel polls, and if one is already running, skip the new iteration
            try {
                logger.LogDebug("Polling GDQ schedule");
                Calendar       calendar      = await calendarGenerator.generateCalendar();
                DateTimeOffset generatedDate = DateTimeOffset.UtcNow;

                if (!calendar.Equals(mostRecentlyPolledCalendar?.calendar)) {
                    mostRecentlyPolledCalendar = new CalendarResponse(calendar, generatedDate);
                    logger.LogInformation("GDQ schedule changed, new etag is {etag}", mostRecentlyPolledCalendar.etag);
                    calendarChanged?.Invoke(this, calendar);
                } else {
                    logger.LogTrace("GDQ schedule is unchanged");
                }

                bool isEventRunning = calendar.Events.Count != 0 &&
                    generatedDate >= calendar.Events.Min(run => run.Start)!.AsDateTimeOffset - OUT_OF_EVENT_POLLING_INTERVAL &&
                    generatedDate < calendar.Events.Max(run => run.Start)!.AsDateTimeOffset;

                if (wasEventRunning != isEventRunning) {
                    TimeSpan desiredPollingInterval = isEventRunning ? config.Value.cacheDuration : OUT_OF_EVENT_POLLING_INTERVAL;
                    pollingTimer.Change(desiredPollingInterval, desiredPollingInterval);
                    wasEventRunning = isEventRunning;
                }
            } catch (Exception e) when (e is not OutOfMemoryException) {
                logger.LogError(e, "Failed to poll GDQ schedule, trying again later");
            } finally {
                pollingLock.Release();
            }
        }
    }

    public void Dispose() {
        pollingTimer.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() {
        await pollingTimer.DisposeAsync();
        GC.SuppressFinalize(this);
    }

}