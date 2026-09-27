namespace IPlayStreetDice.Server.Core;

/// <summary>
/// The "2k park"-style rank ladder: 5 levels, each raising the table's max
/// bet and, from level 3 on, unlocking the $50/$100 note art. Wins-per-level
/// and the level 2/4 bet caps are placeholder numbers -- the user gave 1
/// (start), 3, and 5 (100 / 500 / 1000) and asked for a "smooth ramp" in
/// between; these are the easiest values to retune later, all in one place.
/// </summary>
public static class RankLadder
{
    public const int MaxLevel = 5;
    public const int PrestigeBillUnlockLevel = 3;

    private static readonly int[] MaxBetByLevel = { 100, 250, 500, 750, 1000 };

    // Wins needed to REACH each level (index 0 = level 1, always 0).
    // Placeholder curve -- nothing in this conversation specified how XP is
    // earned, so this uses total shot wins as the simplest available metric.
    private static readonly int[] WinsToReachLevel = { 0, 5, 15, 30, 50 };

    public static int MaxBetForLevel(int level) => MaxBetByLevel[ClampLevel(level) - 1];

    public static bool PrestigeBillsUnlockedAtLevel(int level) => ClampLevel(level) >= PrestigeBillUnlockLevel;

    public static int LevelForWins(int wins)
    {
        var level = 1;
        for (var i = 1; i < WinsToReachLevel.Length; i++)
        {
            if (wins < WinsToReachLevel[i]) break;
            level = i + 1;
        }
        return level;
    }

    /// <summary>Wins still needed to hit the next level, or null if already at MaxLevel.</summary>
    public static int? WinsUntilNextLevel(int wins)
    {
        var level = LevelForWins(wins);
        if (level >= MaxLevel) return null;
        return WinsToReachLevel[level] - wins;
    }

    private static int ClampLevel(int level) => Math.Clamp(level, 1, MaxLevel);
}
