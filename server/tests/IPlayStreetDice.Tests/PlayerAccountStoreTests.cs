using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class PlayerAccountStoreTests
{
    [Fact]
    public void RegisterThenLogin_Succeeds()
    {
        var accounts = new PlayerAccountStore();
        var registered = accounts.Register("Bell", "dice1234");

        var (account, token) = accounts.Login("Bell", "dice1234");

        Assert.Equal(registered.Id, account.Id);
        Assert.Equal(64, token.Length);
        Assert.True(accounts.ValidateSession(account.Id, token));
    }

    [Fact]
    public void Login_WithWrongPassword_Throws()
    {
        var accounts = new PlayerAccountStore();
        accounts.Register("Bell", "dice1234");

        Assert.Throws<InvalidOperationException>(() => accounts.Login("Bell", "wrong-password"));
    }

    [Fact]
    public void Register_DuplicateUsername_Throws()
    {
        var accounts = new PlayerAccountStore();
        accounts.Register("Bell", "dice1234");

        Assert.Throws<InvalidOperationException>(() => accounts.Register("Bell", "different-pw"));
    }

    private static readonly DateOnly Day1 = new(2026, 9, 28);

    private static int Watch(PlayerAccountStore accounts, string id, DateOnly day) =>
        accounts.RecordShot(id, shooterOrCatcher: false, won: false, opponentKey: null, 0, day);

    private static int Win(PlayerAccountStore accounts, string id, string opponent, DateOnly day) =>
        accounts.RecordShot(id, shooterOrCatcher: true, won: true, opponent, 0, day);

    [Fact]
    public void FirstFifteenShotsEachDay_EarnFiveTimesXp()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "dice1234");

        var granted = Enumerable.Range(0, 16).Select(_ => Watch(accounts, account.Id, Day1)).ToList();

        Assert.All(granted.Take(15), xp => Assert.Equal(40, xp)); // 8 seated x 5
        Assert.Equal(8, granted[15]);
        Assert.Equal(40, Watch(accounts, account.Id, Day1.AddDays(1))); // bonus is back tomorrow
    }

    [Fact]
    public void ThereIsNoDailyCap()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "dice1234");

        for (var i = 0; i < 1000; i++) Watch(accounts, account.Id, Day1);

        Assert.Equal(15 * 40 + 985 * 8, account.Xp);
    }

    [Fact]
    public void GrindingOneDay_CrossesIntoLevel2AtExactly12000Xp()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "dice1234");

        for (var i = 0; i < 1439; i++) Watch(accounts, account.Id, Day1);
        Assert.Equal(1, account.Level);
        Watch(accounts, account.Id, Day1);

        Assert.Equal(12_000, account.Xp);
        Assert.Equal(2, account.Level);
        Assert.Equal(0, account.ShotsPlayed); // watching doesn't count as a shot played
    }

    [Fact]
    public void WinBonus_StopsAfterThreeWinsAgainstTheSameOpponentInADay()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "dice1234");
        for (var i = 0; i < 15; i++) Watch(accounts, account.Id, Day1); // use up the daily bonus

        var granted = Enumerable.Range(0, 4).Select(_ => Win(accounts, account.Id, "same-friend", Day1)).ToList();
        Assert.Equal(new[] { 28, 28, 28, 16 }, granted);

        Assert.Equal(28, Win(accounts, account.Id, "someone-new", Day1));
        // The same friend pays again the next day (first shot of the day, so x5 too).
        Assert.Equal(140, Win(accounts, account.Id, "same-friend", Day1.AddDays(1)));
    }

    [Fact]
    public void XpAndDailyCounters_SurviveASnapshotRestoreCycle()
    {
        var original = new PlayerAccountStore();
        var account = original.Register("Bell", "dice1234");
        for (var i = 0; i < 3; i++) Win(original, account.Id, "same-friend", Day1);

        var restarted = new PlayerAccountStore();
        restarted.Restore(original.Snapshot());

        Assert.True(restarted.TryGet(account.Id, out var restored));
        Assert.Equal(account.Xp, restored.Xp);
        // Win bonuses against this friend are used up, but the daily bonus (shot 4 of 15)
        // is still running: 16 x 5.
        Assert.Equal(80, Win(restarted, account.Id, "same-friend", Day1));
    }

    [Fact]
    public void SessionTokens_SurviveASnapshotRestoreCycle()
    {
        var original = new PlayerAccountStore();
        original.Register("Bell", "dice1234");
        var (account, token) = original.Login("Bell", "dice1234");

        var restarted = new PlayerAccountStore();
        restarted.Restore(original.Snapshot());

        Assert.True(restarted.ValidateSession(account.Id, token));
    }

    [Fact]
    public void Restore_AcceptsOlderSnapshotsWrittenBeforeTokensWereSaved()
    {
        var original = new PlayerAccountStore();
        var account = original.Register("Bell", "dice1234");
        var legacy = original.Snapshot().Select(a => a with { SessionToken = null });

        var restarted = new PlayerAccountStore();
        restarted.Restore(legacy);

        Assert.True(restarted.TryGet(account.Id, out _));
        Assert.NotNull(restarted.Login("Bell", "dice1234").Token);
    }

    [Fact]
    public void ValidateSession_RejectsATokenFromADifferentAccount()
    {
        var accounts = new PlayerAccountStore();
        accounts.Register("Bell", "dice1234");
        accounts.Register("Dice", "another-pw1");
        var (_, bellToken) = accounts.Login("Bell", "dice1234");
        var (diceAccount, _) = accounts.Login("Dice", "another-pw1");

        Assert.False(accounts.ValidateSession(diceAccount.Id, bellToken));
    }
}
