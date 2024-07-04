namespace GamesDoneQuickCalendarFactory.Data;

public class Configuration {

    public TimeSpan cacheDuration { get; init; } = TimeSpan.FromMinutes(1);

    public string? googleServiceAccountEmailAddress { get; init; } = null;
    public string? googleCalendarId { get; init; } = null;
    public string? googleServiceAccountPrivateKey { get; init; } = null;

}