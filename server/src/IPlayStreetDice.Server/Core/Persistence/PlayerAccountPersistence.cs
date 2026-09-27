using System.Text.Json;

namespace IPlayStreetDice.Server.Core.Persistence;

/// <summary>Same write-to-temp-then-move pattern as StreetDiceStatePersistence, kept in its own
/// file so a table-state corruption/format change can never take player accounts down with it.</summary>
public static class PlayerAccountPersistence
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Save(string filePath, IReadOnlyCollection<PersistedAccount> accounts)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var tempPath = $"{filePath}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(accounts, Options));
        File.Move(tempPath, filePath, overwrite: true);
    }

    public static List<PersistedAccount>? Load(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = File.ReadAllText(filePath);
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<List<PersistedAccount>>(json, Options);
    }
}
