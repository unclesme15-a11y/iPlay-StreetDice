using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StreetDiceDemoBuild
{
    private const string ScenePath = "Assets/Scenes/StreetDiceDemo.unity";
    private const string ApkPath = "../../builds/iPlay-CeeLo-Craps-Demo.apk";

    public static void EnsureDemoScene()
    {
        Directory.CreateDirectory("Assets/Scenes");

        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "StreetDiceDemo";
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        PlayerSettings.productName = "iPlay Cee-lo & Craps";
        PlayerSettings.companyName = "iPlay";
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    public static void BuildAndroidApk()
    {
        EnsureDemoScene();
        Directory.CreateDirectory("../../builds");

        var report = BuildPipeline.BuildPlayer(
            new[] { ScenePath },
            ApkPath,
            BuildTarget.Android,
            BuildOptions.Development);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new System.InvalidOperationException("Android APK build failed: " + report.summary.result);
        }

        Debug.Log("Built APK: " + Path.GetFullPath(ApkPath));
    }

    public static void CaptureSmokeScreenshot()
    {
        EnsureDemoScene();

        var controllerObject = new GameObject("Smoke Screenshot Controller");
        var controller = controllerObject.AddComponent<StreetDiceGreyboxController>();
        typeof(StreetDiceGreyboxController)
            .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(controller, null);

        var camera = Camera.main;
        if (camera == null)
        {
            throw new System.InvalidOperationException("Street dice demo did not create a main camera.");
        }

        var outputDirectory = Path.GetFullPath("../../artifacts/unity-smoke");
        Directory.CreateDirectory(outputDirectory);
        var outputPath = Path.Combine(outputDirectory, "street-dice-demo-smoke.png");

        var texture = new RenderTexture(1280, 720, 24);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        camera.targetTexture = texture;
        RenderTexture.active = texture;
        camera.Render();

        var screenshot = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        screenshot.Apply();

        File.WriteAllBytes(outputPath, screenshot.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        Object.DestroyImmediate(screenshot);
        Object.DestroyImmediate(texture);
        Debug.Log("Captured smoke screenshot: " + outputPath);
    }
}
