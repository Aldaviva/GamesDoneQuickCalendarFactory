using System.Drawing;

namespace GamesDoneQuickCalendarFactory.Data.Shields;

[JsonConverter(typeof(ShieldsBadgeColorConverter))]
public readonly union ShieldsBadgeColor(ShieldsBadgeColorKeyword, Color, KnownColor, string);