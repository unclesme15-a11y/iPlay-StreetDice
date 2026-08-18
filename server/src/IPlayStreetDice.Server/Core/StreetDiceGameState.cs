namespace IPlayStreetDice.Server.Core;

public sealed class StreetDiceGameState
{
    public string GameId { get; init; } = "";
    public GamePhase Phase { get; set; } = GamePhase.Lobby;
    public List<StreetDicePlayer> Players { get; } = new();
    public List<SideBet> SideBets { get; } = new();
    public string? ShooterId { get; set; }
    public string? CatcherId { get; set; }
    public int ShotAmount { get; set; }
    public int? Point { get; set; }
    public int FadeCount { get; set; }
    public int ShooterMomentum { get; set; }
    public int Streak { get; set; }
    public int HotDiceThreshold { get; init; } = 5;
    public bool HotDiceActive => Streak >= HotDiceThreshold;
    public bool LastResolvedShotWasWin { get; set; }
    public bool LastShotWasDoubleUp { get; set; }
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
