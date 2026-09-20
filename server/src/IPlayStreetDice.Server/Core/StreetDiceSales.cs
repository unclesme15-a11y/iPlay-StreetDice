namespace IPlayStreetDice.Server.Core;

public sealed record DiceSaleBid(string PlayerId, int Amount, bool IsDefault);

public sealed class DiceSale
{
    public string SellerId { get; init; } = "";
    public string DefaultBuyerId { get; init; } = "";
    public long EndsAtUnixMilliseconds { get; init; }
    public List<DiceSaleBid> Bids { get; } = new();
    public bool IsOpen { get; internal set; } = true;
    public string? WinnerId { get; internal set; }
    public int WinningAmount { get; internal set; }
    public bool ForcedDefaultWon { get; internal set; }
    public bool ComeOutProtectionActive { get; internal set; }
}

public sealed partial class StreetDiceGameEngine
{
    public DiceSale SellDice(string sellerId, DateTimeOffset now)
    {
        if (State.Phase is not (GamePhase.Lobby or GamePhase.ShooterDecision) ||
            State.SideBets.Any(bet => bet.Status == SideBetStatus.Open) || _pendingPhysicalRoll != null)
            throw new InvalidOperationException("Settle the shot before selling the dice.");
        var seller = RequirePlayer(sellerId);
        var holderId = State.ShooterId ?? State.Players.FirstOrDefault(player => !player.HasLeft)?.Id;
        if (seller.HasLeft || seller.Id != holderId)
            throw new InvalidOperationException("Only the player holding the dice may sell.");
        if (State.Players.Count(player => !player.HasLeft) < 3)
            throw new InvalidOperationException("Selling requires three active players so the seller can sit out.");

        int sellerIndex = State.Players.IndexOf(seller);
        var defaultBuyer = State.Catcher is { HasLeft: false } catcher && catcher.Id != seller.Id
            ? catcher
            : Enumerable.Range(1, State.Players.Count)
                .Select(offset => State.Players[(sellerIndex + offset) % State.Players.Count])
                .First(player => !player.HasLeft && player.Id != seller.Id);
        if (AvailableBalance(defaultBuyer.Id) < 1)
            throw new InvalidOperationException("The catcher must have at least $1 to cover the default bid.");

        var sale = new DiceSale
        {
            SellerId = seller.Id,
            DefaultBuyerId = defaultBuyer.Id,
            EndsAtUnixMilliseconds = now.AddSeconds(5).ToUnixTimeMilliseconds()
        };
        sale.Bids.Add(new DiceSaleBid(defaultBuyer.Id, 1, true));
        State.DiceSale = sale;
        State.Phase = GamePhase.SellingDice;
        State.Log($"{seller.Name} offered the dice for a five-second sale. {defaultBuyer.Name} holds the $1 default bid.");
        return sale;
    }

    public DiceSaleBid BidForDice(string bidderId, int amount, DateTimeOffset now)
    {
        var sale = State.DiceSale;
        if (State.Phase != GamePhase.SellingDice || sale is not { IsOpen: true } ||
            now.ToUnixTimeMilliseconds() >= sale.EndsAtUnixMilliseconds)
            throw new InvalidOperationException("The dice sale is closed.");
        var bidder = RequirePlayer(bidderId);
        if (bidder.HasLeft || bidder.Id == sale.SellerId)
            throw new InvalidOperationException("The seller cannot buy their own dice.");
        if (amount < 1 || amount <= sale.Bids.Max(bid => bid.Amount))
            throw new InvalidOperationException("Bid must exceed the current high bid.");
        if (AvailableBalance(bidderId) < amount)
            throw new InvalidOperationException("Not enough play money to cover the bid.");

        var bid = new DiceSaleBid(bidder.Id, amount, false);
        sale.Bids.Add(bid);
        State.Log($"{bidder.Name} bid ${amount} for the dice.");
        return bid;
    }

    public DiceSale? ResolveDiceSale(DateTimeOffset now)
    {
        var sale = State.DiceSale;
        if (State.Phase != GamePhase.SellingDice || sale is not { IsOpen: true } ||
            now.ToUnixTimeMilliseconds() < sale.EndsAtUnixMilliseconds) return sale;
        var seller = RequirePlayer(sale.SellerId);
        var winningBid = sale.Bids.OrderByDescending(bid => bid.Amount)
            .FirstOrDefault(bid => State.FindPlayer(bid.PlayerId) is { HasLeft: false } player &&
                player.Balance >= bid.Amount);
        sale.IsOpen = false;
        if (seller.HasLeft || winningBid == null)
        {
            State.Phase = GamePhase.Lobby;
            State.Log("Dice sale canceled; no eligible buyer remains.");
            return sale;
        }

        var buyer = RequirePlayer(winningBid.PlayerId);
        buyer.Debit(winningBid.Amount);
        seller.Credit(winningBid.Amount);
        sale.WinnerId = buyer.Id;
        sale.WinningAmount = winningBid.Amount;
        sale.ForcedDefaultWon = winningBid.IsDefault;
        sale.ComeOutProtectionActive = winningBid.IsDefault;

        int sellerIndex = State.Players.IndexOf(seller);
        var originalSuccessors = Enumerable.Range(1, State.Players.Count - 1)
            .Select(offset => State.Players[(sellerIndex + offset) % State.Players.Count]).ToList();
        State.Players.Clear();
        State.Players.Add(buyer);
        State.Players.AddRange(originalSuccessors.Where(player => player.Id != buyer.Id));
        State.Players.Add(seller);

        State.ShooterId = buyer.Id;
        State.CatcherId = State.Players.Skip(1).FirstOrDefault(player => !player.HasLeft && player.Id != seller.Id)?.Id;
        State.ShotAmount = 0;
        State.Point = null;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastResolvedShotWasWin = false;
        State.LastShotWasDoubleUp = false;
        State.Streak = winningBid.IsDefault ? 1.5f : 0f;
        State.Phase = GamePhase.Lobby;
        State.LastResolution = new RollResolution(RollResultType.None, null, null,
            $"{buyer.Name} bought the dice for ${winningBid.Amount}.");
        State.Log(State.LastResolution.Message);
        return sale;
    }

    private bool ProtectedSaleComeOut => State.Phase == GamePhase.ComeOut &&
        State.DiceSale is { ComeOutProtectionActive: true } sale && sale.WinnerId == State.ShooterId;
}
