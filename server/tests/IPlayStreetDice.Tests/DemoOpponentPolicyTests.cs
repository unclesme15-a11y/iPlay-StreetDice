using IPlay.Demo;

namespace IPlayStreetDice.Tests;

public class DemoOpponentPolicyTests
{
    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 20)]
    [InlineData(2, 1)]
    [InlineData(3, 10)]
    public void OpponentsHaveDifferentPreferredAmounts(int seat, int expected)
        => Assert.Equal(expected, DemoOpponentPolicy.OfferAmount(seat, 1000, 0.5));

    [Fact]
    public void OffersNeverExceedUnreservedFunds()
    {
        for (int seat = 0; seat < 4; seat++)
        for (int funds = 0; funds < 50; funds++)
        for (int sample = 0; sample < 100; sample++)
        {
            int amount = DemoOpponentPolicy.OfferAmount(seat, funds, sample / 100.0);
            Assert.InRange(amount, 0, funds);
            Assert.Contains(amount, new[] { 0, 1, 5, 10, 20 });
        }
    }

    [Fact]
    public void OpponentsCanAcceptOrDeclineTheSameAffordableOffer()
    {
        for (int seat = 0; seat < 4; seat++)
        {
            Assert.True(DemoOpponentPolicy.Accept(seat, 1000, 10, 0));
            Assert.False(DemoOpponentPolicy.Accept(seat, 1000, 10, 0.999));
            Assert.False(DemoOpponentPolicy.Accept(seat, 9, 10, 0));
            Assert.False(DemoOpponentPolicy.Accept(seat, 1000, 3, 0));
        }
    }

    [Fact]
    public void DeclineFrequencyIncreasesWithBankrollPressure()
    {
        int comfortable = 0, pressured = 0;
        for (int sample = 0; sample < 1000; sample++)
        {
            if (DemoOpponentPolicy.Accept(1, 1000, 20, sample / 1000.0)) comfortable++;
            if (DemoOpponentPolicy.Accept(1, 20, 20, sample / 1000.0)) pressured++;
        }
        Assert.True(comfortable > pressured * 2);
        Assert.InRange(comfortable, 700, 900);
    }

    [Fact]
    public void ResponseTimingIsVariedAndFitsThePublicWindow()
    {
        for (int seat = 0; seat < 4; seat++)
        {
            float first = DemoOpponentPolicy.ResponseDelay(seat, 0);
            float last = DemoOpponentPolicy.ResponseDelay(seat, 0.999);
            Assert.InRange(first, 0.5f, 1.1f);
            Assert.InRange(last, 2f, 2.7f);
            Assert.True(last > first + 1f);
        }
    }

    [Fact]
    public void ShootingIsOptionalButDoubleUpNeedsRoomInTheBankroll()
    {
        for (int seat = 0; seat < 4; seat++)
        {
            Assert.True(DemoOpponentPolicy.Pass(seat, 9, 10, 0.99));
            Assert.True(DemoOpponentPolicy.Pass(seat, 1000, 10, 0));
            Assert.False(DemoOpponentPolicy.Pass(seat, 1000, 10, 0.99));
            Assert.False(DemoOpponentPolicy.DoubleUp(seat, 39, 10, 0));
            Assert.True(DemoOpponentPolicy.DoubleUp(seat, 1000, 10, 0));
            Assert.False(DemoOpponentPolicy.DoubleUp(seat, int.MaxValue, int.MaxValue, 0));
        }
    }

    [Fact]
    public void InvalidProfilesAndRandomSamplesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DemoOpponentPolicy.Accept(4, 1000, 1, 0.5));
        Assert.Throws<ArgumentOutOfRangeException>(() => DemoOpponentPolicy.OfferAmount(0, 1000, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => DemoOpponentPolicy.Pass(0, 1000, 1, 1));
    }
}
