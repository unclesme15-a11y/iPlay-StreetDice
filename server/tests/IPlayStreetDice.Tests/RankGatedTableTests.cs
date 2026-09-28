using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

// "As long as the higher ranked player is the host, everyone will be able to
// benefit from that... because that is the high ranked player's dice game
// if he/she is the host" -- the table's cap and prestige-note unlock key
// off the HOST's linked account, not each individual seat.
public class RankGatedTableTests
{
    private static (StreetDiceTableStore Store, string GameId, PlayerAccount Low, string LowSeat, PlayerAccount High, string HighSeat)
        LowVsHigh(int lowXp, int highXp)
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var gameId = store.CreateGame().State.GameId;
        var lowSeat = store.JoinRealPlayer(gameId, "Low").Player.Id;
        var highSeat = store.JoinRealPlayer(gameId, "High").Player.Id;
        var low = accounts.Register("Low", "dice1234");
        var high = accounts.Register("High", "dice1234");
        low.Xp = lowXp;
        high.Xp = highXp;
        store.LinkAccount(gameId, lowSeat, low.Id);
        store.LinkAccount(gameId, highSeat, high.Id);
        return (store, gameId, low, lowSeat, high, highSeat);
    }

    [Theory]
    [InlineData(50, 50)]
    [InlineData(99, 50)]
    [InlineData(100, 100)]
    [InlineData(500, 100)]
    public void BeatingALevel3PlayerForFiftyOrMore_EarnsATrophyNote(int shot, int expectedNote)
    {
        var t = LowVsHigh(lowXp: 0, highXp: 42_000);
        t.Store.AwardTrophyNote(t.GameId, t.LowSeat, t.HighSeat, shot);
        Assert.Equal(new[] { expectedNote }, t.Low.TrophyNotes);
    }

    [Theory]
    [InlineData(0, 42_000, 49)]        // shot under $50
    [InlineData(0, 12_000, 100)]       // loser is only Level 2
    [InlineData(42_000, 112_000, 100)] // winner is already Level 3
    public void NoTrophyNote_WhenAnyConditionIsMissing(int winnerXp, int loserXp, int shot)
    {
        var t = LowVsHigh(winnerXp, loserXp);
        t.Store.AwardTrophyNote(t.GameId, t.LowSeat, t.HighSeat, shot);
        Assert.Empty(t.Low.TrophyNotes);
    }

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
        var account = accounts.Register("Bell", "dice1234");
        account.Xp = 42_000; // -> Level 3
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
        var guestAccount = accounts.Register("Bell", "dice1234");
        guestAccount.Xp = 272_000; // -> Level 5
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
        var hostAccount = accounts.Register("Host", "dice1234"); // stays Level 1
        store.LinkAccount(gameId, host.Player.Id, hostAccount.Id);

        var guest = store.JoinRealPlayer(gameId, "Bell");
        var guestAccount = accounts.Register("Bell", "another-pw1");
        guestAccount.Xp = 272_000; // -> Level 5
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        // "I want it all to be around the host" -- a Level 5 guest still plays at the
        // Level 1 host's table cap. The table's rank is the host's rank, period.
        Assert.Equal(100, store.EffectiveBetCap(gameId));
        Assert.False(store.PrestigeBillsUnlocked(gameId));
    }

    [Fact]
    public void TrophyNote_UsableUnderAHighLevelHostEvenAtLevel1()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Bell");
        var hostAccount = accounts.Register("Bell", "dice1234");
        hostAccount.Xp = 42_000; // -> Level 3
        store.LinkAccount(gameId, host.Player.Id, hostAccount.Id);

        var guest = store.JoinRealPlayer(gameId, "Dice");
        var guestAccount = accounts.Register("Dice", "another-pw1"); // stays Level 1
        guestAccount.TrophyNotes.Add(100);
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        Assert.True(store.PrestigeNoteUsableBy(gameId, guest.Player.Id, 100));
    }

    [Fact]
    public void TrophyNote_StaysUnusableUnderALowLevelHostEvenAfterTheHolderLevelsUpThemselves()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        store.JoinRealPlayer(gameId, "Host"); // Level 1, unlinked -- stays host
        var guest = store.JoinRealPlayer(gameId, "Dice");
        var guestAccount = accounts.Register("Dice", "another-pw1");
        guestAccount.TrophyNotes.Add(50);
        store.LinkAccount(gameId, guest.Player.Id, guestAccount.Id);

        Assert.False(store.PrestigeNoteUsableBy(gameId, guest.Player.Id, 50));

        // "I want it all to be around the host" -- there's no separate personal-level path
        // here either, so leveling up yourself doesn't unlock a trophy note under a host
        // who's still under Level 3.
        guestAccount.Xp = 42_000; // guest reaches Level 3 themselves
        Assert.False(store.PrestigeNoteUsableBy(gameId, guest.Player.Id, 50));
    }

    [Fact]
    public void HoldingATrophyNote_DoesNotByItselfGrantAnythingAHostAlreadyUnlocksForEveryone()
    {
        var accounts = new PlayerAccountStore();
        var store = new StreetDiceTableStore(accounts);
        var engine = store.CreateGame();
        var gameId = engine.State.GameId;

        var host = store.JoinRealPlayer(gameId, "Bell");
        var hostAccount = accounts.Register("Bell", "dice1234");
        hostAccount.Xp = 42_000; // -> Level 3
        store.LinkAccount(gameId, host.Player.Id, hostAccount.Id);

        var guestWithNote = store.JoinRealPlayer(gameId, "Dice");
        var guestWithNoteAccount = accounts.Register("Dice", "another-pw1");
        guestWithNoteAccount.TrophyNotes.Add(100);
        store.LinkAccount(gameId, guestWithNote.Player.Id, guestWithNoteAccount.Id);

        var guestWithoutNote = store.JoinRealPlayer(gameId, "Ray");
        var guestWithoutNoteAccount = accounts.Register("Ray", "another-pw2");
        store.LinkAccount(gameId, guestWithoutNote.Player.Id, guestWithoutNoteAccount.Id);

        // Under a Level 3+ host, $50/$100 is unlocked for the whole table regardless of who
        // won which note -- the note is a trophy, not an exclusive unlock.
        Assert.True(store.PrestigeBillsUnlocked(gameId));
        Assert.True(store.PrestigeNoteUsableBy(gameId, guestWithNote.Player.Id, 100));
    }
}
