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
        var store = new StreetDiceTableStore(new PlayerAccountStore());
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
        var store = new StreetDiceTableStore(new PlayerAccountStore());
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        store.JoinRealPlayer(gameId, "Host");
        var hostId = engine.State.HostId;

        for (int i = 0; i < 3; i++) store.JoinRealPlayer(gameId, $"Guest {i}");

        Assert.Equal(hostId, engine.State.HostId);
    }

    [Fact]
    public void HostLeaves_HighestRankedRemainingPlayerBecomesHost()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Host");
        var lowGuest = store.JoinRealPlayer(gameId, "Low");
        var highGuest = store.JoinRealPlayer(gameId, "High");
        var high = accounts.Register("High", "hustle123");
        high.Xp = 2500; // Level 4 -- sits in a later seat but outranks the earlier guest
        store.LinkAccount(gameId, highGuest.Player.Id, high.Id);

        engine.LeaveGame(host.Player.Id);
        store.HandleDeparture(gameId, host.Player.Id);

        Assert.Equal(highGuest.Player.Id, engine.State.HostId);
        Assert.NotEqual(lowGuest.Player.Id, engine.State.HostId);
    }

    [Fact]
    public void HostLeaves_TableKeepsTheDepartingHostsLevelForTheWholeParty()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Host");
        var hostAccount = accounts.Register("Host", "hustle123");
        hostAccount.Xp = 1000; // Level 3
        store.LinkAccount(gameId, host.Player.Id, hostAccount.Id);
        var guest = store.JoinRealPlayer(gameId, "Guest");
        var guestAccount = accounts.Register("Guest", "another-pw1");
        guestAccount.Xp = 5000; // Level 5
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        engine.LeaveGame(host.Player.Id);
        store.HandleDeparture(gameId, host.Player.Id);

        // The Level 5 guest takes over the host role, but the party stays a Level 3 party.
        Assert.Equal(guest.Player.Id, engine.State.HostId);
        Assert.Equal(500, store.EffectiveBetCap(gameId));
        Assert.True(store.PrestigeBillsUnlocked(gameId));
    }

    [Fact]
    public void NonHostLeaving_ChangesNothing()
    {
        var store = new StreetDiceTableStore(new PlayerAccountStore());
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;
        var host = store.JoinRealPlayer(gameId, "Host");
        var guest = store.JoinRealPlayer(gameId, "Guest");

        engine.LeaveGame(guest.Player.Id);
        store.HandleDeparture(gameId, guest.Player.Id);

        Assert.Equal(host.Player.Id, engine.State.HostId);
        Assert.Null(engine.State.LockedTableLevel);
    }

    [Fact]
    public void HostAndLockedLevel_SurviveAServerRestart()
    {
        var store = new StreetDiceTableStore(new PlayerAccountStore());
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;
        var host = store.JoinRealPlayer(gameId, "Host");
        store.JoinRealPlayer(gameId, "Guest");
        engine.State.LockedTableLevel = 3;

        var path = Path.Combine(Path.GetTempPath(), $"host-{Guid.NewGuid():N}.json");
        try
        {
            store.PersistTo(path);
            var restarted = new StreetDiceTableStore(new PlayerAccountStore());
            restarted.RestoreFrom(path);

            Assert.True(restarted.TryGet(gameId, out var restored));
            Assert.Equal(host.Player.Id, restored.State.HostId);
            Assert.Equal(500, restarted.EffectiveBetCap(gameId));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
