using Unfucked;
using Unfucked.DateTime;

namespace GamesDoneQuickCalendarFactory.Data;

public class Configuration {

    public TimeSpan cacheDuration { get; init; } = (Minutes) 1;

    public string? googleServiceAccountEmailAddress {
        get;
        init => field = value.EmptyToNull(); // .NET Hosting configuration unfortunately interprets null JSON values as "", even if the destination type is nullable
    }

    public string? googleCalendarId {
        get;
        init => field = value.EmptyToNull();
    }

    public string? googleServiceAccountPrivateKey {
        get;
        init => field = value.EmptyToNull();
    }

}