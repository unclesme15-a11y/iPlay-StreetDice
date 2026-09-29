namespace IPlayStreetDice.Server.Core;

// Main-bet Double Up during the point (owner rule 2026-09-29). Once a point is set, the
// shooter or the catcher may propose doubling the main bet to the other, between rolls. The
// other side accepts before the next throw or the offer is gone. Accepting means owing the
// full doubled amount -- a Double Up is never split with anyone else. One per shot.
// Example: shooter bets the catcher $100, point is 6, the catcher proposes a Double Up and
// the shooter accepts -- now $200 rides: a seven-out pays the catcher $200, a 6 pays the
// shooter $200. (The after-win Double Up for the NEXT shot is separate and unchanged.)
public sealed partial class StreetDiceGameEngine
{
    public void ProposeMainDoubleUp(string playerId, int betCap = int.MaxValue)
    {
        if (State.Phase != GamePhase.Point || _pendingPhysicalRoll != null)
            throw new InvalidOperationException("A main-bet Double Up needs a live point, between rolls.");
        if (playerId != State.ShooterId && playerId != State.CatcherId)
            throw new InvalidOperationException("Only the shooter or the catcher can double the main bet.");
        if (State.MainDoubleUpTaken || State.MainDoubleUpProposedBy != null)
            throw new InvalidOperationException("This shot already has a Double Up.");
        if (State.ShotAmount > int.MaxValue / 2 || State.ShotAmount * 2 > betCap)
            throw new InvalidOperationException("The Double Up would go over the table's bet cap.");
        EnsureMainDoubleUpCovered();
        State.MainDoubleUpProposedBy = playerId;
        State.Log($"{RequirePlayer(playerId).Name} proposed a Double Up to {State.ShotAmount * 2}.");
    }

    public void AcceptMainDoubleUp(string playerId)
    {
        var from = State.MainDoubleUpProposedBy ?? throw new InvalidOperationException("No Double Up to accept.");
        if (State.Phase != GamePhase.Point || _pendingPhysicalRoll != null)
            throw new InvalidOperationException("The throw already started.");
        string? other = from == State.ShooterId ? State.CatcherId : State.ShooterId;
        if (playerId != other) throw new InvalidOperationException("Only the other side of the main bet can accept.");
        EnsureMainDoubleUpCovered();
        State.ShotAmount *= 2;
        State.MainDoubleUpTaken = true;
        State.MainDoubleUpProposedBy = null;
        State.LastShotWasDoubleUp = true;
        State.Log($"Double Up accepted. The main bet is now {State.ShotAmount}.");
    }

    // Each side must cover the whole doubled amount on their own, on top of their side bets.
    private void EnsureMainDoubleUpCovered()
    {
        int doubled = State.ShotAmount * 2;
        foreach (var player in new[] { State.Shooter, State.Catcher })
            if (player == null || player.HasLeft || player.Balance - _peerWagers.Exposure(player.Id) < doubled)
                throw new InvalidOperationException("Both main-bet players must cover the Double Up.");
    }

    // Not accepted before the throw -> gone.
    private void ExpireMainDoubleUpOffer() => State.MainDoubleUpProposedBy = null;

    private void ResetMainDoubleUp()
    {
        State.MainDoubleUpProposedBy = null;
        State.MainDoubleUpTaken = false;
    }
}
