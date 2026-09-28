using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public class PlayerAccountStoreTests
{
    [Fact]
    public void RegisterThenLogin_Succeeds()
    {
        var accounts = new PlayerAccountStore();
        var registered = accounts.Register("Bell", "hustle123");

        var (account, token) = accounts.Login("Bell", "hustle123");

        Assert.Equal(registered.Id, account.Id);
        Assert.Equal(64, token.Length);
        Assert.True(accounts.ValidateSession(account.Id, token));
    }

    [Fact]
    public void Login_WithWrongPassword_Throws()
    {
        var accounts = new PlayerAccountStore();
        accounts.Register("Bell", "hustle123");

        Assert.Throws<InvalidOperationException>(() => accounts.Login("Bell", "wrong-password"));
    }

    [Fact]
    public void Register_DuplicateUsername_Throws()
    {
        var accounts = new PlayerAccountStore();
        accounts.Register("Bell", "hustle123");

        Assert.Throws<InvalidOperationException>(() => accounts.Register("Bell", "different-pw"));
    }

    private static readonly DateOnly Day1 = new(2026, 9, 28);

    [Fact]
    public void RecordShot_RaisesLevelOnceThresholdIsCrossed()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "hustle123");
        Assert.Equal(1, account.Level);

        // 20 losses x 10 XP = 200 XP = Level 2 -- playing alone still levels you up.
        for (var i = 0; i < 20; i++) accounts.RecordShot(account.Id, won: false, "opponent", 0, Day1);

        Assert.Equal(200, account.Xp);
        Assert.Equal(2, account.Level);
        Assert.Equal(20, account.ShotsPlayed);
    }

    [Fact]
    public void WinBonus_StopsAfterThreeWinsAgainstTheSameOpponentInADay()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "hustle123");

        var granted = Enumerable.Range(0, 4).Select(_ => accounts.RecordShot(account.Id, true, "same-friend", 0, Day1)).ToList();
        Assert.Equal(new[] { 25, 25, 25, 10 }, granted);

        // A different opponent still pays the full win bonus.
        Assert.Equal(25, accounts.RecordShot(account.Id, true, "someone-new", 0, Day1));
        // And the same friend pays again the next day.
        Assert.Equal(25, accounts.RecordShot(account.Id, true, "same-friend", 0, Day1.AddDays(1)));
    }

    [Fact]
    public void DailyCap_StopsXpAt500PerDay_ThenResetsTomorrow()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "hustle123");

        for (var i = 0; i < 100; i++) accounts.RecordShot(account.Id, false, "farm-partner", 0, Day1);
        Assert.Equal(RankLadder.DailyXpCap, account.Xp);
        Assert.Equal(100, account.ShotsPlayed); // shots still count as stats, just not XP

        accounts.RecordShot(account.Id, false, "farm-partner", 0, Day1.AddDays(1));
        Assert.Equal(RankLadder.DailyXpCap + 10, account.Xp);
    }

    [Fact]
    public void XpAndDailyCounters_SurviveASnapshotRestoreCycle()
    {
        var original = new PlayerAccountStore();
        var account = original.Register("Bell", "hustle123");
        for (var i = 0; i < 3; i++) original.RecordShot(account.Id, true, "same-friend", 0, Day1);

        var restarted = new PlayerAccountStore();
        restarted.Restore(original.Snapshot());

        Assert.True(restarted.TryGet(account.Id, out var restored));
        Assert.Equal(account.Xp, restored.Xp);
        // The three win bonuses already used against this friend today are remembered.
        Assert.Equal(10, restarted.RecordShot(account.Id, true, "same-friend", 0, Day1));
    }

    [Fact]
    public void SessionTokens_SurviveASnapshotRestoreCycle()
    {
        var original = new PlayerAccountStore();
        original.Register("Bell", "hustle123");
        var (account, token) = original.Login("Bell", "hustle123");

        var restarted = new PlayerAccountStore();
        restarted.Restore(original.Snapshot());

        Assert.True(restarted.ValidateSession(account.Id, token));
    }

    [Fact]
    public void Restore_AcceptsOlderSnapshotsWrittenBeforeTokensWereSaved()
    {
        var original = new PlayerAccountStore();
        var account = original.Register("Bell", "hustle123");
        var legacy = original.Snapshot().Select(a => a with { SessionToken = null });

        var restarted = new PlayerAccountStore();
        restarted.Restore(legacy);

        Assert.True(restarted.TryGet(account.Id, out _));
        Assert.NotNull(restarted.Login("Bell", "hustle123").Token);
    }

    [Fact]
    public void ValidateSession_RejectsATokenFromADifferentAccount()
    {
        var accounts = new PlayerAccountStore();
        accounts.Register("Bell", "hustle123");
        accounts.Register("Dice", "another-pw1");
        var (_, bellToken) = accounts.Login("Bell", "hustle123");
        var (diceAccount, _) = accounts.Login("Dice", "another-pw1");

        Assert.False(accounts.ValidateSession(diceAccount.Id, bellToken));
    }
}
