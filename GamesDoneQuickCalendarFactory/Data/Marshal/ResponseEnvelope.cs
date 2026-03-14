namespace GamesDoneQuickCalendarFactory.Data.Marshal;

public sealed class ResponseEnvelope<T> {

    public long count { get; init; }
    public Uri? next { get; init; }
    public Uri? previous { get; init; }
    public required IReadOnlyList<T> results { get; init; }

}