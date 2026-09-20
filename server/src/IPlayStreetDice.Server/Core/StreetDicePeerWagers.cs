using IPlay.Demo;

namespace IPlayStreetDice.Server.Core;

public sealed record PublicBettingWindow(double OfferRemainingMilliseconds, double ShooterRemainingMilliseconds);

public sealed partial class StreetDiceGameEngine
{
    private readonly WagerBook _peerWagers = new();

    public IReadOnlyList<WagerOffer> PeerWagers => _peerWagers.Offers;

    public PublicBettingWindow? CurrentBettingWindow(DateTimeOffset now)
    {
        if (!_peerWagers.Started || State.Phase is not (GamePhase.ComeOut or GamePhase.Point)) return null;
        double seconds = BettingSeconds(now);
        return new PublicBettingWindow(
            Math.Max(0, (_peerWagers.OfferDeadline - seconds) * 1000),
            Math.Max(0, (_peerWagers.ShooterDeadline - seconds) * 1000));
    }

    public WagerOffer OfferPeerWager(string from, string to, WagerOutcome outcome, int number, int amount,
        DateTimeOffset now)
    {
        if (RequirePlayer(from).HasLeft || RequirePlayer(to).HasLeft)
            throw new InvalidOperationException("A wager participant has left.");
        if (from == State.CatcherId && (outcome != WagerOutcome.Crap || to != State.ShooterId))
            throw new InvalidOperationException("The catcher can only offer against the shooter.");
        if (to == State.CatcherId && outcome != WagerOutcome.Hit)
            throw new InvalidOperationException("The catcher cannot accept a wager with the shooter.");
        return _peerWagers.Propose(from, to, outcome, number, amount, BettingSeconds(now), AvailablePeerFunds);
    }

    public WagerOffer AcceptPeerWager(string recipient, int offerId, DateTimeOffset now)
    {
        if (RequirePlayer(recipient).HasLeft) throw new InvalidOperationException("Player has left.");
        _peerWagers.Accept(offerId, recipient, BettingSeconds(now), AvailablePeerFunds);
        return _peerWagers.Offers.First(offer => offer.Id == offerId);
    }

    public WagerOffer OfferPeerAddOn(string bettorId, int sourceOfferId, WagerAddOnKind kind, DateTimeOffset now,
        int amount = 0)
    {
        if (State.Phase != GamePhase.Point || RequirePlayer(bettorId).HasLeft)
            throw new InvalidOperationException("Add-ons require a live point and a seated bettor.");
        return _peerWagers.ProposeAddOn(sourceOfferId, bettorId, kind, BettingSeconds(now), AvailablePeerFunds, amount);
    }

    private int AvailablePeerFunds(string playerId)
    {
        var player = RequirePlayer(playerId);
        if (player.HasLeft) return 0;
        bool liveShot = State.Phase is GamePhase.ComeOut or GamePhase.Point;
        int mainStake = liveShot && (playerId == State.ShooterId || playerId == State.CatcherId)
            ? State.ShotAmount : 0;
        return Math.Max(0, player.Balance - mainStake);
    }

    public int AvailableBalance(string playerId)
    {
        var player = RequirePlayer(playerId);
        if (player.HasLeft) return 0;
        return Math.Max(0, AvailablePeerFunds(playerId) - _peerWagers.Exposure(playerId));
    }

    private void SettlePeerWagers(IEnumerable<WagerOffer> settled)
    {
        foreach (var offer in settled)
        {
            if (offer.Winner == State.ShooterId) _shooterSideWinsThisRoll++;
            string loserId = offer.Winner == offer.From ? offer.To : offer.From;
            RequirePlayer(loserId).Debit(offer.Amount);
            RequirePlayer(offer.Winner!).Credit(offer.Amount);
            State.Log($"Peer wager {offer.Id}: {loserId} paid {offer.Winner} {offer.Amount}.");
        }
    }

    private static double BettingSeconds(DateTimeOffset now) => now.ToUnixTimeMilliseconds() / 1000d;
}
