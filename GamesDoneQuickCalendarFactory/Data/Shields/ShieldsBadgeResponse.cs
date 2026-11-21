namespace GamesDoneQuickCalendarFactory.Data.Shields;

/// <summary>
/// <para><see href="https://shields.io/badges/endpoint-badge"/></para>
/// <para><see href="https://github.com/badges/shields/blob/master/badge-maker/README.md#colors"/></para>
/// <para><see href="https://github.com/badges/shields/blob/master/badge-maker/lib/color.js"/></para>
/// </summary>
public sealed record ShieldsBadgeResponse(
    string label,
    string message,
    ShieldsBadgeColor? color = null,
    bool isError = false,
    string? logoSvg = null
) {

    public int schemaVersion { get; } = 1;

}