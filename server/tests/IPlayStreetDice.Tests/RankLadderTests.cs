using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class RankLadderTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(199, 1)]
    [InlineData(200, 2)]
    [InlineData(999, 2)]
    [InlineData(1000, 3)]
    [InlineData(2499, 3)]
    [InlineData(2500, 4)]
    [InlineData(4999, 4)]
    [InlineData(5000, 5)]
    [InlineData(50000, 5)]
    public void LevelForXp_FollowsTheLadder(int xp, int expectedLevel)
    {
        Assert.Equal(expectedLevel, RankLadder.LevelForXp(xp));
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
        Assert.Null(RankLadder.XpUntilNextLevel(5000));
        Assert.Equal(200, RankLadder.XpUntilNextLevel(0));
    }

    [Theory]
    [InlineData(false, true, 0, 10)]     // lost: just the shot played
    [InlineData(true, true, 20, 27)]     // won $20: 10 played + 15 win + 2 money
    [InlineData(true, false, 20, 12)]    // same win, but the win bonus is used up vs this opponent
    [InlineData(true, true, 1000, 75)]   // money XP tops out at 50 per shot
    public void XpForShot_CombinesPlayingWinningAndMoneyWon(bool won, bool bonus, int amountWon, int expected)
    {
        Assert.Equal(expected, RankLadder.XpForShot(won, bonus, amountWon));
    }
}
