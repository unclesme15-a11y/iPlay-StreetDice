using System.Collections.Generic;
using UnityEngine;

// Camera framing and finger posing share one sampled timeline in game and in editor captures.
public sealed class FirstPersonDiceHand
{
    public const float ReleaseTime = 0.48f;
    public const float SnapTime = 0.66f;
    public const float ExitTime = 1.28f;
    private readonly Transform root;
    private readonly Camera camera;
    private readonly Dictionary<string, Transform> bones = new();
    private readonly Dictionary<Transform, Quaternion> rest = new();
    private readonly Quaternion palmFrame;
    private readonly Vector3 localNormal;
    private float releaseSlide;
    public bool SnapEnabled { get; set; }
    public bool LeftHanded { get; }
    public Transform Wrist { get; }
    public bool IsRigged => Wrist != null && bones.ContainsKey("MiddleBase");
    public Vector3 PalmNormal => root.TransformDirection(localNormal).normalized;
    public Vector3 PalmForward => (bones["MiddleBase"].position - Wrist.position).normalized;
    public Vector3 PalmCenter => Vector3.Lerp(Wrist.position, bones["MiddleBase"].position, 0.72f) + PalmNormal * 0.085f;

    public FirstPersonDiceHand(GameObject hand, Camera view)
    {
        root = hand.transform;
        camera = view;
        LeftHanded = hand.name.StartsWith("Left");
        foreach (var animator in hand.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
        foreach (var bone in hand.GetComponentsInChildren<Transform>(true))
        {
            bones[bone.name] = bone;
            rest[bone] = bone.localRotation;
        }
        bones.TryGetValue("Wrist", out var wrist);
        Wrist = wrist;
        if (!IsRigged) return;
        var forward = root.InverseTransformDirection(bones["MiddleBase"].position - wrist.position).normalized;
        var across = root.InverseTransformDirection(bones["IndexBase"].position - bones["PinkyBase"].position).normalized;
        localNormal = Vector3.Cross(forward, across).normalized * (LeftHanded ? -1f : 1f);
        palmFrame = Quaternion.LookRotation(forward, localNormal);
    }

    public void Sample(float seconds)
    {
        root.gameObject.SetActive(seconds >= 0f && seconds < ExitTime && IsRigged);
        if (!root.gameObject.activeSelf) return;
        foreach (var pair in rest) if (pair.Key != root) pair.Key.localRotation = pair.Value;
        float entrance = Mathf.SmoothStep(0f, 1f, seconds / 0.18f);
        float exit = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.95f, ExitTime, seconds));
        float toss = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.30f, ReleaseTime, seconds));
        releaseSlide = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.32f, ReleaseTime, seconds));
        float handedness = LeftHanded ? -1f : 1f;
        var forward = (camera.transform.up * 0.45f + camera.transform.forward * 0.89f - camera.transform.right * (0.16f * handedness)).normalized;
        var normal = Vector3.Cross(forward, camera.transform.right).normalized;
        var frame = Quaternion.LookRotation(forward, normal);
        root.rotation = Quaternion.AngleAxis(-7f * toss, camera.transform.right) * frame * Quaternion.Inverse(palmFrame);
        if (SnapEnabled)
        {
            float turn = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(ReleaseTime, SnapTime - 0.03f, seconds));
            root.rotation = Quaternion.AngleAxis(38f * handedness * turn, forward) * root.rotation;
        }
        float shake = seconds < 0.32f ? Mathf.Sin(seconds * 25f) * Mathf.Sin(seconds / 0.32f * Mathf.PI) : 0f;
        var anchor = camera.ViewportToWorldPoint(new Vector3((LeftHanded ? 0.39f : 0.61f) + shake * 0.005f,
            -0.10f - (1f - entrance) * 0.34f - exit * 0.4f, 1.65f + toss * 0.08f));
        root.position += anchor - Wrist.position;

        var fingers = new[] { "Index", "Middle", "Ring", "Pinky" };
        for (int i = 0; i < fingers.Length; i++)
        {
            float hold = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.33f + i * 0.01f, ReleaseTime, seconds));
            Curl(fingers[i], hold * (18f + i * 3f), hold * (23f + i * 3f));
        }
        float ready = SnapEnabled ? Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.50f, 0.63f, seconds)) : 0f;
        float flick = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(SnapTime, SnapTime + 0.055f, seconds));
        if (ready > 0f)
        {
            float relax = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.91f, 1.10f, seconds));
            Curl("Index", ready * 8f, ready * 12f);
            Curl("Ring", ready * (65f - relax * 12f), ready * (80f - relax * 15f));
            Curl("Pinky", ready * (70f - relax * 10f), ready * (85f - relax * 12f));
            var across = (bones["IndexBase"].position - bones["PinkyBase"].position).normalized;
            var contact = PalmCenter + PalmForward * 0.085f + PalmNormal * 0.10f + across * 0.045f;
            var middleTarget = Vector3.Lerp(contact, PalmCenter + PalmNormal * (0.028f + relax * 0.025f), flick);
            Reach("Middle", middleTarget, ready);
            Reach("Thumb", contact + across * (0.018f + flick * 0.055f), ready);
        }
        // Re-anchor after posing so no animation can pull the wrist into frame.
        var wristViewport = camera.WorldToViewportPoint(Wrist.position);
        if (wristViewport.y > -0.10f)
            root.position += camera.ViewportToWorldPoint(new Vector3(wristViewport.x, -0.10f, wristViewport.z)) - Wrist.position;
    }

    public void SampleShake(float elapsed)
    {
        Sample(0.22f + Mathf.Sin(elapsed * 22f) * 0.035f);
    }

    public void SampleCatch(Vector3 palmPosition, Vector3 forward)
    {
        root.gameObject.SetActive(IsRigged);
        if (!IsRigged) return;
        foreach (var pair in rest) if (pair.Key != root) pair.Key.localRotation = pair.Value;
        forward.y = 0f;
        root.rotation = Quaternion.LookRotation(forward.normalized, Vector3.down) * Quaternion.Inverse(palmFrame);
        Curl("Index", 3f, 4f);
        Curl("Middle", 2f, 3f);
        Curl("Ring", 4f, 5f);
        Curl("Pinky", 7f, 6f);
        root.position += palmPosition - PalmCenter;
    }

    public Vector3 DicePosition(int index, int count, float size)
    {
        var across = Vector3.Cross(PalmForward, PalmNormal).normalized;
        var offset = count == 3
            ? new Vector2(index == 0 ? -0.53f : index == 1 ? 0.53f : 0f, index == 2 ? 0.9f : 0f)
            : new Vector2(index == 0 ? -0.53f : 0.53f, 0f);
        return PalmCenter + PalmNormal * (size * 0.53f + releaseSlide * 0.04f)
            + across * (offset.x * size) + PalmForward * (offset.y * size + releaseSlide * 0.28f);
    }

    public Quaternion DiceRotation => Quaternion.LookRotation(PalmForward, PalmNormal);

    private void Curl(string finger, float baseAngle, float middleAngle)
    {
        Bend(bones[finger + "Base"], bones[finger + "Middle"], baseAngle);
        Bend(bones[finger + "Middle"], bones[finger + "End"], middleAngle);
    }

    private void Bend(Transform bone, Transform child, float angle)
    {
        var axis = Vector3.Cross(child.position - bone.position, PalmNormal).normalized;
        bone.rotation = Quaternion.AngleAxis(angle, axis) * bone.rotation;
    }

    private void Reach(string finger, Vector3 target, float weight)
    {
        var tip = bones[finger + "End"];
        var joints = new[] { bones[finger + "Middle"], bones[finger + "Base"] };
        var original = new[] { joints[0].localRotation, joints[1].localRotation };
        for (int iteration = 0; iteration < 8; iteration++)
        {
            foreach (var joint in joints)
            {
                var delta = Quaternion.FromToRotation(tip.position - joint.position, target - joint.position);
                joint.rotation = Quaternion.RotateTowards(Quaternion.identity, delta, 12f) * joint.rotation;
                joint.localRotation = Quaternion.RotateTowards(rest[joint], joint.localRotation, 85f);
            }
        }
        for (int i = 0; i < joints.Length; i++) joints[i].localRotation = Quaternion.Slerp(original[i], joints[i].localRotation, weight);
    }
}
