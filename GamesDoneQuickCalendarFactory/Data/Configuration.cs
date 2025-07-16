using Unfucked;

namespace GamesDoneQuickCalendarFactory.Data;

public class Configuration {

    public TimeSpan cacheDuration { get; init; } = TimeSpan.FromMinutes(1);

    private readonly string? _googleServiceAccountEmailAddress;
    private readonly string? _googleCalendarId;
    private readonly string? _googleServiceAccountPrivateKey;

    public string? googleServiceAccountEmailAddress {
        get => _googleServiceAccountEmailAddress;
        init => _googleServiceAccountEmailAddress = value.EmptyToNull(); // .NET Hosting configuration unfortunately interprets null JSON values as "", even if the destination type is nullable
    }

    public string? googleCalendarId {
        get => _googleCalendarId;
        init => _googleCalendarId = value.EmptyToNull();
    }

    public string? googleServiceAccountPrivateKey {
        get => _googleServiceAccountPrivateKey;
        init => _googleServiceAccountPrivateKey = value.EmptyToNull();
    }

}