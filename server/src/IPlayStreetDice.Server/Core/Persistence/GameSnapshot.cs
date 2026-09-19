namespace IPlayStreetDice.Server.Core.Persistence;

/// <summary>Plain, freely-serializable mirror of <see cref="StreetDiceGameState"/> for persistence.</summary>
public sealed class GameSnapshot
{
    public string GameId { get; set; } = "";
    public GamePhase Phase { get; set; }
    public List<PlayerSnapshot> Players { get; set; } = new();
    public List<SideBetSnapshot> SideBets { get; set; } = new();
    public string? ShooterId { get; set; }
    public string? CatcherId { get; set; }
    public int ShotAmount { get; set; }
    public int? Point { get; set; }
    public int FadeCount { get; set; }
    public int ShooterMomentum { get; set; }
    public int Streak { get; set; }
    public int HotDiceThreshold { get; set; } = 5;
    public bool LastResolvedShotWasWin { get; set; }
    public bool LastShotWasDoubleUp { get; set; }
    public RollResolution LastResolution { get; set; } = new(RollResultType.None, null, null, "No rolls yet.");
    public List<string> EventLog { get; set; } = new();
}

public sealed class PlayerSnapshot
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public DiceColor DiceColor { get; set; }
    public int Balance { get; set; }
}

public sealed class SideBetSnapshot
{
    public string Id { get; set; } = "";
    public string PlayerId { get; set; } = "";
    public SideBetType Type { get; set; }
    public int Amount { get; set; }
    public PointNumberGroup? PointGroup { get; set; }
    public SideBetStatus Status { get; set; }
}

/// <summary>Everything a running server needs to restore itself: all tables plus active player sessions.</summary>
public sealed class PersistedStoreState
{
    public List<GameSnapshot> Games { get; set; } = new();
    public Dictionary<string, string> PlayerSessions { get; set; } = new();
}
