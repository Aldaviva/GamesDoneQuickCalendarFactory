namespace GamesDoneQuickCalendarFactory;

public abstract class CalendarFactoryException(string? message = null, Exception? cause = null): ApplicationException(message, cause) {

    /// <summary>An error occurred while fetching the event ID, besides there not being a current event.</summary>
    public sealed class CurrentEventFetchError(string? message = null, Exception? cause = null): CalendarFactoryException(message, cause);

}