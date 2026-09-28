namespace IPlayStreetDice.Server.Core;

/// <summary>
/// The "2k park"-style rank ladder: 5 levels, each raising the table's max bet and, from
/// level 3 on, unlocking the $50/$100 notes. The owner set the level 1/3/5 caps
/// (100 / 500 / 1000); level 2/4 caps and every XP number below are tunable starting
/// points, all kept in this one file.
/// </summary>
public static class RankLadder
{
    public const int MaxLevel = 5;
    public const int PrestigeBillUnlockLevel = 3;

    private static readonly int[] MaxBetByLevel = { 100, 250, 500, 750, 1000 };

    // XP is a combination of playing, winning and money won (owner's call), with guards
    // against two friends farming each other: the win bonus only counts a few times per
    // opponent per day, and total XP per day is capped.
    public const int XpPerShotPlayed = 10;
    public const int XpPerWin = 15;
    public const int WinBonusesPerOpponentPerDay = 3;
    public const int DollarsPerMoneyXp = 10;
    public const int MaxMoneyXpPerShot = 50;
    public const int DailyXpCap = 500;

    // XP needed to REACH each level (index 0 = level 1). At the daily cap, level 5 takes
    // at least 10 days.
    private static readonly int[] XpToReachLevel = { 0, 200, 1000, 2500, 5000 };

    public static int MaxBetForLevel(int level) => MaxBetByLevel[ClampLevel(level) - 1];

    public static bool PrestigeBillsUnlockedAtLevel(int level) => ClampLevel(level) >= PrestigeBillUnlockLevel;

    public static int LevelForXp(int xp)
    {
        var level = 1;
        for (var i = 1; i < XpToReachLevel.Length; i++)
        {
            if (xp < XpToReachLevel[i]) break;
            level = i + 1;
        }
        return level;
    }

    /// <summary>XP still needed to hit the next level, or null if already at MaxLevel.</summary>
    public static int? XpUntilNextLevel(int xp)
    {
        var level = LevelForXp(xp);
        if (level >= MaxLevel) return null;
        return XpToReachLevel[level] - xp;
    }

    /// <summary>XP one finished shot is worth before the daily cap. winBonusAvailable is false
    /// once this player has already collected the win bonus against this opponent
    /// WinBonusesPerOpponentPerDay times today.</summary>
    public static int XpForShot(bool won, bool winBonusAvailable, int amountWon)
    {
        var xp = XpPerShotPlayed;
        if (!won) return xp;
        if (winBonusAvailable) xp += XpPerWin;
        return xp + Math.Min(MaxMoneyXpPerShot, Math.Max(0, amountWon) / DollarsPerMoneyXp);
    }

    private static int ClampLevel(int level) => Math.Clamp(level, 1, MaxLevel);
}
