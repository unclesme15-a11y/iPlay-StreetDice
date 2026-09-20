using System;
using System.Reflection;
using UnityEngine;

public static class ThrowMotionVerification
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly Vector3[] RegularFaces = { Vector3.right, Vector3.left, Vector3.down, Vector3.up, Vector3.back, Vector3.forward };
    private static readonly Vector3[] HotFaces = { Vector3.down, Vector3.forward, Vector3.left, Vector3.back, Vector3.right, Vector3.up };

    public static void Verify(StreetDiceGreyboxController controller, Camera camera)
    {
        var type = controller.GetType();
        var prepare = type.GetMethod("PrepareRollPresentation", Fields);
        var shooter = type.GetField("shooterId", Fields);
        var diceField = type.GetField("replayDice", Fields);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        type.GetField("snapStyle", Fields).SetValue(controller, true);
        type.GetMethod("SelectThrowHand", Fields).Invoke(controller, null);
        int cases = 0;
        float originalAspect = camera.aspect;
        foreach (float aspect in new[] { 16f / 9f, 20f / 9f, 9f / 16f })
        foreach (string player in new[] { "p1", "p2", "p3", "p4", "bot-5" })
        foreach (int count in new[] { 2, 3 })
        for (int face = 1; face <= 6; face++)
        {
            camera.aspect = aspect;
            shooter.SetValue(controller, player);
            int[] values = count == 2 ? new[] { face, 7 - face } : new[] { face, 7 - face, face };
            prepare.Invoke(controller, new object[] { values, 701 + face * 113 + count * 17 });
            var dice = (GameObject[])diceField.GetValue(controller);
            if (player == "p1")
            {
                for (int frame = 0; frame <= 80; frame++)
                {
                    controller.SampleRollPresentation(frame / 60f);
                    Assert(camera.WorldToViewportPoint(controller.ThrowWrist.position).y <= -0.099f, "Wrist crop margin");
                }
                controller.SampleRollPresentation(FirstPersonDiceHand.ReleaseTime - 0.00001f);
                var held = dice[0].transform.position;
                controller.SampleRollPresentation(FirstPersonDiceHand.ReleaseTime);
                Assert(Vector3.Distance(held, dice[0].transform.position) < 0.001f, "Release continuity");
                Transform middleTip = null;
                foreach (var bone in controller.ThrowWrist.GetComponentsInChildren<Transform>())
                    if (bone.name == "MiddleEnd") middleTip = bone;
                Assert(middleTip != null, "Snap finger exists");
                controller.SampleRollPresentation(FirstPersonDiceHand.SnapTime - 0.01f);
                var beforeSnap = middleTip.position;
                controller.SampleRollPresentation(FirstPersonDiceHand.SnapTime + 0.06f);
                Assert(controller.ThrowWrist.gameObject.activeInHierarchy, "Snap remains on-screen before exit");
                Assert(Vector3.Distance(beforeSnap, middleTip.position) > 0.01f, "Visible finger flick movement");
                controller.SampleRollPresentation(FirstPersonDiceHand.ExitTime);
                Assert(!controller.ThrowWrist.gameObject.activeInHierarchy, "Hand hidden after exit");
            }
            controller.SampleRollPresentation(controller.RollPreviewDuration);
            for (int i = 0; i < dice.Length; i++)
            {
                var die = dice[i].transform;
                var regular = die.Find("Macriciox Regular Die");
                var hot = die.Find("Geug Hot Die");
                Assert(Vector3.Dot(regular.rotation * RegularFaces[values[i] - 1], Vector3.up) > 0.985f, "Regular landing face");
                Assert(Vector3.Dot(hot.rotation * HotFaces[values[i] - 1], Vector3.up) > 0.985f, "Hot landing face");
                var viewport = camera.WorldToViewportPoint(die.position);
                Assert(viewport.x > 0.02f && viewport.x < 0.98f && viewport.y > 0.02f && viewport.y < 0.35f, "Visible pavement landing");
                var settled = die.position;
                controller.SampleRollPresentation(controller.RollPreviewDuration + 1f);
                Assert(Vector3.Distance(settled, die.position) < 0.00001f, "Roll lock");
            }
            cases++;
        }
        camera.aspect = originalAspect;
        VerifyServerTransport(controller, camera, prepare, shooter, diceField);
        Debug.Log("Verified " + cases + " physics presentations: all faces, two/three dice, five seats, three aspect ratios, release continuity, wrist margin, snap movement, hand exit and roll lock. Total ms=" + clock.ElapsedMilliseconds);
    }

    private static void VerifyServerTransport(StreetDiceGreyboxController controller, Camera camera,
        MethodInfo prepare, FieldInfo shooter, FieldInfo diceField)
    {
        const string json = "{\"faces\":[1,4],\"frames\":["
            + "{\"time\":0,\"dice\":["
            + "{\"x\":0.31,\"y\":0.675,\"z\":-2.73,\"qx\":0,\"qy\":0,\"qz\":0.7071068,\"qw\":0.7071068},"
            + "{\"x\":0.22,\"y\":0.665,\"z\":-2.74,\"qx\":0,\"qy\":0,\"qz\":0,\"qw\":1}]},"
            + "{\"time\":1,\"dice\":["
            + "{\"x\":-0.1,\"y\":0.045,\"z\":0.15,\"qx\":0,\"qy\":0,\"qz\":0.7071068,\"qw\":0.7071068},"
            + "{\"x\":0.1,\"y\":0.045,\"z\":0.18,\"qx\":0,\"qy\":0,\"qz\":0,\"qw\":1}]}]}";
        var replay = ServerPhysicalRollReplay.Decode(json, -1.095f);
        Assert(replay.DiceCount == 2 && replay.Duration == 1f, "Server frame count and clock");
        Assert(Mathf.Abs(replay.Sample(0.5f, 0).position.z + 1.29f) < 0.0001f,
            "Server frame interpolation");
        Assert(Mathf.Abs(replay.Sample(1f, 0).position.y + 1.05f) < 0.0001f,
            "Server pavement-to-Unity elevation");
        bool rejected = false;
        try { ServerPhysicalRollReplay.Decode("{\"faces\":[1,4],\"frames\":[]}", -1.095f); }
        catch (ArgumentException) { rejected = true; }
        Assert(rejected, "Missing server frames rejected");
        rejected = false;
        try { ServerPhysicalRollReplay.Decode(json.Replace("\"qz\":0.7071068", "\"qz\":20"), -1.095f); }
        catch (ArgumentException) { rejected = true; }
        Assert(rejected, "Invalid server rotation rejected");

        var type = controller.GetType();
        type.GetMethod("PrepareServerPhysicalRollPresentation", Fields).Invoke(controller, new object[] { replay });
        type.GetMethod("SampleServerPhysicalRollPresentation", Fields).Invoke(controller, new object[] { 1f });
        var dice = (GameObject[])diceField.GetValue(controller);
        for (int i = 0; i < dice.Length; i++)
        {
            int face = replay.Faces[i];
            var regular = dice[i].transform.Find("Macriciox Regular Die");
            var hot = dice[i].transform.Find("Geug Hot Die");
            Assert(Vector3.Dot(regular.rotation * RegularFaces[face - 1], Vector3.up) > 0.985f,
                "Server regular landing face");
            Assert(Vector3.Dot(hot.rotation * HotFaces[face - 1], Vector3.up) > 0.985f,
                "Server hot landing face");
        }

        const string launchJson = "{\"rollId\":\"fixture-roll\",\"fadeDeadline\":\"2026-09-15T12:00:00Z\",\"launchPoses\":["
            + "{\"x\":0.31,\"y\":0.675,\"z\":-2.73,\"qx\":0,\"qy\":0,\"qz\":0.7071068,\"qw\":0.7071068},"
            + "{\"x\":0.22,\"y\":0.665,\"z\":-2.74,\"qx\":0,\"qy\":0,\"qz\":0,\"qw\":1}]}";
        var launch = ServerPhysicalLaunch.Decode(launchJson, -1.095f);
        Assert(launch.DiceCount == 2 && launch.RollId == "fixture-roll", "Server launch decoded");
        rejected = false;
        try { ServerPhysicalLaunch.Decode("{\"rollId\":\"fixture-roll\",\"launchPoses\":[]}", -1.095f); }
        catch (ArgumentException) { rejected = true; }
        Assert(rejected, "Incomplete server launch rejected");
        shooter.SetValue(controller, "p1");
        type.GetField("leftHanded", Fields).SetValue(controller, false);
        type.GetMethod("SelectThrowHand", Fields).Invoke(controller, null);
        type.GetMethod("PrepareServerLaunchPresentation", Fields).Invoke(controller, new object[] { launch });
        type.GetMethod("SampleServerLaunchHand", Fields).Invoke(controller,
            new object[] { FirstPersonDiceHand.ReleaseTime - 0.00001f });
        var beforeRelease = dice[0].transform.position;
        var beforeRotation = dice[0].transform.rotation;
        type.GetMethod("SampleServerLaunchHand", Fields).Invoke(controller,
            new object[] { FirstPersonDiceHand.ReleaseTime });
        Assert(Vector3.Distance(beforeRelease, dice[0].transform.position) < 0.001f,
            "Public server launch has no hand-release position jump");
        Assert(Quaternion.Angle(beforeRotation, dice[0].transform.rotation) < 0.001f,
            "Public server launch has no hand-release rotation jump");
        type.GetMethod("PrepareServerPhysicalRollPresentation", Fields).Invoke(controller, new object[] { replay });
        Assert(Vector3.Distance(launch.Dice[0].position, dice[0].transform.position) < 0.0001f,
            "Committed replay starts at public launch position");
        Assert(Quaternion.Angle(launch.Dice[0].rotation, dice[0].transform.rotation) < 0.001f,
            "Committed replay starts at public launch rotation");
        type.GetMethod("SampleServerPhysicalRollPresentation", Fields).Invoke(controller,
            new object[] { FirstPersonDiceHand.SnapTime + 0.06f - FirstPersonDiceHand.ReleaseTime });
        Assert(controller.ThrowWrist.gameObject.activeInHierarchy,
            "Server replay keeps snap hand visible before exit");
        Assert(camera.WorldToViewportPoint(controller.ThrowWrist.position).y <= -0.099f,
            "Server replay keeps wrist cropped");
        type.GetMethod("SampleServerPhysicalRollPresentation", Fields).Invoke(controller,
            new object[] { FirstPersonDiceHand.ExitTime - FirstPersonDiceHand.ReleaseTime });
        Assert(!controller.ThrowWrist.gameObject.activeInHierarchy,
            "Server replay hand exits after snap");

        shooter.SetValue(controller, "p1");
        var handedField = type.GetField("leftHanded", Fields);
        var selectHand = type.GetMethod("SelectThrowHand", Fields);
        foreach (bool left in new[] { false, true })
        {
            handedField.SetValue(controller, left);
            selectHand.Invoke(controller, null);
            prepare.Invoke(controller, new object[] { new[] { 1, 4 }, 701 });
            controller.SampleRollPresentation(FirstPersonDiceHand.ReleaseTime);
            var heldDice = (GameObject[])diceField.GetValue(controller);
            var serverRelease = left ? new Vector3(-0.22f, -0.42f, -2.73f) : replay.FirstPose(0).position;
            Assert(Vector3.Distance(heldDice[0].transform.position, serverRelease) < 0.03f,
                "Server " + (left ? "left" : "right") + " launch origin matches hand release");
            Debug.Log("SERVER REPLAY ALIGNMENT " + (left ? "LEFT" : "RIGHT")
                + ": A=" + heldDice[0].transform.position + " B=" + heldDice[1].transform.position
                + " rotation=" + heldDice[0].transform.rotation
                + " distance=" + Vector3.Distance(heldDice[0].transform.position, serverRelease).ToString("F3")
                + " viewport=" + camera.WorldToViewportPoint(heldDice[0].transform.position));
        }
        handedField.SetValue(controller, false);
        selectHand.Invoke(controller, null);
    }

    private static void Assert(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException("Throw verification failed: " + label);
    }
}
