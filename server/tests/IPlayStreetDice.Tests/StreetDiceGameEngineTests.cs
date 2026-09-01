using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class StreetDiceGameEngineTests
{
    [Fact]
    public void OpenShot_RequiresShooterAndCatcher()
    {
        var engine = NewTwoPlayerGame();

        engine.OpenShot("p1", "p2", 20);

        Assert.Equal(GamePhase.ComeOut, engine.State.Phase);
        Assert.Equal("p1", engine.State.ShooterId);
        Assert.Equal("p2", engine.State.CatcherId);
        Assert.Equal(20, engine.State.ShotAmount);
    }

    [Theory]
    [InlineData(3, 4)]
    [InlineData(5, 6)]
    public void ComeOut_SevenOrEleven_WinsImmediately(int die1, int die2)
    {
        var engine = NewLiveShot(20);
        var shooterStart = engine.State.FindPlayer("p1")!.Balance;
        var catcherStart = engine.State.FindPlayer("p2")!.Balance;

        var result = engine.Roll(new DiceRoll(die1, die2));

        Assert.Equal(RollResultType.ShooterComeOutWin, result.Result);
        Assert.Equal(GamePhase.ShooterDecision, engine.State.Phase);
        Assert.Equal(shooterStart + 20, engine.State.FindPlayer("p1")!.Balance);
        Assert.Equal(catcherStart - 20, engine.State.FindPlayer("p2")!.Balance);
        Assert.Equal(1, engine.State.Streak);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(6, 6)]
    public void ComeOut_TwoThreeOrTwelve_LosesButShooterKeepsDice(int die1, int die2)
    {
        var engine = NewLiveShot(25);

        var result = engine.Roll(new DiceRoll(die1, die2));

        Assert.Equal(RollResultType.ShooterComeOutLoss, result.Result);
        Assert.Equal(GamePhase.ShooterDecision, engine.State.Phase);
        Assert.Equal("p1", engine.State.ShooterId);

        engine.RunSame("p1");

        Assert.Equal(GamePhase.ComeOut, engine.State.Phase);
        Assert.Equal(25, engine.State.ShotAmount);
    }

    [Fact]
    public void ComeOut_OtherNumber_EstablishesPoint()
    {
        var engine = NewLiveShot(20);

        var result = engine.Roll(new DiceRoll(3, 2));

        Assert.Equal(RollResultType.PointEstablished, result.Result);
        Assert.Equal(GamePhase.Point, engine.State.Phase);
        Assert.Equal(5, engine.State.Point);
    }

    [Fact]
    public void PointPhase_HittingPoint_WinsAndBuildsMoreStreak()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(3, 2));

        var result = engine.Roll(new DiceRoll(4, 1));

        Assert.Equal(RollResultType.ShooterPointWin, result.Result);
        Assert.Equal(GamePhase.ShooterDecision, engine.State.Phase);
        Assert.Null(engine.State.Point);
        Assert.Equal(2, engine.State.Streak);
    }

    [Fact]
    public void PointPhase_SevenOut_LosesAndBreaksStreak()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(3, 2));
        engine.Roll(new DiceRoll(4, 1));
        engine.RunSame("p1");
        engine.Roll(new DiceRoll(2, 2));

        var result = engine.Roll(new DiceRoll(3, 4));

        Assert.Equal(RollResultType.ShooterSevenOutLoss, result.Result);
        Assert.Equal(GamePhase.ComeOut, engine.State.Phase);
        Assert.Equal("p2", engine.State.ShooterId);
        Assert.Equal("p1", engine.State.CatcherId);
        Assert.Equal(0, engine.State.Streak);
    }

    [Fact]
    public void FillBots_AddsPlayersUpToTableLimit()
    {
        var engine = new StreetDiceGameEngine("test-game");
        engine.AddPlayer("p1", "Shooter");

        var bots = engine.FillBots(5);

        Assert.Equal(4, bots.Count);
        Assert.Equal(5, engine.State.Players.Count);
        Assert.Equal("bot-2", engine.State.Players[1].Id);
    }

    [Fact]
    public void BotAdvance_CanOpenAndRollATestShot()
    {
        var engine = new StreetDiceGameEngine("test-game");
        engine.AddPlayer("p1", "Shooter");
        engine.FillBots(2);

        var open = engine.AdvanceBotAction(new Random(4));

        Assert.Equal(GamePhase.ComeOut, engine.State.Phase);
        Assert.Equal("p1", engine.State.ShooterId);
        Assert.Equal("bot-2", engine.State.CatcherId);
        Assert.Equal(RollResultType.None, open.Result);

        var roll = engine.AdvanceBotAction(new Random(7));

        Assert.NotNull(roll);
        Assert.NotEqual(GamePhase.Lobby, engine.State.Phase);
    }

    [Fact]
    public void FadeCatch_NullifiesRollAndDoesNotResolveSideBets()
    {
        var engine = NewLiveShot(20);
        var sideBet = engine.PlaceSideBet("p3", SideBetType.ComeOutWin, 5);

        var result = engine.FadeCatch("p2");

        Assert.Equal(RollResultType.Faded, result.Result);
        Assert.Equal(GamePhase.ComeOut, engine.State.Phase);
        Assert.Equal(SideBetStatus.Open, sideBet.Status);
        Assert.Null(result.Roll);
    }

    [Fact]
    public void FadeCatch_AfterThirdFade_BuildsShooterMomentum()
    {
        var engine = NewLiveShot(20);

        engine.FadeCatch("p2");
        engine.FadeCatch("p2");
        engine.FadeCatch("p2");

        Assert.Equal(0, engine.State.ShooterMomentum);

        engine.FadeCatch("p2");
        engine.FadeCatch("p2");

        Assert.Equal(2, engine.State.ShooterMomentum);
    }

    [Fact]
    public void Momentum_IncreasesStreakRewardWhenShooterWinsLater()
    {
        var engine = NewLiveShot(20);
        engine.FadeCatch("p2");
        engine.FadeCatch("p2");
        engine.FadeCatch("p2");
        engine.FadeCatch("p2");

        var result = engine.Roll(new DiceRoll(3, 4));

        Assert.Equal(RollResultType.ShooterComeOutWin, result.Result);
        Assert.Equal(2, engine.State.Streak);
    }

    [Fact]
    public void SideBets_ResolveOnlyOnCountedRolls()
    {
        var engine = NewLiveShot(20);
        var winBet = engine.PlaceSideBet("p3", SideBetType.ComeOutWin, 10);
        var lossBet = engine.PlaceSideBet("p4", SideBetType.ComeOutLoss, 10);

        engine.Roll(new DiceRoll(4, 3));

        Assert.Equal(SideBetStatus.Won, winBet.Status);
        Assert.Equal(SideBetStatus.Lost, lossBet.Status);
    }

    [Fact]
    public void PointSideBets_ResolveWhenPointHitsOrMisses()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(3, 2));
        var hitBet = engine.PlaceSideBet("p3", SideBetType.HitPoint, 10);
        var missBet = engine.PlaceSideBet("p4", SideBetType.MissPoint, 10);

        engine.Roll(new DiceRoll(4, 1));

        Assert.Equal(SideBetStatus.Won, hitBet.Status);
        Assert.Equal(SideBetStatus.Lost, missBet.Status);
    }

    [Fact]
    public void GroupedPointSideBet_LosesWhenShooterHitsGroupedNumberButPointStaysLive()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(6, 4));
        var groupMissBet = engine.PlaceSideBet("p3", SideBetType.MissPointGroup, 10, targetPointNumber: 4);

        var result = engine.Roll(new DiceRoll(2, 2));

        Assert.Equal(RollResultType.None, result.Result);
        Assert.Equal(GamePhase.Point, engine.State.Phase);
        Assert.Equal(10, engine.State.Point);
        Assert.Equal(SideBetStatus.Lost, groupMissBet.Status);
    }

    [Fact]
    public void GroupedPointSideBet_LosesWhenShooterHitsOriginalPointInSameGroup()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(6, 4));
        var groupMissBet = engine.PlaceSideBet("p3", SideBetType.MissPointGroup, 10, targetPointNumber: 4);

        var result = engine.Roll(new DiceRoll(5, 5));

        Assert.Equal(RollResultType.ShooterPointWin, result.Result);
        Assert.Equal(SideBetStatus.Lost, groupMissBet.Status);
    }

    [Fact]
    public void GroupedPointSideBet_WinsWhenShooterSevensOutBeforeGroupHit()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(6, 4));
        var groupMissBet = engine.PlaceSideBet("p3", SideBetType.MissPointGroup, 10, targetPointNumber: 4);

        var result = engine.Roll(new DiceRoll(3, 4));

        Assert.Equal(RollResultType.ShooterSevenOutLoss, result.Result);
        Assert.Equal(SideBetStatus.Won, groupMissBet.Status);
    }

    [Fact]
    public void GroupedPointSideBet_TargetMustMatchActivePointGroup()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(6, 4));

        Assert.Throws<InvalidOperationException>(() =>
            engine.PlaceSideBet("p3", SideBetType.MissPointGroup, 10, targetPointNumber: 6));
    }

    [Theory]
    [InlineData(4, 5, 6, CeeLoOutcomeType.AutomaticWin, null)]
    [InlineData(3, 3, 3, CeeLoOutcomeType.AutomaticWin, null)]
    [InlineData(2, 2, 6, CeeLoOutcomeType.AutomaticWin, 6)]
    [InlineData(1, 2, 3, CeeLoOutcomeType.AutomaticLoss, null)]
    [InlineData(2, 2, 1, CeeLoOutcomeType.AutomaticLoss, 1)]
    [InlineData(4, 4, 2, CeeLoOutcomeType.Point, 2)]
    [InlineData(1, 3, 5, CeeLoOutcomeType.Reroll, null)]
    public void CeeLoRules_EvaluateStreetBankerRolls(int die1, int die2, int die3, CeeLoOutcomeType outcome, int? point)
    {
        var result = CeeLoRules.Evaluate(new CeeLoRoll(die1, die2, die3));

        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(point, result.Point);
    }

    [Fact]
    public void DoubleUp_DoublesNextShotOnlyAfterWin()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(3, 4));

        engine.DoubleUp("p1");

        Assert.Equal(GamePhase.ComeOut, engine.State.Phase);
        Assert.Equal(40, engine.State.ShotAmount);
        Assert.True(engine.State.LastShotWasDoubleUp);
    }

    [Fact]
    public void DoubleUp_IsUnavailableAfterLoss()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(1, 1));

        Assert.Throws<InvalidOperationException>(() => engine.DoubleUp("p1"));
    }

    [Fact]
    public void HotDice_ActivatesAtFullStreak_AndRedOrangeAreNotSelectable()
    {
        var engine = NewLiveShot(20);
        engine.Roll(new DiceRoll(3, 4));
        engine.DoubleUp("p1");
        engine.Roll(new DiceRoll(5, 6));
        engine.DoubleUp("p1");
        engine.Roll(new DiceRoll(3, 2));
        engine.Roll(new DiceRoll(4, 1));

        Assert.True(engine.State.HotDiceActive);
        Assert.Equal(new[] { "Black", "White", "Green", "Blue" }, Enum.GetNames<DiceColor>());
    }

    private static StreetDiceGameEngine NewTwoPlayerGame()
    {
        var engine = new StreetDiceGameEngine("test-game");
        engine.AddPlayer("p1", "Shooter");
        engine.AddPlayer("p2", "Catcher");
        engine.AddPlayer("p3", "Side 1");
        engine.AddPlayer("p4", "Side 2");
        return engine;
    }

    private static StreetDiceGameEngine NewLiveShot(int amount)
    {
        var engine = NewTwoPlayerGame();
        engine.OpenShot("p1", "p2", amount);
        return engine;
    }
}
