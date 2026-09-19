using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPlayStreetDice.Server.Core.Persistence;

/// <summary>Reads/writes the whole server's state to a single JSON file, so a restart doesn't wipe every table.</summary>
public static class StreetDiceStatePersistence
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Save(string filePath, PersistedStoreState state)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        // Write to a temp file first so a crash mid-write can't leave a corrupted state file behind.
        var tempPath = $"{filePath}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(state, Options));
        File.Move(tempPath, filePath, overwrite: true);
    }

    public static PersistedStoreState? Load(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = File.ReadAllText(filePath);
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<PersistedStoreState>(json, Options);
    }
}
