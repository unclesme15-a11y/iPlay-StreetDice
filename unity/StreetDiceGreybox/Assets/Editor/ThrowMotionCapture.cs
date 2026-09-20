using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ThrowMotionCapture
{
    [Serializable]
    private sealed class CaptureMetadata { public int frameCount; public int fps = 30; }
    private static IEnumerator capture;
    private static int lastFrame = -1;
    static ThrowMotionCapture()
    {
        EditorApplication.update += PollCapture;
    }

    public static void Start()
    {
        StreetDiceDemoBuild.EnsureDemoScene();
        SessionState.SetBool("StreetDice.CaptureMotion", true);
        EditorApplication.isPlaying = true;
    }

    private static void PollCapture()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (SessionState.GetBool("StreetDice.CaptureMotion", false))
        {
            SessionState.SetBool("StreetDice.CaptureMotion", false);
            capture = CaptureFrames();
        }
        if (capture == null || lastFrame == Time.frameCount) return;
        lastFrame = Time.frameCount;
        try { if (!capture.MoveNext()) { capture = null; EditorApplication.Exit(0); } }
        catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
    }

    private static IEnumerator CaptureFrames()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Use ThrowMotionCapture.Start for a play-mode capture.");
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(root);
        var controller = new GameObject("Throw motion capture").AddComponent<StreetDiceGreyboxController>();
        controller.BuildEnvironmentPreviewForEditor();
        controller.enabled = false;
        Time.captureFramerate = 30;
        var camera = Camera.main;
        var target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        controller.PrepareRollPreviewForEditor();
        Debug.Log("Replay duration: " + controller.RollPreviewDuration);
        var output = Path.GetFullPath("../../artifacts/unity-smoke/throw-motion");
        Directory.CreateDirectory(output);
        var screenshot = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        int count = Mathf.CeilToInt(Mathf.Max(3f, controller.RollPreviewDuration + 0.5f) * 30f);
        for (int frame = 0; frame < count; frame++)
        {
            float time = frame / 30f;
            controller.SampleRollPresentation(time);
            // Let the player loop update skinning before rendering this pose.
            yield return null;
            float wristY = camera.WorldToViewportPoint(controller.ThrowWrist.position).y;
            if (wristY > 0.0001f) throw new InvalidOperationException("Wrist entered frame at " + time + ": " + wristY);
            camera.Render();
            RenderTexture.active = target;
            screenshot.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            screenshot.Apply();
            File.WriteAllBytes(Path.Combine(output, "frame-" + frame.ToString("D4") + ".png"), screenshot.EncodeToPNG());
        }
        Debug.Log("Captured " + count + " motion frames; wrist remained below the viewport throughout.");
        ThrowMotionVerification.Verify(controller, camera);
        File.WriteAllText(Path.Combine(output, "capture.json"), JsonUtility.ToJson(new CaptureMetadata { frameCount = count }, true));
        foreach (var variant in new[] { ("tan-three-dice", 1280, 720, 1), ("dark-three-dice", 1280, 720, 2), ("wide-hand", 1600, 720, 0), ("portrait-hand", 720, 1280, 0) })
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            controller.GetType().GetField("selectedHandSkin", flags).SetValue(controller, variant.Item4);
            controller.GetType().GetMethod("ApplyHandSkin", flags).Invoke(controller, null);
            var variantTarget = new RenderTexture(variant.Item2, variant.Item3, 24);
            camera.targetTexture = variantTarget;
            camera.aspect = variant.Item2 / (float)variant.Item3;
            controller.PrepareRollPreviewForEditor(713, true);
            controller.SampleRollPresentation(0.3f);
            yield return null;
            camera.Render();
            RenderTexture.active = variantTarget;
            var still = new Texture2D(variantTarget.width, variantTarget.height, TextureFormat.RGB24, false);
            still.ReadPixels(new Rect(0, 0, still.width, still.height), 0, 0);
            still.Apply();
            File.WriteAllBytes(Path.Combine(output, variant.Item1 + ".png"), still.EncodeToPNG());
            RenderTexture.active = null;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(still);
            UnityEngine.Object.DestroyImmediate(variantTarget);
        }
        RenderTexture.active = null;
        camera.targetTexture = null;
        UnityEngine.Object.DestroyImmediate(screenshot);
        UnityEngine.Object.DestroyImmediate(target);
    }
}
