using System;
using System.Globalization;
using UnityEngine;

public sealed class ServerPhysicalLaunch
{
    [Serializable] private sealed class Payload
    {
        public string rollId;
        public string fadeDeadline;
        public float remainingFadeMilliseconds;
        public DiePayload[] launchPoses;
    }

    [Serializable] private sealed class DiePayload
    {
        public float x, y, z, qx, qy, qz, qw;
    }

    public string RollId { get; }
    public DateTimeOffset FadeDeadline { get; }
    public float RemainingFadeSeconds { get; }
    public Pose[] Dice { get; }
    public int DiceCount => Dice.Length;

    private ServerPhysicalLaunch(string rollId, DateTimeOffset deadline, float remainingFadeSeconds, Pose[] dice)
    {
        RollId = rollId;
        FadeDeadline = deadline;
        RemainingFadeSeconds = remainingFadeSeconds;
        Dice = dice;
    }

    public static ServerPhysicalLaunch Decode(string json, float pavementY)
    {
        if (string.IsNullOrEmpty(json) || !IsFinite(pavementY))
            throw new ArgumentException("Invalid physical launch input.");
        var payload = JsonUtility.FromJson<Payload>(json);
        if (payload == null || string.IsNullOrWhiteSpace(payload.rollId)
            || !DateTimeOffset.TryParse(payload.fadeDeadline, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var deadline)
            || !IsFinite(payload.remainingFadeMilliseconds) || payload.remainingFadeMilliseconds < 0f
            || payload.remainingFadeMilliseconds > 2000f
            || payload.launchPoses == null || payload.launchPoses.Length is not (2 or 3))
            throw new ArgumentException("Physical launch is incomplete.");
        var dice = new Pose[payload.launchPoses.Length];
        for (int i = 0; i < dice.Length; i++)
        {
            var die = payload.launchPoses[i];
            if (die == null || !IsFinite(die.x) || !IsFinite(die.y) || !IsFinite(die.z)
                || !IsFinite(die.qx) || !IsFinite(die.qy) || !IsFinite(die.qz) || !IsFinite(die.qw))
                throw new ArgumentException("Physical launch has an invalid die pose.");
            float norm = die.qx * die.qx + die.qy * die.qy + die.qz * die.qz + die.qw * die.qw;
            if (norm < 0.95f || norm > 1.05f)
                throw new ArgumentException("Physical launch rotation is not unit length.");
            dice[i] = new Pose(new Vector3(die.x, die.y + pavementY, die.z),
                new Quaternion(die.qx, die.qy, die.qz, die.qw));
        }
        return new ServerPhysicalLaunch(payload.rollId, deadline, payload.remainingFadeMilliseconds / 1000f, dice);
    }

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
