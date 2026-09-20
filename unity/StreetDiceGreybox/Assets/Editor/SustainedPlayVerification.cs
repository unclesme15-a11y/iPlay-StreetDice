using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SustainedPlayVerification
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static IEnumerator routine;
    private static int lastFrame = -1;
    private static double deadline;
    private static readonly List<string> report = new();
    private static readonly List<float> frameMilliseconds = new();
    private static double sampleAfter;

    static SustainedPlayVerification() { EditorApplication.update += Tick; }

    public static void Start()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        var view = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        view.position = new Rect(30, 30, 1280, 760);
        view.Show();
        SessionState.SetBool("StreetDice.SustainedPlay", true);
        EditorApplication.isPlaying = true;
    }

    private static object Get(object target, string field) => target.GetType().GetField(field, Flags).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, Flags).SetValue(target, value);
    private static object Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, Flags).Invoke(target, args);
    private static bool Flag(object target, string field) => (bool)Get(target, field);

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (SessionState.GetBool("StreetDice.SustainedPlay", false))
        {
            SessionState.SetBool("StreetDice.SustainedPlay", false);
            report.Clear();
            frameMilliseconds.Clear();
            sampleAfter = EditorApplication.timeSinceStartup + 5;
            report.Add("# Sustained play verification\n\nUnity Editor, normal time scale, real animations and opponent loop. Not physical-phone performance evidence.");
            deadline = EditorApplication.timeSinceStartup + 420;
            routine = Verify();
        }
        if (routine == null || lastFrame == Time.frameCount) return;
        lastFrame = Time.frameCount;
        if (EditorApplication.timeSinceStartup >= sampleAfter)
            frameMilliseconds.Add(Time.unscaledDeltaTime * 1000f);
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Sustained play exceeded seven minutes.");
            if (!routine.MoveNext())
            {
                routine = null;
                Save("PASS");
                Debug.Log("SUSTAINED PLAY PASSED: both modes, actual roll animations and opponent turns, nonnegative conserved balances.");
                EditorApplication.Exit(0);
            }
        }
        catch (Exception e)
        {
            routine = null;
            report.Add(e.ToString());
            Save("FAIL");
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    private static void Save(string status)
    {
        if (frameMilliseconds.Count > 0)
        {
            frameMilliseconds.Sort();
            float sum = 0;
            int slow33 = 0, slow50 = 0;
            foreach (float ms in frameMilliseconds)
            {
                sum += ms;
                if (ms > 33.34f) slow33++;
                if (ms > 50f) slow50++;
            }
            float Percentile(float p) => frameMilliseconds[Mathf.Clamp(Mathf.CeilToInt(frameMilliseconds.Count * p) - 1, 0, frameMilliseconds.Count - 1)];
            report.Add($"## Editor timing sample\n\n{frameMilliseconds.Count} rendered-frame intervals after a five-second warmup. " +
                $"Mean {sum / frameMilliseconds.Count:F2}ms; p50 {Percentile(0.5f):F2}ms; p95 {Percentile(0.95f):F2}ms; p99 {Percentile(0.99f):F2}ms; max {frameMilliseconds[frameMilliseconds.Count - 1]:F2}ms. " +
                $"Frames over 33.34ms: {slow33}; over 50ms: {slow50}.\n\n" +
                $"Unity {Application.unityVersion}; {Screen.width}x{Screen.height}; graphics {SystemInfo.graphicsDeviceName}; target {Application.targetFrameRate}fps. " +
                "Includes Editor overhead, betting, rolls and payouts. No screenshots are captured during sampling. This is not GPU timing, a standalone-player benchmark or mobile performance certification.");
        }
        string path = Path.GetFullPath("../../artifacts/unity-smoke/sustained-play.md");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, string.Join("\n\n", report) + "\n\nResult: " + status + "\n");
    }

    private static IEnumerator Verify()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        if (c == null) throw new Exception("Runtime bootstrap missing.");
        Set(c, "botWagersEnabled", true);
        Set(c, "moneyAnimationEnabled", true);
        Set(c, "tutorialMode", false);
        foreach (var mode in new[] { GameMode.Craps, GameMode.CeeLo })
        {
            Set(c, "gameMode", mode);
            Call(c, "StartLocalDemo");
            int completed = 0;
            bool wasRolling = false;
            float lastProgress = Time.realtimeSinceStartup;
            float began = lastProgress;
            var shooters = new HashSet<string>();
            var cash = (Dictionary<string, int>)Get(c, "cash");
            int initialTotal = 0;
            foreach (int value in cash.Values) initialTotal += value;
            while (completed < 6 || shooters.Count < 2)
            {
                bool rolling = Flag(c, "rolling");
                string shooter = (string)Get(c, "shooterId");
                if (rolling && !wasRolling) { shooters.Add(shooter); lastProgress = Time.realtimeSinceStartup; }
                if (!rolling && wasRolling)
                {
                    completed++;
                    if (completed > 12)
                        throw new Exception($"{mode}: did not start a roll for a second shooter within 12 completed cycles.");
                    lastProgress = Time.realtimeSinceStartup;
                    report.Add($"{mode} roll/fade cycle {completed}: shooter {shooter}, phase {Get(c, "phase")}, point {Get(c, "point")}");
                    Debug.Log(report[report.Count - 1]);
                }
                wasRolling = rolling;
                int total = 0;
                foreach (var balance in cash)
                {
                    if (balance.Value < 0) throw new Exception($"{mode}: negative balance for {balance.Key}.");
                    total += balance.Value;
                }
                if (total != initialTotal) throw new Exception($"{mode}: balance total changed from {initialTotal} to {total}.");
                if (Time.realtimeSinceStartup - lastProgress > 45)
                    throw new Exception($"{mode}: no roll progress for 45 seconds; shooter={shooter}, phase={Get(c, "phase")}, committed={Get(c, "shotCommitted")}.");
                if (!rolling && shooter == "p1")
                {
                    if (Flag(c, "awaitingShootChoice"))
                    {
                        if (completed > 0) Call(c, "PassLocalDice");
                        else Call(c, "CommitShoot");
                    }
                    else if ((string)Get(c, "phase") == "ShooterDecision") Call(c, "PassLocalDice");
                    else if ((bool)c.GetType().GetProperty("CanGesture", Flags).GetValue(c))
                    {
                        var area = (Rect)c.GetType().GetProperty("UiSafeArea", Flags).GetValue(c);
                        var origin = new Vector2(area.center.x, area.y + area.height * 0.25f);
                        Call(c, "BeginShake", origin, -2);
                        float releaseAt = Time.time + 0.16f;
                        while (Time.time < releaseAt) yield return null;
                        Call(c, "EndShake", origin + Vector2.up * area.height * 0.2f);
                    }
                }
                yield return null;
            }
            if (shooters.Count < 2) throw new Exception($"{mode}: did not exercise an opponent shooter.");
            report.Add($"{mode}: {completed} completed cycles, {shooters.Count} shooters, {Time.realtimeSinceStartup - began:F1}s elapsed, total money {initialTotal}. Balances: {string.Join(", ", cash)}.");
            Set(c, "mainOptions", true);
            float settleUntil = Time.realtimeSinceStartup + 2;
            while (Time.realtimeSinceStartup < settleUntil) yield return null;
        }
    }
}
