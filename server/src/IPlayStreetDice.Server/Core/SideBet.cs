namespace IPlayStreetDice.Server.Core;

public sealed class SideBet
{
    public SideBet(string id, string playerId, SideBetType type, int amount, PointNumberGroup? pointGroup = null)
    {
        Id = id;
        PlayerId = playerId;
        Type = type;
        Amount = amount;
        PointGroup = pointGroup;
    }

    public string Id { get; }
    public string PlayerId { get; }
    public SideBetType Type { get; }
    public int Amount { get; }
    public PointNumberGroup? PointGroup { get; }
    public SideBetStatus Status { get; private set; } = SideBetStatus.Open;

    /// <summary>Rehydrates a side bet from persisted state, including its resolved status.</summary>
    internal SideBet(string id, string playerId, SideBetType type, int amount, PointNumberGroup? pointGroup, SideBetStatus status)
        : this(id, playerId, type, amount, pointGroup)
    {
        Status = status;
    }

    public void Win() => Status = SideBetStatus.Won;
    public void Lose() => Status = SideBetStatus.Lost;
    public void Cancel() => Status = SideBetStatus.Cancelled;
}
