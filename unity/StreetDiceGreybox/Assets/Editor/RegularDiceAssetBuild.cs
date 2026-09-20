using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RegularDiceAssetBuild
{
    public static void PrepareVerifyAndCapture()
    {
        PrepareAndInspect();
        HotDiceAssetBuild.VerifyAndCapture();
    }

    public static void PrepareAndInspect()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/RegularDice/dice.glb");
        if (source == null) throw new System.InvalidOperationException("Regular dice GLB import failed.");
        var root = new GameObject("Macriciox Regular Die");
        var model = Object.Instantiate(source, root.transform);
        var renderers = model.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        float scale = 1f / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        model.transform.localScale *= scale;
        model.transform.localPosition = -bounds.center * scale;
        Directory.CreateDirectory("Assets/Resources/Dice");
        var material = new Material(renderers[0].sharedMaterial);
        material.shader = Shader.Find("iPlay/Regular Dice Skin");
        material.name = "Macriciox Regular Dice";
        material.SetFloat("_Recolor", 0f);
        const string materialPath = "Assets/Resources/Dice/MacricioxRegularDice.mat";
        var saved = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (saved == null)
        {
            AssetDatabase.CreateAsset(material, materialPath);
            saved = material;
        }
        else
        {
            EditorUtility.CopySerialized(material, saved);
            Object.DestroyImmediate(material);
        }
        foreach (var renderer in renderers) renderer.sharedMaterial = saved;
        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Resources/Dice/MacricioxRegularDie.prefab");
        HotDiceAssetBuild.CaptureFaces("regular-dice-faces.png");
        AssetDatabase.SaveAssets();
        var black = new Material(saved);
        black.SetFloat("_Recolor", 1f);
        black.SetColor("_BodyColor", new Color(0.018f, 0.019f, 0.018f));
        black.SetColor("_PipColor", Color.white);
        foreach (var renderer in renderers) renderer.sharedMaterial = black;
        HotDiceAssetBuild.CaptureFaces("regular-dice-black-faces.png");
        foreach (var renderer in renderers) renderer.sharedMaterial = saved;
        Object.DestroyImmediate(black);
    }
}
