using IPlay.Demo;

namespace IPlayStreetDice.Server.Core;

public sealed partial class StreetDiceGameEngine
{
    private int _shooterSideWinsThisRoll;

    public StreetDiceGameEngine(string gameId)
    {
        State = new StreetDiceGameState { GameId = gameId };
    }

    public StreetDiceGameState State { get; }

    public StreetDicePlayer AddPlayer(string playerId, string name)
    {
        if (State.Phase != GamePhase.Lobby) throw new InvalidOperationException("Players can only join during lobby.");
        if (State.Players.Count >= 5) throw new InvalidOperationException("Street Dice supports at most 5 players.");
        if (State.FindPlayer(playerId) != null) throw new InvalidOperationException("Player already joined.");

        var player = new StreetDicePlayer(playerId, name);
        State.Players.Add(player);
        State.Log($"{name} joined.");
        return player;
    }

    public IReadOnlyList<StreetDicePlayer> FillBots(int targetPlayers)
    {
        if (targetPlayers is < 2 or > 5) throw new ArgumentOutOfRangeException(nameof(targetPlayers));

        var added = new List<StreetDicePlayer>();
        while (State.Players.Count < targetPlayers)
        {
            var number = State.Players.Count + 1;
            var bot = AddPlayer($"bot-{number}", $"Bot {number}");
            added.Add(bot);
        }

        return added;
    }

    public void SelectDiceColor(string playerId, DiceColor color)
    {
        var player = RequirePlayer(playerId);
        player.SelectDiceColor(color);
        State.Log($"{player.Name} selected {color} dice.");
    }

    public void OpenShot(string shooterId, string catcherId, int amount, DateTimeOffset? now = null)
    {
        if (State.Phase is GamePhase.ComeOut or GamePhase.Point or GamePhase.SellingDice ||
            State.SideBets.Any(b => b.Status == SideBetStatus.Open))
            throw new InvalidOperationException("Settle the current shot before opening another.");
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        var shooter = RequirePlayer(shooterId);
        var catcher = RequirePlayer(catcherId);
        var holder = State.ShooterId ?? State.Players.FirstOrDefault(p => !p.HasLeft)?.Id;
        if (!string.Equals(holder, shooter.Id, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only the player holding the dice can open a shot.");
        if (shooter.HasLeft || catcher.HasLeft) throw new InvalidOperationException("Player has left.");
        if (shooter.Id == catcher.Id) throw new InvalidOperationException("Shooter must shoot against another player.");
        if (State.DiceSale is { IsOpen: false } sale && sale.WinnerId == shooter.Id && sale.SellerId == catcher.Id)
            throw new InvalidOperationException("The seller sits out the buyer's main bet.");
        EnsureMainStakeCovered(shooter, catcher, amount);

        State.ShooterId = shooter.Id;
        State.CatcherId = catcher.Id;
        State.ShotAmount = amount;
        State.Point = null;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastResolvedShotWasWin = false;
        State.LastShotWasDoubleUp = false;
        State.Phase = GamePhase.ComeOut;
        State.LastResolution = new RollResolution(RollResultType.None, null, null, "Shot opened.");
        _peerWagers.Open(shooter.Id, 0, BettingSeconds(now ?? DateTimeOffset.UtcNow));
        State.Log($"{shooter.Name} is shooting {amount} against {catcher.Name}.");
    }

    public void PassDice(string playerId)
    {
        var player = RequirePlayer(playerId);
        if (player.HasLeft) throw new InvalidOperationException("Player has left.");
        if (State.Phase is GamePhase.ComeOut or GamePhase.Point or GamePhase.SellingDice ||
            State.SideBets.Any(b => b.Status == SideBetStatus.Open))
            throw new InvalidOperationException("Cannot pass an unsettled wager.");
        var current = State.ShooterId ?? State.Players.First(p => !p.HasLeft).Id;
        if (!string.Equals(current, playerId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only the player holding the dice can pass.");
        OfferNextPlayer(playerId);
        State.Log($"{player.Name} passed and may continue side betting.");
    }

    private void OfferNextPlayer(string previousId)
    {
        var start = State.Players.FindIndex(p => p.Id == previousId);
        var next = Enumerable.Range(1, State.Players.Count)
            .Select(offset => State.Players[(start + offset) % State.Players.Count])
            .FirstOrDefault(p => !p.HasLeft && p.Id != previousId);
        State.ShooterId = next?.Id;
        State.CatcherId = null;
        State.Point = null;
        State.Streak = 0;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastResolvedShotWasWin = false;
        State.LastShotWasDoubleUp = false;
        State.Phase = GamePhase.Lobby;
    }

    public void LeaveGame(string playerId)
    {
        var player = RequirePlayer(playerId);
        if (_pendingPhysicalRoll != null && (playerId == State.ShooterId || playerId == State.CatcherId))
            _pendingPhysicalRoll = null;
        if (player.HasLeft) return; // A repeated leave must never charge the player twice.
        if (_peerWagers.Started) SettlePeerWagers(_peerWagers.Forfeit(playerId));
        bool shooterLeaving = State.ShooterId == playerId;
        if (shooterLeaving && State.Phase is GamePhase.ComeOut or GamePhase.Point)
        {
            var catcher = State.Catcher ?? throw new InvalidOperationException("Catcher missing.");
            player.Debit(State.ShotAmount);
            catcher.Credit(State.ShotAmount);
            foreach (var bet in State.SideBets.Where(b => b.Status == SideBetStatus.Open))
            {
                bool wins = bet.PlayerId != playerId && (State.Phase == GamePhase.Point
                    ? bet.Type is SideBetType.MissPoint or SideBetType.MissPointGroup
                    : bet.Type == SideBetType.ComeOutLoss);
                ResolveSideBet(bet, wins);
            }
            State.LastResolution = new RollResolution(RollResultType.ShooterForfeitLoss, null, null,
                "Shooter left with a live wager. Crap/forfeit loss; wagers settled.");
        }
        else
        {
            foreach (var bet in State.SideBets.Where(b => b.Status == SideBetStatus.Open && b.PlayerId == playerId))
                ResolveSideBet(bet, false);
            if (State.CatcherId == playerId && State.Phase is GamePhase.ComeOut or GamePhase.Point)
            {
                player.Debit(State.ShotAmount);
                State.Shooter!.Credit(State.ShotAmount);
                // The main opponent forfeited; other participants' accepted side bets stay live.
                State.ShotAmount = 0;
            }
        }
        player.HasLeft = true;
        if (shooterLeaving) OfferNextPlayer(playerId);
        if (shooterLeaving && State.DiceSale is { IsOpen: true } sale) sale.IsOpen = false;
        State.Log($"{player.Name} left the game.");
    }

    public SideBet PlaceSideBet(string playerId, SideBetType type, int amount, int? targetPointNumber = null)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (RequirePlayer(playerId).HasLeft) throw new InvalidOperationException("Player has left.");
        if (State.Phase is not (GamePhase.ComeOut or GamePhase.Point))
        {
            throw new InvalidOperationException("Side bets are only available while a shot is live.");
        }

        if (State.Phase == GamePhase.ComeOut && type is SideBetType.HitPoint or SideBetType.MissPoint or SideBetType.HitPointGroup or SideBetType.MissPointGroup)
        {
            throw new InvalidOperationException("Hit/miss point side bets require an established point.");
        }

        if (State.Phase == GamePhase.Point && type is SideBetType.ComeOutWin or SideBetType.ComeOutLoss)
        {
            throw new InvalidOperationException("Come-out side bets are closed after point is established.");
        }

        var pointGroup = ResolvePointGroupForBet(type, targetPointNumber);
        var sideBet = new SideBet(Guid.NewGuid().ToString("N"), playerId, type, amount, pointGroup);
        State.SideBets.Add(sideBet);
        var target = pointGroup == null ? "" : $" ({pointGroup})";
        State.Log($"{playerId} side bet {amount} on {type}{target}.");
        return sideBet;
    }

    public RollResolution FadeCatch(string catcherId)
    {
        if (_pendingPhysicalRoll != null)
            throw new InvalidOperationException("Use the pending physical roll ID to fade this throw.");
        if (State.Phase is not (GamePhase.ComeOut or GamePhase.Point))
        {
            throw new InvalidOperationException("Fade/Catch is only available while a roll is live.");
        }

        if (!string.Equals(State.CatcherId, catcherId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only the Catcher can fade/catch the roll.");
        }

        State.FadeCount++;
        if (State.FadeCount > 3) State.ShooterMomentum++;

        var message = State.FadeCount > 3
            ? $"Faded. Shooter momentum increased to {State.ShooterMomentum}. Shoot again."
            : $"Faded x{State.FadeCount}. Shoot again.";

        var resolution = new RollResolution(RollResultType.Faded, null, State.Point, message);
        State.LastResolution = resolution;
        State.Log(message);
        return resolution;
    }

    public RollResolution Roll(DiceRoll roll, DateTimeOffset? now = null)
    {
        if (_pendingPhysicalRoll != null) throw new InvalidOperationException("A physical throw is awaiting fade or commit.");
        if (!roll.IsValid) throw new ArgumentOutOfRangeException(nameof(roll), "Dice must be in the 1-6 range.");
        if (State.Phase is not (GamePhase.ComeOut or GamePhase.Point))
        {
            throw new InvalidOperationException("Roll is only available while a shot is live.");
        }
        if (ProtectedSaleComeOut && roll.Total is 2 or 3 or 12)
            throw new InvalidOperationException("The forced $1 buyer's come-out protection is active.");
        if (_peerWagers.Started && !_peerWagers.Rolling &&
            _peerWagers.Offers.Any(offer => offer.Status == WagerStatus.Accepted))
            _peerWagers.BeginRoll(BettingSeconds(now ?? DateTimeOffset.UtcNow));

        _shooterSideWinsThisRoll = 0;
        var resolution = State.Phase == GamePhase.ComeOut
            ? ResolveComeOut(roll)
            : ResolvePointRoll(roll);
        if (_peerWagers.Rolling) SettlePeerWagers(_peerWagers.Resolve(roll.Total));
        if (resolution.Result is not (RollResultType.ShooterComeOutLoss or RollResultType.ShooterSevenOutLoss))
            State.Streak = Math.Min(State.HotDiceThreshold, State.Streak + Math.Min(_shooterSideWinsThisRoll, 2) * 0.5f);
        if (State.Phase == GamePhase.Point && _peerWagers.Started)
            _peerWagers.Open(State.ShooterId!, State.Point!.Value, BettingSeconds(now ?? DateTimeOffset.UtcNow));
        return resolution;
    }

    public void RunSame(string shooterId)
    {
        EnsureShooterDecision(shooterId);
        EnsureMainStakeCovered(State.Shooter!, State.Catcher!, State.ShotAmount);
        State.Point = null;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastShotWasDoubleUp = false;
        State.LastResolvedShotWasWin = false;
        State.Phase = GamePhase.ComeOut;
        State.LastResolution = new RollResolution(RollResultType.None, null, null, "Run Same selected.");
        _peerWagers.Open(shooterId, 0, BettingSeconds(DateTimeOffset.UtcNow));
        State.Log($"Run Same for {State.ShotAmount}.");
    }

    public void DoubleUp(string shooterId)
    {
        EnsureShooterDecision(shooterId);
        if (!State.LastResolvedShotWasWin) throw new InvalidOperationException("Double Up is only available after a win.");
        if (State.ShotAmount > int.MaxValue / 2) throw new InvalidOperationException("Double Up exceeds the wager limit.");
        EnsureMainStakeCovered(State.Shooter!, State.Catcher!, State.ShotAmount * 2);

        State.ShotAmount *= 2;
        State.Point = null;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastShotWasDoubleUp = true;
        State.LastResolvedShotWasWin = false;
        State.Phase = GamePhase.ComeOut;
        State.LastResolution = new RollResolution(RollResultType.None, null, null, "Double Up selected.");
        _peerWagers.Open(shooterId, 0, BettingSeconds(DateTimeOffset.UtcNow));
        State.Log($"Double Up. Next shot is {State.ShotAmount}.");
    }

    private static void EnsureMainStakeCovered(StreetDicePlayer shooter, StreetDicePlayer catcher, int amount)
    {
        if (amount <= 0 || shooter.HasLeft || catcher.HasLeft ||
            shooter.Balance < amount || catcher.Balance < amount)
            throw new InvalidOperationException("Both main-bet players must cover the wager.");
    }

    public RollResolution AdvanceBotAction(Random random)
    {
        if (State.Phase == GamePhase.Lobby)
        {
            if (State.Players.Count < 2) throw new InvalidOperationException("At least two players are required.");
            var next = State.ShooterId ?? State.Players.First(p => !p.HasLeft).Id;
            OpenShot(next, State.Players.First(p => !p.HasLeft && p.Id != next).Id, 20);
            return State.LastResolution;
        }

        if (State.Phase == GamePhase.ShooterDecision)
        {
            var shooter = State.Shooter ?? throw new InvalidOperationException("Shooter is missing.");
            if (State.LastResolvedShotWasWin && State.ShotAmount <= 80)
            {
                DoubleUp(shooter.Id);
            }
            else
            {
                RunSame(shooter.Id);
            }

            return State.LastResolution;
        }

        DiceRoll botRoll;
        do { botRoll = new DiceRoll(random.Next(1, 7), random.Next(1, 7)); }
        while (ProtectedSaleComeOut && botRoll.Total is 2 or 3 or 12);
        return Roll(botRoll);
    }

    private RollResolution ResolveComeOut(DiceRoll roll)
    {
        if (roll.Total is 7 or 11)
        {
            return ResolveShooterWin(RollResultType.ShooterComeOutWin, roll, "Come-out win.");
        }

        if (roll.Total is 2 or 3 or 12)
        {
            return ResolveShooterLoss(RollResultType.ShooterComeOutLoss, roll, "Come-out loss. Shooter keeps dice.");
        }

        State.Point = roll.Total;
        State.Phase = GamePhase.Point;
        if (State.DiceSale != null) State.DiceSale.ComeOutProtectionActive = false;
        ResolveSideBets(SideBetType.ComeOutWin, false);
        ResolveSideBets(SideBetType.ComeOutLoss, false);
        var resolution = new RollResolution(RollResultType.PointEstablished, roll, State.Point, $"Point established: {State.Point}.");
        State.LastResolution = resolution;
        State.Log(resolution.Message);
        return resolution;
    }

    private RollResolution ResolvePointRoll(DiceRoll roll)
    {
        if (State.Point == null) throw new InvalidOperationException("Point phase requires a point.");

        if (roll.Total == State.Point)
        {
            return ResolveShooterWin(RollResultType.ShooterPointWin, roll, "Point hit.");
        }

        if (roll.Total == 7)
        {
            return ResolveShooterLoss(RollResultType.ShooterSevenOutLoss, roll, "Seven out.");
        }

        ResolvePointGroupSideBets(roll.Total);
        var resolution = new RollResolution(RollResultType.None, roll, State.Point, $"Rolled {roll.Total}. Shoot again.");
        State.LastResolution = resolution;
        State.Log(resolution.Message);
        return resolution;
    }

    private RollResolution ResolveShooterWin(RollResultType resultType, DiceRoll roll, string message)
    {
        var shooter = State.Shooter ?? throw new InvalidOperationException("Shooter is missing.");
        var catcher = State.Catcher ?? throw new InvalidOperationException("Catcher is missing.");
        catcher.Debit(State.ShotAmount);
        shooter.Credit(State.ShotAmount);

        var streakGain = resultType == RollResultType.ShooterPointWin ? 2 : 1;
        streakGain += State.ShooterMomentum;
        if (State.LastShotWasDoubleUp) streakGain += 3;
        State.Streak = Math.Min(State.HotDiceThreshold, State.Streak + streakGain);

        ResolveSideBets(SideBetType.ComeOutWin, resultType == RollResultType.ShooterComeOutWin);
        ResolveSideBets(SideBetType.ComeOutLoss, false);
        ResolveSideBets(SideBetType.HitPoint, resultType == RollResultType.ShooterPointWin);
        ResolveSideBets(SideBetType.MissPoint, false);
        if (resultType == RollResultType.ShooterPointWin && State.Point != null)
        {
            ResolvePointGroupSideBets(State.Point.Value);
        }

        State.Point = null;
        State.Phase = GamePhase.ShooterDecision;
        State.LastResolvedShotWasWin = true;
        var resolution = new RollResolution(resultType, roll, null, $"{message} Shooter wins {State.ShotAmount}.");
        State.LastResolution = resolution;
        State.Log(resolution.Message);
        return resolution;
    }

    private RollResolution ResolveShooterLoss(RollResultType resultType, DiceRoll roll, string message)
    {
        var shooter = State.Shooter ?? throw new InvalidOperationException("Shooter is missing.");
        var catcher = State.Catcher ?? throw new InvalidOperationException("Catcher is missing.");
        shooter.Debit(State.ShotAmount);
        catcher.Credit(State.ShotAmount);

        ResolveSideBets(SideBetType.ComeOutWin, false);
        ResolveSideBets(SideBetType.ComeOutLoss, resultType == RollResultType.ShooterComeOutLoss);
        ResolveSideBets(SideBetType.HitPoint, false);
        ResolveSideBets(SideBetType.MissPoint, resultType == RollResultType.ShooterSevenOutLoss);
        if (resultType == RollResultType.ShooterSevenOutLoss)
        {
            ResolveGroupedSideBets(null, hitGroup: false);
        }

        State.Point = null;
        State.Streak = 0;
        State.LastShotWasDoubleUp = false;
        State.LastResolvedShotWasWin = false;

        var next = resultType == RollResultType.ShooterSevenOutLoss
            ? HandOffDiceClockwise(shooter)
            : KeepDiceAfterLoss();

        var resolution = new RollResolution(resultType, roll, null, $"{message} Shooter loses {State.ShotAmount}. Streak reset.{next}");
        State.LastResolution = resolution;
        State.Log(resolution.Message);
        return resolution;
    }

    private string KeepDiceAfterLoss()
    {
        State.Phase = GamePhase.ShooterDecision;
        return " Shooter keeps dice.";
    }

    private string HandOffDiceClockwise(StreetDicePlayer shooter)
    {
        var start = State.Players.FindIndex(player => player.Id == shooter.Id);
        var next = Enumerable.Range(1, State.Players.Count)
            .Select(offset => State.Players[(start + offset) % State.Players.Count])
            .FirstOrDefault(player => !player.HasLeft && player.Id != shooter.Id);
        if (next == null)
        {
            OfferNextPlayer(shooter.Id);
            return " Dice offered to the next remaining player.";
        }
        State.ShooterId = next.Id;
        State.CatcherId = shooter.Id;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastShotWasDoubleUp = false;
        State.Phase = GamePhase.Lobby;
        return $" Dice pass clockwise to {next.Name}; {shooter.Name} is now Catcher.";
    }

    private void ResolveSideBets(SideBetType type, bool winningType)
    {
        foreach (var sideBet in State.SideBets.Where(b => b.Status == SideBetStatus.Open && b.Type == type))
        {
            ResolveSideBet(sideBet, winningType);
        }
    }

    private PointNumberGroup? ResolvePointGroupForBet(SideBetType type, int? targetPointNumber)
    {
        if (type is not (SideBetType.HitPointGroup or SideBetType.MissPointGroup))
        {
            if (targetPointNumber != null) throw new InvalidOperationException("Target point number is only valid for grouped point bets.");
            return null;
        }

        if (State.Point == null) throw new InvalidOperationException("Grouped point bets require an established point.");

        var activeGroup = PointGroups.FromPointNumber(State.Point.Value);
        var requestedGroup = targetPointNumber == null
            ? activeGroup
            : PointGroups.FromPointNumber(targetPointNumber.Value);

        if (requestedGroup != activeGroup)
        {
            throw new InvalidOperationException("Grouped point bet target must match the shooter's active point group.");
        }

        return requestedGroup;
    }

    private void ResolvePointGroupSideBets(int rolledTotal)
    {
        if (State.Point == null) return;

        var activeGroup = PointGroups.FromPointNumber(State.Point.Value);
        if (!PointGroups.Contains(activeGroup, rolledTotal)) return;

        ResolveGroupedSideBets(activeGroup, hitGroup: true);
    }

    private void ResolveGroupedSideBets(PointNumberGroup? group, bool hitGroup)
    {
        foreach (var sideBet in State.SideBets.Where(b =>
            b.Status == SideBetStatus.Open
            && b.Type is SideBetType.HitPointGroup or SideBetType.MissPointGroup
            && (group == null || b.PointGroup == group)))
        {
            var winningType = sideBet.Type == SideBetType.HitPointGroup
                ? hitGroup
                : !hitGroup;
            ResolveSideBet(sideBet, winningType);
        }
    }

    private void ResolveSideBet(SideBet sideBet, bool winningType)
    {
        if (winningType)
        {
            sideBet.Win();
            if (sideBet.PlayerId == State.ShooterId) _shooterSideWinsThisRoll++;
            State.FindPlayer(sideBet.PlayerId)?.Credit(sideBet.Amount);
            if (sideBet.PlayerId != State.ShooterId) State.Shooter?.Debit(sideBet.Amount);
        }
        else
        {
            sideBet.Lose();
            State.FindPlayer(sideBet.PlayerId)?.Debit(sideBet.Amount);
            if (sideBet.PlayerId != State.ShooterId) State.Shooter?.Credit(sideBet.Amount);
        }
    }

    private StreetDicePlayer RequirePlayer(string playerId)
    {
        return State.FindPlayer(playerId) ?? throw new InvalidOperationException("Player not found.");
    }

    private void EnsureShooterDecision(string shooterId)
    {
        if (State.Phase != GamePhase.ShooterDecision)
        {
            throw new InvalidOperationException("Shooter decision is only available after a resolved shot.");
        }

        if (!string.Equals(State.ShooterId, shooterId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only the Shooter can choose the next shot.");
        }
    }
}
