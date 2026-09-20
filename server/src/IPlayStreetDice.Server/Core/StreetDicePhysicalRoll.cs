namespace IPlayStreetDice.Server.Core;

public sealed record PreparedPhysicalRoll(string RollId, DateTimeOffset FadeDeadline, PhysicalDiePose[] LaunchPoses);
public sealed record PublicPendingPhysicalRoll(string RollId, DateTimeOffset FadeDeadline, double RemainingFadeMilliseconds);
public sealed record CommittedPhysicalRoll(string RollId, string ShooterId, PhysicalDiceThrow Throw, RollResolution Resolution);

public sealed partial class StreetDiceGameEngine
{
    private static readonly TimeSpan PhysicalFadeWindow = TimeSpan.FromMilliseconds(1250);
    private PendingPhysicalRoll? _pendingPhysicalRoll;
    private CommittedPhysicalRoll? _lastCommittedPhysicalRoll;

    private sealed record PendingPhysicalRoll(string RollId, string ShooterId, DateTimeOffset FadeDeadline,
        PhysicalDiceThrow Throw);

    public PublicPendingPhysicalRoll? CurrentPhysicalRoll(DateTimeOffset now)
        => _pendingPhysicalRoll == null ? null : new PublicPendingPhysicalRoll(_pendingPhysicalRoll.RollId,
            _pendingPhysicalRoll.FadeDeadline, Math.Max(0, (_pendingPhysicalRoll.FadeDeadline - now).TotalMilliseconds));

    public PreparedPhysicalRoll PreparePhysicalRoll(string shooterId, DiceGesture gesture, DateTimeOffset now,
        int seed, int? launchSeed = null)
    {
        if (!gesture.IsValid) throw new ArgumentOutOfRangeException(nameof(gesture));
        if (State.Phase is not (GamePhase.ComeOut or GamePhase.Point) || State.ShotAmount <= 0)
            throw new InvalidOperationException("A live covered shot is required before throwing.");
        if (!string.Equals(State.ShooterId, shooterId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only the current shooter can throw.");
        if (RequirePlayer(shooterId).HasLeft) throw new InvalidOperationException("Shooter has left.");
        if (_pendingPhysicalRoll != null)
        {
            return new PreparedPhysicalRoll(_pendingPhysicalRoll.RollId, _pendingPhysicalRoll.FadeDeadline,
                _pendingPhysicalRoll.Throw.Frames[0]);
        }

        _peerWagers.BeginRoll(BettingSeconds(now));
        PhysicalDiceThrow throwResult;
        try
        {
            throwResult = ServerDicePhysics.Simulate(gesture, 2, seed, launchSeed);
            if (ProtectedSaleComeOut)
            {
                int attempt = 0;
                while (throwResult.IsCounted && throwResult.Faces.Sum() is 2 or 3 or 12)
                {
                    if (++attempt > 128) throw new InvalidOperationException("Unable to prepare a protected come-out throw.");
                    throwResult = ServerDicePhysics.Simulate(gesture, 2,
                        unchecked(seed + attempt * 104729), launchSeed.HasValue ? unchecked(launchSeed.Value + attempt * 13007) : null);
                }
            }
        }
        catch { _peerWagers.Fade(BettingSeconds(now)); throw; }
        string rollId = Guid.NewGuid().ToString("N");
        var deadline = now + PhysicalFadeWindow;
        _pendingPhysicalRoll = new PendingPhysicalRoll(rollId, shooterId, deadline, throwResult);
        State.Log("Physical throw launched; catcher fade window open.");
        return new PreparedPhysicalRoll(rollId, deadline, throwResult.Frames[0]);
    }

    public RollResolution FadePhysicalRoll(string catcherId, string rollId, DateTimeOffset now)
    {
        var pending = RequirePending(rollId);
        if (now >= pending.FadeDeadline) throw new InvalidOperationException("Fade window has closed.");
        if (!string.Equals(State.CatcherId, catcherId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only the catcher can fade.");
        _pendingPhysicalRoll = null;
        var resolution = FadeCatch(catcherId);
        _peerWagers.Fade(BettingSeconds(now));
        return resolution;
    }

    public CommittedPhysicalRoll CommitPhysicalRoll(string shooterId, string rollId, DateTimeOffset now)
    {
        if (_lastCommittedPhysicalRoll?.RollId == rollId)
        {
            if (!string.Equals(_lastCommittedPhysicalRoll.ShooterId, shooterId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only the shooter can read this committed throw.");
            return _lastCommittedPhysicalRoll;
        }
        var pending = RequirePending(rollId);
        if (!string.Equals(pending.ShooterId, shooterId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only the shooter can commit the throw.");
        if (now < pending.FadeDeadline) throw new InvalidOperationException("Catcher fade window is still open.");
        _pendingPhysicalRoll = null;
        RollResolution resolution;
        if (!pending.Throw.IsCounted)
        {
            _peerWagers.Fade(BettingSeconds(now));
            resolution = new RollResolution(RollResultType.NoCount, null, State.Point,
                "No-count throw. Shooter keeps the dice and throws again.");
            State.LastResolution = resolution;
            State.Log(resolution.Message);
        }
        else
        {
            try { resolution = Roll(new DiceRoll(pending.Throw.Faces[0], pending.Throw.Faces[1]), now); }
            catch { _pendingPhysicalRoll = pending; throw; }
        }
        _lastCommittedPhysicalRoll = new CommittedPhysicalRoll(rollId, shooterId, pending.Throw, resolution);
        return _lastCommittedPhysicalRoll;
    }

    private PendingPhysicalRoll RequirePending(string rollId)
    {
        if (_pendingPhysicalRoll == null || _pendingPhysicalRoll.RollId != rollId)
            throw new InvalidOperationException("Physical throw is not pending.");
        return _pendingPhysicalRoll;
    }
}
