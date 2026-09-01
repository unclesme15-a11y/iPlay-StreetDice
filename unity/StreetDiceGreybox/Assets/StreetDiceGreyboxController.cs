using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public sealed class StreetDiceGreyboxController : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5108";

    private const int HotDiceThreshold = 5;

    private GameObject dieA = null!;
    private GameObject dieB = null!;
    private GameObject rollLane = null!;
    private readonly SeatMic[] mics = new SeatMic[4];
    private readonly System.Random random = new();

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
    private int fadeCount;
    private int shooterMomentum;
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
        camera.transform.position = new Vector3(0f, 1.72f, -4.55f);
        camera.transform.rotation = Quaternion.Euler(13f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.055f, 0.06f, 0.058f);
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
    }

    private void CreateStreetGroundScene()
    {
        rollLane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rollLane.name = "Physical Roll Lane";
        rollLane.transform.position = new Vector3(0f, -0.055f, 0.12f);
        rollLane.transform.localScale = new Vector3(4.9f, 0.08f, 6.9f);
        rollLane.GetComponent<Renderer>().material.color = new Color(0.19f, 0.205f, 0.19f);

        var backDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backDoor.name = "Closed Service Door Backdrop";
        backDoor.transform.position = new Vector3(0f, 1.22f, 3.82f);
        backDoor.transform.localScale = new Vector3(5.9f, 2.55f, 0.12f);
        backDoor.GetComponent<Renderer>().material.color = new Color(0.23f, 0.26f, 0.255f);

        for (var i = 0; i < 8; i++)
        {
            var slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slat.name = "Service Door Slat";
            slat.transform.position = new Vector3(0f, 0.2f + i * 0.31f, 3.745f);
            slat.transform.localScale = new Vector3(5.95f, 0.035f, 0.05f);
            slat.GetComponent<Renderer>().material.color = new Color(0.12f, 0.14f, 0.14f);
        }

        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "Left Tight Wall";
        leftWall.transform.position = new Vector3(-2.82f, 0.75f, 0.3f);
        leftWall.transform.localScale = new Vector3(0.14f, 1.6f, 6.9f);
        leftWall.GetComponent<Renderer>().material.color = new Color(0.13f, 0.12f, 0.11f);

        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "Right Tight Wall";
        rightWall.transform.position = new Vector3(2.82f, 0.75f, 0.3f);
        rightWall.transform.localScale = new Vector3(0.14f, 1.6f, 6.9f);
        rightWall.GetComponent<Renderer>().material.color = new Color(0.13f, 0.12f, 0.11f);
    }

    private void CreateMicSeats()
    {
        mics[0] = CreateMic("Catcher", "p2", new Vector3(0f, 0.28f, 3.05f), new Color(0.95f, 0.72f, 0.18f));
        mics[1] = CreateMic("Left Side", "p3", new Vector3(-2.15f, 0.28f, 0.25f), new Color(0.42f, 0.78f, 1f));
        mics[2] = CreateMic("Right Side", "p4", new Vector3(2.15f, 0.28f, 0.25f), new Color(0.42f, 0.78f, 1f));
        mics[3] = CreateMic("Back Side", "bot-5", new Vector3(1.35f, 0.28f, 2.45f), new Color(0.42f, 0.78f, 1f));
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
        var width = tutorialMode ? 270f : 210f;
        var height = tutorialMode ? 112f : 76f;
        var x = Screen.width - width - 18f;
        GUI.Box(new Rect(x, 18f, width, height), "");
        GUI.Label(new Rect(x + 12f, 28f, width - 24f, 22f), point == "-" ? "COME OUT" : "POINT " + point);
        GUI.Label(new Rect(x + 12f, 52f, width - 24f, 22f), Time.time < rollLockFlashUntil ? "ROLL LOCKED" : "SHOT " + shotAmount);

        if (tutorialMode)
        {
            GUI.Label(new Rect(x + 12f, 76f, width - 24f, 22f), die1 + " + " + die2 + " = " + (die1 + die2));
            GUI.Label(new Rect(x + 12f, 96f, width - 24f, 22f), phase);
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

            var rect = new Rect(screen.x - 62f, Screen.height - screen.y - 30f, 124f, 46f);
            GUI.Box(rect, "");
            GUI.Label(new Rect(rect.x + 8f, rect.y + 5f, rect.width - 16f, 18f), seat.Label);

            var action = seat.PlayerId == catcherId ? "Fade/Catch" : "Side Bet";
            GUI.Label(new Rect(rect.x + 8f, rect.y + 24f, rect.width - 16f, 18f), action);
        }
    }

    private void DrawBottomControls()
    {
        var y = Screen.height - 118f;
        var buttonWidth = Mathf.Min(118f, (Screen.width - 40f) / 8f);
        var x = 20f;

        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Demo Table")) StartLocalDemo();
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Open Shot")) StartCoroutine(OpenShot());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Fade")) StartCoroutine(Fade());
        x += buttonWidth + 6f;
        if (GUI.Button(new Rect(x, y, buttonWidth, 36f), "Roll")) StartCoroutine(Roll(random.Next(1, 7), random.Next(1, 7)));
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
        lastResolvedShotWasWin = false;
        lastShotWasDoubleUp = false;
        result = "Local demo table open. Shooter is first-person. Catcher mic is live.";
        PulseMic(catcherId, 1.5f);
        ResetDiceToShooter();
        ApplyDiceColor();
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
            phase = "ComeOut";
            point = "-";
            fadeCount = 0;
            shooterMomentum = 0;
            result = "Shot open. Catcher can fade/catch before the roll counts.";
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
            PulseMic(catcherId, 1.6f);
            ResetDiceToShooter();
            yield break;
        }

        var json = $"{{\"catcherId\":\"{catcherId}\",\"playerSessionToken\":\"{catcherToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/fade", json);
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
            if (total is 7 or 11)
            {
                ShooterWin("Come-out win.");
                return;
            }

            if (total is 2 or 3 or 12)
            {
                ShooterLoss("Come-out loss. Shooter pays but keeps dice.", true);
                return;
            }

            point = total.ToString();
            phase = "Point";
            result = "Point established: " + point + ".";
            PulseMic(catcherId, 1f);
            return;
        }

        var currentPoint = int.Parse(point);
        if (total == currentPoint)
        {
            ShooterWin("Point hit.");
            return;
        }

        if (total == 7)
        {
            ShooterLoss("Seven out. Dice pass to Catcher.", false);
            return;
        }

        result = "Rolled " + total + ". Shooter keeps shooting for " + point + ".";
        PulseMic(random.NextDouble() > 0.5 ? "p3" : "p4", 0.9f);
    }

    private void ShooterWin(string message)
    {
        var gain = point == "-" ? 1 : 2;
        gain += shooterMomentum;
        if (lastShotWasDoubleUp) gain += 1;
        streak += gain;
        phase = "ShooterDecision";
        point = "-";
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

    private IEnumerator AnimateDiceRoll(int finalA, int finalB)
    {
        rolling = true;
        var startA = new Vector3(-0.28f, 0.22f, -2.25f);
        var startB = new Vector3(0.28f, 0.22f, -2.25f);
        var endA = new Vector3(-0.22f, 0.22f, 1.25f + UnityEngine.Random.Range(-0.45f, 0.45f));
        var endB = new Vector3(0.32f, 0.22f, 1.15f + UnityEngine.Random.Range(-0.45f, 0.45f));

        const float duration = 1.08f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            var hop = Mathf.Sin(t * Mathf.PI) * 0.38f;
            dieA.transform.position = Vector3.Lerp(startA, endA, t) + Vector3.up * hop;
            dieB.transform.position = Vector3.Lerp(startB, endB, t) + Vector3.up * (hop * 0.9f);
            yield return null;
        }

        dieA.transform.position = endA;
        dieB.transform.position = endB;
        rolling = false;
        LockDieToValue(dieA, finalA);
        LockDieToValue(dieB, finalB);
        rollLockFlashUntil = Time.time + 0.55f;
        yield return new WaitForSeconds(0.35f);
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
    }

    private void ApplyDiceColor()
    {
        var color = streak >= HotDiceThreshold ? new Color(1f, 0.23f, 0.02f) : selectedDiceColor;
        dieA.GetComponent<Renderer>().material.color = color;
        dieB.GetComponent<Renderer>().material.color = color;
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

    private void UpdateState(StateDto state)
    {
        if (state == null) return;
        phase = state.phase;
        shooterId = state.shooterId;
        catcherId = state.catcherId;
        point = state.point == 0 ? "-" : state.point.ToString();
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
}
