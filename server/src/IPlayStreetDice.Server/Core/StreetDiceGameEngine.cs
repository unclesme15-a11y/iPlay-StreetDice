namespace IPlayStreetDice.Server.Core;

public sealed class StreetDiceGameEngine
{
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

    public void OpenShot(string shooterId, string catcherId, int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        var shooter = RequirePlayer(shooterId);
        var catcher = RequirePlayer(catcherId);
        if (shooter.Id == catcher.Id) throw new InvalidOperationException("Shooter must shoot against another player.");

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
        State.Log($"{shooter.Name} is shooting {amount} against {catcher.Name}.");
    }

    public SideBet PlaceSideBet(string playerId, SideBetType type, int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        RequirePlayer(playerId);
        if (State.Phase is not (GamePhase.ComeOut or GamePhase.Point))
        {
            throw new InvalidOperationException("Side bets are only available while a shot is live.");
        }

        if (State.Phase == GamePhase.ComeOut && type is SideBetType.HitPoint or SideBetType.MissPoint)
        {
            throw new InvalidOperationException("Hit/miss point side bets require an established point.");
        }

        if (State.Phase == GamePhase.Point && type is SideBetType.ComeOutWin or SideBetType.ComeOutLoss)
        {
            throw new InvalidOperationException("Come-out side bets are closed after point is established.");
        }

        var sideBet = new SideBet(Guid.NewGuid().ToString("N"), playerId, type, amount);
        State.SideBets.Add(sideBet);
        State.Log($"{playerId} side bet {amount} on {type}.");
        return sideBet;
    }

    public RollResolution FadeCatch(string catcherId)
    {
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

    public RollResolution Roll(DiceRoll roll)
    {
        if (!roll.IsValid) throw new ArgumentOutOfRangeException(nameof(roll), "Dice must be in the 1-6 range.");
        if (State.Phase is not (GamePhase.ComeOut or GamePhase.Point))
        {
            throw new InvalidOperationException("Roll is only available while a shot is live.");
        }

        return State.Phase == GamePhase.ComeOut
            ? ResolveComeOut(roll)
            : ResolvePointRoll(roll);
    }

    public void RunSame(string shooterId)
    {
        EnsureShooterDecision(shooterId);
        State.Point = null;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastShotWasDoubleUp = false;
        State.LastResolvedShotWasWin = false;
        State.Phase = GamePhase.ComeOut;
        State.LastResolution = new RollResolution(RollResultType.None, null, null, "Run Same selected.");
        State.Log($"Run Same for {State.ShotAmount}.");
    }

    public void DoubleUp(string shooterId)
    {
        EnsureShooterDecision(shooterId);
        if (!State.LastResolvedShotWasWin) throw new InvalidOperationException("Double Up is only available after a win.");

        State.ShotAmount *= 2;
        State.Point = null;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastShotWasDoubleUp = true;
        State.LastResolvedShotWasWin = false;
        State.Phase = GamePhase.ComeOut;
        State.LastResolution = new RollResolution(RollResultType.None, null, null, "Double Up selected.");
        State.Log($"Double Up. Next shot is {State.ShotAmount}.");
    }

    public RollResolution AdvanceBotAction(Random random)
    {
        if (State.Phase == GamePhase.Lobby)
        {
            if (State.Players.Count < 2) throw new InvalidOperationException("At least two players are required.");
            OpenShot(State.Players[0].Id, State.Players[1].Id, 20);
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

        return Roll(new DiceRoll(random.Next(1, 7), random.Next(1, 7)));
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
        if (State.LastShotWasDoubleUp) streakGain += 1;
        State.Streak += streakGain;

        ResolveSideBets(SideBetType.ComeOutWin, resultType == RollResultType.ShooterComeOutWin);
        ResolveSideBets(SideBetType.ComeOutLoss, false);
        ResolveSideBets(SideBetType.HitPoint, resultType == RollResultType.ShooterPointWin);
        ResolveSideBets(SideBetType.MissPoint, false);

        State.Point = null;
        State.Phase = GamePhase.ShooterDecision;
        State.LastResolvedShotWasWin = true;
        var hot = State.HotDiceActive ? " Hot dice active." : "";
        var resolution = new RollResolution(resultType, roll, null, $"{message} Shooter wins {State.ShotAmount}. Streak +{streakGain}.{hot}");
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

        State.Point = null;
        State.Streak = 0;
        State.LastShotWasDoubleUp = false;
        State.LastResolvedShotWasWin = false;

        var next = resultType == RollResultType.ShooterSevenOutLoss
            ? HandOffDiceToCatcher(shooter, catcher)
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

    private string HandOffDiceToCatcher(StreetDicePlayer shooter, StreetDicePlayer catcher)
    {
        State.ShooterId = catcher.Id;
        State.CatcherId = shooter.Id;
        State.FadeCount = 0;
        State.ShooterMomentum = 0;
        State.LastShotWasDoubleUp = false;
        State.Phase = GamePhase.ComeOut;
        return $" Dice pass to {catcher.Name}; {shooter.Name} is now Catcher.";
    }

    private void ResolveSideBets(SideBetType type, bool winningType)
    {
        foreach (var sideBet in State.SideBets.Where(b => b.Status == SideBetStatus.Open && b.Type == type))
        {
            if (winningType)
            {
                sideBet.Win();
                State.FindPlayer(sideBet.PlayerId)?.Credit(sideBet.Amount);
            }
            else
            {
                sideBet.Lose();
                State.FindPlayer(sideBet.PlayerId)?.Debit(sideBet.Amount);
            }
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
