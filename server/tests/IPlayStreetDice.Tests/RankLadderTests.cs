using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class RankLadderTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 2)]
    [InlineData(14, 2)]
    [InlineData(15, 3)]
    [InlineData(29, 3)]
    [InlineData(30, 4)]
    [InlineData(49, 4)]
    [InlineData(50, 5)]
    [InlineData(500, 5)]
    public void LevelForWins_FollowsThePlaceholderCurve(int wins, int expectedLevel)
    {
        Assert.Equal(expectedLevel, RankLadder.LevelForWins(wins));
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
    public void WinsUntilNextLevel_IsNullAtMaxLevel()
    {
        Assert.Null(RankLadder.WinsUntilNextLevel(50));
        Assert.Equal(5, RankLadder.WinsUntilNextLevel(0));
    }
}
