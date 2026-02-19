using System.Text.Json;

namespace GamesDoneQuickCalendarFactory.Data;

public record State {

    public ulong googleCalendarUidCounter { get; set; }

    private string? filename { get; set; }

    public static async Task<State> load(string filename) {
        State? loaded = null;
        try {
            await using Stream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            loaded = await JsonSerializer.DeserializeAsync<State>(fileStream);
        } catch (Exception e) when (e is not OutOfMemoryException) {}
        State result = loaded ?? new State();
        result.filename = filename;
        return result;
    }

    public async Task save() {
        if (filename != null) {
            await using Stream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fileStream, this);
        }
    }

}