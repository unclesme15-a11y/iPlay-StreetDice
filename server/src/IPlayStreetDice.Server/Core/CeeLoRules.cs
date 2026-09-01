namespace IPlayStreetDice.Server.Core;

public static class CeeLoRules
{
    public static CeeLoResolution Evaluate(CeeLoRoll roll)
    {
        if (!roll.IsValid) throw new ArgumentOutOfRangeException(nameof(roll), "Dice must be in the 1-6 range.");

        var values = roll.Sorted();
        if (values is [4, 5, 6])
        {
            return new CeeLoResolution(CeeLoOutcomeType.AutomaticWin, roll, null, 10_000, "Cee-lo 4-5-6. Automatic win.");
        }

        if (values is [1, 2, 3])
        {
            return new CeeLoResolution(CeeLoOutcomeType.AutomaticLoss, roll, null, -10_000, "1-2-3. Automatic loss.");
        }

        if (values[0] == values[1] && values[1] == values[2])
        {
            return new CeeLoResolution(CeeLoOutcomeType.AutomaticWin, roll, null, 9_000 + values[0], $"Trips {values[0]}. Automatic win.");
        }

        var point = PairAndPoint(values);
        if (point == null)
        {
            return new CeeLoResolution(CeeLoOutcomeType.Reroll, roll, null, 0, "No count. Roll again.");
        }

        return point.Value switch
        {
            6 => new CeeLoResolution(CeeLoOutcomeType.AutomaticWin, roll, 6, 8_006, "Pair plus 6. Automatic win."),
            1 => new CeeLoResolution(CeeLoOutcomeType.AutomaticLoss, roll, 1, -8_001, "Pair plus 1. Automatic loss."),
            _ => new CeeLoResolution(CeeLoOutcomeType.Point, roll, point.Value, point.Value, $"Point {point.Value}.")
        };
    }

    public static int Compare(CeeLoResolution challenger, CeeLoResolution banker)
    {
        return challenger.Rank.CompareTo(banker.Rank);
    }

    private static int? PairAndPoint(IReadOnlyList<int> values)
    {
        if (values[0] == values[1]) return values[2];
        if (values[1] == values[2]) return values[0];
        return null;
    }
}
