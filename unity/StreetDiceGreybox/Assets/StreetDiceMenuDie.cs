using System.Collections.Generic;
using UnityEngine;

// The main menu IS the die (owner 2026-10-23). A real 3D rounded die, drawn into the menu
// with its own camera and lights, that you can spin with your finger and tap:
//
//   top     PROFILE   (replaced the iPlay logo that used to sit on top)
//   sides   CRAPS, CEE-LO, TUTORIAL (ON/OFF), SETTINGS
//   bottom  iPlay logo -- the only face that does nothing when tapped
//
// Drag sideways to spin it (it snaps to the next side, a flick is enough); drag up to tip
// it and see the logo underneath (it springs back). Tap any face you can see to use it.
// EXIT is the only menu button that isn't on the die. Face art comes from the approved
// die render (Resources/Branding/menu-die-static), flattened by tools/menu-die/.
public sealed partial class StreetDiceGreyboxController
{
    private enum MenuDieFace { Profile, Logo, Craps, CeeLo, Tutorial, Settings }

    private const int MenuDieLayer = 25;
    private const float MenuDieHalf = 0.5f, MenuDieRadius = 0.13f;
    private const int MenuDieDivisions = 14;
    private static readonly Vector3 MenuDieOrigin = new Vector3(0f, -500f, 0f);

    private GameObject menuDieRoot, menuDieObject;
    private Camera menuDieCamera;
    private RenderTexture menuDieTexture;
    private Mesh menuDieMesh;
    private Material[] menuDieMaterials;
    private Texture2D menuDieTutorialOn, menuDieTutorialOff;
    private Vector2[] menuDieBaseUv, menuDieUv;
    private readonly Dictionary<MenuDieFace, (int start, int count, Vector3 normal, Vector3 right, Vector3 up)> menuDieFaces = new();
    private bool menuDieReady, menuDieFailed;

    // Spin state. Yaw is in degrees; 45 shows CRAPS on the left and CEE-LO on the right.
    private float menuDieYaw = 45f, menuDieYawFrom = 45f, menuDieYawTo = 45f, menuDieTurnStarted = -10f;
    private float menuDiePitch, menuDiePitchFrom, menuDieReleaseAt = -10f, menuDiePressAt = -10f;
    private bool menuDieDragging, menuDieDragMoved;
    private Vector2 menuDieDragStart, menuDieDragLast;
    private float menuDieDragStartTime, menuDieDragYawStart, menuDieDragVelocity;
    private int menuDieDragAxis; // 0 undecided, 1 sideways spin, 2 tip
    private const float MenuDieTurnSeconds = 0.34f;

    private bool EnsureMenuDie()
    {
        if (menuDieReady) return true;
        if (menuDieFailed) return false;
        var shader = Shader.Find("Standard");
        string[] names = { "profile", "logo", "craps", "ceelo", "tutorial-off", "settings" };
        var textures = new Texture2D[names.Length];
        for (int i = 0; i < names.Length; i++)
            textures[i] = Resources.Load<Texture2D>("Branding/MenuDie/menu-die-" + names[i]);
        menuDieTutorialOff = textures[4];
        menuDieTutorialOn = Resources.Load<Texture2D>("Branding/MenuDie/menu-die-tutorial-on");
        if (shader == null || menuDieTutorialOn == null || System.Array.Exists(textures, t => t == null))
        {
            // Falls back to the flat die picture rather than an empty menu.
            menuDieFailed = true;
            Debug.LogWarning("Menu die art or Standard shader missing; using the flat die image.");
            return false;
        }

        menuDieRoot = new GameObject("Menu die studio");
        menuDieRoot.transform.position = MenuDieOrigin;
        menuDieObject = new GameObject("Menu die");
        menuDieObject.transform.SetParent(menuDieRoot.transform, false);
        menuDieObject.layer = MenuDieLayer;
        menuDieMesh = BuildMenuDieMesh();
        menuDieObject.AddComponent<MeshFilter>().sharedMesh = menuDieMesh;
        var renderer = menuDieObject.AddComponent<MeshRenderer>();
        menuDieMaterials = new Material[6];
        for (int i = 0; i < 6; i++)
        {
            var texture = textures[i];
            // Top/bottom labels turn to stay readable, so their edges must tile seamlessly.
            texture.wrapMode = i <= 1 ? TextureWrapMode.Mirror : TextureWrapMode.Clamp;
            texture.anisoLevel = 4;
            var material = new Material(shader) { name = "Menu die " + names[i], mainTexture = texture };
            // Glazed ceramic like the render: soft gloss, no metal.
            material.SetFloat("_Glossiness", 0.62f);
            material.SetFloat("_Metallic", 0f);
            menuDieMaterials[i] = material;
        }
        menuDieTutorialOn.wrapMode = TextureWrapMode.Clamp;
        renderer.sharedMaterials = menuDieMaterials;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        var cameraObject = new GameObject("Menu die camera");
        cameraObject.transform.SetParent(menuDieRoot.transform, false);
        // Looking down at the die from in front and above, like the approved render.
        cameraObject.transform.localPosition = new Vector3(0f, 1.81f, -2.58f);
        cameraObject.transform.LookAt(MenuDieOrigin + new Vector3(0f, -0.04f, 0f));
        menuDieCamera = cameraObject.AddComponent<Camera>();
        menuDieCamera.cullingMask = 1 << MenuDieLayer;
        menuDieCamera.clearFlags = CameraClearFlags.SolidColor;
        menuDieCamera.backgroundColor = Color.clear;
        menuDieCamera.fieldOfView = 30f;
        menuDieCamera.nearClipPlane = 0.5f;
        menuDieCamera.farClipPlane = 10f;
        menuDieCamera.allowHDR = false;
        menuDieTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32)
            { name = "Menu die", antiAliasing = 4 };
        menuDieCamera.targetTexture = menuDieTexture;
        menuDieCamera.enabled = false;

        AddMenuDieLight("Menu die key", new Vector3(-1.6f, 3.2f, -2.2f), 0.85f, new Color(1f, 0.97f, 0.92f));
        AddMenuDieLight("Menu die fill", new Vector3(2.4f, 0.8f, -1.6f), 0.3f, new Color(0.85f, 0.9f, 1f));
        AddMenuDieLight("Menu die rim", new Vector3(0.5f, 1.5f, 3f), 0.3f, Color.white);
        IsolateMenuDieLayer();
        menuDieReady = true;
        return true;
    }

    private void AddMenuDieLight(string name, Vector3 offset, float intensity, Color color)
    {
        var lightObject = new GameObject(name);
        lightObject.transform.SetParent(menuDieRoot.transform, false);
        lightObject.transform.localPosition = offset;
        lightObject.transform.LookAt(MenuDieOrigin);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = intensity;
        light.color = color;
        light.cullingMask = 1 << MenuDieLayer;
        light.shadows = LightShadows.None;
    }

    // Keep the game's lights off the menu die and the menu die out of the game camera.
    private void IsolateMenuDieLayer()
    {
        if (Camera.main != null) Camera.main.cullingMask &= ~(1 << MenuDieLayer);
        foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (menuDieRoot == null || !light.transform.IsChildOf(menuDieRoot.transform))
                light.cullingMask &= ~(1 << MenuDieLayer);
    }

    private Mesh BuildMenuDieMesh()
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var submeshes = new List<List<int>>();
        // Order matches the materials: Profile, Logo, Craps, CeeLo, Tutorial, Settings.
        // At yaw 45 the camera sees local -Z on the left and +X on the right.
        var layout = new (MenuDieFace face, Vector3 normal, Vector3 up)[]
        {
            (MenuDieFace.Profile, Vector3.up, Vector3.forward),
            (MenuDieFace.Logo, Vector3.down, Vector3.forward),
            (MenuDieFace.Craps, Vector3.back, Vector3.up),
            (MenuDieFace.CeeLo, Vector3.right, Vector3.up),
            (MenuDieFace.Tutorial, Vector3.forward, Vector3.up),
            (MenuDieFace.Settings, Vector3.left, Vector3.up),
        };
        float inner = MenuDieHalf - MenuDieRadius;
        foreach (var (face, normal, up) in layout)
        {
            // Text reads left-to-right when you look at the face from outside.
            var right = Vector3.Cross(normal, up);
            int start = vertices.Count;
            for (int y = 0; y <= MenuDieDivisions; y++)
            for (int x = 0; x <= MenuDieDivisions; x++)
            {
                float u = x / (float)MenuDieDivisions, v = y / (float)MenuDieDivisions;
                var point = normal * MenuDieHalf + right * Mathf.Lerp(-MenuDieHalf, MenuDieHalf, u)
                    + up * Mathf.Lerp(-MenuDieHalf, MenuDieHalf, v);
                var core = new Vector3(Mathf.Clamp(point.x, -inner, inner), Mathf.Clamp(point.y, -inner, inner),
                    Mathf.Clamp(point.z, -inner, inner));
                var outward = (point - core).normalized;
                vertices.Add(core + outward * MenuDieRadius);
                normals.Add(outward);
                uvs.Add(new Vector2(u, v));
            }
            var triangles = new List<int>();
            for (int y = 0; y < MenuDieDivisions; y++)
            for (int x = 0; x < MenuDieDivisions; x++)
            {
                int a = start + y * (MenuDieDivisions + 1) + x, b = a + 1, c = a + MenuDieDivisions + 1, d = c + 1;
                // Unity's front faces are clockwise: make every triangle face outward.
                bool flip = Vector3.Dot(Vector3.Cross(vertices[c] - vertices[a], vertices[b] - vertices[a]), normal) < 0f;
                if (flip) triangles.AddRange(new[] { a, b, c, b, d, c });
                else triangles.AddRange(new[] { a, c, b, b, c, d });
            }
            submeshes.Add(triangles);
            menuDieFaces[face] = (start, vertices.Count - start, normal, right, up);
        }
        var mesh = new Mesh { name = "iPlay menu die" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = submeshes.Count;
        for (int i = 0; i < submeshes.Count; i++) mesh.SetTriangles(submeshes[i], i);
        mesh.RecalculateBounds();
        menuDieBaseUv = uvs.ToArray();
        menuDieUv = uvs.ToArray();
        return mesh;
    }

    // Called every frame from the play-experience update.
    private void UpdateMenuDie()
    {
        bool visible = mainOptions && startupScreen == StartupScreen.DieMenu && !exitConfirmation;
        if (!visible)
        {
            if (menuDieCamera != null) menuDieCamera.enabled = false;
            if (menuDieRoot != null && menuDieRoot.activeSelf) menuDieRoot.SetActive(false);
            menuDieDragging = false;
            return;
        }
        if (!EnsureMenuDie()) return;
        if (!menuDieRoot.activeSelf) { menuDieRoot.SetActive(true); IsolateMenuDieLayer(); }
        menuDieCamera.enabled = true;
        menuDieMaterials[(int)MenuDieFace.Tutorial].mainTexture = tutorialMode ? menuDieTutorialOn : menuDieTutorialOff;

        float now = Time.unscaledTime;
        if (!menuDieDragging)
        {
            float t = Mathf.Clamp01((now - menuDieTurnStarted) / MenuDieTurnSeconds);
            menuDieYaw = Mathf.LerpUnclamped(menuDieYawFrom, menuDieYawTo, EaseOutBack(t));
            // Tipped to look at the logo: spring back up.
            float back = Mathf.Clamp01((now - menuDieReleaseAt) / 0.42f);
            menuDiePitch = Mathf.LerpUnclamped(menuDiePitchFrom, 0f, EaseOutBack(back));
        }
        // Idle life: a slow bob and a little sway so it never looks frozen.
        float sway = menuDieDragging ? 0f : Mathf.Sin(now * 0.7f) * 2.5f;
        float press = now - menuDiePressAt < 0.16f ? Mathf.Sin((now - menuDiePressAt) / 0.16f * Mathf.PI) : 0f;
        var cameraRight = menuDieCamera.transform.right;
        menuDieObject.transform.localRotation = Quaternion.AngleAxis(menuDiePitch, cameraRight)
            * Quaternion.Euler(0f, menuDieYaw + sway, 0f);
        menuDieObject.transform.localPosition = new Vector3(0f, Mathf.Sin(now * 1.3f) * 0.02f, 0f);
        menuDieObject.transform.localScale = Vector3.one * (1f - press * 0.05f);
        KeepMenuDieLabelsUpright();
    }

    // PROFILE on top (and the logo underneath) turn with the spin so they always read
    // straight toward you, the way the logo sat on the original render.
    private void KeepMenuDieLabelsUpright()
    {
        var rotation = menuDieObject.transform.rotation;
        bool changed = false;
        foreach (var face in new[] { MenuDieFace.Profile, MenuDieFace.Logo })
        {
            var (start, count, normal, right, up) = menuDieFaces[face];
            Vector3 n = rotation * normal, r = rotation * right, u = rotation * up;
            Vector3 want = Vector3.ProjectOnPlane(menuDieCamera.transform.up, n);
            if (want.sqrMagnitude < 0.01f) want = Vector3.ProjectOnPlane(menuDieCamera.transform.forward, n);
            want.Normalize();
            float angle = Mathf.Atan2(Vector3.Dot(want, r), Vector3.Dot(want, u));
            float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
            for (int i = start; i < start + count; i++)
            {
                var p = menuDieBaseUv[i] - new Vector2(0.5f, 0.5f);
                menuDieUv[i] = new Vector2(p.x * cos - p.y * sin, p.x * sin + p.y * cos) + new Vector2(0.5f, 0.5f);
            }
            changed = true;
        }
        if (changed) menuDieMesh.uv = menuDieUv;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.25f, c3 = c1 + 1f;
        t = Mathf.Clamp01(t);
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Draws the die into the menu and handles its touches. Returns false when the 3D die
    // isn't available, so the caller can fall back to the flat picture.
    private bool DrawMenuDie(Rect rect)
    {
        if (!menuDieReady || menuDieTexture == null) return false;
        GUI.DrawTexture(rect, menuDieTexture, ScaleMode.ScaleToFit, true);
        var e = Event.current;
        if (exitConfirmation) return true;
        switch (e.type)
        {
            case EventType.MouseDown when rect.Contains(e.mousePosition):
                menuDieDragging = true;
                menuDieDragMoved = false;
                menuDieDragAxis = 0;
                menuDieDragStart = menuDieDragLast = e.mousePosition;
                menuDieDragStartTime = Time.unscaledTime;
                menuDieDragYawStart = menuDieYaw;
                menuDieDragVelocity = 0f;
                e.Use();
                break;
            case EventType.MouseDrag when menuDieDragging:
            {
                var delta = e.mousePosition - menuDieDragStart;
                if (!menuDieDragMoved && delta.magnitude > 12f) menuDieDragMoved = true;
                if (menuDieDragMoved && menuDieDragAxis == 0)
                    menuDieDragAxis = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? 1 : 2;
                if (menuDieDragAxis == 1)
                {
                    // Follows the finger: half the die's width is a quarter turn.
                    menuDieYaw = menuDieDragYawStart - delta.x * 180f / rect.width;
                    menuDieDragVelocity = Mathf.Lerp(menuDieDragVelocity,
                        -(e.mousePosition.x - menuDieDragLast.x) / Mathf.Max(Time.unscaledDeltaTime, 0.008f), 0.5f);
                }
                else if (menuDieDragAxis == 2)
                {
                    // Drag up to tip the die toward you and see the logo underneath.
                    menuDiePitch = Mathf.Clamp(-delta.y * 160f / rect.height, -25f, 82f);
                }
                menuDieDragLast = e.mousePosition;
                e.Use();
                break;
            }
            case EventType.MouseUp when menuDieDragging:
                menuDieDragging = false;
                if (!menuDieDragMoved && Time.unscaledTime - menuDieDragStartTime < 0.6f)
                    TapMenuDie(rect, e.mousePosition);
                else if (menuDieDragAxis == 1) SnapMenuDieYaw(menuDieDragVelocity * 180f / rect.width);
                menuDiePitchFrom = menuDiePitch;
                menuDieReleaseAt = Time.unscaledTime;
                e.Use();
                break;
        }
        return true;
    }

    private void SnapMenuDieYaw(float yawVelocity)
    {
        // Aim a little ahead in the direction of the flick so a short, quick swipe still turns.
        float projected = menuDieYaw + Mathf.Clamp(yawVelocity * 0.12f, -70f, 70f);
        float target = 45f + Mathf.Round((projected - 45f) / 90f) * 90f;
        menuDieYawFrom = menuDieYaw;
        menuDieYawTo = target;
        menuDieTurnStarted = Time.unscaledTime;
        if (!Mathf.Approximately(target, menuDieDragYawStart)) PlayAudio(menuClack, 0.38f);
    }

    private void TapMenuDie(Rect rect, Vector2 point)
    {
        if (!MenuDieFaceAt(rect, point, out var face)) return;
        menuDiePressAt = Time.unscaledTime;
        switch (face)
        {
            case MenuDieFace.Craps: SelectMenuFace(0); break;
            case MenuDieFace.CeeLo: SelectMenuFace(1); break;
            case MenuDieFace.Profile: PlayAudio(menuClack, 0.54f); OpenProfile(StartupScreen.DieMenu); break;
            case MenuDieFace.Settings: PlayAudio(menuClack, 0.54f); OpenGlobalSettings(); break;
            case MenuDieFace.Tutorial: PlayAudio(menuClack, 0.54f); SetTutorialMode(!tutorialMode); break;
            case MenuDieFace.Logo: break; // decoration only
        }
    }

    // Which face is under the finger: a ray from the die camera, tested against the die's
    // box in its own space (the rounded edges are close enough for a tap).
    private bool MenuDieFaceAt(Rect rect, Vector2 point, out MenuDieFace face)
    {
        face = MenuDieFace.Logo;
        float side = Mathf.Min(rect.width, rect.height);
        var square = new Rect(rect.center.x - side / 2f, rect.center.y - side / 2f, side, side);
        var viewport = new Vector3((point.x - square.x) / side, 1f - (point.y - square.y) / side, 0f);
        var ray = menuDieCamera.ViewportPointToRay(viewport);
        var t = menuDieObject.transform;
        Vector3 origin = t.InverseTransformPoint(ray.origin), dir = t.InverseTransformDirection(ray.direction);
        float near = float.NegativeInfinity, far = float.PositiveInfinity;
        int axis = -1; float sign = 0f;
        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(dir[i]) < 1e-6f)
            {
                if (Mathf.Abs(origin[i]) > MenuDieHalf) return false;
                continue;
            }
            float t1 = (-MenuDieHalf - origin[i]) / dir[i], t2 = (MenuDieHalf - origin[i]) / dir[i];
            float entry = Mathf.Min(t1, t2), exit = Mathf.Max(t1, t2);
            if (entry > near) { near = entry; axis = i; sign = dir[i] > 0f ? -1f : 1f; }
            far = Mathf.Min(far, exit);
        }
        if (axis < 0 || near > far || far < 0f) return false;
        var normal = Vector3.zero;
        normal[axis] = sign;
        foreach (var pair in menuDieFaces)
            if (pair.Value.normal == normal) { face = pair.Key; return true; }
        return false;
    }
}
