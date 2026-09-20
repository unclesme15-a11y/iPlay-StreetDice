namespace IPlayStreetDice.Server.Core;

public sealed record PhysicalDieFrame(float X, float Y, float Z, float Qx, float Qy, float Qz, float Qw);
public sealed record PhysicalRollFrame(float Time, PhysicalDieFrame[] Dice);

public static class PhysicalRollTransport
{
    public static PhysicalDieFrame[] Launch(PhysicalDiePose[] poses)
    {
        var dice = new PhysicalDieFrame[poses.Length];
        for (int i = 0; i < poses.Length; i++) dice[i] = Die(poses[i]);
        return dice;
    }

    public static IReadOnlyList<PhysicalRollFrame> Frames(PhysicalDiceThrow physicalThrow)
    {
        var result = new List<PhysicalRollFrame>((physicalThrow.Frames.Count + 1) / 2 + 1);
        for (int step = 0; step < physicalThrow.Frames.Count; step += 2)
            result.Add(Frame(physicalThrow, step));
        int last = physicalThrow.Frames.Count - 1;
        if (last % 2 != 0) result.Add(Frame(physicalThrow, last));
        return result;
    }

    private static PhysicalRollFrame Frame(PhysicalDiceThrow physicalThrow, int step)
    {
        var poses = physicalThrow.Frames[step];
        var dice = new PhysicalDieFrame[poses.Length];
        for (int i = 0; i < poses.Length; i++) dice[i] = Die(poses[i]);
        return new PhysicalRollFrame(step * ServerDicePhysics.Step, dice);
    }

    private static PhysicalDieFrame Die(PhysicalDiePose pose)
    {
        var p = pose.Position;
        var q = pose.Rotation;
        return new PhysicalDieFrame(p.X, p.Y, p.Z, q.X, q.Y, q.Z, q.W);
    }
}
