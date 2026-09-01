namespace IPlayStreetDice.Server.Core;

public readonly record struct CeeLoRoll(int Die1, int Die2, int Die3)
{
    public bool IsValid => Die1 is >= 1 and <= 6 && Die2 is >= 1 and <= 6 && Die3 is >= 1 and <= 6;

    public int[] Sorted()
    {
        var values = new[] { Die1, Die2, Die3 };
        Array.Sort(values);
        return values;
    }
}
