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

    // Daily bonus and win-trading guard (see RankLadder). Reset whenever a shot lands on a
    // new UTC day. DailyShots counts every finished shot seated, for the daily bonus.
    public int DailyShots { get; set; }
    public DateOnly? DailyDay { get; set; }
    public Dictionary<string, int> DailyWinBonusesByOpponent { get; } = new();

    /// <summary>$50/$100 notes won as trophies: a player below Level 3 who wins a shot of $50
    /// or more against a Level 3+ opponent keeps one (see StreetDiceTableStore.AwardTrophyNote
    /// in Program.cs). A bragging-rights collectible -- it only shows up at a party whose host
    /// is Level 3+, same as everyone else's $50/$100 access there.</summary>
    public HashSet<int> TrophyNotes { get; } = new();
}
