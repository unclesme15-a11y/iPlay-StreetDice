using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum GameMode
{
    Craps,
    CeeLo
}

public enum RollState
{
    WaitingForShot,
    FadeWindow,
    Rolling,
    Locked,
    Resolving,
    ShooterDecision
}

public sealed class StreetDiceGreyboxController : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5108";

    private const int HotDiceThreshold = 5;
    private const float DiceRestY = -0.12f;
    private const float DiceWorldScale = 0.09f;
    private const float DieHalfSize = 0.5f;
    private const float DieCornerRadius = 0.115f;
    private const int DieMeshDivisions = 16;

    private GameObject dieA = null!;
    private GameObject dieB = null!;
    private GameObject dieC = null!;
    private GameObject dieAShadow = null!;
    private GameObject dieBShadow = null!;
    private GameObject dieCShadow = null!;
    private GameObject dieABlur = null!;
    private GameObject dieBBlur = null!;
    private GameObject dieCBlur = null!;
    private GameObject rollLane = null!;
    private GameObject environmentPlate = null!;
    private GameObject thresholdOccluder = null!;
    private GameObject handRig = null!;
    private GameObject leftThrowHand = null!;
    private GameObject rightThrowHand = null!;
    private AudioSource audioSource = null!;
    private AudioClip rollClip = null!;
    private AudioClip lockClip = null!;
    private AudioClip winClip = null!;
    private AudioClip lossClip = null!;
    private AudioClip fadeClip = null!;
    private readonly SeatMic[] mics = new SeatMic[5];
    private readonly List<DemoSideBet> demoSideBets = new();
    private SideBetDto[] serverSideBets = Array.Empty<SideBetDto>();
    private readonly System.Random random = new();
    private static readonly string[] DemoShooterOrder = { "p1", "p3", "p4", "p2", "bot-5" };

    private GameMode gameMode = GameMode.Craps;
    private RollState rollState = RollState.WaitingForShot;
    private readonly Dictionary<string, string> playerTokens = new();
    private string gameId = "";
    private string shooterToken = "";
    private string catcherToken = "";
    private string shooterId = "p1";
    private string catcherId = "p2";
    private string phase = "Demo";
    private string result = "Tap Demo Table to start a local playable table.";
    private string point = "-";
    private int streak;
    private int shotAmount = 20;
    private int die1 = 1;
    private int die2 = 1;
    private int die3 = 1;
    private int fadeCount;
    private int shooterMomentum;
    private string activePointGroup = "-";
    private string tutorialDetail = "Tutorial mode shows why the latest roll counted.";
    private string deterministicRoll = "Random";
    private string bankerCeeLo = "Banker: not rolled";
    private int bankerCeeLoRank;
    private bool bankerCeeLoReady;
    private bool localDemo = true;
    private bool lastResolvedShotWasWin;
    private bool lastShotWasDoubleUp;
    private bool rolling;
    private bool tutorialMode;
    private bool showPrototypeSeatMarkers;
    private bool sceneInitialized;
    private bool usingPurchasedHandPack;
    private float rollLockFlashUntil;
    private float handThrowStartedAt = -10f;
    private float handThrowVisibleUntil = -10f;
    private ThrowPath activeThrowPath;
    private Color selectedDiceColor = new Color(0.92f, 0.9f, 0.84f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<StreetDiceGreyboxController>() == null)
        {
            new GameObject("iPlay Cee-lo & Craps Demo").AddComponent<StreetDiceGreyboxController>();
        }
    }

    private void Awake()
    {
        BuildRuntimeScene();
    }

    public void BuildEnvironmentPreviewForEditor()
    {
        showPrototypeSeatMarkers = false;
        BuildRuntimeScene();
    }

    public void BuildHandThrowPreviewForEditor()
    {
        showPrototypeSeatMarkers = false;
        BuildRuntimeScene();
        shooterId = "p1";
        rollState = RollState.Rolling;
        rolling = true;
        var path = BuildThrowPath();
        activeThrowPath = path;
        UpdateThrowHandPose(0.28f, forceVisible: true);
        dieA.transform.position = Bezier(path.StartA, path.MidA, path.EndA, 0.52f) + Vector3.up * 0.16f;
        dieB.transform.position = Bezier(path.StartB, path.MidB, path.EndB, 0.5f) + Vector3.up * 0.12f;
        dieC.SetActive(false);
        dieCShadow.SetActive(false);
        UpdateMotionBlur(dieA, dieABlur, new Vector3(480, 650, 370));
        UpdateMotionBlur(dieB, dieBBlur, new Vector3(610, 420, 540));
        rollLockFlashUntil = Time.time + 0.7f;
    }

    private void BuildRuntimeScene()
    {
        if (sceneInitialized) return;
        sceneInitialized = true;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Application.targetFrameRate = 60;
        Camera.main?.gameObject.SetActive(false);
        CreateCamera();
        CreateLighting();
        CreateAudio();
        CreateStreetGroundScene();
        CreateMicSeats();

        dieA = CreateDie("Die A", new Vector3(-0.28f, DiceRestY, -2.25f));
        dieB = CreateDie("Die B", new Vector3(0.28f, DiceRestY, -2.25f));
        dieC = CreateDie("Die C", new Vector3(0f, DiceRestY, -2.58f));
        dieAShadow = CreateDieShadow("Die A Contact Shadow");
        dieBShadow = CreateDieShadow("Die B Contact Shadow");
        dieCShadow = CreateDieShadow("Die C Contact Shadow");
        dieABlur = CreateMotionBlur("Die A Motion Blur");
        dieBBlur = CreateMotionBlur("Die B Motion Blur");
        dieCBlur = CreateMotionBlur("Die C Motion Blur");
        CreateFirstPersonHandRig();
        dieC.SetActive(false);
        dieCShadow.SetActive(false);
        dieCBlur.SetActive(false);
        ResetDiceToShooter();
        ApplyDiceColor();
    }

    private void Update()
    {
        rollLane.GetComponent<Renderer>().material.color = Time.time < rollLockFlashUntil
            ? new Color(0.34f, 0.39f, 0.35f)
            : new Color(0.19f, 0.205f, 0.19f);

        if (rolling)
        {
            dieA.transform.Rotate(new Vector3(480, 650, 370) * Time.deltaTime, Space.World);
            dieB.transform.Rotate(new Vector3(610, 420, 540) * Time.deltaTime, Space.World);
            dieC.transform.Rotate(new Vector3(530, 360, 720) * Time.deltaTime, Space.World);
        }

        UpdateDieShadow(dieA, dieAShadow);
        UpdateDieShadow(dieB, dieBShadow);
        UpdateDieShadow(dieC, dieCShadow);
        UpdateMotionBlur(dieA, dieABlur, new Vector3(480, 650, 370));
        UpdateMotionBlur(dieB, dieBBlur, new Vector3(610, 420, 540));
        UpdateMotionBlur(dieC, dieCBlur, new Vector3(530, 360, 720));
        UpdateThrowHands();

        for (var i = 0; i < mics.Length; i++)
        {
            mics[i]?.Update(Time.time);
        }
    }

    private void OnGUI()
    {
        DrawTopRightStatus();
        DrawPlayerOverlays();
        DrawBottomControls();
        DrawStreakMeter();
    }

    private void CreateCamera()
    {
        var cameraObject = new GameObject("First Person Shooter Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 0.34f, -4.85f);
        camera.transform.rotation = Quaternion.Euler(2.8f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.02f, 0.02f);
        camera.fieldOfView = 58f;
    }

    private void CreateLighting()
    {
        var keyObject = new GameObject("Street Overhead Light");
        var key = keyObject.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 1.15f;
        key.transform.rotation = Quaternion.Euler(52f, -22f, 0f);

        var fillObject = new GameObject("Door Spill Light");
        var fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.intensity = 1.1f;
        fill.range = 8f;
        fill.transform.position = new Vector3(0f, 2.3f, 2.5f);
        fill.color = new Color(0.72f, 0.84f, 0.9f);

        var diceObject = new GameObject("Dice Practical Light");
        var diceLight = diceObject.AddComponent<Light>();
        diceLight.type = LightType.Point;
        diceLight.intensity = 1.8f;
        diceLight.range = 3.8f;
        diceLight.transform.position = new Vector3(0f, 1.15f, -1.2f);
        diceLight.color = new Color(1f, 0.92f, 0.78f);
    }

    private void CreateAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.45f;
        rollClip = CreateDiceRollClip("Dice Roll On Wet Pavement", 0.58f);
        lockClip = CreateImpactClip("Dice Lock On Pavement", 0.12f, 0.26f);
        winClip = CreateToneClip("Win Click", 520f, 0.12f, 0.08f);
        lossClip = CreateImpactClip("Loss Tap", 0.16f, 0.18f);
        fadeClip = CreateImpactClip("Fade Catch Tap", 0.09f, 0.16f);
    }

    private static AudioClip CreateDiceRollClip(string clipName, float duration)
    {
        const int sampleRate = 22050;
        var sampleCount = Mathf.CeilToInt(sampleRate * duration);
        var samples = new float[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var envelope = Mathf.Clamp01(1f - t / duration);
            var grit = HashSigned(i, 41, 9) * 0.035f;
            var rumble = Mathf.Sin(2f * Mathf.PI * 82f * t) * 0.022f * envelope;
            var skip = Mathf.Abs(Mathf.Sin(2f * Mathf.PI * 13.5f * t));
            samples[i] = (grit * skip + rumble) * envelope;
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateImpactClip(string clipName, float duration, float amplitude)
    {
        const int sampleRate = 22050;
        var sampleCount = Mathf.CeilToInt(sampleRate * duration);
        var samples = new float[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var envelope = Mathf.Exp(-t * 34f);
            var click = HashSigned(i, 97, 3) * amplitude * envelope;
            var body = Mathf.Sin(2f * Mathf.PI * 190f * t) * amplitude * 0.24f * envelope;
            samples[i] = click + body;
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateToneClip(string clipName, float frequency, float duration, float amplitude)
    {
        const int sampleRate = 22050;
        var sampleCount = Mathf.CeilToInt(sampleRate * duration);
        var samples = new float[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var envelope = 1f - i / (float)sampleCount;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude * envelope;
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void CreateStreetGroundScene()
    {
        if (CreateKlingEnvironmentPlate())
        {
            rollLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rollLane.name = "Invisible Roll Alignment Plane";
            rollLane.transform.position = new Vector3(0f, 0.035f, 0.35f);
            rollLane.transform.localScale = new Vector3(6.9f, 0.08f, 6.35f);
            rollLane.GetComponent<Renderer>().enabled = false;
            return;
        }

        var asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        asphalt.name = "Wet Asphalt Foreground";
        asphalt.transform.position = new Vector3(0f, -0.08f, -3.95f);
        asphalt.transform.localScale = new Vector3(7.2f, 0.08f, 2.2f);
        asphalt.GetComponent<Renderer>().material.color = new Color(0.01f, 0.012f, 0.012f, 0.08f);

        for (var i = 0; i < 28; i++)
        {
            var glint = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glint.name = "Wet Asphalt Highlight";
            glint.transform.position = new Vector3(UnityEngine.Random.Range(-3.1f, 3.1f), -0.032f, UnityEngine.Random.Range(-4.82f, -3.15f));
            glint.transform.localScale = new Vector3(UnityEngine.Random.Range(0.05f, 0.22f), 0.006f, UnityEngine.Random.Range(0.012f, 0.04f));
            glint.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(-16f, 16f), 0f);
            glint.GetComponent<Renderer>().material.color = new Color(0.14f, 0.18f, 0.19f);
        }

        var curbFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        curbFace.name = "Raised Sidewalk Curb Face";
        curbFace.transform.position = new Vector3(0f, 0.0f, -2.82f);
        curbFace.transform.localScale = new Vector3(6.9f, 0.18f, 0.16f);
        curbFace.GetComponent<Renderer>().material.color = new Color(0.08f, 0.085f, 0.08f);

        rollLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rollLane.name = "Raised Bodega Sidewalk Roll Surface";
        rollLane.transform.position = new Vector3(0f, 0.035f, 0.35f);
        rollLane.transform.localScale = new Vector3(6.9f, 0.08f, 6.35f);
        rollLane.GetComponent<Renderer>().material.color = new Color(0.035f, 0.04f, 0.038f);

        for (var i = 0; i < 11; i++)
        {
            var seam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seam.name = "Concrete Slab Joint";
            seam.transform.position = new Vector3(0f, 0.084f, -2.35f + i * 0.55f);
            seam.transform.localScale = new Vector3(6.85f, 0.012f, 0.018f);
            seam.GetComponent<Renderer>().material.color = new Color(0.04f, 0.044f, 0.042f);
        }

        for (var i = 0; i < 7; i++)
        {
            var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patch.name = "Street Ground Patch";
            patch.transform.position = new Vector3(UnityEngine.Random.Range(-2.85f, 2.85f), 0.092f, -2.25f + i * 0.72f);
            patch.transform.localScale = new Vector3(UnityEngine.Random.Range(0.35f, 0.82f), 0.014f, UnityEngine.Random.Range(0.05f, 0.11f));
            patch.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(-12f, 12f), 0f);
            patch.GetComponent<Renderer>().material.color = new Color(0.055f, 0.06f, 0.055f);
        }

        var backDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backDoor.name = "Closed Bodega Service Door";
        backDoor.transform.position = new Vector3(0f, 1.15f, 3.42f);
        backDoor.transform.localScale = new Vector3(4.7f, 2.28f, 0.12f);
        backDoor.GetComponent<Renderer>().material.color = new Color(0.022f, 0.026f, 0.028f);

        for (var i = 0; i < 13; i++)
        {
            var slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slat.name = "Service Door Slat";
            slat.transform.position = new Vector3(0f, 0.18f + i * 0.17f, 3.345f);
            slat.transform.localScale = new Vector3(4.78f, 0.036f, 0.05f);
            slat.GetComponent<Renderer>().material.color = new Color(0.045f, 0.052f, 0.052f);
        }

        for (var i = 0; i < 7; i++)
        {
            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shadow.name = "Rollup Door Dark Groove";
            shadow.transform.position = new Vector3(0f, 0.265f + i * 0.34f, 3.31f);
            shadow.transform.localScale = new Vector3(4.75f, 0.026f, 0.055f);
            shadow.GetComponent<Renderer>().material.color = new Color(0.008f, 0.01f, 0.011f);
        }

        var signBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signBand.name = "Bodega Sign Band";
        signBand.transform.position = new Vector3(0f, 2.62f, 3.48f);
        signBand.transform.localScale = new Vector3(5.4f, 0.22f, 0.08f);
        signBand.GetComponent<Renderer>().material.color = new Color(0.035f, 0.02f, 0.018f);

        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "Left Tight Brick Wall";
        leftWall.transform.position = new Vector3(-3.02f, 1.08f, 0.42f);
        leftWall.transform.localScale = new Vector3(0.16f, 2.3f, 6.18f);
        leftWall.GetComponent<Renderer>().material.color = new Color(0.035f, 0.022f, 0.02f);

        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "Right Tight Brick Wall";
        rightWall.transform.position = new Vector3(3.02f, 1.08f, 0.42f);
        rightWall.transform.localScale = new Vector3(0.16f, 2.3f, 6.18f);
        rightWall.GetComponent<Renderer>().material.color = new Color(0.035f, 0.022f, 0.02f);

        for (var i = 0; i < 9; i++)
        {
            CreateWallCourse(-2.395f, -2.85f + i * 0.7f);
            CreateWallCourse(2.395f, -2.85f + i * 0.7f);
        }
    }

    private bool CreateKlingEnvironmentPlate()
    {
        var texture = Resources.Load<Texture2D>("Environments/bodega-garage-door-down-one-pavement-02");
        if (texture == null)
        {
            Debug.LogWarning("Kling environment plate texture not found in Resources/Environments.");
            return false;
        }

        environmentPlate = GameObject.CreatePrimitive(PrimitiveType.Quad);
        environmentPlate.name = "Kling Bodega Environment Plate";
        environmentPlate.transform.position = new Vector3(0f, 0.72f, 1.15f);
        environmentPlate.transform.rotation = Quaternion.identity;
        environmentPlate.transform.localScale = new Vector3(12.6f, 8.9f, 1f);

        var shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Unlit");
        var material = new Material(shader);
        material.mainTexture = texture;
        environmentPlate.GetComponent<Renderer>().material = material;
        return true;
    }

    private void CreateThresholdOccluder()
    {
        thresholdOccluder = GameObject.CreatePrimitive(PrimitiveType.Cube);
        thresholdOccluder.name = "Kling Plate Threshold Occlusion Mask";
        thresholdOccluder.transform.position = new Vector3(0f, 0.126f, -0.06f);
        thresholdOccluder.transform.localScale = new Vector3(2.05f, 0.035f, 0.018f);
        thresholdOccluder.GetComponent<Renderer>().material = CreateTransparentMaterial(new Color(0.04f, 0.04f, 0.035f, 0.1f));
    }

    private void CreateWallCourse(float x, float z)
    {
        for (var row = 0; row < 4; row++)
        {
            var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brick.name = "Wall Brick Suggestion";
            brick.transform.position = new Vector3(x, 0.24f + row * 0.27f, z + (row % 2) * 0.17f);
            brick.transform.localScale = new Vector3(0.025f, 0.025f, 0.33f);
            brick.GetComponent<Renderer>().material.color = new Color(0.09f, 0.045f, 0.037f);
        }
    }

    private void CreateMicSeats()
    {
        mics[0] = CreateMic("You", "p1", new Vector3(0f, 0.28f, -2.95f), new Color(0.42f, 0.78f, 1f), true);
        mics[1] = CreateMic("Catcher AI", "p2", new Vector3(0f, 0.28f, 2.2f), new Color(0.95f, 0.72f, 0.18f), false);
        mics[2] = CreateMic("Left Human", "p3", new Vector3(-2.05f, 0.28f, -0.28f), new Color(0.42f, 0.78f, 1f), true);
        mics[3] = CreateMic("Right Human", "p4", new Vector3(2.05f, 0.28f, -0.28f), new Color(0.42f, 0.78f, 1f), true);
        mics[4] = CreateMic("Back AI", "bot-5", new Vector3(1.02f, 0.28f, 1.18f), new Color(0.95f, 0.56f, 0.22f), false);
    }

    private SeatMic CreateMic(string label, string playerId, Vector3 position, Color accent, bool human)
    {
        var root = new GameObject(label + " Mic");
        root.transform.position = position;

        var stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stand.name = label + " Mic Stand";
        stand.transform.SetParent(root.transform, false);
        stand.transform.localPosition = new Vector3(0f, 0.24f, 0f);
        stand.transform.localScale = new Vector3(0.045f, 0.24f, 0.045f);
        stand.GetComponent<Renderer>().material.color = new Color(0.06f, 0.07f, 0.07f);

        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = label + " Mic Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        head.transform.localScale = new Vector3(0.32f, 0.24f, 0.32f);
        head.GetComponent<Renderer>().material.color = accent;

        var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pulse.name = label + " Voice Pulse";
        pulse.transform.SetParent(root.transform, false);
        pulse.transform.localPosition = new Vector3(0f, 0.52f, 0f);
        pulse.transform.localScale = new Vector3(0.5f, 0.08f, 0.5f);
        pulse.GetComponent<Renderer>().material.color = new Color(accent.r, accent.g, accent.b, 0.35f);
        root.SetActive(showPrototypeSeatMarkers);

        return new SeatMic(label, playerId, root, head, pulse, accent, human);
    }

    private void CreateFirstPersonHandRig()
    {
        handRig = new GameObject("Local Shooter First Person Hand Rig");
        leftThrowHand = CreateThrowHand("Left", "FirstPersonHands/FirstPersonHand_L", new Vector3(-0.32f, -0.04f, 0f), Quaternion.Euler(8f, 158f, -12f));
        rightThrowHand = CreateThrowHand("Right", "FirstPersonHands/FirstPersonHand_R", new Vector3(0.32f, -0.04f, 0f), Quaternion.Euler(8f, -158f, 12f));
        handRig.SetActive(false);
    }

    private GameObject CreateThrowHand(string label, string resourcePath, Vector3 localPosition, Quaternion localRotation)
    {
        var prefab = Resources.Load<GameObject>(resourcePath);
        GameObject hand;
        if (prefab != null)
        {
            hand = Instantiate(prefab, handRig.transform);
            hand.name = label + " Purchased Hand";
            usingPurchasedHandPack = true;
            NormalizeHandScale(hand, 0.24f);
        }
        else
        {
            hand = CreateFallbackHand(label);
            hand.transform.SetParent(handRig.transform, false);
        }

        hand.transform.localPosition = localPosition;
        hand.transform.localRotation = localRotation;
        return hand;
    }

    private static void NormalizeHandScale(GameObject hand, float targetMaxSize)
    {
        var renderers = hand.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            hand.transform.localScale = Vector3.one;
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var maxSize = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        hand.transform.localScale = maxSize > 0.001f ? Vector3.one * (targetMaxSize / maxSize) : Vector3.one;
    }

    private static GameObject CreateFallbackHand(string label)
    {
        var root = new GameObject(label + " Fallback Hand");
        var palm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        palm.name = label + " Fallback Palm";
        palm.transform.SetParent(root.transform, false);
        palm.transform.localScale = new Vector3(0.16f, 0.11f, 0.22f);
        palm.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        palm.GetComponent<Renderer>().material = CreateHandMaterial();

        for (var i = 0; i < 4; i++)
        {
            var finger = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            finger.name = label + " Fallback Finger";
            finger.transform.SetParent(root.transform, false);
            finger.transform.localPosition = new Vector3(-0.075f + i * 0.05f, 0.02f, 0.18f);
            finger.transform.localRotation = Quaternion.Euler(68f, 0f, 0f);
            finger.transform.localScale = new Vector3(0.032f, 0.1f, 0.032f);
            finger.GetComponent<Renderer>().material = CreateHandMaterial();
        }

        var thumb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        thumb.name = label + " Fallback Thumb";
        thumb.transform.SetParent(root.transform, false);
        thumb.transform.localPosition = new Vector3(label == "Left" ? 0.14f : -0.14f, -0.015f, 0.04f);
        thumb.transform.localRotation = Quaternion.Euler(80f, 0f, label == "Left" ? -46f : 46f);
        thumb.transform.localScale = new Vector3(0.04f, 0.1f, 0.04f);
        thumb.GetComponent<Renderer>().material = CreateHandMaterial();
        return root;
    }

    private static Material CreateHandMaterial()
    {
        var material = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.5f, 0.31f, 0.22f);
        material.SetFloat("_Glossiness", 0.18f);
        material.SetFloat("_Metallic", 0f);
        return material;
    }

    private GameObject CreateDie(string dieName, Vector3 position)
    {
        var die = new GameObject(dieName);
        die.AddComponent<MeshFilter>().mesh = CreateRoundedCubeMesh(DieHalfSize, DieCornerRadius, DieMeshDivisions);
        var renderer = die.AddComponent<MeshRenderer>();
        renderer.material = CreateDiceMaterial(selectedDiceColor);
        die.name = dieName;
        die.transform.position = position;
        die.transform.localScale = Vector3.one * DiceWorldScale;
        CreatePips(die);
        CreateFaceWear(die);
        return die;
    }

    private static Material CreateDiceMaterial(Color color)
    {
        var material = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        material.mainTexture = CreateDiceAlbedoTexture(color);
        material.SetFloat("_Glossiness", 0.26f);
        material.SetFloat("_Metallic", 0f);
        material.SetTexture("_BumpMap", CreateDiceNormalTexture());
        material.SetFloat("_BumpScale", 0.18f);
        material.EnableKeyword("_NORMALMAP");
        return material;
    }

    private static Material CreateTransparentMaterial(Color color)
    {
        var material = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        return material;
    }

    private static Texture2D CreateDiceAlbedoTexture(Color baseColor)
    {
        const int size = 96;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Procedural Dice Albedo",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var grain = Hash01(x, y, 17) - 0.5f;
                var scratch = Mathf.Abs(Mathf.Sin((x * 0.47f + y * 0.12f) * Mathf.Deg2Rad)) < 0.008f ? 0.045f : 0f;
                var edge = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y)) / (float)size;
                var edgeWear = Mathf.Clamp01(0.12f - edge) * 0.18f;
                var factor = 1f + grain * 0.08f + scratch + edgeWear;
                texture.SetPixel(x, y, new Color(
                    Mathf.Clamp01(baseColor.r * factor),
                    Mathf.Clamp01(baseColor.g * factor),
                    Mathf.Clamp01(baseColor.b * factor),
                    1f));
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateDiceNormalTexture()
    {
        const int size = 96;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Procedural Dice Fine Normal",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear
        };

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var nx = (Hash01(x + 1, y, 31) - Hash01(x - 1, y, 31)) * 0.18f;
                var ny = (Hash01(x, y + 1, 31) - Hash01(x, y - 1, 31)) * 0.18f;
                texture.SetPixel(x, y, new Color(0.5f + nx, 0.5f + ny, 1f, 1f));
            }
        }

        texture.Apply();
        return texture;
    }

    private static GameObject CreateDieShadow(string shadowName)
    {
        var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.name = shadowName;
        shadow.transform.localScale = new Vector3(0.28f, 0.08f, 1f);
        var renderer = shadow.GetComponent<Renderer>();
        renderer.material = CreateTransparentMaterial(new Color(0.002f, 0.002f, 0.002f, 0.72f));
        return shadow;
    }

    private static void UpdateDieShadow(GameObject die, GameObject shadow)
    {
        if (die == null || shadow == null) return;
        shadow.SetActive(die.activeInHierarchy);
        if (!shadow.activeInHierarchy) return;

        var height = Mathf.Clamp(die.transform.position.y - DiceRestY, 0f, 0.75f);
        var scale = Mathf.Lerp(0.34f, 0.2f, height / 0.75f);
        shadow.transform.position = die.transform.position + new Vector3(0f, -0.19f, -0.02f);
        shadow.transform.rotation = Camera.main != null
            ? Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up)
            : Quaternion.identity;
        shadow.transform.localScale = new Vector3(scale, scale * 0.34f, 1f);
    }

    private static GameObject CreateMotionBlur(string blurName)
    {
        var blur = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blur.name = blurName;
        blur.transform.localScale = new Vector3(0.28f, 0.035f, 0.055f);
        blur.GetComponent<Renderer>().material = CreateTransparentMaterial(new Color(0.92f, 0.9f, 0.84f, 0.16f));
        blur.SetActive(false);
        return blur;
    }

    private void UpdateMotionBlur(GameObject die, GameObject blur, Vector3 spin)
    {
        if (die == null || blur == null) return;
        var active = rolling && die.activeInHierarchy;
        blur.SetActive(active);
        if (!active) return;

        var color = streak >= HotDiceThreshold ? new Color(1f, 0.23f, 0.02f, 0.18f) : selectedDiceColor;
        color.a = 0.18f;
        blur.GetComponent<Renderer>().material.color = color;
        blur.transform.position = die.transform.position - die.transform.forward * 0.1f;
        blur.transform.rotation = Quaternion.LookRotation(spin.normalized, Vector3.up);
    }

    private static Mesh CreateRoundedCubeMesh(float halfSize, float radius, int divisions)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        var inner = halfSize - radius;

        AddRoundedCubeFace(vertices, normals, triangles, Vector3.forward, Vector3.right, Vector3.up, halfSize, inner, radius, divisions);
        AddRoundedCubeFace(vertices, normals, triangles, Vector3.back, Vector3.left, Vector3.up, halfSize, inner, radius, divisions);
        AddRoundedCubeFace(vertices, normals, triangles, Vector3.right, Vector3.back, Vector3.up, halfSize, inner, radius, divisions);
        AddRoundedCubeFace(vertices, normals, triangles, Vector3.left, Vector3.forward, Vector3.up, halfSize, inner, radius, divisions);
        AddRoundedCubeFace(vertices, normals, triangles, Vector3.up, Vector3.right, Vector3.back, halfSize, inner, radius, divisions);
        AddRoundedCubeFace(vertices, normals, triangles, Vector3.down, Vector3.right, Vector3.forward, halfSize, inner, radius, divisions);

        var mesh = new Mesh
        {
            name = "Rounded Pip Die Mesh"
        };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddRoundedCubeFace(
        List<Vector3> vertices,
        List<Vector3> normals,
        List<int> triangles,
        Vector3 normal,
        Vector3 right,
        Vector3 up,
        float halfSize,
        float inner,
        float radius,
        int divisions)
    {
        var start = vertices.Count;
        for (var y = 0; y <= divisions; y++)
        {
            var v = Mathf.Lerp(-halfSize, halfSize, y / (float)divisions);
            for (var x = 0; x <= divisions; x++)
            {
                var u = Mathf.Lerp(-halfSize, halfSize, x / (float)divisions);
                var point = normal * halfSize + right * u + up * v;
                var core = new Vector3(
                    Mathf.Clamp(point.x, -inner, inner),
                    Mathf.Clamp(point.y, -inner, inner),
                    Mathf.Clamp(point.z, -inner, inner));
                var outward = (point - core).normalized;
                vertices.Add(core + outward * radius);
                normals.Add(outward);
            }
        }

        for (var y = 0; y < divisions; y++)
        {
            for (var x = 0; x < divisions; x++)
            {
                var a = start + y * (divisions + 1) + x;
                var b = a + 1;
                var c = a + divisions + 1;
                var d = c + 1;
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }
    }

    private void CreatePips(GameObject die)
    {
        CreateFacePips(die, Vector3.up, Vector3.forward, Vector3.right, 1);
        CreateFacePips(die, Vector3.down, Vector3.forward, Vector3.left, 6);
        CreateFacePips(die, Vector3.forward, Vector3.up, Vector3.right, 2);
        CreateFacePips(die, Vector3.back, Vector3.up, Vector3.left, 5);
        CreateFacePips(die, Vector3.right, Vector3.up, Vector3.back, 3);
        CreateFacePips(die, Vector3.left, Vector3.up, Vector3.forward, 4);
    }

    private void CreateFacePips(GameObject die, Vector3 normal, Vector3 up, Vector3 right, int value)
    {
        var offsets = PipOffsets(value);
        for (var i = 0; i < offsets.Length; i++)
        {
            var center = normal * 0.506f + right * offsets[i].x + up * offsets[i].y;
            CreatePipDisc(die, die.name + " Pip Well " + value, center - normal * 0.002f, normal, 0.074f, 0.004f, new Color(0.03f, 0.032f, 0.03f));
            CreatePipDisc(die, die.name + " Pip Inset " + value, center + normal * 0.001f, normal, 0.055f, 0.005f, Color.black);
        }
    }

    private static void CreatePipDisc(GameObject die, string name, Vector3 localPosition, Vector3 normal, float radius, float thickness, Color color)
    {
        var pip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pip.name = name;
        pip.transform.SetParent(die.transform, false);
        pip.transform.localPosition = localPosition;
        pip.transform.localRotation = Quaternion.FromToRotation(Vector3.up, normal);
        pip.transform.localScale = new Vector3(radius, thickness, radius);
        var renderer = pip.GetComponent<Renderer>();
        renderer.material = CreatePipMaterial(color);
    }

    private static Material CreatePipMaterial(Color color)
    {
        var material = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        material.SetFloat("_Glossiness", 0.08f);
        material.SetFloat("_Metallic", 0f);
        return material;
    }

    private static void CreateFaceWear(GameObject die)
    {
        CreateFaceWear(die, Vector3.up, Vector3.forward, Vector3.right, 7);
        CreateFaceWear(die, Vector3.forward, Vector3.up, Vector3.right, 11);
        CreateFaceWear(die, Vector3.right, Vector3.up, Vector3.back, 13);
        CreateFaceWear(die, Vector3.left, Vector3.up, Vector3.forward, 17);
        CreateFaceWear(die, Vector3.back, Vector3.up, Vector3.left, 19);
    }

    private static void CreateFaceWear(GameObject die, Vector3 normal, Vector3 up, Vector3 right, int seed)
    {
        for (var i = 0; i < 4; i++)
        {
            var u = Mathf.Lerp(-0.32f, 0.32f, Hash01(seed, i, 5));
            var v = Mathf.Lerp(-0.32f, 0.32f, Hash01(seed, i, 9));
            var length = Mathf.Lerp(0.045f, 0.13f, Hash01(seed, i, 14));
            var angle = Mathf.Lerp(-35f, 35f, Hash01(seed, i, 23));
            var scratch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            scratch.name = die.name + " Fine Wear Mark";
            scratch.transform.SetParent(die.transform, false);
            scratch.transform.localPosition = normal * 0.512f + right * u + up * v;
            scratch.transform.localRotation = Quaternion.LookRotation(normal, up) * Quaternion.Euler(0f, 0f, angle);
            scratch.transform.localScale = new Vector3(length, 0.006f, 0.002f);
            var renderer = scratch.GetComponent<Renderer>();
            renderer.material = CreatePipMaterial(new Color(0.72f, 0.72f, 0.68f, 0.55f));
        }
    }

    private static Vector2[] PipOffsets(int value)
    {
        const float d = 0.22f;
        switch (value)
        {
            case 1:
                return new[] { Vector2.zero };
            case 2:
                return new[] { new Vector2(-d, -d), new Vector2(d, d) };
            case 3:
                return new[] { new Vector2(-d, -d), Vector2.zero, new Vector2(d, d) };
            case 4:
                return new[] { new Vector2(-d, -d), new Vector2(-d, d), new Vector2(d, -d), new Vector2(d, d) };
            case 5:
                return new[] { new Vector2(-d, -d), new Vector2(-d, d), Vector2.zero, new Vector2(d, -d), new Vector2(d, d) };
            default:
                return new[] { new Vector2(-d, -d), new Vector2(-d, 0f), new Vector2(-d, d), new Vector2(d, -d), new Vector2(d, 0f), new Vector2(d, d) };
        }
    }

    private void DrawTopRightStatus()
    {
        var width = tutorialMode ? 330f : 238f;
        var height = tutorialMode ? 220f : 126f;
        var x = Screen.width - width - 18f;
        GUI.Box(new Rect(x, 18f, width, height), "");
        GUI.Label(new Rect(x + 12f, 28f, width - 24f, 22f), gameMode == GameMode.CeeLo ? "CEE-LO" : point == "-" ? "COME OUT" : "POINT " + point);
        GUI.Label(new Rect(x + 12f, 52f, width - 24f, 22f), Time.time < rollLockFlashUntil ? "ROLL LOCKED" : "SHOT " + shotAmount);

        var dieSize = gameMode == GameMode.CeeLo ? 48f : 54f;
        var diceY = 76f;
        var diceX = x + 14f;
        DrawMagnifiedDie(new Rect(diceX, diceY, dieSize, dieSize), die1);
        DrawMagnifiedDie(new Rect(diceX + dieSize + 10f, diceY, dieSize, dieSize), die2);
        if (gameMode == GameMode.CeeLo)
        {
            DrawMagnifiedDie(new Rect(diceX + (dieSize + 10f) * 2f, diceY, dieSize, dieSize), die3);
        }

        if (tutorialMode)
        {
            var rollText = gameMode == GameMode.CeeLo
                ? die1 + " / " + die2 + " / " + die3
                : die1 + " + " + die2 + " = " + (die1 + die2);
            GUI.Label(new Rect(x + 12f, 136f, width - 24f, 22f), rollText);
            GUI.Label(new Rect(x + 12f, 158f, width - 24f, 22f), "Phase: " + phase);
            GUI.Label(new Rect(x + 12f, 180f, width - 24f, 22f), "Group: " + activePointGroup);
            GUI.Label(new Rect(x + 12f, 202f, width - 24f, 22f), "State: " + rollState + " | Side bets: " + OpenSideBetCount());
        }
    }

    private void DrawMagnifiedDie(Rect rect, int value)
    {
        var previous = GUI.color;
        GUI.color = streak >= HotDiceThreshold ? new Color(1f, 0.23f, 0.02f) : selectedDiceColor;
        GUI.Box(rect, "");
        GUI.color = ShouldUseDarkPips(GUI.color) ? new Color(0.03f, 0.03f, 0.028f) : Color.white;

        var pip = Mathf.Max(5f, rect.width * 0.12f);
        var inset = rect.width * 0.25f;
        foreach (var offset in PipOffsets(value))
        {
            var px = rect.center.x + offset.x / 0.22f * inset - pip * 0.5f;
            var py = rect.center.y - offset.y / 0.22f * inset - pip * 0.5f;
            GUI.DrawTexture(new Rect(px, py, pip, pip), Texture2D.whiteTexture);
        }

        GUI.color = previous;
    }

    private void DrawPlayerOverlays()
    {
        for (var i = 0; i < mics.Length; i++)
        {
            var seat = mics[i];
            if (seat == null) continue;
            if (Camera.main == null) continue;
            var screen = Camera.main.WorldToScreenPoint(seat.Root.transform.position + new Vector3(0f, 0.82f, 0f));
            if (screen.z <= 0f) continue;

            var rect = new Rect(screen.x - 76f, Screen.height - screen.y - 34f, 152f, 82f);
            var previousColor = GUI.color;
            GUI.color = seat.PlayerId == shooterId
                ? new Color(0.66f, 1f, 0.74f)
                : seat.PlayerId == catcherId
                    ? new Color(1f, 0.84f, 0.42f)
                    : Color.white;
            GUI.Box(rect, "");
            GUI.color = previousColor;

            GUI.Box(new Rect(rect.x + 8f, rect.y + 7f, 30f, 28f), seat.ProfileText);
            GUI.Label(new Rect(rect.x + 44f, rect.y + 5f, rect.width - 52f, 18f), seat.Label);
            GUI.Label(new Rect(rect.x + 44f, rect.y + 24f, rect.width - 52f, 18f), SeatStatusLine(seat));

            if (gameMode == GameMode.Craps && seat.PlayerId == catcherId)
            {
                if (GUI.Button(new Rect(rect.x + 8f, rect.y + 42f, rect.width - 16f, 22f), "Fade/Catch"))
                {
                    StartCoroutine(Fade());
                }
            }
            else if (gameMode == GameMode.Craps && phase == "Point" && seat.PlayerId != shooterId)
            {
                if (GUI.Button(new Rect(rect.x + 8f, rect.y + 42f, 66f, 22f), "Bet Hit"))
                {
                    PlaceSideBetFromUi(seat.PlayerId, false);
                }

                if (GUI.Button(new Rect(rect.x + 78f, rect.y + 42f, 66f, 22f), "Bet Miss"))
                {
                    PlaceSideBetFromUi(seat.PlayerId, true);
                }
            }
            else
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 44f, rect.width - 16f, 18f), gameMode == GameMode.CeeLo ? "Cee-lo seat" : ThrowLaneLabel(seat.PlayerId));
            }

            var playerBets = OpenSideBetCount(seat.PlayerId);
            if (playerBets > 0)
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 64f, rect.width - 16f, 18f), playerBets + " open bet");
            }
            else
            {
                var line = LatestSideBetLine(seat.PlayerId);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    GUI.Label(new Rect(rect.x + 8f, rect.y + 64f, rect.width - 16f, 18f), line);
                }
            }
        }
    }

    private string SeatStatusLine(SeatMic seat)
    {
        if (seat.PlayerId == shooterId) return "SHOOTER";
        if (seat.PlayerId == catcherId) return "FADE TARGET";
        return seat.IsHuman ? "HUMAN MIC" : "AI OPPONENT";
    }

    private static string ThrowLaneLabel(string playerId)
    {
        return playerId switch
        {
            "p1" => "bottom throw",
            "p3" => "left throw",
            "p4" => "right throw",
            "p2" => "back throw",
            "bot-5" => "back-right throw",
            _ => "table seat"
        };
    }

    private void DrawBottomControls()
    {
        var y = Screen.height - 118f;
        var buttonWidth = Mathf.Min(112f, (Screen.width - 88f) / 9f);
        var x = 20f;

        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Demo Table")) StartLocalDemo();
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Open Shot")) StartCoroutine(OpenShot());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), gameMode == GameMode.Craps ? "Mode: Craps" : "Mode: Cee-lo")) SwitchGameMode();
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Roll")) StartCoroutine(RollCurrentMode());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Run Same")) StartCoroutine(RunSame());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Double Up")) StartCoroutine(DoubleUp());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Server")) StartCoroutine(CreateTable());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Voice Gate")) StartCoroutine(VoiceGate());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Next Seat")) CycleDemoShooter();

        if (GUI.Button(new Rect(20f, y - 100f, 132f, 32f), tutorialMode ? "Tutorial On" : "Tutorial Off")) tutorialMode = !tutorialMode;
        DrawDeterministicControls(y - 100f);
        DrawDiceSkinControls(y - 142f);

        GUI.Box(new Rect(20f, y - 58f, Screen.width - 40f, 46f), "");
        GUI.Label(new Rect(32f, y - 48f, Screen.width - 64f, 26f), result);

        if (tutorialMode)
        {
            var tutorialWidth = Mathf.Max(240f, Mathf.Min(520f, Screen.width - 380f));
            GUI.Box(new Rect(20f, 18f, tutorialWidth, 88f), "");
            GUI.Label(new Rect(32f, 28f, tutorialWidth - 24f, 22f), tutorialDetail);
            GUI.Label(new Rect(32f, 52f, tutorialWidth - 24f, 22f), "Fade count: " + fadeCount + " | Momentum: " + shooterMomentum);
            GUI.Label(new Rect(32f, 76f, tutorialWidth - 24f, 22f), gameMode == GameMode.Craps ? "Test roll: " + deterministicRoll : bankerCeeLo);
        }
    }

    private void DrawDeterministicControls(float y)
    {
        if (!tutorialMode) return;

        var x = 160f;
        var width = 88f;
        if (GUI.Button(new Rect(x, y, width, 32f), "Random")) deterministicRoll = "Random";
        x += width + 6f;
        if (GUI.Button(new Rect(x, y, width, 32f), "7")) deterministicRoll = "Seven";
        x += width + 6f;
        if (GUI.Button(new Rect(x, y, width, 32f), "Point")) deterministicRoll = "Point";
        x += width + 6f;
        if (GUI.Button(new Rect(x, y, width, 32f), "Group")) deterministicRoll = "Group";
        x += width + 6f;
        if (GUI.Button(new Rect(x, y, width, 32f), "456")) deterministicRoll = "CeeLo456";
        x += width + 6f;
        if (GUI.Button(new Rect(x, y, width, 32f), "123")) deterministicRoll = "CeeLo123";
    }

    private void DrawDiceSkinControls(float y)
    {
        var x = 20f;
        GUI.Label(new Rect(x, y + 6f, 72f, 22f), "Dice");
        x += 58f;
        if (DiceSkinButton(x, y, "White", new Color(0.92f, 0.9f, 0.84f))) return;
        x += 74f;
        if (DiceSkinButton(x, y, "Black", new Color(0.018f, 0.019f, 0.018f))) return;
        x += 74f;
        if (DiceSkinButton(x, y, "Green", new Color(0.08f, 0.55f, 0.23f))) return;
        x += 74f;
        DiceSkinButton(x, y, "Blue", new Color(0.08f, 0.22f, 0.72f));
    }

    private bool DiceSkinButton(float x, float y, string label, Color color)
    {
        var previous = GUI.color;
        GUI.color = color;
        var clicked = GUI.Button(new Rect(x, y, 68f, 28f), "");
        GUI.color = previous;
        GUI.Label(new Rect(x + 6f, y + 5f, 56f, 18f), label);
        if (!clicked) return false;

        selectedDiceColor = color;
        ApplyDiceColor();
        result = "Dice color set to " + label + ". Red/orange is reserved for full streak.";
        return true;
    }

    private void DrawStreakMeter()
    {
        var y = Screen.height - 46f;
        var width = Mathf.Min(360f, Screen.width - 40f);
        GUI.Box(new Rect(20f, y, width, 22f), "");

        var previous = GUI.color;
        GUI.color = streak >= HotDiceThreshold ? new Color(1f, 0.23f, 0.02f) : new Color(0.1f, 0.72f, 0.35f);
        GUI.Box(new Rect(22f, y + 2f, Mathf.Clamp01(streak / (float)HotDiceThreshold) * (width - 4f), 18f), "");
        GUI.color = previous;

        GUI.Label(new Rect(28f, y + 2f, width - 56f, 18f), "STREAK " + streak + "/" + HotDiceThreshold);
    }

    private void StartLocalDemo()
    {
        localDemo = true;
        SetPrototypeSeatMarkersVisible(true);
        gameId = "";
        playerTokens.Clear();
        shooterId = "p1";
        catcherId = "p2";
        shotAmount = 20;
        point = "-";
        phase = "ComeOut";
        streak = 0;
        fadeCount = 0;
        shooterMomentum = 0;
        activePointGroup = "-";
        demoSideBets.Clear();
        lastResolvedShotWasWin = false;
        lastShotWasDoubleUp = false;
        dieC.SetActive(gameMode == GameMode.CeeLo);
        phase = gameMode == GameMode.CeeLo ? "CeeLo" : "ComeOut";
        rollState = gameMode == GameMode.CeeLo ? RollState.FadeWindow : RollState.WaitingForShot;
        bankerCeeLo = "Banker: not rolled";
        bankerCeeLoRank = 0;
        bankerCeeLoReady = false;
        result = gameMode == GameMode.CeeLo
            ? "Local Cee-lo table open. Roll three dice."
            : "Local demo table open. Shooter is first-person. Catcher mic is live.";
        tutorialDetail = usingPurchasedHandPack
            ? "Table reset. Purchased first-person hand pack loaded."
            : "Table reset. Fallback hands loaded; import the hand pack for the real rig.";
        PulseMic(catcherId, 1.5f);
        ResetDiceToShooter();
        ApplyDiceColor();
    }

    private void SwitchGameMode()
    {
        gameMode = gameMode == GameMode.Craps ? GameMode.CeeLo : GameMode.Craps;
        StartLocalDemo();
    }

    private void CycleDemoShooter()
    {
        if (!localDemo)
        {
            result = "Server mode keeps shooter turns authoritative.";
            return;
        }

        var currentIndex = Array.IndexOf(DemoShooterOrder, shooterId);
        var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % DemoShooterOrder.Length;
        shooterId = DemoShooterOrder[nextIndex];
        catcherId = shooterId == "p2" || shooterId == "bot-5" ? "p1" : "p2";
        point = "-";
        activePointGroup = "-";
        phase = gameMode == GameMode.CeeLo ? "CeeLo" : "ComeOut";
        rollState = RollState.FadeWindow;
        demoSideBets.Clear();
        fadeCount = 0;
        shooterMomentum = 0;
        ResetDiceToShooter();
        PulseMic(shooterId, 1.25f);
        PulseMic(catcherId, 1f);
        result = SeatLabel(shooterId) + " is shooting from the " + ThrowLaneLabel(shooterId) + " lane.";
        tutorialDetail = "Camera stays fixed. The dice enter from the active shooter's table position.";
    }

    private string SeatLabel(string playerId)
    {
        for (var i = 0; i < mics.Length; i++)
        {
            if (mics[i]?.PlayerId == playerId) return mics[i].Label;
        }

        return playerId;
    }

    private IEnumerator CreateTable()
    {
        localDemo = false;
        SetPrototypeSeatMarkersVisible(true);
        yield return Post("/api/street-dice/create", "{}", body =>
        {
            var response = JsonUtility.FromJson<CreateResponse>(body);
            gameId = response.gameId;
            UpdateState(response.state);
        });

        if (!string.IsNullOrWhiteSpace(gameId))
        {
            yield return JoinPlayers();
            yield return Post("/api/street-dice/" + gameId + "/bots/fill", "{\"targetPlayers\":5}");
        }
    }

    private IEnumerator JoinPlayers()
    {
        if (string.IsNullOrWhiteSpace(gameId)) yield break;

        yield return JoinPlayer("Shooter", "p1");
        yield return JoinPlayer("Catcher", "p2");
        yield return JoinPlayer("Left 1", "p3");
        yield return JoinPlayer("Right 1", "p4");
    }

    private IEnumerator JoinPlayer(string playerName, string playerId)
    {
        yield return Post("/api/street-dice/" + gameId + "/join", "{\"playerName\":\"" + playerName + "\",\"playerId\":\"" + playerId + "\"}", body =>
        {
            var response = JsonUtility.FromJson<JoinResponse>(body);
            playerTokens[playerId] = response.playerSessionToken;
            if (playerId == "p1") shooterToken = response.playerSessionToken;
            if (playerId == "p2") catcherToken = response.playerSessionToken;
            UpdateState(response.state);
        });
    }

    private IEnumerator OpenShot()
    {
        if (localDemo)
        {
            phase = gameMode == GameMode.CeeLo ? "CeeLo" : "ComeOut";
            point = "-";
            activePointGroup = "-";
            fadeCount = 0;
            shooterMomentum = 0;
            demoSideBets.Clear();
            rollState = RollState.FadeWindow;
            ResetDiceToShooter();
            result = gameMode == GameMode.CeeLo
                ? "Cee-lo shot open. Roll three dice."
                : "Shot open. Catcher can fade/catch before the roll counts.";
            tutorialDetail = gameMode == GameMode.CeeLo
                ? "Cee-lo uses three dice and does not use the craps point phase."
                : "Come-out roll: 7/11 wins, 2/3/12 loses but shooter keeps dice, other totals set point.";
            PulseMic(catcherId, 1.4f);
            yield break;
        }

        var json = $"{{\"shooterId\":\"{shooterId}\",\"shooterSessionToken\":\"{shooterToken}\",\"catcherId\":\"{catcherId}\",\"amount\":{shotAmount}}}";
        yield return Post("/api/street-dice/" + gameId + "/shot", json);
    }

    private IEnumerator Fade()
    {
        if (localDemo)
        {
            if (phase != "ComeOut" && phase != "Point")
            {
                result = "Open a shot first.";
                yield break;
            }

            fadeCount++;
            if (fadeCount > 3) shooterMomentum++;
            rollState = RollState.FadeWindow;
            result = fadeCount > 3
                ? "Fade/Catch. Roll stopped. Shooter momentum +" + shooterMomentum + "."
                : "Fade/Catch. Roll stopped. Shooter shoots again.";
            tutorialDetail = "Fade/Catch nullifies the roll. No payout and no side bet resolves.";
            PlayAudio(fadeClip);
            PulseMic(catcherId, 1.6f);
            ResetDiceToShooter();
            yield break;
        }

        var json = $"{{\"catcherId\":\"{catcherId}\",\"playerSessionToken\":\"{catcherToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/fade", json);
    }

    private IEnumerator RollCurrentMode()
    {
        if (gameMode == GameMode.CeeLo)
        {
            var ceeLoRoll = PickCeeLoRoll();
            yield return RollCeeLo(ceeLoRoll[0], ceeLoRoll[1], ceeLoRoll[2]);
            yield break;
        }

        var roll = PickCrapsRoll();
        yield return Roll(roll[0], roll[1]);
    }

    private int[] PickCrapsRoll()
    {
        if (!tutorialMode || deterministicRoll == "Random") return new[] { random.Next(1, 7), random.Next(1, 7) };
        if (deterministicRoll == "Seven") return new[] { 3, 4 };

        if (deterministicRoll == "Point" && phase == "Point" && int.TryParse(point, out var pointTotal))
        {
            return TwoDiceForTotal(pointTotal);
        }

        if (deterministicRoll == "Group" && phase == "Point" && int.TryParse(point, out var groupedPoint))
        {
            var groupedTotal = groupedPoint switch
            {
                4 => 10,
                10 => 4,
                6 => 8,
                8 => 6,
                5 => 9,
                9 => 5,
                _ => groupedPoint
            };
            return TwoDiceForTotal(groupedTotal);
        }

        return new[] { random.Next(1, 7), random.Next(1, 7) };
    }

    private int[] PickCeeLoRoll()
    {
        if (tutorialMode && deterministicRoll == "CeeLo456") return new[] { 4, 5, 6 };
        if (tutorialMode && deterministicRoll == "CeeLo123") return new[] { 1, 2, 3 };
        return new[] { random.Next(1, 7), random.Next(1, 7), random.Next(1, 7) };
    }

    private static int[] TwoDiceForTotal(int total)
    {
        for (var a = 1; a <= 6; a++)
        {
            var b = total - a;
            if (b is >= 1 and <= 6) return new[] { a, b };
        }

        return new[] { 3, 4 };
    }

    private IEnumerator Roll(int a, int b)
    {
        if (phase != "ComeOut" && phase != "Point")
        {
            result = "Open a shot first.";
            yield break;
        }

        rollState = RollState.Rolling;
        die1 = a;
        die2 = b;
        yield return AnimateDiceRoll(a, b);
        rollState = RollState.Resolving;

        if (localDemo)
        {
            ResolveLocalRoll(a + b);
            ApplyDiceColor();
            yield break;
        }

        var json = $"{{\"shooterId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\",\"die1\":{a},\"die2\":{b}}}";
        yield return Post("/api/street-dice/" + gameId + "/roll", json, body =>
        {
            var response = JsonUtility.FromJson<ActionResponse>(body);
            UpdateState(response.state);
        });
    }

    private IEnumerator RollCeeLo(int a, int b, int c)
    {
        dieC.SetActive(true);
        phase = "CeeLo";
        rollState = RollState.Rolling;
        die1 = a;
        die2 = b;
        die3 = c;
        yield return AnimateDiceRoll(a, b, c);
        rollState = RollState.Resolving;

        if (localDemo)
        {
            ResolveLocalCeeLo(a, b, c);
            ApplyDiceColor();
            yield break;
        }

        var json = $"{{\"die1\":{a},\"die2\":{b},\"die3\":{c}}}";
        yield return PostOpen("/api/cee-lo/evaluate", json, body =>
        {
            var response = JsonUtility.FromJson<CeeLoResponse>(body);
            result = response.result.message;
            tutorialDetail = "Cee-lo server evaluator returned " + response.result.outcome + ".";
            rollState = RollState.ShooterDecision;
        });
    }

    private IEnumerator RunSame()
    {
        if (localDemo)
        {
            if (phase != "ShooterDecision")
            {
                result = "Run Same is available after a resolved shot.";
                yield break;
            }

            phase = "ComeOut";
            rollState = RollState.FadeWindow;
            point = "-";
            activePointGroup = "-";
            fadeCount = 0;
            shooterMomentum = 0;
            lastResolvedShotWasWin = false;
            lastShotWasDoubleUp = false;
            result = "Run Same. Next shot stays " + shotAmount + ".";
            ResetDiceToShooter();
            yield break;
        }

        var json = $"{{\"shooterId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/decision/run-same", json);
    }

    private IEnumerator DoubleUp()
    {
        if (localDemo)
        {
            if (phase != "ShooterDecision" || !lastResolvedShotWasWin)
            {
                result = "Double Up is only available after a shooter win.";
                yield break;
            }

            shotAmount *= 2;
            phase = "ComeOut";
            rollState = RollState.FadeWindow;
            point = "-";
            activePointGroup = "-";
            fadeCount = 0;
            shooterMomentum = 0;
            lastShotWasDoubleUp = true;
            lastResolvedShotWasWin = false;
            result = "Double Up. Next shot is " + shotAmount + ".";
            ResetDiceToShooter();
            yield break;
        }

        var json = $"{{\"shooterId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/decision/double-up", json);
    }

    private IEnumerator VoiceGate()
    {
        PulseMic(catcherId, 1.25f);
        if (localDemo)
        {
            result = "Demo voice markers only. Real Vivox stays server-gated.";
            yield break;
        }

        var json = $"{{\"playerId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/voice/access-token", json);
    }

    private void ResolveLocalRoll(int total)
    {
        if (phase == "ComeOut")
        {
            activePointGroup = "-";
            if (total is 7 or 11)
            {
                tutorialDetail = die1 + " + " + die2 + " = " + total + ". Come-out 7/11 wins immediately.";
                ShooterWin("Come-out win.");
                return;
            }

            if (total is 2 or 3 or 12)
            {
                tutorialDetail = die1 + " + " + die2 + " = " + total + ". Come-out 2/3/12 loses, but shooter keeps dice.";
                ShooterLoss("Come-out loss. Shooter pays but keeps dice.", true);
                return;
            }

            point = total.ToString();
            activePointGroup = PointGroupLabel(total);
            phase = "Point";
            rollState = RollState.FadeWindow;
            result = "Point established: " + point + ".";
            tutorialDetail = "Point " + point + " is set. Active side-bet group is " + activePointGroup + ". Only 7 loses during point phase.";
            PulseMic(catcherId, 1f);
            return;
        }

        var currentPoint = int.Parse(point);
        if (total == currentPoint)
        {
            var resolved = ResolveDemoGroupedBets(hitGroup: true);
            tutorialDetail = "Shooter hit point " + point + ". The main shot wins and " + activePointGroup + " group hit bets resolve.";
            ShooterWin("Point hit.");
            AppendResolvedSideBetMessage(resolved);
            return;
        }

        if (total == 7)
        {
            var resolved = ResolveDemoGroupedBets(hitGroup: false);
            tutorialDetail = "Seven out during point phase. Shooter loses and grouped miss bets win.";
            ShooterLoss("Seven out. Dice pass to Catcher.", false);
            AppendResolvedSideBetMessage(resolved);
            return;
        }

        if (IsInPointGroup(total, currentPoint))
        {
            var resolved = ResolveDemoGroupedBets(hitGroup: true);
            result = "Rolled grouped " + total + ". Group side bets resolve; shooter still needs " + point + ".";
            AppendResolvedSideBetMessage(resolved);
            tutorialDetail = "Rolled " + total + " inside " + activePointGroup + ". Side bets resolve, but the point remains " + point + ".";
            PulseMic(random.NextDouble() > 0.5 ? "p3" : "p4", 0.9f);
            return;
        }

        result = "Rolled " + total + ". Shooter keeps shooting for " + point + ".";
        tutorialDetail = "Rolled " + total + ". No point, no seven, no active group hit. Shooter rolls again.";
        PulseMic(random.NextDouble() > 0.5 ? "p3" : "p4", 0.9f);
    }

    private void ResolveLocalCeeLo(int a, int b, int c)
    {
        var evaluated = EvaluateLocalCeeLo(a, b, c);
        activePointGroup = "-";

        if (!bankerCeeLoReady)
        {
            if (evaluated.Outcome == "Reroll")
            {
                result = "Banker no-count. Banker rolls again.";
                tutorialDetail = evaluated.Message;
                rollState = RollState.FadeWindow;
                return;
            }

            bankerCeeLo = "Banker: " + evaluated.Message;
            bankerCeeLoRank = evaluated.Rank;

            if (evaluated.Outcome == "AutomaticWin")
            {
                result = "Banker " + evaluated.Message + " Banker wins this Cee-lo round.";
                tutorialDetail = "Banker automatic win resolves the round. Next roll starts a new banker turn.";
                rollState = RollState.ShooterDecision;
                PlayAudio(lossClip);
                PulseMic(catcherId, 1.3f);
                return;
            }

            if (evaluated.Outcome == "AutomaticLoss")
            {
                streak += 1;
                result = "Banker " + evaluated.Message + " Players win this Cee-lo round.";
                tutorialDetail = "Banker automatic loss resolves the round. Next roll starts a new banker turn.";
                rollState = RollState.ShooterDecision;
                PlayAudio(winClip);
                PulseMic("p3", 1.3f);
                return;
            }

            bankerCeeLoReady = true;
            result = "Banker set " + evaluated.Message + " Player rolls next.";
            tutorialDetail = "Banker has a live point. Next Cee-lo roll compares against banker rank " + bankerCeeLoRank + ".";
            rollState = RollState.FadeWindow;
            PulseMic("p3", 1.1f);
            return;
        }

        if (evaluated.Outcome == "Reroll")
        {
            result = "Player no-count. Player rolls again.";
            tutorialDetail = evaluated.Message;
            rollState = RollState.FadeWindow;
            return;
        }

        var comparison = evaluated.Rank.CompareTo(bankerCeeLoRank);
        if (evaluated.Outcome == "AutomaticWin" || comparison > 0)
        {
            streak += 1;
            result = "Player " + evaluated.Message + " Player beats banker.";
            PlayAudio(winClip);
        }
        else if (evaluated.Outcome == "AutomaticLoss" || comparison < 0)
        {
            streak = 0;
            result = "Player " + evaluated.Message + " Banker wins.";
            PlayAudio(lossClip);
        }
        else
        {
            result = "Player " + evaluated.Message + " Push with banker.";
            PlayAudio(lockClip);
        }

        tutorialDetail = bankerCeeLo + " | Player: " + evaluated.Message + " Next Cee-lo roll starts a new banker turn.";
        bankerCeeLoReady = false;
        bankerCeeLoRank = 0;
        bankerCeeLo = "Banker: not rolled";
        rollState = RollState.ShooterDecision;
        PulseMic(random.NextDouble() > 0.5 ? "p3" : "p4", 0.9f);
    }

    private static CeeLoLocalResult EvaluateLocalCeeLo(int a, int b, int c)
    {
        var values = new[] { a, b, c };
        Array.Sort(values);

        if (values[0] == 4 && values[1] == 5 && values[2] == 6)
        {
            return new CeeLoLocalResult("AutomaticWin", null, 10000, "4-5-6. Automatic win.");
        }

        if (values[0] == 1 && values[1] == 2 && values[2] == 3)
        {
            return new CeeLoLocalResult("AutomaticLoss", null, -10000, "1-2-3. Automatic loss.");
        }

        if (values[0] == values[1] && values[1] == values[2])
        {
            return new CeeLoLocalResult("AutomaticWin", null, 9000 + values[0], "trips " + values[0] + ". Automatic win.");
        }

        var ceeLoPoint = PairAndPoint(values);
        if (ceeLoPoint == null)
        {
            return new CeeLoLocalResult("Reroll", null, 0, "No count. Roll again.");
        }

        if (ceeLoPoint.Value == 6)
        {
            return new CeeLoLocalResult("AutomaticWin", 6, 8006, "pair plus 6. Automatic win.");
        }

        if (ceeLoPoint.Value == 1)
        {
            return new CeeLoLocalResult("AutomaticLoss", 1, -8001, "pair plus 1. Automatic loss.");
        }

        return new CeeLoLocalResult("Point", ceeLoPoint.Value, ceeLoPoint.Value, "point " + ceeLoPoint.Value + ".");
    }

    private static int? PairAndPoint(int[] values)
    {
        if (values[0] == values[1]) return values[2];
        if (values[1] == values[2]) return values[0];
        return null;
    }

    private void ShooterWin(string message)
    {
        var gain = point == "-" ? 1 : 2;
        gain += shooterMomentum;
        if (lastShotWasDoubleUp) gain += 1;
        streak += gain;
        phase = "ShooterDecision";
        rollState = RollState.ShooterDecision;
        point = "-";
        activePointGroup = "-";
        fadeCount = 0;
        shooterMomentum = 0;
        lastResolvedShotWasWin = true;
        result = message + " Shooter wins " + shotAmount + ".";
        if (streak >= HotDiceThreshold) result += " Hot dice active.";
        PlayAudio(winClip);
        PulseMic(catcherId, 1.2f);
    }

    private void ShooterLoss(string message, bool keepDice)
    {
        streak = 0;
        point = "-";
        activePointGroup = "-";
        fadeCount = 0;
        shooterMomentum = 0;
        lastResolvedShotWasWin = false;
        lastShotWasDoubleUp = false;

        if (keepDice)
        {
            phase = "ShooterDecision";
            rollState = RollState.ShooterDecision;
            result = message;
        }
        else
        {
            phase = "ComeOut";
            rollState = RollState.FadeWindow;
            (shooterId, catcherId) = (catcherId, shooterId);
            result = message;
        }

        PulseMic(catcherId, 1.5f);
        PlayAudio(lossClip);
    }

    private IEnumerator AnimateDiceRoll(int finalA, int finalB, int? finalC = null)
    {
        rolling = true;
        PlayAudio(rollClip);
        PulseMic(shooterId, 1.35f);
        var path = BuildThrowPath();
        StartThrowHands(path);
        var endA = path.EndA + new Vector3(UnityEngine.Random.Range(-0.12f, 0.12f), 0f, UnityEngine.Random.Range(-0.18f, 0.18f));
        var endB = path.EndB + new Vector3(UnityEngine.Random.Range(-0.12f, 0.12f), 0f, UnityEngine.Random.Range(-0.18f, 0.18f));
        var endC = path.EndC + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 0f, UnityEngine.Random.Range(-0.16f, 0.16f));

        const float duration = 1.32f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            var arc = Mathf.Sin(t * Mathf.PI) * 0.12f;
            var bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 6.4f)) * Mathf.Lerp(0.22f, 0.012f, t);
            var skidA = new Vector3(Mathf.Sin(t * 38f) * 0.012f, 0f, Mathf.Cos(t * 31f) * 0.01f) * (1f - t);
            var skidB = new Vector3(Mathf.Cos(t * 35f) * 0.011f, 0f, Mathf.Sin(t * 29f) * 0.013f) * (1f - t);
            dieA.transform.position = Bezier(path.StartA, path.MidA, endA, t) + Vector3.up * (arc + bounce) + skidA;
            dieB.transform.position = Bezier(path.StartB, path.MidB, endB, t) + Vector3.up * (arc * 0.9f + bounce * 0.86f) + skidB;
            if (finalC != null)
            {
                var skidC = new Vector3(Mathf.Sin(t * 33f) * 0.01f, 0f, Mathf.Cos(t * 27f) * 0.011f) * (1f - t);
                dieC.transform.position = Bezier(path.StartC, path.MidC, endC, t) + Vector3.up * (arc * 0.82f + bounce * 0.76f) + skidC;
            }
            yield return null;
        }

        dieA.transform.position = endA;
        dieB.transform.position = endB;
        if (finalC != null) dieC.transform.position = endC;
        rolling = false;
        rollState = RollState.Locked;
        LockDieToValue(dieA, finalA);
        LockDieToValue(dieB, finalB);
        if (finalC != null) LockDieToValue(dieC, finalC.Value);
        rollLockFlashUntil = Time.time + 0.7f;
        PlayAudio(lockClip);
        yield return new WaitForSeconds(0.48f);
    }

    private ThrowPath BuildThrowPath()
    {
        return shooterId switch
        {
            "p3" => new ThrowPath(
                new Vector3(-2.05f, DiceRestY, -0.36f),
                new Vector3(-2.05f, DiceRestY, 0.02f),
                new Vector3(-2.1f, DiceRestY, 0.34f),
                new Vector3(-1.18f, 0.34f, -0.18f),
                new Vector3(-1.08f, 0.32f, 0.18f),
                new Vector3(-1.0f, 0.3f, 0.42f),
                new Vector3(-0.24f, DiceRestY, 0.42f),
                new Vector3(0.38f, DiceRestY, 0.25f),
                new Vector3(0.08f, DiceRestY, 0.72f)),
            "p4" => new ThrowPath(
                new Vector3(2.05f, DiceRestY, -0.36f),
                new Vector3(2.05f, DiceRestY, 0.02f),
                new Vector3(2.1f, DiceRestY, 0.34f),
                new Vector3(1.18f, 0.34f, -0.18f),
                new Vector3(1.08f, 0.32f, 0.18f),
                new Vector3(1.0f, 0.3f, 0.42f),
                new Vector3(-0.38f, DiceRestY, 0.26f),
                new Vector3(0.24f, DiceRestY, 0.44f),
                new Vector3(-0.08f, DiceRestY, 0.72f)),
            "p2" => new ThrowPath(
                new Vector3(-0.32f, DiceRestY, 1.78f),
                new Vector3(0.32f, DiceRestY, 1.78f),
                new Vector3(0f, DiceRestY, 1.52f),
                new Vector3(-0.34f, 0.34f, 1.06f),
                new Vector3(0.34f, 0.32f, 1.06f),
                new Vector3(0f, 0.3f, 0.92f),
                new Vector3(-0.34f, DiceRestY, 0.42f),
                new Vector3(0.34f, DiceRestY, 0.28f),
                new Vector3(0f, DiceRestY, 0.68f)),
            "bot-5" => new ThrowPath(
                new Vector3(1.28f, DiceRestY, 1.24f),
                new Vector3(1.56f, DiceRestY, 1.02f),
                new Vector3(1.02f, DiceRestY, 1.44f),
                new Vector3(0.82f, 0.34f, 0.88f),
                new Vector3(1.0f, 0.32f, 0.62f),
                new Vector3(0.74f, 0.3f, 0.98f),
                new Vector3(-0.28f, DiceRestY, 0.4f),
                new Vector3(0.34f, DiceRestY, 0.22f),
                new Vector3(0.04f, DiceRestY, 0.72f)),
            _ => new ThrowPath(
                new Vector3(-0.28f, DiceRestY, -2.25f),
                new Vector3(0.28f, DiceRestY, -2.25f),
                new Vector3(0f, DiceRestY, -2.58f),
                new Vector3(-0.62f, 0.34f, -0.35f),
                new Vector3(0.66f, 0.32f, -0.1f),
                new Vector3(0.05f, 0.3f, -0.48f),
                new Vector3(-0.36f, DiceRestY, 0.48f),
                new Vector3(0.38f, DiceRestY, 0.3f),
                new Vector3(0.02f, DiceRestY, 0.78f))
        };
    }

    private void StartThrowHands(ThrowPath path)
    {
        activeThrowPath = path;
        handThrowStartedAt = Time.time;
        handThrowVisibleUntil = Time.time + 0.82f;
        UpdateThrowHandPose(0f, forceVisible: shooterId == "p1");
        PlayHandThrowAnimation();
    }

    private void UpdateThrowHands()
    {
        if (handRig == null) return;
        if (shooterId != "p1" || Time.time > handThrowVisibleUntil)
        {
            StopHandThrowAnimation();
            handRig.SetActive(false);
            return;
        }

        var t = Mathf.Clamp01((Time.time - handThrowStartedAt) / 0.72f);
        UpdateThrowHandPose(t, forceVisible: true);
    }

    private void UpdateThrowHandPose(float t, bool forceVisible)
    {
        if (handRig == null) return;
        handRig.SetActive(forceVisible);
        if (!forceVisible) return;

        var handAnchor = new Vector3(0f, 0.04f, -3.84f);
        var windup = handAnchor + new Vector3(0f, -0.16f, -0.24f);
        var release = handAnchor + new Vector3(0f, -0.08f, 0.08f);
        var followThrough = handAnchor + new Vector3(0.04f, -0.1f, 0.22f);
        var a = t < 0.72f
            ? Vector3.Lerp(windup, release, Mathf.SmoothStep(0f, 1f, t / 0.72f))
            : Vector3.Lerp(release, followThrough, Mathf.SmoothStep(0f, 1f, (t - 0.72f) / 0.28f));
        handRig.transform.position = a;
        handRig.transform.rotation = Quaternion.Euler(Mathf.Lerp(4f, -12f, t), 0f, Mathf.Sin(t * Mathf.PI) * 2f);
        handRig.transform.localScale = Vector3.one;

        if (leftThrowHand != null)
        {
            leftThrowHand.transform.localPosition = Vector3.Lerp(new Vector3(-0.32f, -0.08f, 0f), new Vector3(-0.23f, -0.04f, 0.11f), t);
            leftThrowHand.transform.localRotation = Quaternion.Euler(Mathf.Lerp(-12f, -28f, t), Mathf.Lerp(150f, 136f, t), Mathf.Lerp(166f, 174f, t));
        }

        if (rightThrowHand != null)
        {
            rightThrowHand.transform.localPosition = Vector3.Lerp(new Vector3(0.32f, -0.08f, 0f), new Vector3(0.23f, -0.04f, 0.11f), t);
            rightThrowHand.transform.localRotation = Quaternion.Euler(Mathf.Lerp(-12f, -28f, t), Mathf.Lerp(-150f, -136f, t), Mathf.Lerp(-166f, -174f, t));
        }
    }

    private void PlayHandThrowAnimation()
    {
        if (handRig == null || shooterId != "p1") return;
        var animators = handRig.GetComponentsInChildren<Animator>(true);
        for (var i = 0; i < animators.Length; i++)
        {
            var animator = animators[i];
            if (TrySetAnimatorBool(animator, "bShoot", true))
            {
                animator.speed = 1.15f;
                continue;
            }

            var shootHash = Animator.StringToHash("PoseShoot");
            if (animator.HasState(0, shootHash))
            {
                animator.Play(shootHash, 0, 0f);
            }
        }
    }

    private static bool TrySetAnimatorBool(Animator animator, string parameterName, bool value)
    {
        if (animator == null) return false;
        var parameters = animator.parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.type != AnimatorControllerParameterType.Bool || parameter.name != parameterName) continue;
            animator.SetBool(parameterName, value);
            return true;
        }

        return false;
    }

    private void StopHandThrowAnimation()
    {
        if (handRig == null) return;
        var animators = handRig.GetComponentsInChildren<Animator>(true);
        for (var i = 0; i < animators.Length; i++)
        {
            TrySetAnimatorBool(animators[i], "bShoot", false);
        }
    }

    private static Vector3 Bezier(Vector3 start, Vector3 mid, Vector3 end, float t)
    {
        var a = Vector3.Lerp(start, mid, t);
        var b = Vector3.Lerp(mid, end, t);
        return Vector3.Lerp(a, b, t);
    }

    private void LockDieToValue(GameObject die, int value)
    {
        die.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(-9f, 9f), Vector3.up) * TopValueRotation(value);
    }

    private static Quaternion TopValueRotation(int value)
    {
        switch (value)
        {
            case 1:
                return Quaternion.identity;
            case 2:
                return Quaternion.Euler(-90f, 0f, 0f);
            case 3:
                return Quaternion.Euler(0f, 0f, 90f);
            case 4:
                return Quaternion.Euler(0f, 0f, -90f);
            case 5:
                return Quaternion.Euler(90f, 0f, 0f);
            case 6:
                return Quaternion.Euler(180f, 0f, 0f);
            default:
                return Quaternion.identity;
        }
    }

    private void ResetDiceToShooter()
    {
        var path = BuildThrowPath();
        dieA.transform.position = path.StartA;
        dieB.transform.position = path.StartB;
        dieC.transform.position = path.StartC;
        dieA.transform.rotation = Quaternion.Euler(0f, 12f, 0f);
        dieB.transform.rotation = Quaternion.Euler(0f, -10f, 0f);
        dieC.transform.rotation = Quaternion.Euler(0f, 4f, 0f);
        dieC.SetActive(gameMode == GameMode.CeeLo);
        dieCShadow.SetActive(gameMode == GameMode.CeeLo);
    }

    private void ApplyDiceColor()
    {
        var color = streak >= HotDiceThreshold ? new Color(1f, 0.23f, 0.02f) : selectedDiceColor;
        ApplyDieColor(dieA, color);
        ApplyDieColor(dieB, color);
        ApplyDieColor(dieC, color);
    }

    private static void ApplyDieColor(GameObject die, Color color)
    {
        foreach (var renderer in die.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.gameObject.name.Contains("Pip Well", StringComparison.OrdinalIgnoreCase))
            {
                var well = ShouldUseDarkPips(color)
                    ? new Color(0.045f, 0.047f, 0.043f)
                    : new Color(0.64f, 0.64f, 0.6f);
                renderer.material = CreatePipMaterial(well);
                continue;
            }

            if (renderer.gameObject.name.Contains("Pip Inset", StringComparison.OrdinalIgnoreCase))
            {
                renderer.material = CreatePipMaterial(ShouldUseDarkPips(color) ? Color.black : Color.white);
                continue;
            }

            if (renderer.gameObject.name.Contains("Wear", StringComparison.OrdinalIgnoreCase))
            {
                var wear = ShouldUseDarkPips(color)
                    ? new Color(0.68f, 0.68f, 0.63f, 0.55f)
                    : new Color(0.16f, 0.16f, 0.15f, 0.48f);
                renderer.material = CreatePipMaterial(wear);
                continue;
            }

            renderer.material = CreateDiceMaterial(color);
        }
    }

    private static bool ShouldUseDarkPips(Color dieColor)
    {
        var luminance = dieColor.r * 0.2126f + dieColor.g * 0.7152f + dieColor.b * 0.0722f;
        return luminance > 0.62f;
    }

    private static float Hash01(int x, int y, int seed)
    {
        unchecked
        {
            var h = x * 374761393 + y * 668265263 + seed * 2147483647;
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0x7fffffff) / 2147483647f;
        }
    }

    private static float HashSigned(int x, int y, int seed)
    {
        return Hash01(x, y, seed) * 2f - 1f;
    }

    private void PlaceSideBetFromUi(string playerId, bool missGroup)
    {
        if (localDemo)
        {
            PlaceDemoSideBet(playerId, missGroup);
            return;
        }

        StartCoroutine(PlaceServerSideBet(playerId, missGroup));
    }

    private void PlaceDemoSideBet(string playerId, bool missGroup)
    {
        if (gameMode != GameMode.Craps || phase != "Point" || point == "-")
        {
            result = "Grouped side bets need an active point.";
            return;
        }

        var bet = new DemoSideBet(playerId, missGroup, activePointGroup, 10);
        demoSideBets.Add(bet);
        result = (missGroup ? "Miss" : "Hit") + " group side bet placed on " + activePointGroup + ".";
        tutorialDetail = "Side bet sits beside " + playerId + ". It resolves when " + activePointGroup + " hits or a 7 comes first.";
        PulseMic(playerId, 1.1f);
    }

    private IEnumerator PlaceServerSideBet(string playerId, bool missGroup)
    {
        if (gameMode != GameMode.Craps || phase != "Point" || point == "-")
        {
            result = "Grouped side bets need an active point.";
            yield break;
        }

        if (!playerTokens.TryGetValue(playerId, out var token))
        {
            result = "No server token for " + playerId + ". Recreate the server table.";
            yield break;
        }

        var type = missGroup ? "MissPointGroup" : "HitPointGroup";
        var json = "{\"playerId\":\"" + playerId + "\",\"playerSessionToken\":\"" + token + "\",\"type\":\"" + type + "\",\"amount\":10,\"targetPointNumber\":" + point + "}";
        yield return Post("/api/street-dice/" + gameId + "/side-bet", json, body =>
        {
            var response = JsonUtility.FromJson<ActionResponse>(body);
            UpdateState(response.state);
            result = (missGroup ? "Miss" : "Hit") + " group side bet sent for " + playerId + " on " + activePointGroup + ".";
            tutorialDetail = "Server-backed side bet created beside " + playerId + ". It resolves from the authoritative roll result.";
            PulseMic(playerId, 1.1f);
        });
    }

    private int ResolveDemoGroupedBets(bool hitGroup)
    {
        var resolved = 0;
        foreach (var sideBet in demoSideBets)
        {
            if (sideBet.Resolved || sideBet.PointGroup != activePointGroup) continue;
            sideBet.Resolved = true;
            sideBet.Won = sideBet.MissGroup ? !hitGroup : hitGroup;
            resolved++;
            PulseMic(sideBet.PlayerId, 1.2f);
        }

        return resolved;
    }

    private void AppendResolvedSideBetMessage(int resolved)
    {
        if (resolved > 0) result += " " + resolved + " grouped side bet resolved.";
    }

    private int OpenSideBetCount(string playerId = "")
    {
        var count = 0;
        foreach (var sideBet in demoSideBets)
        {
            if (sideBet.Resolved) continue;
            if (string.IsNullOrWhiteSpace(playerId) || sideBet.PlayerId == playerId) count++;
        }

        foreach (var sideBet in serverSideBets)
        {
            if (!string.Equals(sideBet.status, "Open", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.IsNullOrWhiteSpace(playerId) || sideBet.playerId == playerId) count++;
        }

        return count;
    }

    private string LatestSideBetLine(string playerId)
    {
        for (var i = demoSideBets.Count - 1; i >= 0; i--)
        {
            var sideBet = demoSideBets[i];
            if (sideBet.PlayerId != playerId) continue;
            if (!sideBet.Resolved) return sideBet.MissGroup ? "miss grp open" : "hit grp open";
            return sideBet.Won ? "bet won" : "bet lost";
        }

        for (var i = serverSideBets.Length - 1; i >= 0; i--)
        {
            var sideBet = serverSideBets[i];
            if (sideBet.playerId != playerId) continue;
            if (string.Equals(sideBet.status, "Open", StringComparison.OrdinalIgnoreCase)) return sideBet.type + " open";
            return string.Equals(sideBet.status, "Won", StringComparison.OrdinalIgnoreCase) ? "bet won" : "bet lost";
        }

        return "";
    }

    private static bool IsInPointGroup(int rollTotal, int pointTotal)
    {
        return PointGroupLabel(rollTotal) == PointGroupLabel(pointTotal);
    }

    private static string PointGroupLabel(int total)
    {
        switch (total)
        {
            case 4:
            case 10:
                return "4/10";
            case 6:
            case 8:
                return "6/8";
            case 5:
            case 9:
                return "5/9";
            default:
                return "-";
        }
    }

    private void PulseMic(string playerId, float seconds)
    {
        for (var i = 0; i < mics.Length; i++)
        {
            if (mics[i].PlayerId == playerId)
            {
                mics[i].Talk(seconds);
            }
        }
    }

    private void SetPrototypeSeatMarkersVisible(bool visible)
    {
        showPrototypeSeatMarkers = visible;
        for (var i = 0; i < mics.Length; i++)
        {
            mics[i]?.SetVisible(visible);
        }
    }

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    private IEnumerator Post(string path, string json, Action<string> onSuccess = null)
    {
        if (string.IsNullOrWhiteSpace(gameId) && !path.EndsWith("/create", StringComparison.Ordinal))
        {
            result = "Create a server table first, or use Demo Table for standalone play.";
            yield break;
        }

        using var request = new UnityWebRequest(baseUrl + path, "POST");
        var body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            result = request.responseCode == 501 ? "Voice gate reached; Vivox config is missing." : request.error;
            yield break;
        }

        var text = request.downloadHandler.text;
        onSuccess?.Invoke(text);
        if (onSuccess == null)
        {
            var response = JsonUtility.FromJson<ActionResponse>(text);
            UpdateState(response.state);
        }
    }

    private IEnumerator PostOpen(string path, string json, Action<string> onSuccess)
    {
        using var request = new UnityWebRequest(baseUrl + path, "POST");
        var body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            result = request.error;
            yield break;
        }

        onSuccess(request.downloadHandler.text);
    }

    private void UpdateState(StateDto state)
    {
        if (state == null) return;
        phase = state.phase;
        shooterId = state.shooterId;
        catcherId = state.catcherId;
        point = state.point == 0 ? "-" : state.point.ToString();
        activePointGroup = point == "-" ? "-" : PointGroupLabel(state.point);
        streak = state.streak;
        shotAmount = state.shotAmount == 0 ? shotAmount : state.shotAmount;
        serverSideBets = state.sideBets ?? Array.Empty<SideBetDto>();
        rollState = phase == "ShooterDecision"
            ? RollState.ShooterDecision
            : phase == "ComeOut" || phase == "Point"
                ? RollState.FadeWindow
                : rollState;
        result = state.lastResolution.message;
        ApplyDiceColor();
        PulseMic(catcherId, 1f);
    }

    private sealed class SeatMic
    {
        private readonly GameObject head;
        private readonly GameObject pulse;
        private readonly Color accent;
        private float talkUntil;

        public SeatMic(string label, string playerId, GameObject root, GameObject head, GameObject pulse, Color accent, bool isHuman)
        {
            Label = label;
            PlayerId = playerId;
            Root = root;
            this.head = head;
            this.pulse = pulse;
            this.accent = accent;
            IsHuman = isHuman;
        }

        public string Label { get; }
        public string PlayerId { get; }
        public GameObject Root { get; }
        public bool IsHuman { get; }
        public string ProfileText => PlayerId == "p1" ? "YOU" : IsHuman ? "PFP" : "AI";

        public void Talk(float seconds)
        {
            talkUntil = Time.time + seconds;
        }

        public void SetVisible(bool visible)
        {
            Root.SetActive(visible);
        }

        public void Update(float time)
        {
            var talking = time < talkUntil;
            var strength = talking ? 0.55f + Mathf.Abs(Mathf.Sin(time * 12f)) * 0.45f : 0.18f;
            head.GetComponent<Renderer>().material.color = Color.Lerp(new Color(0.08f, 0.09f, 0.09f), accent, strength);
            pulse.transform.localScale = new Vector3(0.48f + strength * 0.42f, 0.05f, 0.48f + strength * 0.42f);
            pulse.SetActive(talking);
        }
    }

    [Serializable] private sealed class CreateResponse { public string gameId = ""; public StateDto state = null!; }
    [Serializable] private sealed class JoinResponse { public string playerId = ""; public string playerSessionToken = ""; public StateDto state = null!; }
    [Serializable] private sealed class ActionResponse { public StateDto state = null!; }
    [Serializable] private sealed class CeeLoResponse { public CeeLoResultDto result = new CeeLoResultDto(); }
    [Serializable] private sealed class CeeLoResultDto
    {
        public string outcome = "";
        public int point;
        public int rank;
        public string message = "";
    }
    [Serializable] private sealed class StateDto
    {
        public string phase = "";
        public string shooterId = "";
        public string catcherId = "";
        public int point;
        public int streak;
        public int shotAmount;
        public SideBetDto[] sideBets = Array.Empty<SideBetDto>();
        public ResolutionDto lastResolution = new ResolutionDto();
    }
    [Serializable] private sealed class ResolutionDto { public string message = ""; }
    [Serializable] private sealed class SideBetDto
    {
        public string playerId = "";
        public string type = "";
        public string status = "";
        public int amount;
        public string pointGroup = "";
    }

    private readonly struct ThrowPath
    {
        public ThrowPath(
            Vector3 startA,
            Vector3 startB,
            Vector3 startC,
            Vector3 midA,
            Vector3 midB,
            Vector3 midC,
            Vector3 endA,
            Vector3 endB,
            Vector3 endC)
        {
            StartA = startA;
            StartB = startB;
            StartC = startC;
            MidA = midA;
            MidB = midB;
            MidC = midC;
            EndA = endA;
            EndB = endB;
            EndC = endC;
        }

        public Vector3 StartA { get; }
        public Vector3 StartB { get; }
        public Vector3 StartC { get; }
        public Vector3 MidA { get; }
        public Vector3 MidB { get; }
        public Vector3 MidC { get; }
        public Vector3 EndA { get; }
        public Vector3 EndB { get; }
        public Vector3 EndC { get; }
    }

    private readonly struct CeeLoLocalResult
    {
        public CeeLoLocalResult(string outcome, int? point, int rank, string message)
        {
            Outcome = outcome;
            Point = point;
            Rank = rank;
            Message = message;
        }

        public string Outcome { get; }
        public int? Point { get; }
        public int Rank { get; }
        public string Message { get; }
    }

    private sealed class DemoSideBet
    {
        public DemoSideBet(string playerId, bool missGroup, string pointGroup, int amount)
        {
            PlayerId = playerId;
            MissGroup = missGroup;
            PointGroup = pointGroup;
            Amount = amount;
        }

        public string PlayerId { get; }
        public bool MissGroup { get; }
        public string PointGroup { get; }
        public int Amount { get; }
        public bool Resolved { get; set; }
        public bool Won { get; set; }
    }
}
