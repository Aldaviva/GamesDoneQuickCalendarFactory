using GamesDoneQuickCalendarFactory.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Options;
using ThrottleDebounce;
using Calendar = Ical.Net.Calendar;
using Event = Google.Apis.Calendar.v3.Data.Event;

namespace GamesDoneQuickCalendarFactory.Services;

public interface IGoogleCalendarSynchronizer: IDisposable {

    Task start();

}

public class GoogleCalendarSynchronizer: IGoogleCalendarSynchronizer {

    private const int MAX_EVENTS_PER_PAGE = 2500; // week-long GDQ events usually comprise about 150 runs

    private readonly CalendarService?                    calendarService;
    private readonly ICalendarPoller                     calendarPoller;
    private readonly IOptions<Configuration>             configuration;
    private readonly ILogger<GoogleCalendarSynchronizer> logger;

    private IDictionary<string, Event> existingGoogleEventsByIcalUid = null!;

    public GoogleCalendarSynchronizer(ICalendarPoller calendarPoller, IOptions<Configuration> configuration, ILogger<GoogleCalendarSynchronizer> logger) {
        this.calendarPoller = calendarPoller;
        this.configuration  = configuration;
        this.logger         = logger;

        if (configuration.Value is { googleCalendarId: not null, googleServiceAccountEmailAddress: { } serviceAccount, googleServiceAccountPrivateKey: { } privateKey }) {
            calendarService = new CalendarService(new BaseClientService.Initializer {
                HttpClientInitializer = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccount) {
                    Scopes = [CalendarService.Scope.CalendarEvents]
                }.FromPrivateKey(privateKey)),
                ApplicationName = "Aldaviva/GamesDoneQuickCalendarFactory"
            });
        }
    }

    public async Task start() {
        if (calendarService != null) {
            string googleCalendarId = configuration.Value.googleCalendarId!;

            Events googleCalendarEvents = await Retrier.Attempt(_ => {
                logger.LogDebug("Downloading existing events from Google Calendar {calendarId}", googleCalendarId);
                EventsResource.ListRequest listRequest = calendarService.Events.List(googleCalendarId);
                listRequest.MaxResults = MAX_EVENTS_PER_PAGE;
                return listRequest.ExecuteAsync();
            }, null, exponentialBackoff);

            existingGoogleEventsByIcalUid = googleCalendarEvents.Items.ToDictionary(googleEvent => googleEvent.ICalUID);
            logger.LogDebug("Found {count:N0} existing events in Google Calendar", existingGoogleEventsByIcalUid.Values.Count);

            calendarPoller.calendarChanged += sync;
        }

        await calendarPoller.pollCalendar();
    }

    private async void sync(object? sender, Calendar newCalendar) {
        string googleCalendarId = configuration.Value.googleCalendarId!;

        IEnumerable<Event> eventsToDelete = existingGoogleEventsByIcalUid.ExceptBy(newCalendar.Events.Select(icsEvent => icsEvent.Uid), gcalEvent => gcalEvent.Key).Select(pair => pair.Value).ToList();
        IEnumerable<CalendarEvent> eventsToCreate = newCalendar.Events.ExceptBy(existingGoogleEventsByIcalUid.Keys, icsEvent => icsEvent.Uid).ToList();
        IEnumerable<CalendarEvent> eventsToUpdate = newCalendar.Events.Except(eventsToCreate).Where(icsEvent => {
            Event googleEvent = existingGoogleEventsByIcalUid[icsEvent.Uid];
            return icsEvent.Summary != googleEvent.Summary ||
                !icsEvent.Start.AsDateTimeOffset.Equals(googleEvent.Start.DateTimeDateTimeOffset) ||
                !icsEvent.End.AsDateTimeOffset.Equals(googleEvent.End.DateTimeDateTimeOffset) ||
                icsEvent.Location != googleEvent.Location ||
                icsEvent.Description != googleEvent.Description;
        }).ToList();

        logger.LogDebug("Deleting {count:N0} orphaned events from Google Calendar", eventsToDelete.Count());
        foreach (Event eventToDelete in eventsToDelete) {
            await calendarService!.Events.Delete(googleCalendarId, eventToDelete.Id).ExecuteAsync();
            existingGoogleEventsByIcalUid.Remove(eventToDelete.ICalUID);
            logger.LogTrace("Deleted event {summary} from Google Calendar", eventToDelete.Summary);
        }

        logger.LogDebug("Creating {count:N0} new events in Google Calendar", eventsToCreate.Count());
        foreach (CalendarEvent eventToCreate in eventsToCreate) {
            existingGoogleEventsByIcalUid[eventToCreate.Uid] = await calendarService!.Events.Insert(eventToCreate.toGoogleEvent(), googleCalendarId).ExecuteAsync();
            logger.LogTrace("Created event {summary} in Google Calendar", eventToCreate.Summary);
        }

        logger.LogDebug("Updating {count:N0} outdated events in Google Calendar", eventsToUpdate.Count());
        foreach (CalendarEvent eventToUpdate in eventsToUpdate) {
            existingGoogleEventsByIcalUid[eventToUpdate.Uid] =
                await calendarService!.Events.Update(eventToUpdate.toGoogleEvent(), googleCalendarId, existingGoogleEventsByIcalUid[eventToUpdate.Uid].Id).ExecuteAsync();
            logger.LogTrace("Updated event {summary} in Google Calendar", eventToUpdate.Summary);
        }
    }

    private static TimeSpan exponentialBackoff(int attempt) => TimeSpan.FromSeconds(Math.Min(attempt * attempt, 300));

    public void Dispose() {
        calendarPoller.calendarChanged -= sync;
        calendarService?.Dispose();
        GC.SuppressFinalize(this);
    }

}