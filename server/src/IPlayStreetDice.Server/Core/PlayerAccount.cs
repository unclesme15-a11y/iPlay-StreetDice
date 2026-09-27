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
    public int Level => RankLadder.LevelForWins(Wins);

    /// <summary>Prestige notes ($50/$100) this account has "hustled" as a
    /// flex -- won off a higher-ranked player somehow -- but can't actually
    /// use at the table unless the account itself is Level 3+ or is playing
    /// under a Level 3+ host. Nothing in this session yet decides how a
    /// note lands in here; this is just the holding data other systems can
    /// populate once that's designed.</summary>
    public HashSet<int> HustledPrestigeNotes { get; } = new();
}
