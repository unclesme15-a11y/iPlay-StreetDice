using System;
using System.Collections.Generic;
using IPlay.Demo;
using UnityEngine;

public sealed partial class StreetDiceGreyboxController
{
    private WagerBook wagerBook = new WagerBook();
    private bool botWagersEnabled = true;
    private int BotProfile(string player) => Math.Max(0, Array.IndexOf(DemoShooterOrder, player) - 1);
    private readonly Dictionary<int, float> botOfferResponseAt = new Dictionary<int, float>();
    private readonly Dictionary<int, float> wagerAcceptedAt = new Dictionary<int, float>();
    private readonly List<WagerOffer> onlineWagerOffers = new List<WagerOffer>();
    private bool onlineWagerWindowKnown, onlineWagerRequestInFlight;
    private bool onlineWalletPollInFlight;
    private float nextOnlineWalletPollAt;
    private double onlineOfferDeadline, onlineShooterDeadline;
    private IEnumerable<WagerOffer> ActiveWagerOffers => localDemo ? wagerBook.Offers : onlineWagerOffers;
    private bool CanRequestPointAddOn
    {
        get
        {
            if (gameMode != GameMode.Craps || phase != "Point" || rolling || !shotCommitted) return false;
            foreach (var offer in ActiveWagerOffers)
                if (offer.Status == WagerStatus.Accepted && offer.SourceOfferId == 0 &&
                    offer.From == SelfId && offer.To == shooterId) return true;
            return false;
        }
    }
    private double AcceptanceDeadline => localDemo ? wagerBook.ShooterDeadline : onlineShooterDeadline;
    private double OfferDeadline => localDemo ? wagerBook.OfferDeadline : onlineOfferDeadline;
    private bool AwaitingWagerAcceptance => gameMode == GameMode.Craps && shotCommitted &&
        !rolling && (localDemo ? wagerBook.Started && Time.unscaledTimeAsDouble < AcceptanceDeadline
            : !onlineWagerWindowKnown || Time.unscaledTimeAsDouble < AcceptanceDeadline);

    private void ApplyOnlineWagerSnapshot(OnlineWagerDto[] offers, BettingWindowDto window)
    {
        onlineWagerOffers.Clear();
        if (offers != null)
            foreach (var item in offers)
            {
                if (!Enum.TryParse(item.outcome, true, out WagerOutcome outcome) ||
                    !Enum.TryParse(item.status, true, out WagerStatus status)) continue;
                onlineWagerOffers.Add(new WagerOffer { Id = item.id, From = item.from, To = item.to,
                    Outcome = outcome, Number = item.number, Amount = item.amount, Status = status,
                    Winner = item.winner, SourceOfferId = item.sourceOfferId,
                    AddOnKind = Enum.TryParse(item.addOnKind, true, out WagerAddOnKind addOn) ? addOn : WagerAddOnKind.None });
                if (status == WagerStatus.Accepted && !wagerAcceptedAt.ContainsKey(item.id))
                    wagerAcceptedAt[item.id] = Time.unscaledTime;
            }
        if (window == null) { onlineWagerWindowKnown = false; return; }
        onlineWagerWindowKnown = true;
        nextOnlineWalletPollAt = 0f;
        onlineOfferDeadline = Time.unscaledTimeAsDouble + window.offerRemainingMilliseconds / 1000.0;
        onlineShooterDeadline = Time.unscaledTimeAsDouble + window.shooterRemainingMilliseconds / 1000.0;
    }

    private int WagerFunds(string player)
    {
        if (Array.IndexOf(DemoShooterOrder, player) < 0) return 0;
        int mainStake = shotCommitted && (player == shooterId || player == catcherId) ? shotAmount : 0;
        return Math.Max(0, Balance(player) - mainStake);
    }

    private int ActiveWagerExposure(string player)
    {
        if (localDemo) return wagerBook.Exposure(player);
        int amount = 0;
        foreach (var offer in onlineWagerOffers)
            if ((offer.Status == WagerStatus.Offered && offer.From == player) ||
                (offer.Status == WagerStatus.Accepted && (offer.From == player || offer.To == player)))
                amount += offer.Amount;
        return amount;
    }

    private WagerOffer OfferWager(string from, string to, WagerOutcome outcome, int number, int amount)
    {
        if (gameMode != GameMode.Craps || !shotCommitted || rolling) return null;
        if (Array.IndexOf(DemoShooterOrder, from) < 0 || Array.IndexOf(DemoShooterOrder, to) < 0) return null;
        // The catcher takes the opposing side of the shooter's outcome.
        if ((from == catcherId && outcome != WagerOutcome.Crap) ||
            (to == catcherId && outcome != WagerOutcome.Hit)) return null;
        if (!localDemo)
        {
            if (onlineWagerRequestInFlight || !onlineWagerWindowKnown ||
                Time.unscaledTimeAsDouble >= onlineOfferDeadline || from != SelfId ||
                !playerTokens.TryGetValue(from, out var token)) return null;
            var request = new OnlineWagerOfferRequest { fromId = from, toId = to,
                playerSessionToken = token, outcome = outcome.ToString(), number = number, amount = amount };
            StartCoroutine(PostOnlineWager("/wager/offer", JsonUtility.ToJson(request), true));
            return null;
        }
        try
        {
            var offer = wagerBook.Propose(from, to, outcome, number, amount, Time.unscaledTimeAsDouble, WagerFunds);
            if (to != "p1") botOfferResponseAt[offer.Id] = Time.unscaledTime + DemoOpponentPolicy.ResponseDelay(BotProfile(to), random.NextDouble());
            return offer;
        }
        catch (InvalidOperationException) { return null; }
    }

    private bool AcceptWager(int id, string recipient)
    {
        if (!localDemo)
        {
            if (onlineWagerRequestInFlight || recipient != SelfId ||
                !playerTokens.TryGetValue(recipient, out var token)) return false;
            var request = new OnlineWagerAcceptRequest { recipientId = recipient,
                playerSessionToken = token, offerId = id };
            StartCoroutine(PostOnlineWager("/wager/accept", JsonUtility.ToJson(request), false));
            return false;
        }
        try
        {
            wagerBook.Accept(id, recipient, Time.unscaledTimeAsDouble, WagerFunds);
            if (!wagerAcceptedAt.ContainsKey(id)) wagerAcceptedAt[id] = Time.unscaledTime;
            return true;
        }
        catch (InvalidOperationException) { return false; }
    }

    private void OfferPointAddOn(WagerOffer source, WagerAddOnKind kind) =>
        OfferPointAddOnWithAmount(source, kind, 0);

    private void OfferPointAddOnWithAmount(WagerOffer source, WagerAddOnKind kind, int amount)
    {
        if (source == null || !CanRequestPointAddOn || source.From != SelfId || source.To != shooterId ||
            source.Status != WagerStatus.Accepted || source.SourceOfferId != 0) return;
        if (!localDemo)
        {
            if (onlineWagerRequestInFlight || !playerTokens.TryGetValue(SelfId, out var token)) return;
            var request = new OnlineWagerAddOnRequest { bettorId = SelfId, playerSessionToken = token,
                sourceOfferId = source.Id, kind = kind.ToString(), amount = amount };
            StartCoroutine(PostOnlineWager("/wager/add-on", JsonUtility.ToJson(request), false));
            return;
        }
        try
        {
            var offer = wagerBook.ProposeAddOn(source.Id, SelfId, kind, Time.unscaledTimeAsDouble, WagerFunds, amount);
            if (offer.To != SelfId)
                botOfferResponseAt[offer.Id] = Time.unscaledTime +
                    DemoOpponentPolicy.ResponseDelay(BotProfile(offer.To), random.NextDouble());
            PlayAudio(lockClip, 0.65f);
        }
        catch (InvalidOperationException) { result = "That add-on is not available for this locked bet."; }
    }

    private System.Collections.IEnumerator PostOnlineWager(string route, string json, bool offering)
    {
        onlineWagerRequestInFlight = true;
        bool succeeded = false;
        yield return Post("/api/street-dice/" + gameId + route, json, body =>
        {
            var response = JsonUtility.FromJson<OnlineWagerResponseDto>(body);
            if (response?.state == null) return;
            UpdateState(response.state);
            ApplyOnlineWagerSnapshot(response.wagers, response.bettingWindow);
            succeeded = true;
        });
        onlineWagerRequestInFlight = false;
        if (succeeded)
        {
            if (offering)
            {
                ResetWagerDraft();
                if (wagerOverlayOpen)
                {
                    wagerTarget = CanComposeWagerAgainst(shooterId) ? shooterId : FirstWagerTarget();
                    SetWagerStage(1);
                }
            }
            PlayAudio(lockClip, 0.65f);
        }
    }

    private System.Collections.IEnumerator PollOwnWallet()
    {
        if (!playerTokens.TryGetValue(SelfId, out var token)) yield break;
        onlineWalletPollInFlight = true;
        var request = new OnlineWalletRequest { playerId = SelfId, playerSessionToken = token };
        yield return Post("/api/street-dice/" + gameId + "/wallet", JsonUtility.ToJson(request), body =>
        {
            var wallet = JsonUtility.FromJson<OnlineWalletDto>(body);
            if (wallet?.playerId == SelfId)
            {
                cash[SelfId] = wallet.balance;
                onlineAvailableBalance = wallet.availableBalance;
            }
        });
        onlineWalletPollInFlight = false;
    }

    private void UpdateWagerOffers()
    {
        if (!localDemo) return;
        wagerBook.Expire(Time.unscaledTimeAsDouble);
        if (!localDemo || mainOptions || rolling || !botWagersEnabled) return;
        if (botWagersEnabled && BettingWindowOpen && Time.unscaledTime >= nextOfferAt)
        {
            nextOfferAt = Time.unscaledTime + 1.5f + (float)random.NextDouble();
            var from = DemoShooterOrder[random.Next(1, DemoShooterOrder.Length)];
            if (from != shooterId)
            {
                string to = shooterId == "p1" ? "p1" : DemoShooterOrder[random.Next(DemoShooterOrder.Length)];
                bool hit = to != shooterId && from != catcherId && wagerBook.Point != 0 && random.NextDouble() > 0.4;
                int number = wagerBook.Point == 0 ? 0
                    : random.NextDouble() > 0.5 ? wagerBook.Point : WagerBook.GroupMate(wagerBook.Point);
                int amount = DemoOpponentPolicy.OfferAmount(BotProfile(from), WagerFunds(from) - wagerBook.Exposure(from), random.NextDouble());
                if (to != from && amount > 0) OfferWager(from, to, hit ? WagerOutcome.Hit : WagerOutcome.Crap, number, amount);
            }
        }
        foreach (var offer in wagerBook.Offers)
        {
            if (offer.Status != WagerStatus.Offered) { botOfferResponseAt.Remove(offer.Id); continue; }
            if (offer.To == "p1") continue;
            if (botOfferResponseAt.TryGetValue(offer.Id, out float at) && Time.unscaledTime >= at)
            {
                if (DemoOpponentPolicy.Accept(BotProfile(offer.To), WagerFunds(offer.To) - wagerBook.Exposure(offer.To), offer.Amount, random.NextDouble()))
                    AcceptWager(offer.Id, offer.To);
                botOfferResponseAt.Remove(offer.Id);
            }
        }
    }

    private void PayWagers(IEnumerable<WagerOffer> settled)
    {
        foreach (var offer in settled)
        {
            if (offer.Winner == shooterId) shooterSideWinsThisRoll++;
            TransferCash(offer.Winner == offer.From ? offer.To : offer.From, offer.Winner, offer.Amount);
        }
    }

    private int AcceptedWagerStake(string player)
    {
        int amount = 0;
        foreach (var offer in ActiveWagerOffers)
            if (offer.Status == WagerStatus.Accepted && (offer.From == player || offer.To == player)) amount += offer.Amount;
        return amount;
    }

    private int OfferedWagerStake(string player)
    {
        int amount = 0;
        foreach (var offer in ActiveWagerOffers)
            if (offer.Status == WagerStatus.Offered && offer.From == player) amount += offer.Amount;
        return amount;
    }
}
