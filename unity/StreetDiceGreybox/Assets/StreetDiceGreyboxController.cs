using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public sealed class StreetDiceGreyboxController : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://localhost:5108";

    private GameObject dieA = null!;
    private GameObject dieB = null!;
    private GameObject[] seats = Array.Empty<GameObject>();
    private readonly System.Random random = new();

    private string gameId = "";
    private string shooterToken = "";
    private string catcherToken = "";
    private string shooterId = "p1";
    private string catcherId = "p2";
    private string phase = "Offline";
    private string result = "Create or connect to a table.";
    private string point = "-";
    private int streak;
    private int die1 = 1;
    private int die2 = 1;
    private Color selectedDiceColor = Color.green;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<StreetDiceGreyboxController>() == null)
        {
            new GameObject("Street Dice Greybox").AddComponent<StreetDiceGreyboxController>();
        }
    }

    private void Awake()
    {
        Camera.main?.gameObject.SetActive(false);

        var cameraObject = new GameObject("Top Down Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 11, -6);
        camera.transform.rotation = Quaternion.Euler(62, 0, 0);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.09f, 0.09f);
        camera.fieldOfView = 42;

        var lightObject = new GameObject("Key Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        var lane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lane.name = "Street Dice Rolling Area";
        lane.transform.position = new Vector3(0, -0.08f, 0.7f);
        lane.transform.localScale = new Vector3(5.6f, 0.1f, 6.4f);
        lane.GetComponent<Renderer>().material.color = new Color(0.22f, 0.24f, 0.24f);

        seats = new[]
        {
            CreateSeat("First Person", new Vector3(0, 0.2f, -3.2f), new Color(0.1f, 0.6f, 0.9f)),
            CreateSeat("Left 1", new Vector3(-3.2f, 0.2f, -0.9f), new Color(0.9f, 0.9f, 0.9f)),
            CreateSeat("Left 2", new Vector3(-3.2f, 0.2f, 1.6f), new Color(0.9f, 0.9f, 0.9f)),
            CreateSeat("Right 1", new Vector3(3.2f, 0.2f, -0.9f), new Color(0.9f, 0.9f, 0.9f)),
            CreateSeat("Right 2", new Vector3(3.2f, 0.2f, 1.6f), new Color(0.9f, 0.9f, 0.9f)),
        };

        dieA = CreateDie("Die A", new Vector3(-0.45f, 0.35f, 0.4f));
        dieB = CreateDie("Die B", new Vector3(0.45f, 0.35f, 0.4f));
        ApplyDiceColor();
    }

    private void Update()
    {
        dieA.transform.Rotate(new Vector3(18, 27, 13) * Time.deltaTime);
        dieB.transform.Rotate(new Vector3(23, 14, 31) * Time.deltaTime);
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(20, 16, Screen.width - 40, 76), $"Dice: {die1} + {die2} = {die1 + die2}     Point: {point}     Phase: {phase}     Streak: {streak}/5");
        GUI.Label(new Rect(32, 48, Screen.width - 64, 24), result);

        var y = Screen.height - 116;
        if (GUI.Button(new Rect(20, y, 112, 36), "Create")) StartCoroutine(CreateTable());
        if (GUI.Button(new Rect(140, y, 112, 36), "Join")) StartCoroutine(JoinPlayers());
        if (GUI.Button(new Rect(260, y, 112, 36), "Fill Bots")) StartCoroutine(Post("/api/street-dice/" + gameId + "/bots/fill", "{\"targetPlayers\":5}"));
        if (GUI.Button(new Rect(380, y, 112, 36), "Open Shot")) StartCoroutine(OpenShot());
        if (GUI.Button(new Rect(500, y, 112, 36), "Fade")) StartCoroutine(Fade());
        if (GUI.Button(new Rect(620, y, 112, 36), "Roll")) StartCoroutine(Roll(random.Next(1, 7), random.Next(1, 7)));
        if (GUI.Button(new Rect(740, y, 112, 36), "Run Same")) StartCoroutine(RunSame());
        if (GUI.Button(new Rect(860, y, 112, 36), "Double Up")) StartCoroutine(DoubleUp());
        if (GUI.Button(new Rect(980, y, 112, 36), "Voice Gate")) StartCoroutine(VoiceGate());

        var meterWidth = Mathf.Min(360, Screen.width - 40);
        GUI.Box(new Rect(20, y + 50, meterWidth, 22), "");
        var fillColor = streak >= 5 ? new Color(1f, 0.23f, 0.02f) : new Color(0.1f, 0.72f, 0.35f);
        var previous = GUI.color;
        GUI.color = fillColor;
        GUI.Box(new Rect(22, y + 52, Mathf.Clamp01(streak / 5f) * (meterWidth - 4), 18), "");
        GUI.color = previous;
    }

    private GameObject CreateSeat(string seatName, Vector3 position, Color color)
    {
        var seat = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        seat.name = seatName;
        seat.transform.position = position;
        seat.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        seat.GetComponent<Renderer>().material.color = color;
        return seat;
    }

    private GameObject CreateDie(string dieName, Vector3 position)
    {
        var die = GameObject.CreatePrimitive(PrimitiveType.Cube);
        die.name = dieName;
        die.transform.position = position;
        die.transform.localScale = Vector3.one * 0.52f;
        return die;
    }

    private void ApplyDiceColor()
    {
        var color = streak >= 5 ? new Color(1f, 0.23f, 0.02f) : selectedDiceColor;
        dieA.GetComponent<Renderer>().material.color = color;
        dieB.GetComponent<Renderer>().material.color = color;
    }

    private IEnumerator CreateTable()
    {
        yield return Post("/api/street-dice/create", "{}", body =>
        {
            var response = JsonUtility.FromJson<CreateResponse>(body);
            gameId = response.gameId;
            UpdateState(response.state);
        });
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
        var json = $"{{\"shooterId\":\"{shooterId}\",\"shooterSessionToken\":\"{shooterToken}\",\"catcherId\":\"{catcherId}\",\"amount\":20}}";
        yield return Post("/api/street-dice/" + gameId + "/shot", json);
    }

    private IEnumerator Fade()
    {
        var json = $"{{\"catcherId\":\"{catcherId}\",\"playerSessionToken\":\"{catcherToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/fade", json);
    }

    private IEnumerator Roll(int a, int b)
    {
        var json = $"{{\"shooterId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\",\"die1\":{a},\"die2\":{b}}}";
        yield return Post("/api/street-dice/" + gameId + "/roll", json, body =>
        {
            die1 = a;
            die2 = b;
            var response = JsonUtility.FromJson<ActionResponse>(body);
            UpdateState(response.state);
        });
    }

    private IEnumerator RunSame()
    {
        var json = $"{{\"shooterId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/decision/run-same", json);
    }

    private IEnumerator DoubleUp()
    {
        var json = $"{{\"shooterId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/decision/double-up", json);
    }

    private IEnumerator VoiceGate()
    {
        var json = $"{{\"playerId\":\"{shooterId}\",\"playerSessionToken\":\"{shooterToken}\"}}";
        yield return Post("/api/street-dice/" + gameId + "/voice/access-token", json);
    }

    private IEnumerator Post(string path, string json, Action<string>? onSuccess = null)
    {
        if (string.IsNullOrWhiteSpace(gameId) && !path.EndsWith("/create", StringComparison.Ordinal))
        {
            result = "Create a table first.";
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
        result = state.lastResolution.message;
        ApplyDiceColor();
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
        public ResolutionDto lastResolution = new();
    }
    [Serializable] private sealed class ResolutionDto { public string message = ""; }
}
