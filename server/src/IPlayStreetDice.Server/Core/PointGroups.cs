namespace IPlayStreetDice.Server.Core;

public static class PointGroups
{
    public static PointNumberGroup FromPointNumber(int number)
    {
        return number switch
        {
            4 or 10 => PointNumberGroup.FourTen,
            6 or 8 => PointNumberGroup.SixEight,
            5 or 9 => PointNumberGroup.FiveNine,
            _ => throw new ArgumentOutOfRangeException(nameof(number), "Grouped point bets only support 4/10, 6/8, and 5/9.")
        };
    }

    public static bool Contains(PointNumberGroup group, int number)
    {
        return group switch
        {
            PointNumberGroup.FourTen => number is 4 or 10,
            PointNumberGroup.SixEight => number is 6 or 8,
            PointNumberGroup.FiveNine => number is 5 or 9,
            _ => false
        };
    }
}
