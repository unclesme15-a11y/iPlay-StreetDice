using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public sealed class DiceSaleTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    private static StreetDiceGameEngine Table()
    {
        var game = new StreetDiceGameEngine("sale-tests");
        for (int i = 1; i <= 4; i++) game.AddPlayer("p" + i, "Player " + i);
        return game;
    }

    [Fact]
    public void NoBidForcesCatcherToBuyForOneDollarAndGrantsSmallHeatBoost()
    {
        var game = Table();
        var sale = game.SellDice("p1", Start);
        Assert.Equal("p2", sale.DefaultBuyerId);
        Assert.Equal(1, sale.Bids.Single().Amount);
        Assert.Equal(GamePhase.SellingDice, game.State.Phase);
        Assert.Throws<InvalidOperationException>(() => game.OpenShot("p1", "p2", 20));
        game.ResolveDiceSale(Start.AddMilliseconds(4999));
        Assert.True(sale.IsOpen);

        game.ResolveDiceSale(Start.AddSeconds(5));
        Assert.False(sale.IsOpen);
        Assert.True(sale.ForcedDefaultWon);
        Assert.Equal("p2", sale.WinnerId);
        Assert.Equal(1, sale.WinningAmount);
        Assert.Equal(999, game.State.FindPlayer("p2")!.Balance);
        Assert.Equal(1001, game.State.FindPlayer("p1")!.Balance);
        Assert.Equal(1.5f, game.State.Streak);
        Assert.Equal(new[] { "p2", "p3", "p4", "p1" }, game.State.Players.Select(player => player.Id));
        Assert.Equal("p3", game.State.CatcherId);
        Assert.Throws<InvalidOperationException>(() => game.OpenShot("p2", "p1", 20, Start.AddSeconds(6)));
        game.OpenShot("p2", "p3", 20, Start.AddSeconds(6));
    }

    [Fact]
    public void WinningBidSubstitutesOneTurnThenOriginalNextPlayerFollows()
    {
        var game = Table();
        var sale = game.SellDice("p1", Start);
        game.BidForDice("p4", 20, Start.AddSeconds(2));
        Assert.Throws<InvalidOperationException>(() => game.BidForDice("p3", 20, Start.AddSeconds(3)));
        game.ResolveDiceSale(Start.AddSeconds(5));

        Assert.Equal("p4", sale.WinnerId);
        Assert.Equal(20, sale.WinningAmount);
        Assert.False(sale.ForcedDefaultWon);
        Assert.Equal(980, game.State.FindPlayer("p4")!.Balance);
        Assert.Equal(1020, game.State.FindPlayer("p1")!.Balance);
        Assert.Equal(0f, game.State.Streak);
        Assert.Equal(new[] { "p4", "p2", "p3", "p1" }, game.State.Players.Select(player => player.Id));
        game.OpenShot("p4", "p2", 20, Start.AddSeconds(6));
        game.Roll(new DiceRoll(4, 6), Start.AddSeconds(22));
        game.Roll(new DiceRoll(3, 4), Start.AddSeconds(38));
        Assert.Equal("p2", game.State.ShooterId);
        Assert.Equal("p4", game.State.CatcherId);
    }

    [Fact]
    public void SaleRequiresThreePlayersAndCannotInterruptPoint()
    {
        var game = new StreetDiceGameEngine("two-player-sale");
        game.AddPlayer("p1", "Seller");
        game.AddPlayer("p2", "Catcher");
        Assert.Throws<InvalidOperationException>(() => game.SellDice("p1", Start));

        var fourPlayerGame = Table();
        fourPlayerGame.OpenShot("p1", "p2", 20, Start);
        fourPlayerGame.Roll(new DiceRoll(4, 6), Start.AddSeconds(20));
        Assert.Throws<InvalidOperationException>(() => fourPlayerGame.SellDice("p1", Start.AddSeconds(21)));
    }

    [Fact]
    public void ForcedBuyerProtectionEndsWhenPointIsSetAndNeverAppliesToPointRolls()
    {
        var game = Table();
        var sale = game.SellDice("p1", Start);
        game.ResolveDiceSale(Start.AddSeconds(5));
        game.OpenShot("p2", "p3", 20, Start.AddSeconds(6));

        Assert.True(sale.ComeOutProtectionActive);
        Assert.Throws<InvalidOperationException>(() => game.Roll(new DiceRoll(1, 1), Start.AddSeconds(22)));
        game.Roll(new DiceRoll(4, 4), Start.AddSeconds(22));
        Assert.False(sale.ComeOutProtectionActive);
        Assert.Equal(GamePhase.Point, game.State.Phase);
        game.Roll(new DiceRoll(1, 1), Start.AddSeconds(38));
        Assert.Equal(GamePhase.Point, game.State.Phase);
        game.Roll(new DiceRoll(3, 4), Start.AddSeconds(54));
        Assert.Equal("p3", game.State.ShooterId);
        Assert.Equal("p2", game.State.CatcherId);
    }
}
