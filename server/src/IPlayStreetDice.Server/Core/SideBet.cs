namespace IPlayStreetDice.Server.Core;

public sealed class SideBet
{
    public SideBet(string id, string playerId, SideBetType type, int amount)
    {
        Id = id;
        PlayerId = playerId;
        Type = type;
        Amount = amount;
    }

    public string Id { get; }
    public string PlayerId { get; }
    public SideBetType Type { get; }
    public int Amount { get; }
    public SideBetStatus Status { get; private set; } = SideBetStatus.Open;

    public void Win() => Status = SideBetStatus.Won;
    public void Lose() => Status = SideBetStatus.Lost;
    public void Cancel() => Status = SideBetStatus.Cancelled;
}
