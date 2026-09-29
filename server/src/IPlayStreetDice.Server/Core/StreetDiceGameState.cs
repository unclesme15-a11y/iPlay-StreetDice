namespace IPlayStreetDice.Server.Core;

public sealed class StreetDiceGameState
{
    public string GameId { get; init; } = "";
    public GamePhase Phase { get; set; } = GamePhase.Lobby;
    public List<StreetDicePlayer> Players { get; } = new();
    public List<SideBet> SideBets { get; } = new();
    public string? ShooterId { get; set; }
    public string? CatcherId { get; set; }
    // The first real player to join a table becomes its host (same pattern as
    // ShooterId's default -- see StreetDiceTableStore.JoinRealPlayer). Nothing
    // else in this game had a "host" concept before the music controls below.
    public string? HostId { get; set; }
    // Set once the original host leaves: the party keeps that host's level for the rest of
    // the party, even though the host role itself passes to someone else. Null while the
    // original host is still seated (their live level applies).
    public int? LockedTableLevel { get; set; }
    // Host-controlled "now playing" state, piggybacking the same real-time
    // poll every client already uses for dice/wager sync (GET
    // /api/street-dice/{gameId}) -- no separate sync channel needed. This is
    // metadata only (which track, playhead position, playing/paused): the
    // actual audio plays locally on each guest's own device, through their
    // own Spotify app. The server never touches audio.
    public string? MusicTrackUri { get; set; }
    public double MusicPositionMilliseconds { get; set; }
    public bool MusicIsPlaying { get; set; }
    public long MusicUpdatedAtUnixMilliseconds { get; set; }
    public int ShotAmount { get; set; }
    public int? Point { get; set; }
    public int FadeCount { get; set; }
    public int ShooterMomentum { get; set; }
    public float Streak { get; set; }
    public int HotDiceThreshold { get; init; } = 10;
    public bool HotDiceActive => Streak >= HotDiceThreshold;
    public bool LastResolvedShotWasWin { get; set; }
    public bool LastShotWasDoubleUp { get; set; }
    // Main-bet Double Up during the point: who proposed it (waiting on the other side), and
    // whether this shot already doubled.
    public string? MainDoubleUpProposedBy { get; set; }
    public bool MainDoubleUpTaken { get; set; }
    public RollResolution LastResolution { get; set; } = new(RollResultType.None, null, null, "No rolls yet.");
    public List<string> EventLog { get; } = new();

    public StreetDicePlayer? Shooter => FindPlayer(ShooterId);
    public StreetDicePlayer? Catcher => FindPlayer(CatcherId);

    public StreetDicePlayer? FindPlayer(string? playerId)
    {
        return string.IsNullOrWhiteSpace(playerId)
            ? null
            : Players.FirstOrDefault(p => string.Equals(p.Id, playerId, StringComparison.OrdinalIgnoreCase));
    }

    public void Log(string message)
    {
        EventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
    }
}
