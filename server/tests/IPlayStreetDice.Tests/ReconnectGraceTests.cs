namespace IPlayStreetDice.Tests;

public sealed class ReconnectGraceTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReconnectWithinTwentySecondsKeepsTheWager()
    {
        var store = NewLiveTable(out var gameId);
        Assert.True(store.TryGet(gameId, out var engine));
        store.MarkDisconnected(gameId, "p1", Start);
        Assert.True(store.TryReconnect(gameId, "p1", Start.AddSeconds(20)));
        store.ExpireDisconnected(gameId, Start.AddSeconds(21));
        Assert.False(engine.State.FindPlayer("p1")!.HasLeft);
        Assert.Equal(1000, engine.State.FindPlayer("p1")!.Balance);
    }

    [Fact]
    public void ReconnectAfterTwentySecondsForfeitsOnce()
    {
        var store = NewLiveTable(out var gameId);
        Assert.True(store.TryGet(gameId, out var engine));
        store.MarkDisconnected(gameId, "p1", Start);
        Assert.False(store.TryReconnect(gameId, "p1", Start.AddSeconds(21)));
        Assert.True(engine.State.FindPlayer("p1")!.HasLeft);
        Assert.Equal(980, engine.State.FindPlayer("p1")!.Balance);
        store.ExpireDisconnected(gameId, Start.AddSeconds(30));
        Assert.Equal(980, engine.State.FindPlayer("p1")!.Balance);
    }

    [Fact]
    public void CommittedRollCanBeReplayedByAnotherSeatWithoutCommittingAgain()
    {
        var store = NewLiveTable(out var gameId);
        Assert.True(store.TryGet(gameId, out var engine));
        Assert.Null(store.LastCommittedRoll(gameId));
        var prepared = engine.PreparePhysicalRoll("p1", new(0.5f, 0), Start, 713);
        var committed = engine.CommitPhysicalRoll("p1", prepared.RollId, prepared.FadeDeadline);
        store.RecordCommittedRoll(gameId, "p1", committed);
        var replay = store.LastCommittedRoll(gameId)!;
        Assert.Equal(1, replay.Sequence);
        Assert.Equal("p1", replay.ShooterId);
        Assert.Equal(committed.Throw.Faces, replay.Faces);
        Assert.NotEmpty(replay.Frames);
        Assert.Equal(committed, engine.CommitPhysicalRoll("p1", prepared.RollId, prepared.FadeDeadline));
        store.RecordCommittedRoll(gameId, "p1", committed);
        Assert.Equal(1, store.LastCommittedRoll(gameId)!.Sequence);
    }

    private static StreetDiceTableStore NewLiveTable(out string gameId)
    {
        var store = new StreetDiceTableStore();
        var engine = store.CreateGame();
        gameId = engine.State.GameId;
        engine.AddPlayer("p1", "Shooter");
        engine.AddPlayer("p2", "Catcher");
        engine.OpenShot("p1", "p2", 20, Start.AddSeconds(-16));
        return store;
    }

    [Fact]
    public void RealPlayersReceiveDifferentSeatsAndCannotClaimAnotherSeatToken()
    {
        var store = new StreetDiceTableStore();
        var gameId = store.CreateGame().State.GameId;
        var host = store.JoinRealPlayer(gameId, "Host");
        var guest = store.JoinRealPlayer(gameId, "Guest");
        Assert.Equal("p1", host.Player.Id);
        Assert.Equal("p2", guest.Player.Id);
        Assert.True(store.ValidatePlayerSession(gameId, host.Player.Id, host.Token));
        Assert.True(store.ValidatePlayerSession(gameId, guest.Player.Id, guest.Token));
        Assert.False(store.ValidatePlayerSession(gameId, host.Player.Id, guest.Token));
        store.RevokePlayerSession(gameId, host.Player.Id);
        Assert.False(store.ValidatePlayerSession(gameId, host.Player.Id, host.Token));
    }

    [Fact]
    public void MissedHeartbeatsAllowTwentySecondsBeforeForfeit()
    {
        var store = new StreetDiceTableStore();
        var gameId = store.CreateGame().State.GameId;
        var host = store.JoinRealPlayer(gameId, "Host", Start);
        store.JoinRealPlayer(gameId, "Guest", Start);
        Assert.True(store.TryGet(gameId, out var engine));
        engine.OpenShot("p1", "p2", 20, Start.AddSeconds(-16));
        store.ExpireDisconnected(gameId, Start.AddSeconds(20));
        Assert.False(engine.State.FindPlayer("p1")!.HasLeft);
        Assert.True(store.TryReconnect(gameId, "p1", Start.AddSeconds(20)));
        Assert.True(store.TryReconnect(gameId, "p2", Start.AddSeconds(20)));
        store.ExpireDisconnected(gameId, Start.AddSeconds(21));
        Assert.False(engine.State.FindPlayer("p1")!.HasLeft);
        store.ExpireDisconnected(gameId, Start.AddSeconds(41));
        Assert.True(engine.State.FindPlayer("p1")!.HasLeft);
        Assert.False(store.ValidatePlayerSession(gameId, "p1", host.Token));
    }
}
