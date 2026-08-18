namespace IPlayStreetDice.Server.Core;

public readonly record struct DiceRoll(int Die1, int Die2)
{
    public int Total => Die1 + Die2;

    public bool IsValid => Die1 is >= 1 and <= 6 && Die2 is >= 1 and <= 6;
}
