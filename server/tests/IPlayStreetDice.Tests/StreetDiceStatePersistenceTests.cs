using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class StreetDiceStatePersistenceTests : IDisposable
{
    private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"street-dice-test-{Guid.NewGuid():N}.json");

    [Fact]
    public void GameStateSurvivesASaveAndRestoreCycle()
    {
        var originalStore = new StreetDiceTableStore();
        var engine = originalStore.CreateGame();
        var gameId = engine.State.GameId;

        var shooter = engine.AddPlayer("shooter-1", "Shooter");
        var catcher = engine.AddPlayer("catcher-1", "Catcher");
        var shooterToken = originalStore.CreateOrReplacePlayerSession(gameId, shooter.Id);
        var catcherToken = originalStore.CreateOrReplacePlayerSession(gameId, catcher.Id);

        engine.SelectDiceColor(shooter.Id, DiceColor.Green);
        engine.OpenShot(shooter.Id, catcher.Id, 25);
        engine.PlaceSideBet(catcher.Id, SideBetType.ComeOutLoss, 10);
        engine.Roll(new DiceRoll(3, 4)); // point established on 7? no - 3+4=7 -> come-out win, resolves the shot

        originalStore.PersistTo(_filePath);

        var restoredStore = new StreetDiceTableStore();
        restoredStore.RestoreFrom(_filePath);

        Assert.True(restoredStore.TryGet(gameId, out var restoredEngine));
        Assert.Equal(engine.State.Phase, restoredEngine.State.Phase);
        Assert.Equal(2, restoredEngine.State.Players.Count);

        var restoredShooter = restoredEngine.State.FindPlayer(shooter.Id);
        Assert.NotNull(restoredShooter);
        Assert.Equal(shooter.Balance, restoredShooter!.Balance);
        Assert.Equal(DiceColor.Green, restoredShooter.DiceColor);

        var restoredCatcher = restoredEngine.State.FindPlayer(catcher.Id);
        Assert.NotNull(restoredCatcher);
        Assert.Equal(catcher.Balance, restoredCatcher!.Balance);

        Assert.Single(restoredEngine.State.SideBets);
        Assert.Equal(engine.State.SideBets[0].Status, restoredEngine.State.SideBets[0].Status);

        Assert.NotEmpty(restoredEngine.State.EventLog);
        Assert.Equal(engine.State.EventLog.Count, restoredEngine.State.EventLog.Count);

        // Session tokens survive the restart too - restored players shouldn't have to rejoin.
        Assert.True(restoredStore.ValidatePlayerSession(gameId, shooter.Id, shooterToken));
        Assert.True(restoredStore.ValidatePlayerSession(gameId, catcher.Id, catcherToken));
    }

    [Fact]
    public void RestoringFromAMissingFileIsANoOp()
    {
        var store = new StreetDiceTableStore();
        var missingPath = Path.Combine(Path.GetTempPath(), $"does-not-exist-{Guid.NewGuid():N}.json");

        store.RestoreFrom(missingPath);

        Assert.False(store.TryGet("anything", out _));
    }

    public void Dispose()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
    }
}
