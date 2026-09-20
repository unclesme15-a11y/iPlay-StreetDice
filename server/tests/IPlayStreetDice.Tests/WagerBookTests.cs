using IPlay.Demo;

namespace IPlayStreetDice.Tests;

public class WagerBookTests
{
    private static int Funds(string _) => 1000;
    [Fact]
    public void LeavingSettlesOnlyAcceptedWagersOnce()
    {
        var book = PointTen();
        var hit = book.Propose("a", "b", WagerOutcome.Hit, 4, 10, 1, Funds);
        var pending = book.Propose("a", "shooter", WagerOutcome.Crap, 4, 20, 1, Funds);
        book.Accept(hit.Id, "b", 2, Funds);
        Assert.Single(book.Forfeit("shooter"));
        Assert.Equal("b", hit.Winner);
        Assert.Equal(WagerStatus.Expired, pending.Status);
        Assert.Empty(book.Forfeit("shooter"));
    }
    private static WagerBook PointTen()
    {
        var book = new WagerBook();
        book.Open("shooter", 10, 0);
        return book;
    }

    [Fact]
    public void ShooterMayOfferHitButCannotOfferCrapAgainstTheirOwnRoll()
    {
        var book = PointTen();
        var hit = book.Propose("shooter", "a", WagerOutcome.Hit, 10, 5, 1, Funds);
        Assert.Throws<InvalidOperationException>(() =>
            book.Propose("shooter", "a", WagerOutcome.Crap, 10, 5, 1, Funds));
        book.Accept(hit.Id, "a", 2, Funds);
        book.BeginRoll(15);
        book.Resolve(10);
        Assert.Equal("shooter", hit.Winner);
    }

    [Fact]
    public void ShooterGetsFiveExtraSecondsButOtherRecipientsDoNot()
    {
        var book = PointTen();
        var shooterOffer = book.Propose("a", "shooter", WagerOutcome.Crap, 10, 10, 9.9, Funds);
        var otherOffer = book.Propose("a", "b", WagerOutcome.Hit, 4, 5, 9.9, Funds);
        Assert.Throws<InvalidOperationException>(() => book.Propose("b", "a", WagerOutcome.Crap, 10, 1, 10, Funds));
        Assert.Throws<InvalidOperationException>(() => book.Accept(otherOffer.Id, "b", 10, Funds));
        book.Accept(shooterOffer.Id, "shooter", 14.9, Funds);
        Assert.False(book.CanRoll(14.9));
        book.BeginRoll(15);
        Assert.Equal(WagerStatus.Accepted, shooterOffer.Status);
        Assert.Equal(WagerStatus.Expired, otherOffer.Status);
    }

    [Fact]
    public void GroupedHitDoesNotSettleActualPointWager()
    {
        var book = PointTen();
        var four = book.Propose("a", "b", WagerOutcome.Hit, 4, 10, 1, Funds);
        var ten = book.Propose("a", "b", WagerOutcome.Hit, 10, 10, 1, Funds);
        book.Accept(four.Id, "b", 2, Funds);
        book.Accept(ten.Id, "b", 2, Funds);
        book.BeginRoll(15);
        Assert.Single(book.Resolve(4));
        Assert.Equal("a", four.Winner);
        Assert.Equal(WagerStatus.Accepted, ten.Status);
        book.Open("shooter", 10, 20);
        book.BeginRoll(35);
        Assert.Single(book.Resolve(10));
        Assert.Equal("a", ten.Winner);
    }

    [Theory]
    [InlineData(10, "a", "b")]
    [InlineData(7, "b", "a")]
    public void PointWinsAllHitsAndSevenWinsCrap(int roll, string hitWinner, string crapWinner)
    {
        var book = PointTen();
        var hit = book.Propose("a", "b", WagerOutcome.Hit, 4, 20, 1, Funds);
        var crap = book.Propose("a", "b", WagerOutcome.Crap, 10, 10, 1, Funds);
        book.Accept(hit.Id, "b", 2, Funds);
        book.Accept(crap.Id, "b", 2, Funds);
        book.BeginRoll(15);
        Assert.Equal(2, book.Resolve(roll).Count);
        Assert.Equal(hitWinner, hit.Winner);
        Assert.Equal(crapWinner, crap.Winner);
        Assert.Throws<InvalidOperationException>(() => book.Resolve(roll));
    }

    [Fact]
    public void FadePreservesLockedWagersAndPermitsImmediateRethrow()
    {
        var book = PointTen();
        var bet = book.Propose("a", "b", WagerOutcome.Hit, 4, 20, 1, Funds);
        var pending = book.Propose("a", "shooter", WagerOutcome.Crap, 4, 5, 1, Funds);
        book.Accept(bet.Id, "b", 2, Funds);
        book.BeginRoll(15);
        book.Fade(16);
        Assert.Equal(WagerStatus.Accepted, bet.Status);
        Assert.Equal(WagerStatus.Expired, pending.Status);
        Assert.Null(bet.Winner);
        Assert.False(book.CanOffer(16));
        Assert.True(book.CanRoll(16));
        book.BeginRoll(16);
        Assert.Empty(book.Resolve(5));
        Assert.Equal(WagerStatus.Accepted, bet.Status);
        book.BeginRoll(17);
        Assert.Single(book.Resolve(4));
        Assert.Equal("a", bet.Winner);
    }

    [Fact]
    public void PermissionsAndFundsAreCheckedBeforeLocking()
    {
        var book = PointTen();
        Assert.Throws<InvalidOperationException>(() => book.Propose("a", "shooter", WagerOutcome.Hit, 10, 10, 1, Funds));
        var bet = book.Propose("a", "b", WagerOutcome.Hit, 4, 20, 1, Funds);
        Assert.Throws<InvalidOperationException>(() => book.Accept(bet.Id, "c", 2, Funds));
        Assert.Throws<InvalidOperationException>(() => book.Accept(bet.Id, "b", 2, _ => 19));
        book.Accept(bet.Id, "b", 2, Funds);
        book.Accept(bet.Id, "b", 3, Funds);
        Assert.Equal(20, book.Exposure("b"));
    }

    [Theory]
    [InlineData(4, "b")]
    [InlineData(10, "b")]
    [InlineData(7, "a")]
    public void NumberedCrapOffersRemainDistinctAndFollowTheActiveGroup(int roll, string winner)
    {
        var book = PointTen();
        var againstTen = book.Propose("a", "b", WagerOutcome.Crap, 10, 10, 1, Funds);
        var againstFour = book.Propose("a", "b", WagerOutcome.Crap, 4, 5, 1, Funds);
        Assert.Equal(10, againstTen.Number);
        Assert.Equal(4, againstFour.Number);
        Assert.NotEqual(againstTen.Id, againstFour.Id);
        book.Accept(againstTen.Id, "b", 2, Funds);
        book.Accept(againstFour.Id, "b", 2, Funds);
        book.BeginRoll(15);
        Assert.Equal(2, book.Resolve(roll).Count);
        Assert.Equal(winner, againstTen.Winner);
        Assert.Equal(winner, againstFour.Winner);
    }

    [Fact]
    public void ComeOutCrapUsesTwoThreeTwelveButPointCrapNeedsAGroupedNumber()
    {
        var point = PointTen();
        Assert.Throws<InvalidOperationException>(() =>
            point.Propose("a", "b", WagerOutcome.Crap, 0, 5, 1, Funds));
        Assert.Throws<InvalidOperationException>(() =>
            point.Propose("a", "b", WagerOutcome.Crap, 6, 5, 1, Funds));
        var comeOut = new WagerBook();
        comeOut.Open("shooter", 0, 0);
        Assert.Throws<InvalidOperationException>(() =>
            comeOut.Propose("a", "b", WagerOutcome.Crap, 10, 5, 1, Funds));
        var offer = comeOut.Propose("a", "b", WagerOutcome.Crap, 0, 5, 1, Funds);
        comeOut.Accept(offer.Id, "b", 2, Funds);
        comeOut.BeginRoll(15);
        Assert.Single(comeOut.Resolve(3));
        Assert.Equal("a", offer.Winner);
    }

    [Fact]
    public void LockedPointBetCanRequestDoubleAndPairedNumberIndependentlyAfterCountdown()
    {
        var book = PointTen();
        var source = book.Propose("a", "shooter", WagerOutcome.Crap, 10, 5, 1, Funds);
        book.Accept(source.Id, "shooter", 2, Funds);
        var doubled = book.ProposeAddOn(source.Id, "a", WagerAddOnKind.DoubleUp, 16, Funds);
        var paired = book.ProposeAddOn(source.Id, "a", WagerAddOnKind.PairedNumber, 17, Funds);
        Assert.Equal(10, doubled.Number);
        Assert.Equal(4, paired.Number);
        Assert.Equal(5, doubled.Amount);
        Assert.Equal(5, paired.Amount);
        Assert.Throws<InvalidOperationException>(() => book.ProposeAddOn(source.Id, "a", WagerAddOnKind.DoubleUp, 17, Funds));
        book.Accept(doubled.Id, "shooter", 17, Funds);
        Assert.Equal(WagerStatus.Offered, paired.Status);
        book.BeginRoll(18);
        Assert.Equal(WagerStatus.Expired, paired.Status);
        Assert.Equal(2, book.Resolve(7).Count);
        Assert.Equal("a", doubled.Winner);
        Assert.Null(paired.Winner);
    }

    [Fact]
    public void PairedNumberCanUseItsOwnSelectedBillAmount()
    {
        var book = PointTen();
        var source = book.Propose("a", "shooter", WagerOutcome.Crap, 10, 5, 1, Funds);
        book.Accept(source.Id, "shooter", 2, Funds);
        Assert.Throws<ArgumentException>(() =>
            book.ProposeAddOn(source.Id, "a", WagerAddOnKind.PairedNumber, 16, Funds, 3));
        var paired = book.ProposeAddOn(source.Id, "a", WagerAddOnKind.PairedNumber, 16, Funds, 20);
        Assert.Equal(4, paired.Number);
        Assert.Equal(20, paired.Amount);
    }

    [Fact]
    public void AddOnRequiresOriginalLockedBetAndFundsForBothSides()
    {
        var book = PointTen();
        var source = book.Propose("a", "shooter", WagerOutcome.Crap, 10, 20, 1, Funds);
        Assert.Throws<InvalidOperationException>(() => book.ProposeAddOn(source.Id, "a", WagerAddOnKind.PairedNumber, 2, Funds));
        book.Accept(source.Id, "shooter", 2, Funds);
        Assert.Throws<InvalidOperationException>(() => book.ProposeAddOn(source.Id, "b", WagerAddOnKind.PairedNumber, 16, Funds));
        Assert.Throws<InvalidOperationException>(() => book.ProposeAddOn(source.Id, "a", WagerAddOnKind.PairedNumber, 16,
            player => player == "a" ? 39 : 1000));
        var paired = book.ProposeAddOn(source.Id, "a", WagerAddOnKind.PairedNumber, 16, Funds);
        Assert.Throws<InvalidOperationException>(() => book.Accept(paired.Id, "a", 16, Funds));
        Assert.Throws<InvalidOperationException>(() => book.Accept(paired.Id, "shooter", 16,
            player => player == "shooter" ? 19 : 1000));
    }
}
