using IPlay.Demo;
using IPlayStreetDice.Server.Core;

namespace IPlayStreetDice.Tests;

public sealed class StreetDicePeerWagerTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PublicWindowAndRoleRulesMatchPairedStreetBets()
    {
        var game = NewShot();
        Assert.Equal(new PublicBettingWindow(10000, 15000), game.CurrentBettingWindow(Start));
        Assert.Equal(new PublicBettingWindow(0, 5000), game.CurrentBettingWindow(Start.AddSeconds(10)));

        var catcher = game.OfferPeerWager("p2", "p1", WagerOutcome.Crap, 0, 5, Start.AddSeconds(1));
        Assert.Equal("p2", catcher.From);
        Assert.Throws<InvalidOperationException>(() =>
            game.OfferPeerWager("p2", "p1", WagerOutcome.Hit, 0, 5, Start.AddSeconds(2)));
        Assert.Throws<InvalidOperationException>(() =>
            game.OfferPeerWager("p3", "p1", WagerOutcome.Hit, 0, 5, Start.AddSeconds(2)));
        Assert.Throws<InvalidOperationException>(() =>
            game.OfferPeerWager("p3", "p2", WagerOutcome.Crap, 0, 5, Start.AddSeconds(2)));
        Assert.Throws<InvalidOperationException>(() =>
            game.OfferPeerWager("p1", "p3", WagerOutcome.Crap, 0, 5, Start.AddSeconds(2)));
        Assert.NotNull(game.OfferPeerWager("p4", "p3", WagerOutcome.Crap, 0, 10, Start.AddSeconds(3)));
        Assert.Throws<InvalidOperationException>(() =>
            game.OfferPeerWager("p3", "p4", WagerOutcome.Crap, 0, 10, Start.AddSeconds(10)));
    }

    [Fact]
    public void TenSecondOfferAndFifteenSecondShooterAcceptanceAreServerGates()
    {
        var game = NewShot();
        var toShooter = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 0, 5,
            Start.AddMilliseconds(9999));
        game.AcceptPeerWager("p1", toShooter.Id, Start.AddMilliseconds(14999));
        Assert.Equal(WagerStatus.Accepted, toShooter.Status);
        var lateShooter = game.OfferPeerWager("p4", "p1", WagerOutcome.Crap, 0, 10,
            Start.AddSeconds(2));
        Assert.Throws<InvalidOperationException>(() =>
            game.AcceptPeerWager("p1", lateShooter.Id, Start.AddSeconds(15)));
        var lateRecipient = game.OfferPeerWager("p4", "p3", WagerOutcome.Crap, 0, 1,
            Start.AddSeconds(3));
        Assert.Throws<InvalidOperationException>(() =>
            game.AcceptPeerWager("p3", lateRecipient.Id, Start.AddSeconds(10)));
        Assert.Throws<InvalidOperationException>(() =>
            game.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), Start.AddMilliseconds(14999), 713));
        Assert.NotNull(game.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), Start.AddSeconds(15), 713));
    }

    [Fact]
    public void PrivateAvailableBalanceReservesLockedStakesAndReleasesThemOnSettlement()
    {
        var game = NewShot();
        Assert.Equal(980, game.AvailableBalance("p1"));
        Assert.Equal(980, game.AvailableBalance("p2"));

        var offer = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 0, 5, Start.AddSeconds(1));
        Assert.Equal(995, game.AvailableBalance("p3"));
        Assert.Equal(980, game.AvailableBalance("p1"));
        game.AcceptPeerWager("p1", offer.Id, Start.AddSeconds(2));
        Assert.Equal(975, game.AvailableBalance("p1"));
        Assert.Equal(1000, game.State.FindPlayer("p1")!.Balance);

        game.Roll(new DiceRoll(3, 4), Start.AddSeconds(20));
        Assert.Equal(1025, game.AvailableBalance("p1"));
        Assert.Equal(980, game.AvailableBalance("p2"));
        Assert.Equal(995, game.AvailableBalance("p3"));
        Assert.Equal(4000, game.State.Players.Sum(player => player.Balance));
    }

    [Fact]
    public void PointGroupOffersKeepTheirNumberAndAcceptedPeerCashSettlesOnce()
    {
        var game = NewShot();
        game.Roll(new DiceRoll(4, 6), Start.AddSeconds(20));
        var ten = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 10, 10, Start.AddSeconds(21));
        var four = game.OfferPeerWager("p4", "p1", WagerOutcome.Crap, 4, 5, Start.AddSeconds(22));
        Assert.NotEqual(ten.Id, four.Id);
        Assert.NotNull(game.OfferPeerWager("p2", "p1", WagerOutcome.Crap, 4, 1, Start.AddSeconds(23)));
        Assert.NotNull(game.OfferPeerWager("p3", "p4", WagerOutcome.Hit, 4, 5, Start.AddSeconds(24)));
        game.AcceptPeerWager("p1", ten.Id, Start.AddSeconds(25));
        var pending = game.PreparePhysicalRoll("p1", new DiceGesture(0.5f, 0), Start.AddSeconds(35), 713);
        var committed = game.CommitPhysicalRoll("p1", pending.RollId, pending.FadeDeadline);
        int total = committed.Throw.Faces.Sum();
        if (committed.Throw.IsCounted && (total == 7 || total == 10 || total == 4))
            Assert.Equal(WagerStatus.Settled, ten.Status);
        else
            Assert.Equal(WagerStatus.Accepted, ten.Status);
        Assert.Equal(4000, game.State.Players.Sum(player => player.Balance));
        int before = game.State.Players.Sum(player => player.Balance);
        game.CommitPhysicalRoll("p1", pending.RollId, pending.FadeDeadline.AddSeconds(1));
        Assert.Equal(before, game.State.Players.Sum(player => player.Balance));
    }

    [Fact]
    public void ShooterHeatAwardsHalfPointForOnePeerWinAndOneForMultipleOnSameRoll()
    {
        var game = NewShot();
        game.Roll(new DiceRoll(4, 6), Start.AddSeconds(20));
        var ten = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 10, 5, Start.AddSeconds(21));
        var four = game.OfferPeerWager("p4", "p1", WagerOutcome.Crap, 4, 5, Start.AddSeconds(21));
        game.AcceptPeerWager("p1", ten.Id, Start.AddSeconds(22));
        game.AcceptPeerWager("p1", four.Id, Start.AddSeconds(22));

        game.Roll(new DiceRoll(2, 2), Start.AddSeconds(36));

        Assert.Equal(1f, game.State.Streak);
        Assert.False(game.State.HotDiceActive);
        Assert.Equal(GamePhase.Point, game.State.Phase);
        Assert.Equal("p1", ten.Winner);
        Assert.Equal("p1", four.Winner);
    }

    [Fact]
    public void OneShooterPeerWinAwardsHalfPointAndComeOutCrapClearsIt()
    {
        var game = NewShot();
        game.Roll(new DiceRoll(4, 6), Start.AddSeconds(20));
        var four = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 4, 5, Start.AddSeconds(21));
        game.AcceptPeerWager("p1", four.Id, Start.AddSeconds(22));

        game.Roll(new DiceRoll(2, 2), Start.AddSeconds(36));
        Assert.Equal(0.5f, game.State.Streak);

        game.Roll(new DiceRoll(4, 6), Start.AddSeconds(37));
        game.RunSame("p1");
        game.Roll(new DiceRoll(1, 1), Start.AddSeconds(60));
        Assert.Equal(0f, game.State.Streak);
        Assert.Equal("p1", game.State.ShooterId);
    }

    [Fact]
    public void ShooterForfeitPaysAcceptedPeerCrapAndMainStakeOnce()
    {
        var game = NewShot();
        var offer = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 0, 10, Start.AddSeconds(1));
        game.AcceptPeerWager("p1", offer.Id, Start.AddSeconds(2));
        game.LeaveGame("p1");
        Assert.Equal(WagerStatus.Settled, offer.Status);
        Assert.Equal("p3", offer.Winner);
        Assert.Equal(970, game.State.FindPlayer("p1")!.Balance);
        Assert.Equal(1020, game.State.FindPlayer("p2")!.Balance);
        Assert.Equal(1010, game.State.FindPlayer("p3")!.Balance);
        game.LeaveGame("p1");
        Assert.Equal(970, game.State.FindPlayer("p1")!.Balance);
        Assert.Equal(4000, game.State.Players.Sum(player => player.Balance));
    }

    [Fact]
    public void ServerKeepsDoubleAndPairedNumberAsSeparateShooterApprovedRequests()
    {
        var game = NewShot();
        game.Roll(new DiceRoll(4, 6), Start.AddSeconds(20));
        var source = game.OfferPeerWager("p3", "p1", WagerOutcome.Crap, 10, 5, Start.AddSeconds(21));
        game.AcceptPeerWager("p1", source.Id, Start.AddSeconds(22));

        var doubled = game.OfferPeerAddOn("p3", source.Id, WagerAddOnKind.DoubleUp, Start.AddSeconds(36));
        var paired = game.OfferPeerAddOn("p3", source.Id, WagerAddOnKind.PairedNumber, Start.AddSeconds(37));
        Assert.Equal(10, doubled.Number);
        Assert.Equal(4, paired.Number);
        game.AcceptPeerWager("p1", paired.Id, Start.AddSeconds(37));
        Assert.Equal(WagerStatus.Offered, doubled.Status);
        Assert.Equal(WagerStatus.Accepted, paired.Status);

        game.Roll(new DiceRoll(2, 2), Start.AddSeconds(38));
        Assert.Equal(WagerStatus.Expired, doubled.Status);
        Assert.Equal(WagerStatus.Settled, paired.Status);
        Assert.Equal("p1", paired.Winner);
        Assert.Equal(4000, game.State.Players.Sum(player => player.Balance));
    }

    private static StreetDiceGameEngine NewShot()
    {
        var game = new StreetDiceGameEngine("peer-test");
        for (int i = 1; i <= 4; i++) game.AddPlayer("p" + i, "Player " + i);
        game.OpenShot("p1", "p2", 20, Start);
        return game;
    }
}
