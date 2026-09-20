using UnityEngine;

public partial class StreetDiceGreyboxController
{
    private RenderTexture[] handPreviews;
    private RenderTexture[] dicePreviews;

    private void EnsureHandPreviews()
    {
        if (handPreviews != null) return;
        handPreviews = new RenderTexture[3];
        var studio = new GameObject("Hand selection preview studio");
        var camera = studio.AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.cullingMask = 1 << 26;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 10;
        camera.aspect = 1;
        Camera.main.cullingMask &= ~(1 << 26);
        var lightObject = new GameObject("Hand display fill");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.cullingMask = 1 << 26;
        light.range = 4;
        light.intensity = 0.9f;
        light.color = Color.white;
        var sceneLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        var lightMasks = new int[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
        {
            lightMasks[i] = sceneLights[i].cullingMask;
            if (sceneLights[i] != light) sceneLights[i].cullingMask &= ~(1 << 26);
        }
        var ambientMode = RenderSettings.ambientMode;
        var ambientLight = RenderSettings.ambientLight;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.35f, 0.35f, 0.35f);
        try
        {
        var prefab = Resources.Load<GameObject>("FirstPersonHands/FirstPersonHand_R");
        for (int i = 0; i < handPreviews.Length; i++)
        {
            var hand = Instantiate(prefab);
            hand.name = "Hand display " + HandSkinResources[i];
            NormalizeHandScale(hand, 0.85f);
            foreach (var part in hand.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 26;
            var motion = new FirstPersonDiceHand(hand, camera);
            motion.SampleCatch(new Vector3(1000, 0, 0), Vector3.forward);
            var skin = Resources.Load<Material>("FirstPersonHands/Skins/" + HandSkinResources[i]);
            var material = new Material(Shader.Find("Standard"));
            material.mainTexture = skin.GetTexture("_Base_Albedo");
            // Preserve the textured pigmentation; UI swatch colors would over-darken this albedo.
            material.color = i == 2 ? new Color(0.9f, 0.6f, 0.4f)
                : i == 1 ? new Color(0.72f, 0.52f, 0.38f) : Color.white;
            material.SetFloat("_Glossiness", 0.18f);
            material.SetFloat("_GlossyReflections", 0f);
            material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            material.SetTexture("_BumpMap", skin.GetTexture("_NormalMap"));
            material.EnableKeyword("_NORMALMAP");
            var renderers = hand.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var renderer in renderers)
            {
                var slots = renderer.sharedMaterials;
                for (int slot = 0; slot < slots.Length; slot++) slots[slot] = material;
                renderer.sharedMaterials = slots;
                renderer.updateWhenOffscreen = true;
            }
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            camera.transform.position = bounds.center + new Vector3(0.3f, 1.8f, 0.35f);
            camera.transform.LookAt(bounds.center);
            camera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * 0.55f;
            light.transform.position = bounds.center + new Vector3(-0.5f, 1.2f, -0.5f);
            light.transform.LookAt(bounds.center);
            handPreviews[i] = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32) { name = "Hand preview " + HandSkinResources[i] };
            camera.targetTexture = handPreviews[i];
            RenderSelectionPreview(camera);
            hand.SetActive(false);
            Destroy(hand);
            Destroy(material);
        }
        EnsureDicePreviews(camera, light);
        }
        finally
        {
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientLight;
            for (int i = 0; i < sceneLights.Length; i++)
                if (sceneLights[i] != null) sceneLights[i].cullingMask = lightMasks[i];
            camera.targetTexture = null;
            Destroy(studio);
            Destroy(lightObject);
        }
    }

    private static void RenderSelectionPreview(Camera camera)
    {
        var previousTarget = RenderTexture.active;
#if UNITY_EDITOR
            // A cached preview must not capture Unity's cyan compiling-shader placeholder.
            bool asyncCompilation = UnityEditor.ShaderUtil.allowAsyncCompilation;
            UnityEditor.ShaderUtil.allowAsyncCompilation = false;
#endif
            try { camera.Render(); }
            finally
            {
#if UNITY_EDITOR
            UnityEditor.ShaderUtil.allowAsyncCompilation = asyncCompilation;
#endif
            RenderTexture.active = previousTarget;
            }
    }

    private void EnsureDicePreviews(Camera camera, Light light)
    {
        dicePreviews = new RenderTexture[DiceColors.Length];
        var prefab = Resources.Load<GameObject>("Dice/MacricioxRegularDie");
        for (int i = 0; i < dicePreviews.Length; i++)
        {
            var die = Instantiate(prefab, new Vector3(1000, 0, 0), Quaternion.Euler(0, 15, 0));
            try
            {
                foreach (var part in die.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 26;
                ApplyRegularDieColor(die, DiceColors[i]);
                var renderers = die.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                float extent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                camera.transform.position = bounds.center + new Vector3(2, 1.65f, -2) * extent;
                camera.transform.LookAt(bounds.center);
                camera.nearClipPlane = extent * 0.01f;
                camera.farClipPlane = extent * 10;
                camera.orthographicSize = extent * 1.26f;
                light.transform.rotation = Quaternion.Euler(35, -35, 0);
                light.intensity = 1.25f;
                dicePreviews[i] = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32) { name = "Dice selection " + i };
                camera.targetTexture = dicePreviews[i];
                RenderSelectionPreview(camera);
            }
            finally
            {
                foreach (var renderer in die.GetComponentsInChildren<Renderer>())
                    Destroy(renderer.sharedMaterial); // ApplyRegularDieColor clones only the first material slot.
                die.SetActive(false);
                Destroy(die);
            }
        }
    }

    private bool DrawDicePreview(Rect rect, int index)
    {
        EnsureHandPreviews();
        bool pressed = GUI.Button(rect, new GUIContent("", new[] { "White", "Black", "Green", "Blue" }[index]), GUIStyle.none);
        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, dicePreviews[index], ScaleMode.ScaleToFit, true);
        if (selectedDiceColor == DiceColors[index])
        {
            GUI.color = new Color(0.05f, 0.8f, 0.95f);
            GUI.DrawTexture(new Rect(rect.x + 20, rect.yMax - 3, rect.width - 40, 3), Texture2D.whiteTexture);
        }
        GUI.color = previous;
        return pressed;
    }

    private bool DrawHandPreview(Rect rect, int shade)
    {
        EnsureHandPreviews();
        bool clicked = GUI.Button(rect, new GUIContent("", "Hand shade " + (shade + 1)), GUIStyle.none);
        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, handPreviews[shade], ScaleMode.ScaleToFit, true);
        GUI.color = selectedHandSkin == shade ? Color.white : new Color(0.3f, 0.32f, 0.34f);
        GUI.DrawTexture(new Rect(rect.x + 20, rect.yMax - 3, rect.width - 40, selectedHandSkin == shade ? 3 : 1), Texture2D.whiteTexture);
        GUI.color = previous;
        return clicked;
    }
}
