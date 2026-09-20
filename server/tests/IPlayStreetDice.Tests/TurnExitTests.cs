using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class TurnExitTests
{
    private static StreetDiceGameEngine Table()
    {
        var game = new StreetDiceGameEngine("exit-tests");
        for (int i = 1; i <= 4; i++) game.AddPlayer("p" + i, "Player " + i);
        return game;
    }

    [Fact]
    public void PassDoesNotChargeAndPlayerCanSideBet()
    {
        var game = Table();
        game.PassDice("p1");
        Assert.Equal("p2", game.State.ShooterId);
        Assert.Equal(1000, game.State.FindPlayer("p1")!.Balance);
        game.OpenShot("p2", "p3", 20);
        Assert.NotNull(game.PlaceSideBet("p1", SideBetType.ComeOutLoss, 10));
    }

    [Fact]
    public void FirstDiceHolderMustPassBeforeAnotherPlayerCanShoot()
    {
        var game = Table();
        Assert.Throws<InvalidOperationException>(() => game.OpenShot("p2", "p1", 20));
        Assert.Equal(GamePhase.Lobby, game.State.Phase);
        Assert.Null(game.State.ShooterId);
        game.PassDice("p1");
        game.OpenShot("p2", "p1", 20);
        Assert.Equal("p2", game.State.ShooterId);
        Assert.Equal("p1", game.State.CatcherId);
    }

    [Fact]
    public void ActivePointCannotBePassedOrReopened()
    {
        var game = Table();
        game.OpenShot("p1", "p2", 20);
        game.Roll(new DiceRoll(5, 5));
        Assert.Throws<InvalidOperationException>(() => game.PassDice("p1"));
        Assert.Throws<InvalidOperationException>(() => game.OpenShot("p1", "p2", 20));
        Assert.Equal(10, game.State.Point);
    }

    [Fact]
    public void QuitDuringPointSettlesLossAndGroupBetsExactlyOnce()
    {
        var game = Table();
        game.OpenShot("p1", "p2", 20);
        game.Roll(new DiceRoll(5, 5));
        var miss = game.PlaceSideBet("p3", SideBetType.MissPointGroup, 10, 4);
        var hit = game.PlaceSideBet("p4", SideBetType.HitPoint, 5);
        game.LeaveGame("p1");
        Assert.Equal(RollResultType.ShooterForfeitLoss, game.State.LastResolution.Result);
        Assert.Null(game.State.LastResolution.Roll);
        Assert.Equal(SideBetStatus.Won, miss.Status);
        Assert.Equal(SideBetStatus.Lost, hit.Status);
        Assert.Equal(975, game.State.FindPlayer("p1")!.Balance);
        Assert.Equal(1020, game.State.FindPlayer("p2")!.Balance);
        Assert.Equal(4000, game.State.Players.Sum(p => p.Balance));
        game.LeaveGame("p1");
        Assert.Equal(975, game.State.FindPlayer("p1")!.Balance);
        Assert.Null(game.State.Point);
        Assert.Equal(0, game.State.Streak);
        Assert.Equal("p2", game.State.ShooterId);
    }

    [Fact]
    public void LeavingAfterSettlementDoesNotLoseAgain()
    {
        var game = Table();
        game.OpenShot("p1", "p2", 20);
        game.Roll(new DiceRoll(6, 5));
        game.LeaveGame("p1");
        Assert.Equal(1020, game.State.FindPlayer("p1")!.Balance);
    }

    [Fact]
    public void SideBettorLeavingLosesOnlyTheirOpenBets()
    {
        var game = Table();
        game.OpenShot("p1", "p2", 20);
        game.Roll(new DiceRoll(5, 5));
        var bet = game.PlaceSideBet("p3", SideBetType.MissPointGroup, 10, 4);
        game.LeaveGame("p3");
        Assert.Equal(SideBetStatus.Lost, bet.Status);
        Assert.Equal(10, game.State.Point);
        Assert.Equal(990, game.State.FindPlayer("p3")!.Balance);
    }

    [Fact]
    public void DepartedCatcherCannotReceiveDiceAfterSevenOut()
    {
        var game = Table();
        game.OpenShot("p1", "p2", 20);
        game.Roll(new DiceRoll(5, 5));
        game.LeaveGame("p2");
        game.Roll(new DiceRoll(3, 4));
        Assert.Equal("p3", game.State.ShooterId);
        Assert.Equal(1020, game.State.FindPlayer("p1")!.Balance);
    }

    [Fact]
    public void OtherPlayerCannotPassShootersDice()
    {
        var game = Table();
        Assert.Throws<InvalidOperationException>(() => game.PassDice("p2"));
    }

    [Fact]
    public void SevenOutGoesClockwiseEvenWhenCatcherIsElsewhere()
    {
        var game = Table();
        game.OpenShot("p1", "p3", 20);
        game.Roll(new DiceRoll(4, 6));
        game.Roll(new DiceRoll(3, 4));

        Assert.Equal("p2", game.State.ShooterId);
        Assert.Equal("p1", game.State.CatcherId);
    }
}
