using GamesDoneQuickCalendarFactory.Data;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Options;
using NodaTime.Extensions;
using System.Net;
using ThrottleDebounce.Retry;
using Unfucked;
using Unfucked.DateTime;
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
    private readonly State                               state;
    private readonly IOptions<Configuration>             configuration;
    private readonly ILogger<GoogleCalendarSynchronizer> logger;
    private readonly SemaphoreSlim                       gcalClientLock = new(1);

    private IDictionary<string, Event> existingGoogleEventsByIcalUid = null!;

    public GoogleCalendarSynchronizer(ICalendarPoller calendarPoller, State state, IOptions<Configuration> configuration, ILogger<GoogleCalendarSynchronizer> logger) {
        this.calendarPoller = calendarPoller;
        this.state          = state;
        this.configuration  = configuration;
        this.logger         = logger;

        if (configuration.Value is { googleCalendarId: not null, googleServiceAccountEmailAddress: { } serviceAccount, googleServiceAccountPrivateKey: { } privateKey }) {
            calendarService = new UnfuckedGoogleCalendarService(new BaseClientService.Initializer {
                HttpClientInitializer = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccount) {
                    Scopes = [CalendarService.Scope.CalendarEvents]
                }.FromPrivateKey(privateKey)),
                ApplicationName                 = "Aldaviva/GamesDoneQuickCalendarFactory",
                DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.RecommendedOrDefault
            });
        }
    }

    public async Task start() {
        if (calendarService != null) {
            string googleCalendarId = configuration.Value.googleCalendarId!;

            Events googleCalendarEvents = await Retrier.Attempt(async _ => {
                logger.LogDebug("Downloading existing events from Google Calendar {calendarId}", googleCalendarId);
                EventsResource.ListRequest listRequest = calendarService.Events.List(googleCalendarId);
                listRequest.MaxResults = MAX_EVENTS_PER_PAGE;
                return await listRequest.ExecuteAsync();
            }, new RetryOptions { Delay = Delays.Exponential(new Seconds(1), max: new Seconds(300)) });

            existingGoogleEventsByIcalUid = googleCalendarEvents.Items.ToDictionary(googleEvent => googleEvent.ICalUID);
            logger.LogDebug("Found {count:N0} existing events in Google Calendar", existingGoogleEventsByIcalUid.Values.Count);

            calendarPoller.calendarChanged += sync;
        }

        await calendarPoller.pollCalendar();
    }

    private async void sync(object? sender, Calendar newCalendar) => await sync(newCalendar);

    private async Task sync(Calendar newCalendar) {
        try {
            await gcalClientLock.WaitAsync();
            try {
                string googleCalendarId = configuration.Value.googleCalendarId!;

                IEnumerable<Event> eventsToDelete = existingGoogleEventsByIcalUid.ExceptBy(newCalendar.Events.Select(icsEvent => icsEvent.Uid), gcalEvent => gcalEvent.Key).Select(pair => pair.Value)
                    .ToList();
                IEnumerable<CalendarEvent> eventsToCreate = newCalendar.Events.ExceptBy(existingGoogleEventsByIcalUid.Keys, icsEvent => icsEvent.Uid).ToList();
                IEnumerable<CalendarEvent> eventsToUpdate = newCalendar.Events.Except(eventsToCreate).Where(icsEvent => {
                    Event googleEvent = existingGoogleEventsByIcalUid[icsEvent.Uid!];
                    return icsEvent.Summary != googleEvent.Summary ||
                        !icsEvent.Start!.ToInstant().Equals(googleEvent.Start.DateTimeDateTimeOffset?.ToInstant()) ||
                        !icsEvent.Start!.Add(icsEvent.Duration!.Value).ToInstant().Equals(googleEvent.End.DateTimeDateTimeOffset?.ToInstant()) ||
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
                    Event googleEventToCreate = eventToCreate.toGoogleEvent();
                    try {
                        existingGoogleEventsByIcalUid[googleEventToCreate.ICalUID] = await calendarService!.Events.Insert(googleEventToCreate, googleCalendarId).ExecuteAsync();
                        logger.LogTrace("Created event {summary} in Google Calendar", eventToCreate.Summary);
                    } catch (GoogleApiException e) {
                        logger.LogError(e, "Failed to create event {summary} in Google Calendar (iCalUID={uid})", googleEventToCreate.Summary, googleEventToCreate.ICalUID);
                        throw;
                    }
                }

                logger.LogDebug("Updating {count:N0} outdated events in Google Calendar", eventsToUpdate.Count());
                foreach (CalendarEvent eventToUpdate in eventsToUpdate) {
                    existingGoogleEventsByIcalUid[eventToUpdate.Uid!] =
                        await calendarService!.Events.Update(eventToUpdate.toGoogleEvent(), googleCalendarId, existingGoogleEventsByIcalUid[eventToUpdate.Uid!].Id).ExecuteAsync();
                    logger.LogTrace("Updated event {summary} in Google Calendar", eventToUpdate.Summary);
                }
            } finally {
                gcalClientLock.Release();
            }
        } catch (GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.Conflict) {
            ulong oldCounter = state.googleCalendarUidCounter;
            ulong newCounter = ++state.googleCalendarUidCounter;
            await state.save("state.json");
            logger.LogWarning(e,
                "Google Calendar failed to delete an old event and had a conflict when we later tried to recreate it with the same iCal UID, generating new UIDs with counter {new} instead of {old}",
                newCounter, oldCounter);
            await calendarPoller.pollCalendar();
        }
    }

    public void Dispose() {
        calendarPoller.calendarChanged -= sync;
        calendarService?.Dispose();
        GC.SuppressFinalize(this);
    }

}