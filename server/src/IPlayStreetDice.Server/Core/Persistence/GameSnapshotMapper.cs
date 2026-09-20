namespace IPlayStreetDice.Server.Core.Persistence;

public static class GameSnapshotMapper
{
    public static GameSnapshot ToSnapshot(this StreetDiceGameState state)
    {
        return new GameSnapshot
        {
            GameId = state.GameId,
            Phase = state.Phase,
            Players = state.Players.Select(p => new PlayerSnapshot
            {
                Id = p.Id,
                Name = p.Name,
                DiceColor = p.DiceColor,
                Balance = p.Balance,
                HasLeft = p.HasLeft
            }).ToList(),
            SideBets = state.SideBets.Select(b => new SideBetSnapshot
            {
                Id = b.Id,
                PlayerId = b.PlayerId,
                Type = b.Type,
                Amount = b.Amount,
                PointGroup = b.PointGroup,
                Status = b.Status
            }).ToList(),
            ShooterId = state.ShooterId,
            CatcherId = state.CatcherId,
            ShotAmount = state.ShotAmount,
            Point = state.Point,
            FadeCount = state.FadeCount,
            ShooterMomentum = state.ShooterMomentum,
            Streak = state.Streak,
            HotDiceThreshold = state.HotDiceThreshold,
            LastResolvedShotWasWin = state.LastResolvedShotWasWin,
            LastShotWasDoubleUp = state.LastShotWasDoubleUp,
            LastResolution = state.LastResolution,
            EventLog = new List<string>(state.EventLog)
        };
    }

    public static StreetDiceGameEngine ToEngine(this GameSnapshot snapshot)
    {
        var state = new StreetDiceGameState
        {
            GameId = snapshot.GameId,
            HotDiceThreshold = snapshot.HotDiceThreshold,
            Phase = snapshot.Phase,
            ShooterId = snapshot.ShooterId,
            CatcherId = snapshot.CatcherId,
            ShotAmount = snapshot.ShotAmount,
            Point = snapshot.Point,
            FadeCount = snapshot.FadeCount,
            ShooterMomentum = snapshot.ShooterMomentum,
            Streak = snapshot.Streak,
            LastResolvedShotWasWin = snapshot.LastResolvedShotWasWin,
            LastShotWasDoubleUp = snapshot.LastShotWasDoubleUp,
            LastResolution = snapshot.LastResolution
        };

        foreach (var player in snapshot.Players)
        {
            state.Players.Add(new StreetDicePlayer(player.Id, player.Name, player.DiceColor, player.Balance)
            {
                HasLeft = player.HasLeft
            });
        }

        foreach (var bet in snapshot.SideBets)
        {
            state.SideBets.Add(new SideBet(bet.Id, bet.PlayerId, bet.Type, bet.Amount, bet.PointGroup, bet.Status));
        }

        foreach (var entry in snapshot.EventLog)
        {
            state.EventLog.Add(entry);
        }

        return new StreetDiceGameEngine(state);
    }
}
