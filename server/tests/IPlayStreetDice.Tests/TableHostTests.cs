using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

// The host concept didn't exist before the music controls needed it: only
// the player who created the table gets to drive what's "now playing" for
// everyone else. Mirrors the existing ShooterId ??= playerId pattern in
// StreetDiceTableStore.JoinRealPlayer.
public class TableHostTests
{
    [Fact]
    public void FirstRealPlayerToJoinBecomesHost()
    {
        var store = new StreetDiceTableStore();
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var first = store.JoinRealPlayer(gameId, "Alice");
        Assert.Equal(first.Player.Id, engine.State.HostId);

        var second = store.JoinRealPlayer(gameId, "Bob");
        Assert.Equal(first.Player.Id, engine.State.HostId);
        Assert.NotEqual(second.Player.Id, engine.State.HostId);
    }

    [Fact]
    public void HostSurvivesAcrossSubsequentJoins()
    {
        var store = new StreetDiceTableStore();
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        store.JoinRealPlayer(gameId, "Host");
        var hostId = engine.State.HostId;

        for (int i = 0; i < 3; i++) store.JoinRealPlayer(gameId, $"Guest {i}");

        Assert.Equal(hostId, engine.State.HostId);
    }
}
