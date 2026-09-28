using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class RankLadderTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(11_999, 1)]
    [InlineData(12_000, 2)]
    [InlineData(41_999, 2)]
    [InlineData(42_000, 3)]
    [InlineData(111_999, 3)]
    [InlineData(112_000, 4)]
    [InlineData(271_999, 4)]
    [InlineData(272_000, 5)]
    [InlineData(5_000_000, 5)]
    public void LevelForXp_FollowsTheLadder(int xp, int expectedLevel)
    {
        Assert.Equal(expectedLevel, RankLadder.LevelForXp(xp));
    }

    [Fact]
    public void EachLevel_CostsMoreThanTheOneBefore()
    {
        var thresholds = new[] { 0, 12_000, 42_000, 112_000, 272_000 };
        for (var i = 2; i < thresholds.Length; i++)
            Assert.True(thresholds[i] - thresholds[i - 1] > thresholds[i - 1] - thresholds[i - 2]);
        Assert.Equal(1, RankLadder.LevelForXp(thresholds[1] - 1));
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 250)]
    [InlineData(3, 500)]
    [InlineData(4, 750)]
    [InlineData(5, 1000)]
    public void MaxBetForLevel_MatchesTheUserSpecifiedFloorAndCeiling(int level, int expectedMaxBet)
    {
        Assert.Equal(expectedMaxBet, RankLadder.MaxBetForLevel(level));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    public void PrestigeBillsUnlock_AtLevel3(int level, bool expectedUnlocked)
    {
        Assert.Equal(expectedUnlocked, RankLadder.PrestigeBillsUnlockedAtLevel(level));
    }

    [Fact]
    public void XpUntilNextLevel_IsNullAtMaxLevel()
    {
        Assert.Null(RankLadder.XpUntilNextLevel(272_000));
        Assert.Equal(12_000, RankLadder.XpUntilNextLevel(0));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2_399, 1)]
    [InlineData(2_400, 2)]      // 20% through Level 1 -> step 2
    [InlineData(11_999, 5)]
    [InlineData(12_000, 1)]     // new level, back to step 1
    [InlineData(272_000, 5)]    // max level shows a full bar
    public void LevelStep_SplitsEachLevelIntoFiveSteps(int xp, int expectedStep)
    {
        Assert.Equal(expectedStep, RankLadder.LevelStep(xp));
    }

    [Theory]
    [InlineData(false, false, false, 0, 8)]     // just seated, watching
    [InlineData(true, false, false, 0, 16)]     // shooter/catcher who lost
    [InlineData(true, true, true, 20, 30)]      // won $20: 16 + 12 win + 2 money
    [InlineData(true, true, false, 20, 18)]     // same win, bonus used up vs this opponent
    [InlineData(true, true, true, 1000, 48)]    // money XP tops out at 20 per shot
    public void XpForShot_CombinesSeatRoleWinAndMoney(bool involved, bool won, bool bonus, int amountWon, int expected)
    {
        Assert.Equal(expected, RankLadder.XpForShot(involved, won, bonus, amountWon));
    }
}
