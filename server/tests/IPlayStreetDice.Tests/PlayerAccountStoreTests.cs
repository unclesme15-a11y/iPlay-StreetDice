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

    [Fact]
    public void RecordWin_RaisesLevelOnceThresholdIsCrossed()
    {
        var accounts = new PlayerAccountStore();
        var account = accounts.Register("Bell", "hustle123");
        Assert.Equal(1, account.Level);

        for (var i = 0; i < 5; i++) accounts.RecordWin(account.Id);

        Assert.Equal(2, account.Level);
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
