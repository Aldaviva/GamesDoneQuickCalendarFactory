namespace GamesDoneQuickCalendarFactory.Data;

public class Configuration {

    public TimeSpan cacheDuration { get; init; } = TimeSpan.FromMinutes(3);

    public string? googleServiceAccountEmailAddress { get; init; }
    public string? googleCalendarId { get; init; }
    public string? googleServiceAccountPrivateKey { get; init; }

}