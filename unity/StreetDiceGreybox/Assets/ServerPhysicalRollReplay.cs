using System;
using UnityEngine;

// Replays the server's committed poses without changing the committed faces.
public sealed class ServerPhysicalRollReplay
{
    [Serializable] private sealed class CommitPayload
    {
        public int[] faces;
        public FramePayload[] frames;
    }

    [Serializable] private sealed class FramePayload
    {
        public float time;
        public DiePayload[] dice;
    }

    [Serializable] private sealed class DiePayload
    {
        public float x, y, z, qx, qy, qz, qw;
    }

    private readonly FramePayload[] frames;
    private readonly float floorY;
    public int[] Faces { get; }
    public int DiceCount => Faces.Length;
    public float Duration => frames[frames.Length - 1].time;
    public Pose FirstPose(int die) => Sample(0f, die);

    private ServerPhysicalRollReplay(CommitPayload payload, float pavementY)
    {
        frames = payload.frames;
        Faces = payload.faces;
        floorY = pavementY;
    }

    public static ServerPhysicalRollReplay Decode(string json, float pavementY)
    {
        if (string.IsNullOrEmpty(json) || !IsFinite(pavementY))
            throw new ArgumentException("Invalid physical replay input.");
        var payload = JsonUtility.FromJson<CommitPayload>(json);
        if (payload == null || payload.faces == null || payload.faces.Length is not (2 or 3)
            || payload.frames == null || payload.frames.Length < 2)
            throw new ArgumentException("Physical replay has no valid frames or faces.");
        for (int i = 0; i < payload.faces.Length; i++)
            if (payload.faces[i] is < 0 or > 6)
                throw new ArgumentException("Physical replay has an invalid face.");
        float lastTime = -1f;
        foreach (var frame in payload.frames)
        {
            if (frame == null || !IsFinite(frame.time) || frame.time <= lastTime
                || frame.dice == null || frame.dice.Length != payload.faces.Length)
                throw new ArgumentException("Physical replay frame clock or die count is invalid.");
            foreach (var die in frame.dice)
            {
                if (die == null || !IsFinite(die.x) || !IsFinite(die.y) || !IsFinite(die.z)
                    || !IsFinite(die.qx) || !IsFinite(die.qy) || !IsFinite(die.qz) || !IsFinite(die.qw))
                    throw new ArgumentException("Physical replay contains an invalid pose.");
                float norm = die.qx * die.qx + die.qy * die.qy + die.qz * die.qz + die.qw * die.qw;
                if (norm < 0.95f || norm > 1.05f)
                    throw new ArgumentException("Physical replay contains a non-unit rotation.");
            }
            lastTime = frame.time;
        }
        if (payload.frames[0].time != 0f)
            throw new ArgumentException("Physical replay must begin at release.");
        return new ServerPhysicalRollReplay(payload, pavementY);
    }

    public Pose Sample(float seconds, int die)
    {
        if (die < 0 || die >= DiceCount || !IsFinite(seconds))
            throw new ArgumentOutOfRangeException(nameof(die));
        if (seconds <= 0f) return ToPose(frames[0].dice[die]);
        if (seconds >= Duration) return ToPose(frames[frames.Length - 1].dice[die]);
        int low = 0, high = frames.Length - 1;
        while (low + 1 < high)
        {
            int mid = (low + high) / 2;
            if (frames[mid].time <= seconds) low = mid;
            else high = mid;
        }
        float t = (seconds - frames[low].time) / (frames[high].time - frames[low].time);
        var a = ToPose(frames[low].dice[die]);
        var b = ToPose(frames[high].dice[die]);
        return new Pose(Vector3.Lerp(a.position, b.position, t), Quaternion.Slerp(a.rotation, b.rotation, t));
    }

    private Pose ToPose(DiePayload die)
        => new Pose(new Vector3(die.x, die.y + floorY, die.z),
            new Quaternion(die.qx, die.qy, die.qz, die.qw));

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
