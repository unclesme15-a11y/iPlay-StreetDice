namespace IPlayStreetDice.Server.Core;

/// <summary>
/// The rank ladder, modeled on a "junior" NBA 2K rep bar: a real grind, no daily cap, each
/// level costing more than the last, and five progress steps inside every level so there's
/// always a step coming. The owner set the level 1/3/5 bet caps (100 / 500 / 1000) and the
/// pacing targets; every other number here is a tunable estimate until real playtests
/// measure how many shots a party actually plays per hour.
/// </summary>
public static class RankLadder
{
    public const int MaxLevel = 5;
    public const int PrestigeBillUnlockLevel = 3;
    public const int StepsPerLevel = 5;

    private static readonly int[] MaxBetByLevel = { 100, 250, 500, 750, 1000 };

    // XP per finished shot. Everyone seated earns, so a full 5-player party levels as fast
    // as a 1-on-1 -- the party is the point of the game.
    public const int XpSeatedPerShot = 8;
    public const int XpShooterOrCatcherPerShot = 8;   // on top of the seated XP
    public const int XpPerWin = 12;
    public const int WinBonusesPerOpponentPerDay = 3;  // stops two friends trading wins
    public const int DollarsPerMoneyXp = 10;
    public const int MaxMoneyXpPerShot = 20;

    // Daily bonus instead of a cap: the first shots each day are worth more, so someone
    // who plays 30 minutes daily keeps pace (roughly half a grinder's speed) while a
    // grinder is never stopped.
    public const int DailyBonusShots = 15;
    public const int DailyBonusMultiplier = 5;

    // Total XP to REACH each level (index 0 = level 1). Estimated pacing, grinder ~3 hours a
    // day vs casual ~30 minutes: L2 ~5 vs ~10 days, L3 ~2 weeks vs ~5 weeks, L4 ~6 weeks vs
    // ~3 months, L5 ~3 months vs ~7 months.
    private static readonly int[] XpToReachLevel = { 0, 12_000, 42_000, 112_000, 272_000 };

    // Rank names (owner 2026-09-29). Level 5 shows as "DICE G[die]D" with a turning die as the
    // O -- the client draws that; this is the plain-text name. Players see progress as SP
    // ("shooter points"); the code keeps the older Xp names internally.
    private static readonly string[] LevelNames = { "Unranked", "Shooter", "Skilled Shooter", "Pro Shooter", "Dice God" };

    public static string LevelName(int level) => LevelNames[ClampLevel(level) - 1];

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

    /// <summary>How far through the current level, 0 to 1 (always 1 at MaxLevel) -- drives
    /// the progress bar.</summary>
    public static float LevelProgress(int xp)
    {
        var level = LevelForXp(xp);
        if (level >= MaxLevel) return 1f;
        var start = XpToReachLevel[level - 1];
        return (float)(xp - start) / (XpToReachLevel[level] - start);
    }

    /// <summary>Which of the StepsPerLevel steps inside the current level, 1 to 5.</summary>
    public static int LevelStep(int xp) =>
        LevelForXp(xp) >= MaxLevel ? StepsPerLevel : Math.Min(StepsPerLevel, 1 + (int)(LevelProgress(xp) * StepsPerLevel));

    /// <summary>XP one finished shot is worth to one seated player, before the daily bonus.
    /// winBonusAvailable is false once they've collected the win bonus against this opponent
    /// WinBonusesPerOpponentPerDay times today.</summary>
    public static int XpForShot(bool shooterOrCatcher, bool won, bool winBonusAvailable, int amountWon)
    {
        var xp = XpSeatedPerShot;
        if (shooterOrCatcher) xp += XpShooterOrCatcherPerShot;
        if (!won) return xp;
        if (winBonusAvailable) xp += XpPerWin;
        return xp + Math.Min(MaxMoneyXpPerShot, Math.Max(0, amountWon) / DollarsPerMoneyXp);
    }

    private static int ClampLevel(int level) => Math.Clamp(level, 1, MaxLevel);
}
