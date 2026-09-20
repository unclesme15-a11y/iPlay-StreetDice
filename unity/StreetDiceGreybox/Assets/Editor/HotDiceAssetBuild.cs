using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HotDiceAssetBuild
{
    public static void VerifyAndCapture()
    {
        StreetDiceDemoBuild.EnsureDemoScene();
        var controller = new GameObject("Hot dice verification").AddComponent<StreetDiceGreyboxController>();
        controller.BuildHotDicePreviewForEditor();
        var type = typeof(StreetDiceGreyboxController);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var apply = type.GetMethod("ApplyDiceColor", flags);
        var streak = type.GetField("streak", flags);
        var lockDie = type.GetMethod("LockDieToValue", flags);
        var normals = new[] { Vector3.down, Vector3.forward, Vector3.left, Vector3.back, Vector3.right, Vector3.up };
        foreach (string field in new[] { "dieA", "dieB", "dieC" })
        {
            var die = (GameObject)type.GetField(field, flags).GetValue(controller);
            var visual = die.transform.Find("Geug Hot Die");
            if (visual == null || !visual.gameObject.activeSelf || die.GetComponent<Renderer>().enabled)
                throw new System.InvalidOperationException("Hot appearance failed: " + field);
            var materials = visual.GetComponentsInChildren<Renderer>(true)[0].sharedMaterials;
            for (int value = 1; value <= 6; value++)
            {
                lockDie.Invoke(controller, new object[] { die, value });
                if (Vector3.Dot(die.transform.rotation * normals[value - 1], Vector3.up) < 0.999f)
                    throw new System.InvalidOperationException("Hot face mapping failed: " + value);
            }
            streak.SetValue(controller, 0f);
            apply.Invoke(controller, null);
            var regular = die.transform.Find("Macriciox Regular Die");
            if (visual.gameObject.activeSelf || regular == null || !regular.gameObject.activeSelf || die.GetComponent<Renderer>().enabled)
                throw new System.InvalidOperationException("Normal appearance did not return: " + field);
            var regularNormals = new[] { Vector3.right, Vector3.left, Vector3.down, Vector3.up, Vector3.back, Vector3.forward };
            for (int value = 1; value <= 6; value++)
            {
                lockDie.Invoke(controller, new object[] { die, value });
                if (Vector3.Dot(die.transform.rotation * regularNormals[value - 1], Vector3.up) < 0.999f)
                    throw new System.InvalidOperationException("Regular face mapping failed: " + value);
            }
            foreach (var color in new[] { Color.white, new Color(0.018f, 0.019f, 0.018f), new Color(0.08f, 0.55f, 0.23f), new Color(0.08f, 0.22f, 0.72f) })
            {
                type.GetField("selectedDiceColor", flags).SetValue(controller, color);
                apply.Invoke(controller, null);
                var regularMaterial = regular.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial;
                if (regularMaterial.shader.name != "iPlay/Regular Dice Skin" || regularMaterial.GetTexture("baseColorTexture") == null
                    || regularMaterial.GetFloat("_Recolor") != (color == Color.white ? 0f : 1f))
                    throw new System.InvalidOperationException("Regular skin color/texture validation failed.");
            }
            streak.SetValue(controller, 10f);
            apply.Invoke(controller, null);
            if (visual.GetComponentsInChildren<Renderer>(true)[0].sharedMaterial != materials[0])
                throw new System.InvalidOperationException("Imported hot material was overwritten.");
        }
        foreach (string shade in new[] { "White", "Tan", "Dark" })
            if (Resources.Load<Material>("FirstPersonHands/Skins/" + shade) == null)
                throw new System.InvalidOperationException("Missing initial hand material: " + shade);
        Debug.Log("Verified imported regular/hot transitions for all three dice, all six faces of both models, four regular colors, preserved textures/hot materials, and three hand shades.");
        StreetDiceDemoBuild.CaptureHotDiceScreenshot();
        StreetDiceDemoBuild.CaptureSmokeScreenshot();
    }

    public static void PrepareAndInspect()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/HotDice/dice.glb");
        if (source == null) throw new System.InvalidOperationException("Hot dice GLB import failed.");
        var root = new GameObject("Geug Hot Die");
        var model = Object.Instantiate(source, root.transform);
        var renderers = model.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
            Debug.Log("Hot die part: " + renderer.name + "; bounds=" + renderer.bounds + "; shader=" + renderer.sharedMaterial.shader.name);
        }
        float scale = 1f / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        model.transform.localScale *= scale;
        model.transform.localPosition = -bounds.center * scale;
        Directory.CreateDirectory("Assets/Resources/Dice");
        // Preserve authored texture detail while preventing opposite-face pips showing through.
        var material = new Material(renderers[0].sharedMaterial);
        material.name = "Geug Hot Dice Gameplay";
        material.SetFloat("_Mode", 0f);
        material.SetFloat("_SrcBlend", 1f);
        material.SetFloat("_DstBlend", 0f);
        material.SetFloat("_ZWrite", 1f);
        material.SetFloat("_CullMode", 2f);
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_ALPHATEST_ON");
        material.SetOverrideTag("RenderType", "Opaque");
        material.renderQueue = 2000;
        var color = material.GetColor("baseColorFactor");
        color.a = 1f;
        material.SetColor("baseColorFactor", color);
        const string materialPath = "Assets/Resources/Dice/GeugHotDice.mat";
        var savedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (savedMaterial == null)
        {
            AssetDatabase.CreateAsset(material, materialPath);
            savedMaterial = material;
        }
        else
        {
            EditorUtility.CopySerialized(material, savedMaterial);
            Object.DestroyImmediate(material);
        }
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = savedMaterial;
        }
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Resources/Dice/GeugHotDie.prefab");

        CaptureFaces("hot-dice-faces.png");
        AssetDatabase.SaveAssets();
    }

    public static void CaptureFaces(string fileName)
    {
        RenderSettings.ambientLight = new Color(0.65f, 0.65f, 0.65f);
        var light = new GameObject("Inspection light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        var camera = new GameObject("Inspection camera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 0.67f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.22f, 0.25f, 0.28f);
        var target = new RenderTexture(384, 384, 24);
        camera.targetTexture = target;
        var atlas = new Texture2D(1152, 768, TextureFormat.RGB24, false);
        var directions = new[] { Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
        for (int i = 0; i < directions.Length; i++)
        {
            camera.transform.position = directions[i] * 3f;
            camera.transform.LookAt(Vector3.zero, i < 2 ? Vector3.forward : Vector3.up);
            light.transform.rotation = camera.transform.rotation;
            camera.Render();
            RenderTexture.active = target;
            atlas.ReadPixels(new Rect(0, 0, 384, 384), i % 3 * 384, (1 - i / 3) * 384);
        }
        atlas.Apply();
        Directory.CreateDirectory("../../artifacts/unity-smoke");
        File.WriteAllBytes("../../artifacts/unity-smoke/" + fileName, atlas.EncodeToPNG());
        Debug.Log("Face atlas: top row +Y, -Y, +Z; bottom row -Z, +X, -X.");
        RenderTexture.active = null;
        camera.targetTexture = null;
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(atlas);
        Object.DestroyImmediate(camera.gameObject);
        Object.DestroyImmediate(light.gameObject);
        AssetDatabase.SaveAssets();
    }
}
