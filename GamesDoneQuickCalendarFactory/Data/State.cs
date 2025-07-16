using System.Text.Json;

namespace GamesDoneQuickCalendarFactory.Data;

public record State {

    public ulong googleCalendarUidCounter { get; set; } = 5;

    public static async Task<State> load(string filename) {
        State? loaded = null;
        try {
            await using Stream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            loaded = await JsonSerializer.DeserializeAsync<State>(fileStream);
        } catch (Exception e) when (e is not OutOfMemoryException) { }
        return loaded ?? new State();
    }

    public async Task save(string filename) {
        await using Stream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(fileStream, this);
    }

}