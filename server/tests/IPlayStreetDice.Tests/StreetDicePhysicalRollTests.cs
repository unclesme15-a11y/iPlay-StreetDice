using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public sealed class StreetDicePhysicalRollTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PendingThrowWithholdsResultAndBlocksOtherRolls()
    {
        var engine = NewLiveShot();
        var prepared = engine.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), Start, 713);
        var duplicate = engine.PreparePhysicalRoll("p1", new DiceGesture(1f, 1f), Start.AddMilliseconds(20), 947);

        Assert.Equal(prepared, duplicate);
        Assert.Equal(TimeSpan.FromMilliseconds(1250), prepared.FadeDeadline - Start);
        Assert.Equal(1000, engine.State.FindPlayer("p1")!.Balance);
        Assert.Equal(1000, engine.State.FindPlayer("p2")!.Balance);
        Assert.Equal(RollResultType.None, engine.State.LastResolution!.Result);
        Assert.Throws<InvalidOperationException>(() => engine.Roll(new DiceRoll(6, 6)));
        Assert.Throws<InvalidOperationException>(() => engine.FadeCatch("p2"));
        Assert.Equal(0, engine.State.FadeCount);
        var publicRoll = engine.CurrentPhysicalRoll(Start.AddMilliseconds(250));
        Assert.Equal(prepared.RollId, publicRoll!.RollId);
        Assert.Equal(1000, publicRoll.RemainingFadeMilliseconds);
        Assert.Throws<InvalidOperationException>(() =>
            engine.CommitPhysicalRoll("p1", prepared.RollId, prepared.FadeDeadline.AddTicks(-1)));
        Assert.Throws<InvalidOperationException>(() =>
            engine.CommitPhysicalRoll("p2", prepared.RollId, prepared.FadeDeadline));
    }

    [Fact]
    public void OnlyCatcherCanFadeBeforeDeadlineWithoutPayout()
    {
        var engine = NewLiveShot();
        for (int i = 0; i < 4; i++)
        {
            var at = Start.AddSeconds(i);
            var pending = engine.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), at, 713 + i);
            Assert.Throws<InvalidOperationException>(() => engine.FadePhysicalRoll("p1", pending.RollId, at));
            Assert.Throws<InvalidOperationException>(() =>
                engine.FadePhysicalRoll("p2", pending.RollId, pending.FadeDeadline));
            var faded = engine.FadePhysicalRoll("p2", pending.RollId, at.AddMilliseconds(200));
            Assert.Equal(RollResultType.Faded, faded.Result);
            Assert.Null(faded.Roll);
            Assert.Throws<InvalidOperationException>(() => engine.FadePhysicalRoll("p2", pending.RollId, at));
            Assert.Throws<InvalidOperationException>(() =>
                engine.CommitPhysicalRoll("p1", pending.RollId, pending.FadeDeadline));
        }
        Assert.Equal(4, engine.State.FadeCount);
        Assert.Equal(1, engine.State.ShooterMomentum);
        Assert.Equal("p1", engine.State.ShooterId);
        Assert.Equal(1000, engine.State.FindPlayer("p1")!.Balance);
        Assert.Equal(1000, engine.State.FindPlayer("p2")!.Balance);
        Assert.Null(engine.CurrentPhysicalRoll(Start.AddSeconds(10)));
    }

    [Fact]
    public void PhysicalOutcomeCommitsOnceAndMatchesTheDiceThatSettled()
    {
        var engine = NewLiveShot();
        var pending = engine.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), Start, 713);
        var committed = engine.CommitPhysicalRoll("p1", pending.RollId, pending.FadeDeadline);
        int balance = engine.State.FindPlayer("p1")!.Balance;

        Assert.True(committed.Throw.IsCounted);
        Assert.NotNull(committed.Resolution.Roll);
        Assert.Equal(committed.Throw.Faces[0], committed.Resolution.Roll!.Value.Die1);
        Assert.Equal(committed.Throw.Faces[1], committed.Resolution.Roll!.Value.Die2);
        Assert.Equal(committed, engine.CommitPhysicalRoll("p1", pending.RollId, pending.FadeDeadline.AddSeconds(1)));
        Assert.Throws<InvalidOperationException>(() =>
            engine.CommitPhysicalRoll("p2", pending.RollId, pending.FadeDeadline.AddSeconds(1)));
        Assert.Equal(balance, engine.State.FindPlayer("p1")!.Balance);
        Assert.Equal(2000, engine.State.FindPlayer("p1")!.Balance + engine.State.FindPlayer("p2")!.Balance);
        Assert.Null(engine.CurrentPhysicalRoll(pending.FadeDeadline));
    }

    [Fact]
    public void LeavingWithPendingThrowForfeitsTheLiveShotAndCannotCommit()
    {
        var engine = NewLiveShot();
        var pending = engine.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), Start, 713);
        engine.LeaveGame("p1");

        Assert.Equal(RollResultType.ShooterForfeitLoss, engine.State.LastResolution!.Result);
        Assert.Equal(980, engine.State.FindPlayer("p1")!.Balance);
        Assert.Equal(1020, engine.State.FindPlayer("p2")!.Balance);
        Assert.Throws<InvalidOperationException>(() =>
            engine.CommitPhysicalRoll("p1", pending.RollId, pending.FadeDeadline));
    }

    private static StreetDiceGameEngine NewLiveShot()
    {
        var engine = new StreetDiceGameEngine("physical-test");
        engine.AddPlayer("p1", "Shooter");
        engine.AddPlayer("p2", "Catcher");
        engine.OpenShot("p1", "p2", 20, Start.AddSeconds(-16));
        return engine;
    }
}
