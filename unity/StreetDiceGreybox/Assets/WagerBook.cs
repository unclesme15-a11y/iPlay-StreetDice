#nullable enable
using System;
using System.Collections.Generic;

namespace IPlay.Demo
{
    public enum WagerOutcome { Hit, Crap }
    public enum WagerStatus { Offered, Accepted, Expired, Settled }
    public enum WagerAddOnKind { None, DoubleUp, PairedNumber }

    public sealed class WagerOffer
    {
        public int Id { get; internal set; }
        public string From { get; internal set; } = "";
        public string To { get; internal set; } = "";
        public WagerOutcome Outcome { get; internal set; }
        public int Number { get; internal set; }
        public int Amount { get; internal set; }
        public WagerStatus Status { get; internal set; }
        public string? Winner { get; internal set; }
        public int SourceOfferId { get; internal set; }
        public WagerAddOnKind AddOnKind { get; internal set; }
    }

    // Both clients use caller-supplied monotonic time; the host owns acceptance and settlement.
    public sealed class WagerBook
    {
        private readonly List<WagerOffer> offers = new List<WagerOffer>();
        private int nextId;
        public IReadOnlyList<WagerOffer> Offers => offers;
        public string Shooter { get; private set; } = "";
        public int Point { get; private set; }
        public double OfferDeadline { get; private set; }
        public double ShooterDeadline { get; private set; }
        public bool Started { get; private set; }
        public bool Rolling { get; private set; }

        public void Open(string shooter, int point, double now)
        {
            if (string.IsNullOrEmpty(shooter)) throw new ArgumentException("Shooter required.");
            if (point != 0 && GroupMate(point) == 0) throw new ArgumentException("Invalid point.");
            foreach (var offer in offers)
            {
                if (offer.Status == WagerStatus.Accepted && (Shooter != shooter || Point != point))
                    throw new InvalidOperationException("Resolve accepted wagers before changing the shot.");
            }
            foreach (var offer in offers)
                if (offer.Status == WagerStatus.Offered) offer.Status = WagerStatus.Expired;
            Shooter = shooter;
            Point = point;
            OfferDeadline = now + 10;
            ShooterDeadline = OfferDeadline + 5;
            Started = true;
            Rolling = false;
        }

        public static int GroupMate(int point)
        {
            switch (point) { case 4: return 10; case 10: return 4; case 6: return 8; case 8: return 6; case 5: return 9; case 9: return 5; default: return 0; }
        }

        public bool CanOffer(double now) => Started && !Rolling && now < OfferDeadline;
        public bool CanRoll(double now) => Started && !Rolling && now >= ShooterDeadline;

        public int Exposure(string player)
        {
            int sum = 0;
            foreach (var offer in offers)
                if ((offer.Status == WagerStatus.Offered && offer.From == player) ||
                    (offer.Status == WagerStatus.Accepted && (offer.From == player || offer.To == player)))
                    sum += offer.Amount;
            return sum;
        }

        public WagerOffer Propose(string from, string to, WagerOutcome outcome, int number, int amount,
            double now, Func<string, int> available)
        {
            Expire(now);
            if (!CanOffer(now)) throw new InvalidOperationException("Betting is closed.");
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || from == to)
                throw new InvalidOperationException("Invalid wager participants.");
            if (amount != 1 && amount != 5 && amount != 10 && amount != 20) throw new ArgumentException("Invalid wager amount.");
            if (outcome != WagerOutcome.Hit && outcome != WagerOutcome.Crap) throw new ArgumentException("Invalid outcome.");
            if (from == Shooter && outcome == WagerOutcome.Crap)
                throw new InvalidOperationException("The shooter cannot bet against their own roll.");
            if (outcome == WagerOutcome.Crap && (Point == 0 ? number != 0 :
                number != Point && number != GroupMate(Point)))
                throw new InvalidOperationException("Invalid crap wager target.");
            if (outcome == WagerOutcome.Hit && (to == Shooter || Point == 0 || (number != Point && number != GroupMate(Point))))
                throw new InvalidOperationException("Invalid hit wager.");
            if (available(from) - Exposure(from) < amount) throw new InvalidOperationException("Insufficient funds.");
            foreach (var offer in offers)
                if (offer.From == from && offer.To == to && offer.Outcome == outcome && offer.Number == number &&
                    (offer.Status == WagerStatus.Offered || offer.Status == WagerStatus.Accepted))
                    throw new InvalidOperationException("This outcome already has a live wager.");
            var created = new WagerOffer { Id = ++nextId, From = from, To = to, Outcome = outcome,
                Number = number, Amount = amount, Status = WagerStatus.Offered };
            offers.Add(created);
            return created;
        }

        public WagerOffer ProposeAddOn(int sourceOfferId, string bettor, WagerAddOnKind kind,
            double now, Func<string, int> available, int requestedAmount = 0)
        {
            Expire(now);
            if (!Started || Rolling || Point == 0)
                throw new InvalidOperationException("Add-ons require a live point before the next roll.");
            if (kind != WagerAddOnKind.DoubleUp && kind != WagerAddOnKind.PairedNumber)
                throw new InvalidOperationException("Unknown add-on request.");
            var source = offers.Find(item => item.Id == sourceOfferId);
            if (source == null || source.Status != WagerStatus.Accepted || source.SourceOfferId != 0 ||
                source.From != bettor || source.To != Shooter)
                throw new InvalidOperationException("A locked bet against the shooter is required.");
            int number = kind == WagerAddOnKind.PairedNumber ? GroupMate(source.Number) : source.Number;
            if (number == 0 || (number != Point && number != GroupMate(Point)))
                throw new InvalidOperationException("This bet has no eligible paired number.");
            int amount = kind == WagerAddOnKind.PairedNumber && requestedAmount != 0 ? requestedAmount : source.Amount;
            if (amount != 1 && amount != 5 && amount != 10 && amount != 20)
                throw new ArgumentException("Invalid wager amount.");
            if (available(bettor) - Exposure(bettor) < amount)
                throw new InvalidOperationException("Insufficient funds for the add-on.");
            foreach (var offer in offers)
                if (offer.SourceOfferId == sourceOfferId && offer.AddOnKind == kind &&
                    (offer.Status == WagerStatus.Offered || offer.Status == WagerStatus.Accepted))
                    throw new InvalidOperationException("This add-on has already been requested.");
            var created = new WagerOffer { Id = ++nextId, SourceOfferId = sourceOfferId, AddOnKind = kind,
                From = bettor, To = Shooter, Outcome = source.Outcome, Number = number,
                Amount = amount, Status = WagerStatus.Offered };
            offers.Add(created);
            return created;
        }

        public void Accept(int id, string player, double now, Func<string, int> available)
        {
            Expire(now);
            var offer = offers.Find(item => item.Id == id);
            if (offer == null || offer.To != player) throw new InvalidOperationException("Only the recipient may accept.");
            if (offer.Status == WagerStatus.Accepted) return;
            if (Rolling || offer.Status != WagerStatus.Offered) throw new InvalidOperationException("Offer is unavailable.");
            if (available(player) - Exposure(player) < offer.Amount) throw new InvalidOperationException("Insufficient funds.");
            offer.Status = WagerStatus.Accepted;
        }

        public void Expire(double now)
        {
            foreach (var offer in offers)
                if (offer.SourceOfferId == 0 && offer.Status == WagerStatus.Offered &&
                    now >= (offer.To == Shooter ? ShooterDeadline : OfferDeadline))
                    offer.Status = WagerStatus.Expired;
        }

        public void BeginRoll(double now)
        {
            Expire(now);
            if (!CanRoll(now)) throw new InvalidOperationException("Acceptance countdown is still running.");
            foreach (var offer in offers)
                if (offer.SourceOfferId != 0 && offer.Status == WagerStatus.Offered)
                    offer.Status = WagerStatus.Expired;
            Rolling = true;
        }

        public List<WagerOffer> Resolve(int total)
        {
            if (!Rolling) throw new InvalidOperationException("No live roll.");
            if (total < 2 || total > 12) throw new ArgumentException("Invalid dice total.");
            var settled = new List<WagerOffer>();
            foreach (var offer in offers)
            {
                if (offer.Status != WagerStatus.Accepted) continue;
                bool? wins = null;
                if (Point == 0) wins = total == 2 || total == 3 || total == 12;
                else if (total == 7) wins = offer.Outcome == WagerOutcome.Crap;
                else if (total == Point) wins = offer.Outcome == WagerOutcome.Hit;
                else if (offer.Outcome == WagerOutcome.Crap && total == GroupMate(Point)) wins = false;
                else if (offer.Outcome == WagerOutcome.Hit && total == offer.Number) wins = true;
                if (!wins.HasValue) continue;
                offer.Winner = wins.Value ? offer.From : offer.To;
                offer.Status = WagerStatus.Settled;
                settled.Add(offer);
            }
            Rolling = false;
            return settled;
        }

        public void Fade(double now)
        {
            if (!Rolling) throw new InvalidOperationException("No live roll to fade.");
            Expire(now);
            Rolling = false;
        }

        public List<WagerOffer> Forfeit(string player)
        {
            var settled = new List<WagerOffer>();
            foreach (var offer in offers)
            {
                if (offer.Status == WagerStatus.Offered && (player == Shooter || offer.From == player || offer.To == player))
                    offer.Status = WagerStatus.Expired;
                if (offer.Status != WagerStatus.Accepted) continue;
                if (player == Shooter) offer.Winner = offer.Outcome == WagerOutcome.Crap ? offer.From : offer.To;
                else if (offer.From == player || offer.To == player) offer.Winner = offer.From == player ? offer.To : offer.From;
                else continue;
                offer.Status = WagerStatus.Settled;
                settled.Add(offer);
            }
            return settled;
        }
    }
}
