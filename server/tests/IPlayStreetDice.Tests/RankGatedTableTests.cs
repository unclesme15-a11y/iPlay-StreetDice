using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

// "As long as the higher ranked player is the host, everyone will be able to
// benefit from that... because that is the high ranked player's dice game
// if he/she is the host" -- the table's cap and prestige-note unlock key
// off the HOST's linked account, not each individual seat.
public class RankGatedTableTests
{
    [Fact]
    public void UnlinkedTable_DefaultsToLevel1Cap()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;
        store.JoinRealPlayer(gameId, "Guest");

        Assert.Equal(100, store.EffectiveBetCap(gameId));
        Assert.False(store.PrestigeBillsUnlocked(gameId));
    }

    [Fact]
    public void HostsAccountLevel_RaisesTheWholeTablesCap()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Bell");
        var account = accounts.Register("Bell", "hustle123");
        for (var i = 0; i < 15; i++) accounts.RecordWin(account.Id); // -> Level 3
        store.LinkAccount(gameId, host.Player.Id, account.Id);

        Assert.Equal(500, store.EffectiveBetCap(gameId));
        Assert.True(store.PrestigeBillsUnlocked(gameId));
    }

    [Fact]
    public void AGuestsHighLevel_NeverRaisesTheTableAboveTheHosts()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        store.JoinRealPlayer(gameId, "Host"); // Level 1, unlinked -- stays host
        var guest = store.JoinRealPlayer(gameId, "Bell");
        var guestAccount = accounts.Register("Bell", "hustle123");
        for (var i = 0; i < 50; i++) accounts.RecordWin(guestAccount.Id); // -> Level 5
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        Assert.Equal(100, store.EffectiveBetCap(gameId));
        Assert.False(store.PrestigeBillsUnlocked(gameId));
    }

    [Fact]
    public void AHostsLowLevel_CapsEverySeatEvenAGuestWhoOutranksThem()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Host");
        var hostAccount = accounts.Register("Host", "hustle123"); // stays Level 1
        store.LinkAccount(gameId, host.Player.Id, hostAccount.Id);

        var guest = store.JoinRealPlayer(gameId, "Bell");
        var guestAccount = accounts.Register("Bell", "another-pw1");
        for (var i = 0; i < 50; i++) accounts.RecordWin(guestAccount.Id); // -> Level 5
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        // "I want it all to be around the host" -- a Level 5 guest still plays at the
        // Level 1 host's table cap. The table's rank is the host's rank, period.
        Assert.Equal(100, store.EffectiveBetCap(gameId));
        Assert.False(store.PrestigeBillsUnlocked(gameId));
    }

    [Fact]
    public void HustledNote_UsableUnderAHighLevelHostEvenAtLevel1()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Bell");
        var hostAccount = accounts.Register("Bell", "hustle123");
        for (var i = 0; i < 15; i++) accounts.RecordWin(hostAccount.Id); // -> Level 3
        store.LinkAccount(gameId, host.Player.Id, hostAccount.Id);

        var guest = store.JoinRealPlayer(gameId, "Dice");
        var guestAccount = accounts.Register("Dice", "another-pw1"); // stays Level 1
        guestAccount.HustledPrestigeNotes.Add(100);
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        Assert.True(store.PrestigeNoteUsableBy(gameId, guest.Player.Id, 100));
    }

    [Fact]
    public void HustledNote_UnusableUnderALowLevelHostUntilYouLevelUpYourself()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        store.JoinRealPlayer(gameId, "Host"); // Level 1, unlinked
        var guest = store.JoinRealPlayer(gameId, "Dice");
        var guestAccount = accounts.Register("Dice", "another-pw1");
        guestAccount.HustledPrestigeNotes.Add(50);
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        Assert.False(store.PrestigeNoteUsableBy(gameId, guest.Player.Id, 50));

        for (var i = 0; i < 15; i++) accounts.RecordWin(guestAccount.Id); // guest levels up to 3 themselves
        Assert.True(store.PrestigeNoteUsableBy(gameId, guest.Player.Id, 50));
    }
}
