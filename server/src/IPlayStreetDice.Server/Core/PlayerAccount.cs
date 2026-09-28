namespace IPlayStreetDice.Server.Core;

/// <summary>
/// A persistent player profile -- separate from a StreetDicePlayer, which
/// only exists for the lifetime of one table. This is what makes rank
/// follow you across games rather than resetting every time you join a
/// table (the user explicitly chose real accounts over a device-local-only
/// rank).
/// </summary>
public sealed class PlayerAccount
{
    public PlayerAccount(string id, string username, string passwordHash, string passwordSalt)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public string Id { get; }
    public string Username { get; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public int Wins { get; set; }
    public int ShotsPlayed { get; set; }
    public int Xp { get; set; }
    public int Level => RankLadder.LevelForXp(Xp);

    // Daily farming guards (see RankLadder). Reset whenever a shot lands on a new UTC day.
    public int DailyXp { get; set; }
    public DateOnly? DailyXpDay { get; set; }
    public Dictionary<string, int> DailyWinBonusesByOpponent { get; } = new();

    /// <summary>Prestige notes ($50/$100) this account has "hustled" as a
    /// flex -- won off a higher-ranked player somehow -- but only ever
    /// actually show up at a table whose host is Level 3+ (see
    /// StreetDiceTableStore.PrestigeNoteUsableBy in Program.cs), same as
    /// everyone else's $50/$100 access there. Nothing in this session yet
    /// decides how a note lands in here; this is just the holding data
    /// other systems can populate once that's designed.</summary>
    public HashSet<int> HustledPrestigeNotes { get; } = new();
}
