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

public sealed class StreetDiceGreyboxController : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5108";

    private const int HotDiceThreshold = 5;

    private GameObject dieA = null!;
    private GameObject dieB = null!;
    private GameObject dieC = null!;
    private GameObject rollLane = null!;
    private readonly SeatMic[] mics = new SeatMic[4];
    private readonly List<DemoSideBet> demoSideBets = new();
    private readonly System.Random random = new();

    private GameMode gameMode = GameMode.Craps;
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
    private bool localDemo = true;
    private bool lastResolvedShotWasWin;
    private bool lastShotWasDoubleUp;
    private bool rolling;
    private bool tutorialMode;
    private float rollLockFlashUntil;
    private Color selectedDiceColor = new Color(0.08f, 0.55f, 0.23f);

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
        Camera.main?.gameObject.SetActive(false);
        CreateCamera();
        CreateLighting();
        CreateStreetGroundScene();
        CreateMicSeats();

        dieA = CreateDie("Die A", new Vector3(-0.28f, 0.22f, -2.25f));
        dieB = CreateDie("Die B", new Vector3(0.28f, 0.22f, -2.25f));
        dieC = CreateDie("Die C", new Vector3(0f, 0.22f, -2.58f));
        dieC.SetActive(false);
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

        for (var i = 0; i < mics.Length; i++)
        {
            mics[i].Update(Time.time);
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
        camera.transform.position = new Vector3(0f, 1.36f, -4.35f);
        camera.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.038f, 0.036f);
        camera.fieldOfView = 63f;
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
    }

    private void CreateStreetGroundScene()
    {
        rollLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rollLane.name = "Bodega Ground Roll Lane";
        rollLane.transform.position = new Vector3(0f, -0.055f, 0.08f);
        rollLane.transform.localScale = new Vector3(4.3f, 0.08f, 7.15f);
        rollLane.GetComponent<Renderer>().material.color = new Color(0.115f, 0.125f, 0.12f);

        for (var i = 0; i < 11; i++)
        {
            var seam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seam.name = "Concrete Slab Joint";
            seam.transform.position = new Vector3(0f, 0.003f, -3.15f + i * 0.62f);
            seam.transform.localScale = new Vector3(4.25f, 0.012f, 0.018f);
            seam.GetComponent<Renderer>().material.color = new Color(0.045f, 0.048f, 0.046f);
        }

        for (var i = 0; i < 7; i++)
        {
            var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            patch.name = "Street Ground Patch";
            patch.transform.position = new Vector3(UnityEngine.Random.Range(-1.65f, 1.65f), 0.004f, -2.55f + i * 0.82f);
            patch.transform.localScale = new Vector3(UnityEngine.Random.Range(0.35f, 0.75f), 0.014f, UnityEngine.Random.Range(0.06f, 0.13f));
            patch.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(-12f, 12f), 0f);
            patch.GetComponent<Renderer>().material.color = new Color(0.075f, 0.08f, 0.077f);
        }

        var backDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backDoor.name = "Closed Bodega Service Door";
        backDoor.transform.position = new Vector3(0f, 1.1f, 3.56f);
        backDoor.transform.localScale = new Vector3(4.65f, 2.35f, 0.12f);
        backDoor.GetComponent<Renderer>().material.color = new Color(0.23f, 0.265f, 0.255f);

        for (var i = 0; i < 8; i++)
        {
            var slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slat.name = "Service Door Slat";
            slat.transform.position = new Vector3(0f, 0.11f + i * 0.28f, 3.485f);
            slat.transform.localScale = new Vector3(4.72f, 0.032f, 0.05f);
            slat.GetComponent<Renderer>().material.color = new Color(0.12f, 0.14f, 0.14f);
        }

        var signBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signBand.name = "Bodega Sign Band";
        signBand.transform.position = new Vector3(0f, 2.72f, 3.51f);
        signBand.transform.localScale = new Vector3(5.1f, 0.38f, 0.08f);
        signBand.GetComponent<Renderer>().material.color = new Color(0.62f, 0.18f, 0.12f);

        var curb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        curb.name = "Street Curb Edge";
        curb.transform.position = new Vector3(0f, 0.035f, -3.48f);
        curb.transform.localScale = new Vector3(4.6f, 0.12f, 0.22f);
        curb.GetComponent<Renderer>().material.color = new Color(0.36f, 0.35f, 0.31f);

        var asphalt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        asphalt.name = "Street Asphalt Beyond Shooter";
        asphalt.transform.position = new Vector3(0f, -0.07f, -4.22f);
        asphalt.transform.localScale = new Vector3(4.8f, 0.07f, 1.1f);
        asphalt.GetComponent<Renderer>().material.color = new Color(0.055f, 0.06f, 0.058f);

        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "Left Tight Brick Wall";
        leftWall.transform.position = new Vector3(-2.32f, 0.75f, 0.25f);
        leftWall.transform.localScale = new Vector3(0.14f, 1.6f, 6.75f);
        leftWall.GetComponent<Renderer>().material.color = new Color(0.18f, 0.09f, 0.065f);

        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "Right Tight Brick Wall";
        rightWall.transform.position = new Vector3(2.32f, 0.75f, 0.25f);
        rightWall.transform.localScale = new Vector3(0.14f, 1.6f, 6.75f);
        rightWall.GetComponent<Renderer>().material.color = new Color(0.18f, 0.09f, 0.065f);

        for (var i = 0; i < 9; i++)
        {
            CreateWallCourse(-2.395f, -2.85f + i * 0.7f);
            CreateWallCourse(2.395f, -2.85f + i * 0.7f);
        }
    }

    private void CreateWallCourse(float x, float z)
    {
        for (var row = 0; row < 4; row++)
        {
            var brick = GameObject.CreatePrimitive(PrimitiveType.Cube);
            brick.name = "Wall Brick Suggestion";
            brick.transform.position = new Vector3(x, 0.2f + row * 0.27f, z + (row % 2) * 0.17f);
            brick.transform.localScale = new Vector3(0.025f, 0.025f, 0.33f);
            brick.GetComponent<Renderer>().material.color = new Color(0.09f, 0.045f, 0.037f);
        }
    }

    private void CreateMicSeats()
    {
        mics[0] = CreateMic("Catcher", "p2", new Vector3(0f, 0.28f, 2.82f), new Color(0.95f, 0.72f, 0.18f));
        mics[1] = CreateMic("Left 1", "p3", new Vector3(-1.72f, 0.28f, -0.55f), new Color(0.42f, 0.78f, 1f));
        mics[2] = CreateMic("Right 1", "p4", new Vector3(1.72f, 0.28f, -0.55f), new Color(0.42f, 0.78f, 1f));
        mics[3] = CreateMic("Right 2", "bot-5", new Vector3(1.68f, 0.28f, 1.35f), new Color(0.42f, 0.78f, 1f));
    }

    private SeatMic CreateMic(string label, string playerId, Vector3 position, Color accent)
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

        return new SeatMic(label, playerId, root, head, pulse, accent);
    }

    private GameObject CreateDie(string dieName, Vector3 position)
    {
        var die = GameObject.CreatePrimitive(PrimitiveType.Cube);
        die.name = dieName;
        die.transform.position = position;
        die.transform.localScale = Vector3.one * 0.34f;
        CreatePips(die);
        return die;
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
            var pip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pip.name = die.name + " Pip " + value;
            pip.transform.SetParent(die.transform, false);
            pip.transform.localPosition = normal * 0.515f + right * offsets[i].x + up * offsets[i].y;
            pip.transform.localScale = Vector3.one * 0.105f;
            pip.GetComponent<Renderer>().material.color = Color.white;
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
        var width = tutorialMode ? 330f : 210f;
        var height = tutorialMode ? 172f : 76f;
        var x = Screen.width - width - 18f;
        GUI.Box(new Rect(x, 18f, width, height), "");
        GUI.Label(new Rect(x + 12f, 28f, width - 24f, 22f), gameMode == GameMode.CeeLo ? "CEE-LO" : point == "-" ? "COME OUT" : "POINT " + point);
        GUI.Label(new Rect(x + 12f, 52f, width - 24f, 22f), Time.time < rollLockFlashUntil ? "ROLL LOCKED" : "SHOT " + shotAmount);

        if (tutorialMode)
        {
            var rollText = gameMode == GameMode.CeeLo
                ? die1 + " / " + die2 + " / " + die3
                : die1 + " + " + die2 + " = " + (die1 + die2);
            GUI.Label(new Rect(x + 12f, 76f, width - 24f, 22f), rollText);
            GUI.Label(new Rect(x + 12f, 98f, width - 24f, 22f), "Phase: " + phase);
            GUI.Label(new Rect(x + 12f, 120f, width - 24f, 22f), "Group: " + activePointGroup);
            GUI.Label(new Rect(x + 12f, 142f, width - 24f, 22f), "Side bets: " + OpenSideBetCount());
        }
    }

    private void DrawPlayerOverlays()
    {
        for (var i = 0; i < mics.Length; i++)
        {
            var seat = mics[i];
            if (Camera.main == null) continue;
            var screen = Camera.main.WorldToScreenPoint(seat.Root.transform.position + new Vector3(0f, 0.82f, 0f));
            if (screen.z <= 0f) continue;

            var rect = new Rect(screen.x - 72f, Screen.height - screen.y - 34f, 144f, 74f);
            GUI.Box(rect, "");
            GUI.Label(new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, 18f), seat.Label);

            if (gameMode == GameMode.Craps && seat.PlayerId == catcherId)
            {
                if (GUI.Button(new Rect(rect.x + 8f, rect.y + 28f, rect.width - 16f, 22f), "Fade/Catch"))
                {
                    StartCoroutine(Fade());
                }
            }
            else if (gameMode == GameMode.Craps && phase == "Point")
            {
                if (GUI.Button(new Rect(rect.x + 8f, rect.y + 25f, 62f, 22f), "Hit Grp"))
                {
                    PlaceDemoSideBet(seat.PlayerId, false);
                }

                if (GUI.Button(new Rect(rect.x + 74f, rect.y + 25f, 62f, 22f), "Miss Grp"))
                {
                    PlaceDemoSideBet(seat.PlayerId, true);
                }
            }
            else
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 28f, rect.width - 16f, 18f), gameMode == GameMode.CeeLo ? "Cee-lo seat" : "Side Bet");
            }

            var playerBets = OpenSideBetCount(seat.PlayerId);
            if (playerBets > 0)
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 52f, rect.width - 16f, 18f), playerBets + " open bet");
            }
        }
    }

    private void DrawBottomControls()
    {
        var y = Screen.height - 118f;
        var buttonWidth = Mathf.Min(112f, (Screen.width - 40f) / 9f);
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

        if (GUI.Button(new Rect(20f, y - 100f, 132f, 32f), tutorialMode ? "Tutorial On" : "Tutorial Off")) tutorialMode = !tutorialMode;

        GUI.Box(new Rect(20f, y - 58f, Screen.width - 40f, 46f), "");
        GUI.Label(new Rect(32f, y - 48f, Screen.width - 64f, 26f), result);

        if (tutorialMode)
        {
            var tutorialWidth = Mathf.Max(240f, Mathf.Min(520f, Screen.width - 380f));
            GUI.Box(new Rect(20f, 18f, tutorialWidth, 88f), "");
            GUI.Label(new Rect(32f, 28f, tutorialWidth - 24f, 22f), tutorialDetail);
            GUI.Label(new Rect(32f, 52f, tutorialWidth - 24f, 22f), "Fade count: " + fadeCount + " | Momentum: " + shooterMomentum);
            GUI.Label(new Rect(32f, 76f, tutorialWidth - 24f, 22f), gameMode == GameMode.Craps ? "Point group side bets resolve on either grouped number or seven-out." : "Cee-lo: 4-5-6, trips, pair+6 win; 1-2-3 and pair+1 lose.");
        }
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
        gameId = "";
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
        result = gameMode == GameMode.CeeLo
            ? "Local Cee-lo table open. Roll three dice."
            : "Local demo table open. Shooter is first-person. Catcher mic is live.";
        tutorialDetail = "Table reset. Use Roll to count a live throw.";
        PulseMic(catcherId, 1.5f);
        ResetDiceToShooter();
        ApplyDiceColor();
    }

    private void SwitchGameMode()
    {
        gameMode = gameMode == GameMode.Craps ? GameMode.CeeLo : GameMode.Craps;
        StartLocalDemo();
    }

    private IEnumerator CreateTable()
    {
        localDemo = false;
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

        yield return Post("/api/street-dice/" + gameId + "/join", "{\"playerName\":\"Shooter\",\"playerId\":\"p1\"}", body =>
        {
            var response = JsonUtility.FromJson<JoinResponse>(body);
            shooterToken = response.playerSessionToken;
            UpdateState(response.state);
        });

        yield return Post("/api/street-dice/" + gameId + "/join", "{\"playerName\":\"Catcher\",\"playerId\":\"p2\"}", body =>
        {
            var response = JsonUtility.FromJson<JoinResponse>(body);
            catcherToken = response.playerSessionToken;
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
            result = fadeCount > 3
                ? "Fade/Catch. Roll stopped. Shooter momentum +" + shooterMomentum + "."
                : "Fade/Catch. Roll stopped. Shooter shoots again.";
            tutorialDetail = "Fade/Catch nullifies the roll. No payout and no side bet resolves.";
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
            yield return RollCeeLo(random.Next(1, 7), random.Next(1, 7), random.Next(1, 7));
            yield break;
        }

        yield return Roll(random.Next(1, 7), random.Next(1, 7));
    }

    private IEnumerator Roll(int a, int b)
    {
        if (phase != "ComeOut" && phase != "Point")
        {
            result = "Open a shot first.";
            yield break;
        }

        die1 = a;
        die2 = b;
        yield return AnimateDiceRoll(a, b);

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
        die1 = a;
        die2 = b;
        die3 = c;
        yield return AnimateDiceRoll(a, b, c);

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
        var values = new[] { a, b, c };
        Array.Sort(values);
        activePointGroup = "-";

        if (values[0] == 4 && values[1] == 5 && values[2] == 6)
        {
            streak += 2;
            result = "Cee-lo 4-5-6. Automatic win.";
            tutorialDetail = "4-5-6 is the strongest street Cee-lo automatic win.";
            PulseMic(catcherId, 1.3f);
            return;
        }

        if (values[0] == 1 && values[1] == 2 && values[2] == 3)
        {
            streak = 0;
            result = "1-2-3. Automatic loss.";
            tutorialDetail = "1-2-3 is an automatic Cee-lo loss.";
            PulseMic(catcherId, 1.3f);
            return;
        }

        if (values[0] == values[1] && values[1] == values[2])
        {
            streak += 2;
            result = "Trips " + values[0] + ". Automatic win.";
            tutorialDetail = "Any triples are an automatic Cee-lo win.";
            PulseMic(catcherId, 1.3f);
            return;
        }

        var ceeLoPoint = PairAndPoint(values);
        if (ceeLoPoint == null)
        {
            result = "No count. Roll again.";
            tutorialDetail = "No pair, no 4-5-6, no 1-2-3. This Cee-lo roll does not count.";
            return;
        }

        if (ceeLoPoint.Value == 6)
        {
            streak += 2;
            result = "Pair plus 6. Automatic win.";
            tutorialDetail = "Pair plus 6 is an automatic Cee-lo win.";
            PulseMic(catcherId, 1.3f);
            return;
        }

        if (ceeLoPoint.Value == 1)
        {
            streak = 0;
            result = "Pair plus 1. Automatic loss.";
            tutorialDetail = "Pair plus 1 is an automatic Cee-lo loss.";
            PulseMic(catcherId, 1.3f);
            return;
        }

        result = "Cee-lo point " + ceeLoPoint.Value + ".";
        tutorialDetail = "Pair plus " + ceeLoPoint.Value + " sets the Cee-lo point to compare against the banker/player.";
        PulseMic(random.NextDouble() > 0.5 ? "p3" : "p4", 0.9f);
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
        point = "-";
        activePointGroup = "-";
        fadeCount = 0;
        shooterMomentum = 0;
        lastResolvedShotWasWin = true;
        result = message + " Shooter wins " + shotAmount + ".";
        if (streak >= HotDiceThreshold) result += " Hot dice active.";
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
            result = message;
        }
        else
        {
            phase = "ComeOut";
            (shooterId, catcherId) = (catcherId, shooterId);
            result = message;
        }

        PulseMic(catcherId, 1.5f);
    }

    private IEnumerator AnimateDiceRoll(int finalA, int finalB, int? finalC = null)
    {
        rolling = true;
        var startA = new Vector3(-0.28f, 0.22f, -2.25f);
        var startB = new Vector3(0.28f, 0.22f, -2.25f);
        var startC = new Vector3(0f, 0.22f, -2.58f);
        var endA = new Vector3(-0.36f, 0.22f, 1.35f + UnityEngine.Random.Range(-0.35f, 0.35f));
        var endB = new Vector3(0.38f, 0.22f, 1.12f + UnityEngine.Random.Range(-0.35f, 0.35f));
        var endC = new Vector3(0.02f, 0.22f, 1.7f + UnityEngine.Random.Range(-0.28f, 0.28f));
        var midA = new Vector3(-0.62f, 0.22f, -0.35f);
        var midB = new Vector3(0.66f, 0.22f, -0.1f);
        var midC = new Vector3(0.05f, 0.22f, -0.48f);

        const float duration = 1.32f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            var hop = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 3.2f)) * Mathf.Lerp(0.34f, 0.04f, t);
            dieA.transform.position = Bezier(startA, midA, endA, t) + Vector3.up * hop;
            dieB.transform.position = Bezier(startB, midB, endB, t) + Vector3.up * (hop * 0.9f);
            if (finalC != null)
            {
                dieC.transform.position = Bezier(startC, midC, endC, t) + Vector3.up * (hop * 0.82f);
            }
            yield return null;
        }

        dieA.transform.position = endA;
        dieB.transform.position = endB;
        if (finalC != null) dieC.transform.position = endC;
        rolling = false;
        LockDieToValue(dieA, finalA);
        LockDieToValue(dieB, finalB);
        if (finalC != null) LockDieToValue(dieC, finalC.Value);
        rollLockFlashUntil = Time.time + 0.7f;
        yield return new WaitForSeconds(0.48f);
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
        dieA.transform.position = new Vector3(-0.28f, 0.22f, -2.25f);
        dieB.transform.position = new Vector3(0.28f, 0.22f, -2.25f);
        dieC.transform.position = new Vector3(0f, 0.22f, -2.58f);
        dieC.SetActive(gameMode == GameMode.CeeLo);
    }

    private void ApplyDiceColor()
    {
        var color = streak >= HotDiceThreshold ? new Color(1f, 0.23f, 0.02f) : selectedDiceColor;
        dieA.GetComponent<Renderer>().material.color = color;
        dieB.GetComponent<Renderer>().material.color = color;
        dieC.GetComponent<Renderer>().material.color = color;
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

        return count;
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

        public SeatMic(string label, string playerId, GameObject root, GameObject head, GameObject pulse, Color accent)
        {
            Label = label;
            PlayerId = playerId;
            Root = root;
            this.head = head;
            this.pulse = pulse;
            this.accent = accent;
        }

        public string Label { get; }
        public string PlayerId { get; }
        public GameObject Root { get; }

        public void Talk(float seconds)
        {
            talkUntil = Time.time + seconds;
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
        public ResolutionDto lastResolution = new ResolutionDto();
    }
    [Serializable] private sealed class ResolutionDto { public string message = ""; }

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
