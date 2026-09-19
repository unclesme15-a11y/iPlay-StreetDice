namespace IPlayStreetDice.Server.Core;

public sealed class StreetDicePlayer
{
    public StreetDicePlayer(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Rehydrates a player from persisted state. Bypasses the normal starting balance.</summary>
    internal StreetDicePlayer(string id, string name, DiceColor diceColor, int balance)
        : this(id, name)
    {
        DiceColor = diceColor;
        Balance = balance;
    }

    public string Id { get; }
    public string Name { get; }
    public DiceColor DiceColor { get; private set; } = DiceColor.Black;
    public int Balance { get; private set; } = 1_000;

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
