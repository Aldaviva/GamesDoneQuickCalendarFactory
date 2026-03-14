namespace GamesDoneQuickCalendarFactory.Data;

/// <summary>
/// <see href="https://shields.io/badges/endpoint-badge"/>
/// </summary>
public sealed record ShieldsBadgeResponse(
    string label,
    string message,
    string? color = null,
    bool isError = false,
    string? logoSvg = null
) {

    public int schemaVersion { get; } = 1;

}