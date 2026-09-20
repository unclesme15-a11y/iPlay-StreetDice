using System.Text.Json.Serialization;

namespace IPlayStreetDice.Server.Core;

public sealed class StreetDicePlayer
{
    public StreetDicePlayer(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; }
    public DiceColor DiceColor { get; private set; } = DiceColor.Black;
    [JsonIgnore]
    public int Balance { get; private set; } = 1_000;
    public bool HasLeft { get; internal set; }

    public void SelectDiceColor(DiceColor color)
    {
        DiceColor = color;
    }

    public void Credit(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Balance += amount;
    }

    public void Debit(int amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        Balance -= amount;
    }
}
