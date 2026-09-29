using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

// Owner rule 2026-09-29: after the point, the shooter or the catcher proposes a Double Up on
// the main bet; the other side accepts and then owes the full doubled amount.
public sealed class StreetDiceMainDoubleUpTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 29, 12, 0, 0, TimeSpan.Zero);

    private static StreetDiceGameEngine PointSix()
    {
        var game = new StreetDiceGameEngine("main-double");
        for (int i = 1; i <= 3; i++) game.AddPlayer("p" + i, "Player " + i);
        game.OpenShot("p1", "p2", 100, Start);                    // shooter p1 bets catcher p2 $100
        game.Roll(new DiceRoll(2, 4), Start.AddSeconds(20));      // point 6
        return game;
    }

    [Fact]
    public void CatcherDoublesAndShooterAccepts_SevenOutPaysTheCatcherTwoHundred()
    {
        var game = PointSix();
        game.ProposeMainDoubleUp("p2");
        game.AcceptMainDoubleUp("p1");
        Assert.Equal(200, game.State.ShotAmount);
        game.Roll(new DiceRoll(3, 4), Start.AddSeconds(40));      // seven-out
        Assert.Equal(800, game.State.Players.First(p => p.Id == "p1").Balance);
        Assert.Equal(1200, game.State.Players.First(p => p.Id == "p2").Balance);
    }

    [Fact]
    public void ShooterDoublesAndCatcherAccepts_HittingThePointPaysTheShooterTwoHundred()
    {
        var game = PointSix();
        game.ProposeMainDoubleUp("p1");
        game.AcceptMainDoubleUp("p2");
        game.Roll(new DiceRoll(3, 3), Start.AddSeconds(40));      // hits 6
        Assert.Equal(1200, game.State.Players.First(p => p.Id == "p1").Balance);
        Assert.Equal(800, game.State.Players.First(p => p.Id == "p2").Balance);
    }

    [Fact]
    public void OnlyAfterThePoint_OnlyTheTwoMainPlayers_OnlyTheOtherSideAccepts_OncePerShot()
    {
        var game = new StreetDiceGameEngine("main-double-rules");
        for (int i = 1; i <= 3; i++) game.AddPlayer("p" + i, "Player " + i);
        game.OpenShot("p1", "p2", 100, Start);
        Assert.Throws<InvalidOperationException>(() => game.ProposeMainDoubleUp("p2"));   // come-out
        game.Roll(new DiceRoll(2, 4), Start.AddSeconds(20));
        Assert.Throws<InvalidOperationException>(() => game.ProposeMainDoubleUp("p3"));   // bystander
        game.ProposeMainDoubleUp("p2");
        Assert.Throws<InvalidOperationException>(() => game.AcceptMainDoubleUp("p2"));   // own offer
        Assert.Throws<InvalidOperationException>(() => game.AcceptMainDoubleUp("p3"));
        game.AcceptMainDoubleUp("p1");
        Assert.Throws<InvalidOperationException>(() => game.ProposeMainDoubleUp("p1"));   // once per shot
    }

    [Fact]
    public void NotAcceptedBeforeTheThrow_TheOfferIsGone()
    {
        var game = PointSix();
        game.ProposeMainDoubleUp("p2");
        game.Roll(new DiceRoll(1, 3), Start.AddSeconds(40));      // no decision on a 4
        Assert.Null(game.State.MainDoubleUpProposedBy);
        Assert.Throws<InvalidOperationException>(() => game.AcceptMainDoubleUp("p1"));
        Assert.Equal(100, game.State.ShotAmount);
    }

    [Fact]
    public void DoubleUpCantGoOverTheTableCap()
    {
        var game = PointSix();
        Assert.Throws<InvalidOperationException>(() => game.ProposeMainDoubleUp("p2", betCap: 150));
        game.ProposeMainDoubleUp("p2", betCap: 250);
    }
}
