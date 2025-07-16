namespace GamesDoneQuickCalendarFactory.Data.Marshal;

public class ResponseEnvelope<T> {

    public long count { get; init; }
    public Uri? next { get; init; }
    public Uri? previous { get; init; }
    public required IEnumerable<T> results { get; init; }

}