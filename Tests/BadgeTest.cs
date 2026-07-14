namespace Tests;

public class BadgeTest {

    [Theory, MemberData(nameof(badgeNameData))]
    public void badgeName(string eventShortName, string expected, string eventLongName) {
        Program.getBadgeName(eventShortName).Should().Be(expected, eventLongName);
    }

    /**
     * Generated with runs JSON array using the following Javascript function.
     * <code>
     * function query (data) {
     *     return ["short","name"].join("\t") + "\n" + _.chain(data.results)
     *         .map(r => _.pick(r, "short", "name"))
     *         .map(r => [r.short, r.name].join("\t"))
     *         .value()
     *         .join("\n");
     * }
     * </code>
     */
    public static TheoryData<string, string, string> badgeNameData => new() {
        { "SpeedAtPAXWest26", "speed at paxwest 26", "Speedrun Stage @ PAX West 2026" },
        { "gamescomgdq", "gamescomgdq", "gamescom GDQ" },
        { "SGDQ2026", "sgdq 2026", "Summer Games Done Quick 2026" },
        { "SpeedAtPAXEast26", "speed at paxeast 26", "Speedrun Stage @ PAX East 2026" },
        { "frostfatales2026", "frostfatales 2026", "Frost Fatales 2026" },
        { "BTB26", "btb 26", "Back to Black 2026" },
        { "AGDQ2026", "agdq 2026", "Awesome Games Done Quick 2026" },
        { "GDQueer", "gdqueer", "Games Done Queer 2025" },
        { "GDQX2025", "gdqx 2025", "Games Done Quick Express 2025" },
        { "flamefatales2025", "flamefatales 2025", "Flame Fatales 2025" },
        { "SpeedAtPAXWest25", "speed at paxwest 25", "Speedrun Stage @ PAX West 2025" },
        { "sgdq2025", "sgdq 2025", "Summer Games Done Quick 2025" },
        { "SpeedAtPAXEast25", "speed at paxeast 25", "Speedrun Stage @ PAX East 2025" },
        { "frostfatales2025", "frostfatales 2025", "Frost Fatales 2025" },
        { "BTB2025", "btb 2025", "Back to Black 2025" },
        { "AGDQ2025", "agdq 2025", "Awesome Games Done Quick 2025" },
        { "DRDQ2024", "drdq 2024", "Disaster Relief Done Quick 2024" },
        { "GDQX2024", "gdqx 2024", "Games Done Quick Express 2024" },
        { "SpeedAtPAXWest24", "speed at paxwest 24", "Speedrun Stage @ PAX West 24" },
        { "flamefatales2024", "flamefatales 2024", "Flame Fatales 2024" },
        { "SGDQ2024", "sgdq 2024", "Summer Games Done Quick 2024" },
        { "frostfatales2024", "frostfatales 2024", "Frost Fatales 2024" },
        { "AGDQ2024", "agdq 2024", "Awesome Games Done Quick 2024" },
        { "GDQX2023", "gdqx 2023", "Games Done Quick Express 2023" },
        { "flamefatales2023", "flamefatales 2023", "Flame Fatales 2023" },
        { "SGDQ2023", "sgdq 2023", "Summer Games Done Quick 2023" },
        { "FrostFatales2023", "frost fatales 2023", "Frost Fatales 2023" },
        { "AGDQ2023", "agdq 2023", "Awesome Games Done Quick 2023" },
        { "flamefatales2022", "flamefatales 2022", "Flame Fatales 2022" },
        { "SGDQ2022", "sgdq 2022", "Summer Games Done Quick 2022" },
        { "frostfatales2022", "frostfatales 2022", "Frost Fatales 2022" },
        { "AGDQ2022", "agdq 2022", "Awesome Games Done Quick 2022 Online" },
        { "flamefatales2021", "flamefatales 2021", "Flame Fatales 2021" },
        { "SGDQ2021", "sgdq 2021", "Summer Games Done Quick 2021 Online" },
        { "AGDQ2021", "agdq 2021", "Awesome Games Done Quick 2021 Online" },
        { "fleetfatales2020", "fleetfatales 2020", "Fleet Fatales 2020" },
        { "thpslaunch", "thpslaunch", "Tony Hawk's Pro Skater 1 + 2 Launch Celebration" },
        { "sgdq2020", "sgdq 2020", "Summer Games Done Quick 2020" },
        { "crdq", "crdq", "Corona Relief Done Quick" },
        { "frostfatales2020", "frostfatales 2020", "Frost Fatales 2020" },
        { "agdq2020", "agdq 2020", "Awesome Games Done Quick 2020" },
        { "GDQX2019", "gdqx 2019", "Games Done Quick Express 2019" },
        { "sgdq2019", "sgdq 2019", "Summer Games Done Quick 2019" },
        { "agdq2019", "agdq 2019", "Awesome Games Done Quick 2019" },
        { "GDQX2018", "gdqx 2018", "Games Done Quick Express 2018" },
        { "sgdq2018", "sgdq 2018", "Summer Games Done Quick 2018" },
        { "agdq2018", "agdq 2018", "Awesome Games Done Quick 2018" },
        { "hrdq", "hrdq", "Harvey Relief Done Quick" },
        { "sgdq2017", "sgdq 2017", "Summer Games Done Quick 2017" },
        { "agdq2017", "agdq 2017", "Awesome Games Done Quick 2017" },
        { "sgdq2016", "sgdq 2016", "Summer Games Done Quick 2016" },
        { "agdq2016", "agdq 2016", "Awesome Games Done Quick 2016" },
        { "sgdq2015", "sgdq 2015", "Summer Games Done Quick 2015" },
        { "agdq2015", "agdq 2015", "Awesome Games Done Quick 2015" },
        { "sgdq2014", "sgdq 2014", "Summer Games Done Quick 2014" },
        { "agdq2014", "agdq 2014", "Awesome Games Done Quick 2014" },
        { "sgdq2013", "sgdq 2013", "Summer Games Done Quick 2013" },
        { "agdq2013", "agdq 2013", "Awesome Games Done Quick 2013" },
        { "spook", "spook", "Speedrun Spooktacular" },
        { "sgdq2012", "sgdq 2012", "Summer Games Done Quick 2012" },
        { "agdq2012", "agdq 2012", "Awesome Games Done Quick 2012" },
        { "sgdq2011", "sgdq 2011", "Summer Games Done Quick 2011" },
        { "jrdq", "jrdq", "Japan Relief Done Quick" },
        { "agdq2011", "agdq 2011", "Awesome Games Done Quick 2011" },
        { "cgdq", "cgdq", "Classic Games Done Quick" }
    };

}