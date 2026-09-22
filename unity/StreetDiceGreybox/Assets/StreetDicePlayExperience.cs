using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class StreetDiceGreyboxController
{
    private FirstPersonDiceHand leftMotion, rightMotion;
    private Camera skyCamera;
    private RenderTexture skyTexture;
    private bool skyCamVisible, skyResultVisible;
    private float skyResultDuration = 1.05f;
    private bool diceCalling, diceCallingAvailable;
    private AudioClip[] diceCallClips;
    private Coroutine diceCallingRoutine;
    private bool mainOptions = true, drawerOpen, confirmLeave, awaitingShootChoice, shotCommitted;
    private int onlineAvailableBalance;
    private bool shakeHeld, rollFaded, snapStyle, leftHanded, micMuted;
    private const float BettingWindowSeconds = 10f;
    private float drawerAmount, heldSince, throwPower = 0.5f, throwAim, nextBotAt, bettingClosesAt;
    private float effectsVolume = 0.7f;
    private float throwLeadIn;
    private string ceeLoBanker = "p1";
    private int ceeLoSeatIndex;
    private readonly List<string> ceeLoSeats = new();
    private Vector2 touchOrigin, touchLatest, drawerScroll;
    private int touchId = -1, nextImpact;
    private bool edgeDrag;
    private Vector2 edgeOrigin;
    private string drawerPage = "Options";
    private bool showCredits;
    private Vector2 creditsScroll;
    private readonly Dictionary<string, int> cash = new();
    private readonly List<CashBet> cashBets = new();
    private readonly List<GameObject> moneyPiles = new();
    private static readonly int[] WagerAmounts = { 1, 5, 10, 20 };
    private static readonly string[] WagerLabels = { "$1", "$5", "$10", "$20" };
    private static readonly Vector2[] OpponentMoneyViewports =
    {
        new(0.14f, 0.275f), new(0.15f, 0.13f),
        new(0.78f, 0.275f), new(0.74f, 0.13f)
    };
    private readonly Dictionary<int, Material> billMaterials = new();
    private readonly Dictionary<int, int> displayedMoney = new();
    private int selectedSideWager = 10;
    private AudioClip metalClip, moneyPullClip, moneyLandClip;
    private readonly Queue<MoneyTransfer> moneyTransfers = new();
    private readonly HashSet<GameObject> activeMoneyBills = new();
    private Coroutine moneyTransferRoutine;
    private bool moneyTransferRunning, moneyAnimationEnabled = true;
    private float moneyTransferDuration = 1.05f;
    private float lastBrickHapticAt = -10f;
    private Texture2D voiceFaceIcon;
#if UNITY_EDITOR
    private Rect? safeAreaOverride;
#endif
    private Rect UiSafeArea
    {
        get
        {
            Rect area = Screen.safeArea;
#if UNITY_EDITOR
            area = safeAreaOverride ?? area;
#endif
            return area.width > 0 && area.height > 0 ? area : new Rect(0, 0, Screen.width, Screen.height);
        }
    }
    private Vector2 UiOrigin => new Vector2(UiSafeArea.x, Screen.height - UiSafeArea.yMax);
    private float UiScale => Mathf.Max(0.45f, Mathf.Min(UiSafeArea.width / 1100f, UiSafeArea.height / 620f));
    private float UiWidth => UiSafeArea.width / UiScale;
    private float UiHeight => UiSafeArea.height / UiScale;
    private bool applicationPaused;
    private bool onlineHeartbeatInFlight;
    private float nextOnlineHeartbeatAt;
    private bool SkyDisplayActive => skyCamVisible && (rolling || fadeInProgress);
    private bool CanGesture => !applicationPaused && !mainOptions && !drawerOpen && !confirmLeave && !awaitingShootChoice
        && shooterId == SelfId && !rolling && shotCommitted && !AwaitingWagerAcceptance
        && (phase == "ComeOut" || phase == "Point" || phase == "CeeLo");
    private bool BettingWindowOpen => gameMode == GameMode.Craps && shotCommitted && !rolling &&
        (localDemo ? Time.unscaledTime < bettingClosesAt : onlineWagerWindowKnown &&
            Time.unscaledTimeAsDouble < onlineOfferDeadline);

    private sealed class CashBet
    {
        public string Player, Type;
        public int Amount;
        public bool Settled, Won;
    }

    private readonly struct BillGroup
    {
        public readonly int Denomination, Count;
        public BillGroup(int denomination, int count) { Denomination = denomination; Count = count; }
    }

    private readonly struct MoneyTransfer
    {
        public readonly string From, To;
        public readonly int Amount;
        public MoneyTransfer(string from, string to, int amount) { From = from; To = to; Amount = amount; }
    }

    private void InitializePlayExperience()
    {
        if (skyCamera != null) return;
        snapStyle = PlayerPrefs.GetInt("StreetDice.ThrowStyle", 0) == 1;
        leftHanded = PlayerPrefs.GetInt("StreetDice.ThrowHand", 0) == 1;
        effectsVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("StreetDice.Effects", 0.7f));
        tutorialMode = PlayerPrefs.GetInt("StreetDice.Tutorial", 0) == 1;
        diceCallClips = new AudioClip[13];
        diceCallingAvailable = true;
        for (int value = 1; value <= 12; value++)
        {
            diceCallClips[value] = Resources.Load<AudioClip>("Audio/DiceCalls/" + value);
            diceCallingAvailable &= diceCallClips[value] != null;
        }
        diceCalling = diceCallingAvailable && PlayerPrefs.GetInt("StreetDice.DiceCalling", 1) == 1;
        SelectThrowHand();
        int color = Mathf.Clamp(PlayerPrefs.GetInt("StreetDice.DiceColor", 0), 0, 3);
        selectedDiceColor = DiceColors[color];
        ApplyDiceColor();
        Camera.main.cullingMask &= ~(1 << 29);
        if (environmentPlate != null) environmentPlate.layer = 30;
        foreach (var node in handRig.GetComponentsInChildren<Transform>(true)) node.gameObject.layer = 28;
        foreach (var die in new[] { dieA, dieB, dieC })
            foreach (var node in die.GetComponentsInChildren<Transform>(true)) node.gameObject.layer = 27;
        var skyObject = new GameObject("Required Sky Cam - same live dice");
        skyCamera = skyObject.AddComponent<Camera>();
        skyCamera.enabled = false;
        skyCamera.orthographic = true;
        skyCamera.orthographicSize = 0.7f;
        skyCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        skyCamera.transform.position = new Vector3(0, 6, -1);
        skyCamera.cullingMask = (1 << 27) | (1 << 29);
        skyCamera.clearFlags = CameraClearFlags.SolidColor;
        skyCamera.backgroundColor = new Color(0.045f, 0.045f, 0.04f);
        skyTexture = new RenderTexture(768, 384, 24) { name = "Live dice overhead" };
        skyCamera.targetTexture = skyTexture;
        skyCamera.aspect = 2f;
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Sky Cam pavement";
        ground.layer = 29;
        ground.transform.position = new Vector3(0, DiceRestY - DiceWorldScale * 0.5f, 0);
        ground.transform.localScale = new Vector3(2f, 1, 2f);
        var surface = new Material(Resources.Load<Shader>("Environments/PavementPhoto"));
        var matchingPavement = Resources.Load<Texture2D>("Environments/bodega-pavement-exact-crop");
        surface.mainTexture = matchingPavement;
        matchingPavement.wrapMode = TextureWrapMode.Mirror;
        surface.mainTextureScale = Vector2.one * (20f / 0.85f);
        surface.mainTextureOffset = Vector2.zero;
        ground.GetComponent<Renderer>().material = surface;
        metalClip = CreateToneClip("Rollup metal impact", 680f, 0.22f, 0.5f);
        moneyPullClip = CreateMoneyClip("Paper money peeled from pavement", 0.24f, false);
        moneyLandClip = CreateMoneyClip("Paper money lands on pile", 0.16f, true);
        voiceFaceIcon = CreateVoiceFaceIcon();
        CreateGroundMoney();
        foreach (var player in DemoShooterOrder) cash[player] = 1000;
    }

    private void LateUpdate()
    {
        // Coroutine roll/fade state is final for this frame before cameras render.
        if (skyCamera != null)
        {
            UpdateSkyCam();
            skyCamera.enabled = SkyDisplayActive && !mainOptions;
        }
    }

    private void SelectThrowHand()
    {
        leftThrowHand.SetActive(false);
        rightThrowHand.SetActive(false);
        throwHand = leftHanded ? leftMotion : rightMotion;
        leftMotion.SnapEnabled = rightMotion.SnapEnabled = snapStyle;
    }

    private void ResetPlaySession()
    {
        realOnlineTable = false;
        lastSeenCommittedRoll = 0;
        remoteReplayInProgress = false;
        nextOnlineHeartbeatAt = 0f;
        onlineHeartbeatInFlight = false;
        ClearMoneyTransfers();
        ResetDiceSale();
        mainOptions = false;
        drawerOpen = confirmLeave = shakeHeld = false;
        awaitingShootChoice = true;
        shotCommitted = false;
        pendingRemoteRollId = null;
        pendingRemoteFadeSeconds = 0f;
        nextServerPollAt = 0f;
        nextOnlineWalletPollAt = 0f;
        onlineAvailableBalance = 1000;
        onlineWalletPollInFlight = onlineWagerWindowKnown = onlineWagerRequestInFlight = false;
        onlineWagerOffers.Clear();
        cashBets.Clear();
        ClearGroundOfferVisuals();
        wagerBook = new IPlay.Demo.WagerBook();
        botOfferResponseAt.Clear();
        wagerAcceptedAt.Clear();
        offerPage.Clear();
        offerSwipeStart.Clear();
        wagerOverlayOpen = false;
        armedOfferId = 0;
        ResetWagerDraft();
        selectedSideWager = 10;
        foreach (var player in DemoShooterOrder) cash[player] = 1000;
        nextBotAt = Time.time + 3;
    }

    private void UpdatePlayExperience()
    {
        if (skyCamera == null) return;
        drawerAmount = Mathf.MoveTowards(drawerAmount, drawerOpen ? 1f : 0f, Time.unscaledDeltaTime * 5f);
        audioSource.volume = effectsVolume;
        UpdateStartupExperience();
        UpdateWagerOffers();
        UpdateDiceSale();
        RefreshGroundMoney();
        UpdateGroundOfferBills();
        if (mainOptions) return;
        if (!localDemo && realOnlineTable && !applicationPaused && !onlineHeartbeatInFlight &&
            playerTokens.ContainsKey(SelfId) && Time.realtimeSinceStartup >= nextOnlineHeartbeatAt)
        {
            nextOnlineHeartbeatAt = Time.realtimeSinceStartup + 2f;
            StartCoroutine(HeartbeatOnlineTable());
        }
        if (!localDemo && !serverPollInFlight && !fadeInProgress
            && !string.IsNullOrEmpty(gameId) && Time.realtimeSinceStartup >= nextServerPollAt)
        {
            nextServerPollAt = Time.realtimeSinceStartup + 0.2f;
            StartCoroutine(PollOnlineTable());
        }
        if (!localDemo && !onlineWalletPollInFlight && !string.IsNullOrEmpty(gameId) &&
            playerTokens.ContainsKey(SelfId) && Time.realtimeSinceStartup >= nextOnlineWalletPollAt)
        {
            nextOnlineWalletPollAt = Time.realtimeSinceStartup + 1f;
            StartCoroutine(PollOwnWallet());
        }
        ReadDrawerSwipe();
        if (Input.GetKeyDown(KeyCode.Escape)) { CancelShake(); drawerOpen = !drawerOpen; }
        if (CanGesture || shakeHeld) ReadThrowInput();
        if (shakeHeld)
        {
            handRig.SetActive(true);
            throwHand.SampleShake(Time.time - heldSince);
            int count = gameMode == GameMode.CeeLo ? 3 : 2;
            var dice = new[] { dieA, dieB, dieC };
            for (int i = 0; i < count; i++) dice[i].transform.SetPositionAndRotation(throwHand.DicePosition(i, count, DiceWorldScale), throwHand.DiceRotation);
        }
        if (localDemo && shooterId != "p1" && !rolling && !confirmLeave && !drawerOpen && Time.time >= nextBotAt)
        {
            nextBotAt = Time.time + 4f;
            if (awaitingShootChoice)
            {
                if (!CanCover(shotAmount) || IPlay.Demo.DemoOpponentPolicy.Pass(BotProfile(shooterId), Balance(shooterId), shotAmount, random.NextDouble())) SellCurrentDice();
                else { CommitShoot(); if (!shotCommitted) SellCurrentDice(); }
            }
            else if (phase == "ShooterDecision")
            {
                if (!CanCover(shotAmount) || IPlay.Demo.DemoOpponentPolicy.Pass(BotProfile(shooterId), Balance(shooterId), shotAmount, random.NextDouble())) SellCurrentDice();
                else if (lastResolvedShotWasWin && shotAmount <= int.MaxValue / 2 && CanCover(shotAmount * 2) &&
                    IPlay.Demo.DemoOpponentPolicy.DoubleUp(BotProfile(shooterId), Balance(shooterId), shotAmount, random.NextDouble())) StartCoroutine(DoubleUp());
                else StartCoroutine(RunSame());
            }
            else if (shotCommitted && !AwaitingWagerAcceptance) StartCoroutine(RollCurrentMode());
        }
    }

    private void ReadThrowInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began && touchId < 0) BeginShake(touch.position, touch.fingerId);
                if (touch.fingerId != touchId) continue;
                touchLatest = touch.position;
                if (touch.phase == TouchPhase.Canceled) CancelShake();
                else if (touch.phase == TouchPhase.Ended) EndShake(touch.position);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) BeginShake(Input.mousePosition, -2);
            if (shakeHeld && touchId == -2) touchLatest = Input.mousePosition;
            if (Input.GetMouseButtonUp(0) && touchId == -2) EndShake(Input.mousePosition);
            if (touchId >= 0) CancelShake();
        }
    }

    private void ReadDrawerSwipe()
    {
        if (confirmLeave || Input.touchCount != 1) { edgeDrag = false; return; }
        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            edgeOrigin = touch.position;
            float edgeX = touch.position.x - UiSafeArea.x;
            float fromRight = UiSafeArea.width - edgeX;
            edgeDrag = fromRight >= 0 && UiSafeArea.Contains(touch.position) &&
                (drawerOpen ? fromRight < 345f * UiScale : fromRight < 24f * UiScale);
        }
        if (!edgeDrag) return;
        float delta = touch.position.x - edgeOrigin.x;
        if (!drawerOpen && delta < -55f * UiScale) { CancelShake(); drawerOpen = true; edgeDrag = false; }
        else if (drawerOpen && delta > 55f * UiScale) { drawerOpen = false; edgeDrag = false; }
        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) edgeDrag = false;
    }

    private void BeginShake(Vector2 position, int id)
    {
        // Only the clear lower-center pavement accepts a throw, never HUD controls or side betting.
        var p = new Vector2((position.x - UiSafeArea.x) / UiSafeArea.width, (position.y - UiSafeArea.y) / UiSafeArea.height);
        if (!CanGesture || p.x < 0.25f || p.x > 0.75f || p.y > 0.43f || p.y < 0.08f) return;
        touchId = id;
        touchOrigin = touchLatest = position;
        heldSince = Time.time;
        shakeHeld = true;
        preserveRestingPose = false;
        nextImpact = 0;
    }

    private void EndShake(Vector2 position)
    {
        if (!shakeHeld) return;
        Vector2 delta = position - touchOrigin;
        bool valid = delta.y >= UiSafeArea.height * 0.06f && Time.time - heldSince >= 0.12f;
        shakeHeld = false;
        touchId = -1;
        if (!valid || !CanGesture) { handRig.SetActive(false); ResetDiceToShooter(); return; }
        throwPower = Mathf.Clamp01(delta.y / (UiSafeArea.height * 0.42f));
        throwAim = Mathf.Clamp(delta.x / (UiSafeArea.width * 0.24f), -1f, 1f);
        throwLeadIn = 0.24f;
        StartCoroutine(RollCurrentMode());
    }

    private void CancelShake()
    {
        bool wasHeld = shakeHeld;
        shakeHeld = false;
        touchId = -1;
        if (!rolling && handRig != null) handRig.SetActive(false);
        if (wasHeld && !rolling) ResetDiceToShooter();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) { edgeDrag = false; CancelShake(); }
    }

    private void OnApplicationPause(bool paused)
    {
        applicationPaused = paused;
        if (paused) { edgeDrag = false; CancelShake(); }
        if (paused && realOnlineTable && playerTokens.TryGetValue(SelfId, out var token))
            StartCoroutine(Post("/api/street-dice/" + gameId + "/presence/disconnect",
                JsonUtility.ToJson(new OnlineWalletRequest { playerId = SelfId, playerSessionToken = token }), _ => { }));
        if (!paused) nextOnlineHeartbeatAt = 0f;
    }

    private void UpdateSkyCam()
    {
        if (fadeInProgress)
        {
            skyCamera.orthographicSize = 0.24f;
            skyCamera.transform.position = new Vector3(catchViewCenter.x, 6f, catchViewCenter.z);
            return;
        }
        var min = dieA.transform.position;
        var max = min;
        foreach (var die in new[] { dieB, dieC })
            if (die.activeSelf) { min = Vector3.Min(min, die.transform.position); max = Vector3.Max(max, die.transform.position); }
        var center = (min + max) * 0.5f;
        // Frame the whole pair rather than chasing only one die or letting it leave the inset.
        float size = Mathf.Max(0.18f, (max.z - min.z) * 0.5f + 0.08f, (max.x - min.x) * 0.25f + 0.07f);
        skyCamera.orthographicSize = StepSkyCamSize(skyCamera.orthographicSize, size, Time.unscaledDeltaTime);
        skyCamera.transform.position = new Vector3(center.x, 6f, center.z);
    }

    private static float StepSkyCamSize(float current, float required, float deltaTime)
    {
        // Never crop a spreading roll; ease inward independently of the display frame rate.
        if (required >= current) return required;
        return Mathf.Lerp(current, required, 1f - Mathf.Exp(-21.4005f * Mathf.Max(0, deltaTime)));
    }

    private void BeginSkyResultHold(int a, int b, int? c)
    {
        skyResultVisible = true;
        if (diceCallingRoutine != null) StopCoroutine(diceCallingRoutine);
        diceCallingRoutine = diceCalling ? StartCoroutine(CallDice(a, b, c)) : null;
    }

    private IEnumerator CallDice(int a, int b, int? c)
    {
        if (c.HasValue)
        {
            foreach (int value in new[] { a, b, c.Value })
            {
                var clip = diceCallClips[value];
                if (clip == null) continue;
                PlayAudio(clip);
                yield return new WaitForSecondsRealtime(clip.length);
            }
        }
        else
        {
            PlayAudio(diceCallClips[a + b]);
        }
        diceCallingRoutine = null;
    }

    private static readonly Color[] DiceColors = { new Color(0.92f, 0.9f, 0.84f), new Color(0.018f, 0.019f, 0.018f), new Color(0.08f, 0.55f, 0.23f), new Color(0.08f, 0.22f, 0.72f) };
    private Texture2D hotFireTexture, hotMeterFrame;
    private Font doorGraffitiFont;
    private bool comeOutCueShown;
    private float comeOutCueStarted;

    private void DrawPlayExperience()
    {
        if (skyTexture == null) return;
        var previousSkin = GUI.skin;
        EnsureIPlaySkin(previousSkin);
        GUI.skin = pregameSkin;
        if (mainOptions && (startupScreen == StartupScreen.GlobalSettings || startupScreen == StartupScreen.LegacyPregame))
        {
            if (pregameBackground == null) pregameBackground = Resources.Load<Texture2D>("UI/pregame-dice-background");
            var oldTint = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pregameBackground, ScaleMode.ScaleAndCrop);
            GUI.color = oldTint;
        }
        var matrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(UiOrigin.x, UiOrigin.y, 0), Quaternion.identity, Vector3.one * UiScale);
#if UNITY_EDITOR
        if (startupTestEvent != null && Event.current.type == EventType.Repaint)
        {
            Event.current = startupTestEvent;
            startupTestEvent = null;
        }
#endif
        GUI.skin.label.fontSize = 16;
        GUI.skin.button.fontSize = 16;
        GUI.skin.toggle.fontSize = 16;
        GUI.skin.label.wordWrap = true;
        if (mainOptions)
        {
            if (startupScreen == StartupScreen.GlobalSettings || startupScreen == StartupScreen.LegacyPregame)
                DrawPregameOptions();
            else DrawStartupExperience();
        }
        else if (confirmLeave) DrawLeaveConfirmation();
        else
        {
            float w = UiWidth, h = UiHeight;
            GUI.enabled = !drawerOpen && !confirmLeave;
            string phaseLabel = PhaseHudLabel();
            if (phaseLabel.Length > 0) GUI.Label(new Rect(w - 190, 78, 175, 34), phaseLabel);
            if (realOnlineTable && phase == "Lobby" && !string.IsNullOrEmpty(gameId))
            {
                GUI.Label(new Rect(w / 2f - 240f, 14f, 480f, 26f), "TABLE  " + gameId);
                if (GUI.Button(new Rect(w / 2f - 58f, 42f, 116f, 30f), "COPY CODE"))
                    GUIUtility.systemCopyBuffer = gameId;
            }
            if (tutorialMode)
                GUI.Label(new Rect(w / 2 - 210, 180, 420, 65), die1 + " + " + die2 + (gameMode == GameMode.CeeLo ? " + " + die3 : "") + " | " + phase + "\n" + tutorialDetail);
            if (gameMode == GameMode.Craps) DrawBettingToggle();
            var settingsControl = new Rect(w - 82, 9, 70, 70);
            GUI.DrawTexture(settingsControl, settingsGear, ScaleMode.ScaleToFit, true);
            if (GUI.Button(settingsControl, new GUIContent("", "Game options"), GUIStyle.none))
            { CancelShake(); wagerOverlayOpen = false; drawerOpen = !drawerOpen; }
            DrawSeatHud();
            if (SkyDisplayActive) DrawSkyCamDisplay(w);
            DrawBettingTimer();
            if (wagerOverlayOpen) DrawBettingOverlay();
            DrawDiceSale(w);
            if (gameMode == GameMode.Craps && shooterId == SelfId)
                DrawHotMeter(new Rect(settingsControl.x - 56f, settingsControl.y, 48f, 124f));
            if (!rolling && !SaleOpen && awaitingShootChoice && shooterId == SelfId)
            {
                int choice = GUI.Toolbar(new Rect(w / 2 - 146, h - 148, 292, 36), Array.IndexOf(WagerAmounts, shotAmount), WagerLabels);
                if (choice >= 0) SelectMainWager(WagerAmounts[choice]);
                GUI.enabled = !drawerOpen && CanCover(shotAmount) &&
                    (localDemo || Array.FindAll(onlinePlayers, player => !player.hasLeft).Length >= 2);
                if (GUI.Button(new Rect(w / 2 - 146, h - 104, 140, 46), "Shoot")) CommitShoot();
                GUI.enabled = !drawerOpen;
                GUI.enabled = !drawerOpen && (localDemo || Array.FindAll(onlinePlayers, player => !player.hasLeft).Length >= 3);
                if (GUI.Button(new Rect(w / 2 + 6, h - 104, 140, 46), "Sell")) SellCurrentDice();
            }
            else if (!rolling && phase == "ShooterDecision" && shooterId == SelfId)
            {
                GUI.enabled = !drawerOpen && CanCover(shotAmount);
                if (GUI.Button(new Rect(w / 2 - 210, h - 104, 132, 46), "Run Same")) StartCoroutine(RunSame());
                GUI.enabled = !drawerOpen && lastResolvedShotWasWin && shotAmount <= int.MaxValue / 2 && CanCover(shotAmount * 2);
                if (GUI.Button(new Rect(w / 2 - 70, h - 104, 132, 46), "Double Up")) StartCoroutine(DoubleUp());
                GUI.enabled = !drawerOpen;
                GUI.enabled = !drawerOpen && (localDemo || Array.FindAll(onlinePlayers, player => !player.hasLeft).Length >= 3);
                if (GUI.Button(new Rect(w / 2 + 70, h - 104, 132, 46), "Sell")) SellCurrentDice();
            }
            GUI.enabled = !confirmLeave;
            if (drawerAmount > 0) DrawDrawer();
            GUI.enabled = true;
            if (confirmLeave) DrawLeaveConfirmation();
        }
        GUI.matrix = matrix;
        GUI.skin = previousSkin;
    }

#if UNITY_EDITOR
    private Event startupTestEvent;
    private void QueueStartupTestEvent(Event input) { startupTestEvent = input; }
#endif

    private GUISkin pregameSkin;
    private Texture2D pregameBackground;

    private void EnsureIPlaySkin(GUISkin source)
    {
        if (pregameSkin != null) return;
        pregameSkin = Instantiate(source);
        var marker = Resources.Load<Font>("UI/PermanentMarker-Regular");
        if (marker == null) throw new InvalidOperationException("The iPlay graffiti font is missing.");
        pregameSkin.font = marker;
        foreach (var style in new[]
        {
            pregameSkin.label, pregameSkin.button, pregameSkin.box, pregameSkin.toggle,
            pregameSkin.window, pregameSkin.textField, pregameSkin.textArea
        }) style.font = marker;
        foreach (var style in pregameSkin.customStyles) style.font = marker;
    }

    private void DrawPregameOptions()
    {
        var previous = GUI.skin;
        EnsureIPlaySkin(previous);
        pregameSkin.label.fontSize = 20;
        pregameSkin.button.fontSize = 20;
        try
        {
            if (!showCredits) GUI.skin = pregameSkin;
            DrawMainOptions();
        }
        finally { GUI.skin = previous; }
    }

    private void DrawMainOptions()
    {
        float x = UiWidth / 2 - 235;
        if (!showCredits)
        {
            float bandX = UiWidth / 2f - 480f;
            DrawMetalPlate(new Rect(bandX, 143f, 960f, 194f));
            DrawMetalPlate(new Rect(bandX, 338f, 960f, 58f));
            DrawMetalPlate(new Rect(bandX, 402f, 960f, 85f));
            DrawMetalPlate(new Rect(bandX, 490f, 960f, 78f));
        }
        var heading = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 30 };
        GUI.Label(new Rect(UiWidth / 2 - 450, 40, 900, 44),
            showCredits ? "Credits"
                : startupScreen == StartupScreen.GlobalSettings ? "Global Settings" : "iPlay Cee-lo & Craps",
            heading);
        // S12/S14: the mark was missing from Global Settings and Credits. Scaled to
        // 0.55 so it ends at y=84 and clears the button row at y=90; at full size it
        // would run to y=145 and sit on top of the Exit button. The legacy pregame
        // screen is skipped because its own title is already the brand name.
        if (showCredits || startupScreen == StartupScreen.GlobalSettings) DrawBrandLogo(0.55f);
        if (showCredits)
        {
            creditsScroll = GUI.BeginScrollView(new Rect(x, 98, 470, UiHeight - 230), creditsScroll, new Rect(0, 0, 445, 580));
            GUI.Label(new Rect(0, 0, 440, 125), "Regular Dice\n\"Dice\" by macriciox\nCreative Commons Attribution 4.0\nImported, scaled and recolored for gameplay.");
            if (DrawMetalButton(new Rect(0, 130, 440, 42), "Regular dice source")) Application.OpenURL("https://skfb.ly/6xKHM");
            GUI.Label(new Rect(0, 196, 440, 125), "Hot Dice\n\"Dice\" by Geug\nCreative Commons Attribution 4.0\nImported, scaled and aligned for gameplay.");
            if (DrawMetalButton(new Rect(0, 326, 440, 42), "Hot dice source")) Application.OpenURL("https://skfb.ly/6UoEV");
            if (DrawMetalButton(new Rect(0, 385, 440, 42), "CC BY 4.0 License")) Application.OpenURL("https://creativecommons.org/licenses/by/4.0/");
            GUI.Label(new Rect(0, 456, 440, 120), "Prop money generated for iPlay. Overhead pavement is cropped from the approved iPlay environment.\n\nFirst-person hand pack: RRFreelance.\nMotion references are not redistributed.");
            GUI.EndScrollView();
            if (DrawMetalButton(new Rect(x, UiHeight - 102, 470, 44), "Back")) showCredits = false;
            return;
        }
        x = UiWidth / 2 - 450;
        float choices = x + 300;
        if (startupScreen == StartupScreen.GlobalSettings)
        {
            // "Advanced" used to open a screen holding only the Server Address
            // field, which now lives on the Online screen where it's actually
            // used. Nothing left here worth its own button.
            if (DrawMetalButton(new Rect(x, 90, 220, 48), "Back")) ReturnToDieMenu();
            if (DrawMetalButton(new Rect(x + 680, 90, 220, 48), "Exit"))
            {
                ReturnToDieMenu();
                exitConfirmation = true;
            }
        }
        else
        {
            if (DrawMetalButton(new Rect(x, 90, 440, 48), "Play Craps")) { gameMode = GameMode.Craps; StartLocalDemo(); }
            if (DrawMetalButton(new Rect(x + 460, 90, 440, 48), "Play Cee-lo")) { gameMode = GameMode.CeeLo; StartLocalDemo(); }
        }
        GUI.Label(new Rect(x, 218, 270, 36), "Hand Color");
        for (int i = 0; i < 3; i++)
        {
            if (DrawHandPreview(new Rect(choices + i * 200, 152, 190, 146), i))
            { selectedHandSkin = i; ApplyHandSkin(); PlayerPrefs.SetInt("StreetDice.HandSkin", i); PlayerPrefs.Save(); }
            var shadeLabel = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            GUI.Label(new Rect(choices + i * 200, 302, 190, 26), new[] { "White", "Brown", "Black" }[i], shadeLabel);
        }
        GUI.Label(new Rect(x, 350, 270, 36), "Fade Style");
        int fadeChoice = selectedFadeStyle;
        var fadeLabels = new[] { "Tap", "Plant", "Wave" };
        for (int i = 0; i < fadeLabels.Length; i++)
        {
            var option = new Rect(choices + i * 200f, 344f, 190f, 44f);
            if (DrawMetalButton(option, fadeLabels[i])) fadeChoice = i;
            if (i == selectedFadeStyle) DrawSelectedPlateEdge(option);
        }
        if (fadeChoice != selectedFadeStyle)
        {
            selectedFadeStyle = fadeChoice;
            PlayerPrefs.SetInt("StreetDice.FadeStyle", fadeChoice);
            PlayerPrefs.Save();
        }
        GUI.Label(new Rect(x, 426, 270, 36), "Dice Color");
        for (int i = 0; i < 4; i++)
        {
            if (DrawDicePreview(new Rect(choices + i * 150, 404, 110, 76), i))
            { selectedDiceColor = DiceColors[i]; ApplyDiceColor(); PlayerPrefs.SetInt("StreetDice.DiceColor", i); PlayerPrefs.Save(); }
        }
        // Sound moved to the in-game Voice & Sound drawer (Dice & Impact Volume slider,
        // same effectsVolume field) -- Global Settings is reached before a table exists,
        // so a sound slider here was controlling a mix the player couldn't hear yet.
        // Tutorial moved to its own control on the die menu. One row left in this band.
        GUI.Label(new Rect(x, 522, 600, 28), "Offline demo | Play money");
        if (DrawMetalButton(new Rect(x + 650, 512, 250, 44), "Credits")) showCredits = true;
    }

    private void SetTutorialMode(bool enabled)
    {
        if (tutorialMode == enabled) return;
        tutorialMode = enabled;
        PlayerPrefs.SetInt("StreetDice.Tutorial", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetEffectsVolume(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(effectsVolume, value)) return;
        effectsVolume = value;
        PlayerPrefs.SetFloat("StreetDice.Effects", value);
        PlayerPrefs.Save();
    }

    private void SetDiceCalling(bool enabled)
    {
        if (!diceCallingAvailable) return;
        if (diceCalling == enabled) return;
        diceCalling = enabled;
        PlayerPrefs.SetInt("StreetDice.DiceCalling", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static bool DrawColorSwatch(Rect rect, Color color, bool selected, string tooltip)
    {
        bool pressed = GUI.Button(rect, new GUIContent("", tooltip), GUIStyle.none);
        Color previous = GUI.color;
        GUI.color = selected ? Color.white : new Color(0.38f, 0.4f, 0.42f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4), Texture2D.whiteTexture);
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10), Texture2D.whiteTexture);
        GUI.color = previous;
        return pressed;
    }

    private void DrawSkyCamDisplay(float width)
    {
        var frame = klingCatchActive ? new Rect(width / 2 - 129, 6, 258, 258) : new Rect(width / 2 - 174, 6, 348, 178);
        var screen = new Rect(frame.x + 9, frame.y + 9, frame.width - 18, frame.height - 18);
        Color old = GUI.color;
        GUI.color = new Color(0.025f, 0.026f, 0.024f, 0.98f);
        GUI.DrawTexture(frame, Texture2D.whiteTexture);
        GUI.color = new Color(0.34f, 0.36f, 0.35f, 1f);
        GUI.DrawTexture(new Rect(frame.x, frame.y, frame.width, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(frame.x, frame.yMax - 3, frame.width, 3), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(frame.x, frame.y, 3, frame.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(frame.xMax - 3, frame.y, 3, frame.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.DrawTexture(screen, skyTexture, ScaleMode.StretchToFill);
        GUI.color = old;
    }

    private void DrawDoorGhostNumber(string text, float progress, Color color)
    {
        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
        float alpha = Mathf.Lerp(0.14f, 0.68f, t) * color.a;
        float fontSize = Mathf.Lerp(162f, 190f, t) * (text == "COME OUT" ? 0.52f : 1f);
        float centerY = Mathf.Lerp(UiHeight * 0.29f, UiHeight * 0.31f, t);
        var rect = new Rect(35, centerY - 130f, UiWidth - 70, 260f);
        if (doorGraffitiFont == null) doorGraffitiFont = Resources.Load<Font>("UI/SedgwickAveDisplay-Regular");
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            font = doorGraffitiFont != null ? doorGraffitiFont : GUI.skin.label.font,
            fontStyle = FontStyle.Normal,
            fontSize = Mathf.RoundToInt(fontSize),
            clipping = TextClipping.Overflow,
            wordWrap = false
        };
        for (int trail = 3; trail >= 1; trail--)
        {
            float ghostAlpha = alpha * (0.045f + trail * 0.02f);
            style.normal.textColor = new Color(0.66f, 0.88f, 0.94f, ghostAlpha);
            float spread = trail * Mathf.Lerp(1f, 3f, t);
            GUI.Label(new Rect(rect.x - spread, rect.y - spread, rect.width + spread * 2, rect.height + spread * 2), text, style);
        }
        style.normal.textColor = new Color(color.r, color.g, color.b, alpha);
        GUI.Label(rect, text, style);
    }

    private void DrawSeatHud()
    {
        bool interact = GUI.enabled;
        for (int i = 0; i < DemoShooterOrder.Length; i++)
        {
            string id = DemoShooterOrder[i];
            if (!localDemo && !Array.Exists(onlinePlayers, player => player.id == id && !player.hasLeft)) continue;
            bool local = id == SelfId;
            Rect seat = SeatHudRect(id);
            float x = seat.x, y = seat.y;
            string name = local ? "You" : localDemo ? "Opponent " + i :
                Array.Find(onlinePlayers, player => player.id == id)?.name ?? "Opponent " + i;
            GUI.Label(new Rect(x, y, seat.width, 26), name + (id == shooterId ? " | Shooter" : ""));
            if (!IsBotSeat(id))
                GUI.DrawTexture(new Rect(x, y + 26, 28, 28), voiceFaceIcon);
            if (local) GUI.Label(new Rect(x + 30, y + 27, 166, 24), "$" + DisplayAvailableBalance());
        }
        DrawGroundWagerLocks(interact);
        DrawCenteredFade(interact);
    }

    private Rect SeatHudRect(string id)
    {
        int index = Array.IndexOf(DemoShooterOrder, id);
        int selfIndex = Array.IndexOf(DemoShooterOrder, SelfId);
        int relative = index < 0 || selfIndex < 0 ? index :
            (index - selfIndex + DemoShooterOrder.Length) % DemoShooterOrder.Length;
        if (relative == 0) return new Rect(UiWidth * 0.5f - 100f, 25f, 200f, 54f);
        bool left = relative is 1 or 2;
        bool lower = relative is 2 or 4;
        return new Rect(left ? 18f : UiWidth - (lower ? 178f : 218f),
            lower ? UiHeight - 205f : UiHeight * 0.58f - 76f, 200f, 54f);
    }

    private void DrawBettingTimer()
    {
        if (phase != "ComeOut" || !shotCommitted) comeOutCueShown = false;
        if (gameMode != GameMode.Craps || !shotCommitted || rolling || SkyDisplayActive) return;
        double now = Time.unscaledTimeAsDouble;
        double remaining = CurrentBettingDeadline() - now;
        if (remaining <= 0) return;
        if (phase == "ComeOut" && !comeOutCueShown)
        {
            comeOutCueShown = true;
            comeOutCueStarted = Time.unscaledTime;
        }
        float cueAge = Time.unscaledTime - comeOutCueStarted;
        if (phase == "ComeOut" && cueAge < 1.15f)
        {
            float fade = 1f - Mathf.SmoothStep(0.55f, 1.15f, cueAge);
            DrawDoorGhostNumber("COME OUT", Mathf.Clamp01(cueAge / 0.38f), new Color(0.92f, 0.95f, 0.92f, fade));
            return;
        }
        int seconds = CurrentBettingCountdown(now);
        float withinSecond = 1f - Mathf.Clamp01((float)(remaining - (seconds - 1)));
        Color color = seconds <= 3 ? new Color(1f, 0.2f, 0.12f) : new Color(0.92f, 0.95f, 0.92f);
        DrawDoorGhostNumber(seconds.ToString(), withinSecond, color);
    }

    private void DrawHotMeter(Rect rect)
    {
        if (hotFireTexture == null) hotFireTexture = Resources.Load<Texture2D>("UI/hot-meter-fire");
        if (hotMeterFrame == null) hotMeterFrame = MakeHotMeterFrame();
        GUI.DrawTexture(rect, hotMeterFrame);
        float fill = Mathf.Clamp01(streak / HotDiceThreshold);
        if (hotFireTexture == null || fill <= 0f) return;
        const float innerHeight = 91f;
        float fillHeight = innerHeight * fill;
        var previous = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.86f + Mathf.Sin(Time.unscaledTime * 9f) * 0.08f);
        GUI.BeginGroup(new Rect(rect.x + 9, rect.y + 16 + innerHeight - fillHeight, 30, fillHeight));
        float drift = Mathf.Sin(Time.unscaledTime * 1.7f) * 0.025f;
        GUI.DrawTextureWithTexCoords(new Rect(0, fillHeight - innerHeight, 30, innerHeight), hotFireTexture,
            new Rect(0.41f + drift, 0.30f, 0.12f, 0.28f), true);
        GUI.EndGroup();
        GUI.color = previous;
    }

    private static Texture2D MakeHotMeterFrame()
    {
        const int width = 48, height = 124;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float vertical = Mathf.Max(0, Mathf.Abs(y - (height - 1) * 0.5f) - 38f);
            float distance = Mathf.Sqrt(Mathf.Pow(x - (width - 1) * 0.5f, 2) + vertical * vertical);
            pixels[y * width + x] = distance > 23f ? Color.clear
                : distance > 20.5f ? new Color(0.48f, 0.49f, 0.48f, 0.92f)
                : new Color(0.035f, 0.035f, 0.034f, 0.96f);
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private double CurrentBettingDeadline() => shooterId == SelfId ? AcceptanceDeadline : OfferDeadline;

    private int CurrentBettingCountdown(double now)
    {
        int maximum = shooterId == SelfId ? 15 : 10;
        return Mathf.Clamp(Mathf.CeilToInt((float)(CurrentBettingDeadline() - now)), 0, maximum);
    }

    private string PhaseHudLabel()
    {
        if (tutorialMode)
            return gameMode == GameMode.CeeLo ? "CEE-LO" : point == "-" ? "COME OUT" : "POINT " + point;
        return "";
    }

    private void DrawDrawer()
    {
        float width = 345, x = UiWidth - width * drawerAmount, h = UiHeight;
        DrawOpaquePanel(new Rect(x, 0, width, h));
        GUI.Label(new Rect(x + 18, 16, 220, 32), drawerPage);
        if (DrawMetalButton(new Rect(x + width - 56, 12, 40, 36), "X")) drawerOpen = false;
        drawerScroll = GUI.BeginScrollView(new Rect(x + 12, 60, width - 24, h - 138), drawerScroll, new Rect(0, 0, width - 46, drawerPage == "Options" ? 389 : 530));
        if (drawerPage == "Options")
        {
            GUI.Label(new Rect(4, 10, 284, 26), "Throw Style");
            GUI.enabled = !rolling && !shakeHeld;
            int style = snapStyle ? 1 : 0;
            if (DrawMetalButton(new Rect(4, 42, 138, 42), "Classic")) style = 0;
            if (DrawMetalButton(new Rect(150, 42, 138, 42), "Snap")) style = 1;
            DrawSelectedPlateEdge(new Rect(style == 0 ? 4 : 150, 42, 138, 42));
            if ((style == 1) != snapStyle) { snapStyle = style == 1; SelectThrowHand(); PlayerPrefs.SetInt("StreetDice.ThrowStyle", style); }
            GUI.Label(new Rect(4, 100, 284, 26), "Throwing Hand");
            int hand = leftHanded ? 0 : 1;
            if (DrawMetalButton(new Rect(4, 132, 138, 42), "Left")) hand = 0;
            if (DrawMetalButton(new Rect(150, 132, 138, 42), "Right")) hand = 1;
            DrawSelectedPlateEdge(new Rect(hand == 0 ? 4 : 150, 132, 138, 42));
            if ((hand == 0) != leftHanded) { leftHanded = hand == 0; SelectThrowHand(); PlayerPrefs.SetInt("StreetDice.ThrowHand", leftHanded ? 1 : 0); }
            GUI.enabled = true;
            SetTutorialMode(DrawOptionSwitch(new Rect(4, 197, 284, 40), tutorialMode, "Tutorial Mode"));
            if (DrawMetalButton(new Rect(4, 257, 284, 44), "Voice & Sound")) drawerPage = "Voice & Sound";
            if (DrawMetalButton(new Rect(4, 313, 284, 44), "Rules")) drawerPage = "Rules";
        }
        else if (drawerPage == "Voice & Sound")
        {
            GUI.Label(new Rect(4, 4, 284, 65), localDemo ? "Voice is unavailable in the offline demo." :
                voiceClient != null ? voiceClient.Status : "Voice is connecting.");
            GUI.enabled = !localDemo;
            bool mute = DrawOptionSwitch(new Rect(4, 76, 284, 40), micMuted, "Mute Microphone");
            if (mute != micMuted) { micMuted = mute; voiceClient?.SetMuted(micMuted); }
            GUI.enabled = true;
            GUI.Label(new Rect(4, 142, 284, 30), "Dice & Impact Volume");
            SetEffectsVolume(GUI.HorizontalSlider(new Rect(4, 184, 284, 32), effectsVolume, 0, 1));
            GUI.enabled = diceCallingAvailable;
            SetDiceCalling(DrawOptionSwitch(new Rect(4, 235, 284, 40), diceCalling, "Dice Calling",
                diceCallingAvailable ? "Spoken dice results" : "iPlay number recordings are needed"));
            GUI.enabled = true;
            if (DrawMetalButton(new Rect(4, 310, 284, 42), "Back")) drawerPage = "Options";
        }
        else if (drawerPage == "Rules")
        {
            string rules = gameMode == GameMode.Craps
                ? "COME OUT: 7/11 wins. 2/3/12 loses; shooter keeps the dice.\n\nPOINT: Hit your point to win. Only 7 outs.\n\nSELL: Five-second bids. The catcher buys for $1 if nobody bids. That forced buyer gets +1.5 heat and no 2/3/12 until the first point is set.\n\nFADE: Catcher stops a roll; shooter rolls again.\n\nBETS: Lock before the roll. Accepted point bets can request Double or Pair separately. Balances stay private; sale prices are public."
                : "CEE-LO: The banker rolls first. Players roll against the banker.\n\nYour bankroll stays private. The server decides the dice and pays each bet once.";
            GUI.Label(new Rect(4, 4, 284, 365), rules);
            if (DrawMetalButton(new Rect(4, 386, 284, 42), "Back")) drawerPage = "Options";
        }
        GUI.EndScrollView();
        // Always pinned last, outside the scrolling options.
        if (DrawMetalButton(new Rect(x + 16, h - 62, width - 32, 46), "Leave Game")) { confirmLeave = true; CancelShake(); }
    }

    private void DrawLeaveConfirmation()
    {
        float x = UiWidth / 2 - 230, y = UiHeight / 2 - 130;
        DrawOpaquePanel(new Rect(x, y, 460, 260));
        GUI.Label(new Rect(x + 20, y + 18, 420, 36), "Leave game?");
        string warning = shooterId == SelfId && shotCommitted
            ? "Leaving with a live wager counts as a crap. You lose the shot and your active wagers settle before you leave."
            : "Any unsettled wagers you made will be forfeited.";
        GUI.Label(new Rect(x + 20, y + 66, 420, 100), warning);
        if (DrawMetalButton(new Rect(x + 20, y + 191, 196, 46), "Stay")) confirmLeave = false;
        GUI.enabled = !rolling;
        if (DrawMetalButton(new Rect(x + 244, y + 191, 196, 46), "Leave & Forfeit")) LeaveLocalGame();
        GUI.enabled = true;
        if (rolling) GUI.Label(new Rect(x + 20, y + 160, 420, 28), "Finishing the current roll...");
    }

    private int Balance(string player) => cash.TryGetValue(player, out var value) ? value : 1000;

    private int DisplayAvailableBalance()
    {
        if (!localDemo) return onlineAvailableBalance;
        int mainStake = shotCommitted && (SelfId == shooterId || SelfId == catcherId) ? shotAmount : 0;
        return Mathf.Max(0, Balance(SelfId) - mainStake - wagerBook.Exposure(SelfId));
    }

    private void DrawOpaquePanel(Rect rect)
    {
        Color previous = GUI.color;
        if (pregameBackground == null) pregameBackground = Resources.Load<Texture2D>("UI/pregame-dice-background");
        GUI.color = new Color(1f, 1f, 1f, 0.72f);
        GUI.DrawTexture(rect, pregameBackground, ScaleMode.ScaleAndCrop);
        GUI.color = new Color(0.012f, 0.018f, 0.025f, 0.34f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.16f, 0.79f, 0.9f, 0.75f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 2f, rect.height), Texture2D.whiteTexture);
        GUI.color = new Color(0.7f, 0.52f, 0.29f, 0.65f);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static void DrawSelectedPlateEdge(Rect rect)
    {
        Color old = GUI.color;
        GUI.color = new Color(0.11f, 0.82f, 0.96f, 0.95f);
        GUI.DrawTexture(new Rect(rect.x + 12f, rect.yMax - 5f, rect.width - 24f, 3f), Texture2D.whiteTexture);
        GUI.color = old;
    }

    private static Texture2D CreateVoiceFaceIcon()
    {
        var texture = new Texture2D(48, 48, TextureFormat.RGBA32, false);
        for (int y = 0; y < 48; y++) for (int x = 0; x < 48; x++)
        {
            float head = Vector2.Distance(new Vector2(x, y), new Vector2(17, 25));
            bool face = head <= 15 && x <= 27;
            bool nose = x >= 26 && x <= 32 && y >= 25 && y <= 28;
            bool lips = x >= 26 && x <= 32 && (y == 19 || y == 22);
            bool eye = Vector2.Distance(new Vector2(x, y), new Vector2(24, 31)) <= 1.5f;
            float wave = Vector2.Distance(new Vector2(x, y), new Vector2(32, 20));
            bool sound = x >= 35 && ((wave >= 5 && wave <= 6.5f) ||
                (wave >= 10 && wave <= 11.5f) || (wave >= 15 && wave <= 16.5f));
            texture.SetPixel(x, y, eye ? new Color(0.07f, 0.08f, 0.08f) :
                face || nose || lips || sound ? new Color(0.9f, 0.91f, 0.88f) : Color.clear);
        }
        texture.Apply();
        return texture;
    }
    private string FundedCatcher(int amount)
    {
        if (catcherId != shooterId && !(shooterId == localSoldBuyerId && catcherId == localSoldSellerId) &&
            Balance(catcherId) >= amount) return catcherId;
        foreach (var seat in localTurnOrder)
            if (seat != shooterId && !(shooterId == localSoldBuyerId && seat == localSoldSellerId) &&
                Balance(seat) >= amount) return seat;
        return null;
    }

    private bool CanCover(int amount)
    {
        if (amount <= 0 || Balance(shooterId) < amount) return false;
        if (gameMode != GameMode.CeeLo) return FundedCatcher(amount) != null;
        int opponents = 0;
        foreach (var seat in DemoShooterOrder)
            if (seat != shooterId && Balance(seat) >= amount) opponents++;
        return opponents > 0 && Balance(shooterId) >= (long)amount * opponents;
    }
    private void TransferCash(string from, string to, int amount)
    {
        if (amount <= 0 || from == to) return;
        QueueMoneyTransfer(from, to, amount);
        cash[from] = Balance(from) - amount;
        cash[to] = Balance(to) + amount;
    }

    private void CommitShoot()
    {
        if (!localDemo)
        {
            if (shooterId != SelfId || !playerTokens.ContainsKey(SelfId)) return;
            if (string.IsNullOrWhiteSpace(catcherId) || catcherId == shooterId ||
                activeSale is { isOpen: false } sale && sale.winnerId == shooterId && sale.sellerId == catcherId)
            {
                foreach (var player in onlinePlayers)
                    if (!player.hasLeft && player.id != shooterId &&
                        !(activeSale is { isOpen: false } sold && sold.winnerId == shooterId && sold.sellerId == player.id))
                    { catcherId = player.id; break; }
            }
            awaitingShootChoice = false;
            StartCoroutine(OpenShot());
            return;
        }
        if (!CanCover(shotAmount)) { result = "Not enough play money for this shot."; return; }
        catcherId = FundedCatcher(shotAmount);
        awaitingShootChoice = false;
        shotCommitted = true;
        phase = gameMode == GameMode.CeeLo ? "CeeLo" : "ComeOut";
        rollState = RollState.FadeWindow;
        OpenBettingWindow();
        if (gameMode == GameMode.CeeLo && !bankerCeeLoReady)
        {
            ceeLoBanker = shooterId;
            ceeLoSeats.Clear();
            foreach (var seat in DemoShooterOrder)
                if (seat != ceeLoBanker && Balance(seat) >= shotAmount) ceeLoSeats.Add(seat);
            ceeLoSeatIndex = 0;
        }
        nextBotAt = Time.time + (gameMode == GameMode.Craps ? BettingWindowSeconds + 0.25f : 4f);
    }

    private void OpenBettingWindow()
    {
        offerPage.Clear();
        offerSwipeStart.Clear();
        bettingClosesAt = gameMode == GameMode.Craps ? Time.unscaledTime + BettingWindowSeconds : Time.unscaledTime;
        if (gameMode == GameMode.Craps)
            wagerBook.Open(shooterId, int.TryParse(point, out int target) ? target : 0, Time.unscaledTimeAsDouble);
        ResetWagerDraft();
        nextOfferAt = Time.unscaledTime + 1.6f;
    }

    private void SelectMainWager(int amount)
    {
        if (!awaitingShootChoice || shotCommitted || rolling || point != "-" || Array.IndexOf(WagerAmounts, amount) < 0) return;
        shotAmount = amount;
    }

    private void PassLocalDice()
    {
        if (rolling || shotCommitted || point != "-") return;
        if (!localDemo)
        {
            if (shooterId == SelfId && playerTokens.TryGetValue(SelfId, out var token))
                StartCoroutine(Post("/api/street-dice/" + gameId + "/pass",
                    JsonUtility.ToJson(new OnlineWalletRequest { playerId = SelfId, playerSessionToken = token })));
            return;
        }
        CycleDemoShooter();
        awaitingShootChoice = true;
        streak = 0;
        lastResolvedShotWasWin = lastShotWasDoubleUp = false;
        nextBotAt = Time.time + 2f;
    }

    private void AddCashBet(string player, string type)
    {
        if (rolling || !shotCommitted || !BettingWindowOpen || player == shooterId) return;
        int amount = selectedSideWager;
        if (Array.IndexOf(WagerAmounts, amount) < 0) return;
        int exposure = 0;
        foreach (var bet in cashBets) if (!bet.Settled && bet.Player == player) exposure += bet.Amount;
        if (Balance(player) < exposure + amount + (player == catcherId ? shotAmount : 0)) return;
        int shooterExposure = shotAmount;
        foreach (var bet in cashBets) if (!bet.Settled) shooterExposure += bet.Amount;
        if (Balance(shooterId) < shooterExposure + amount) return;
        cashBets.Add(new CashBet { Player = player, Type = type, Amount = amount });
    }

    private void SettleCashBet(CashBet bet, bool won)
    {
        if (bet.Settled) return;
        bet.Settled = true;
        bet.Won = won;
        if (!won) shooterSideWinsThisRoll++;
        TransferCash(won ? shooterId : bet.Player, won ? bet.Player : shooterId, bet.Amount);
    }

    private void ResolveCashSideBets(int total)
    {
        if (wagerBook.Rolling) PayWagers(wagerBook.Resolve(total));
        foreach (var bet in cashBets)
        {
            if (bet.Settled) continue;
            if (phase == "ComeOut") SettleCashBet(bet, bet.Type == "ComeOutWin" ? total == 7 || total == 11 : total == 2 || total == 3 || total == 12);
            else if (int.TryParse(point, out int target))
            {
                if (bet.Type == "HitPoint" && (total == target || total == 7)) SettleCashBet(bet, total == target);
                if (bet.Type == "MissPointGroup" && (total == 7 || IsInPointGroup(total, target))) SettleCashBet(bet, total == 7);
            }
        }
    }

    private void LeaveLocalGame()
    {
        if (rolling) return;
        if (!localDemo)
        {
            voiceClient?.Leave();
            if (playerTokens.TryGetValue(SelfId, out var token))
                StartCoroutine(Post("/api/street-dice/" + gameId + "/leave",
                    JsonUtility.ToJson(new OnlineWalletRequest { playerId = SelfId, playerSessionToken = token }), _ => { }));
            gameId = "";
            playerTokens.Clear();
            localDemo = true;
            realOnlineTable = false;
            mainOptions = true;
            startupScreen = StartupScreen.DieMenu;
            confirmLeave = drawerOpen = false;
            result = "Left online table.";
            return;
        }
        PayWagers(wagerBook.Forfeit("p1"));
        if (gameMode == GameMode.CeeLo && shotCommitted)
        {
            int next = bankerCeeLoReady ? ceeLoSeatIndex : 0;
            if (ceeLoBanker == "p1")
                for (int i = next; i < ceeLoSeats.Count; i++) TransferCash("p1", ceeLoSeats[i], shotAmount);
            else if (ceeLoSeats.IndexOf("p1") >= next) TransferCash("p1", ceeLoBanker, shotAmount);
        }
        else if (shooterId == "p1" && shotCommitted)
        {
            TransferCash(shooterId, catcherId, shotAmount);
            foreach (var bet in cashBets) if (!bet.Settled)
                SettleCashBet(bet, bet.Type == "MissPointGroup" || bet.Type == "ComeOutLoss");
            streak = 0;
            point = "-";
        }
        else
        {
            foreach (var bet in cashBets) if (bet.Player == "p1") SettleCashBet(bet, false);
            if (catcherId == "p1" && shotCommitted) TransferCash("p1", shooterId, shotAmount);
        }
        shotCommitted = false;
        mainOptions = true;
        startupScreen = StartupScreen.DieMenu;
        exitConfirmation = false;
        ClearMoneyTransfers();
        confirmLeave = drawerOpen = false;
        phase = "Lobby";
        handRig.SetActive(false);
        result = "Left game. Final play-money balance: $" + Balance("p1");
        ApplyDiceColor();
        PlayerPrefs.Save();
    }

    private IEnumerator HeartbeatOnlineTable()
    {
        onlineHeartbeatInFlight = true;
        var tableId = gameId;
        var playerId = SelfId;
        var token = playerTokens[playerId];
        using var request = new UnityEngine.Networking.UnityWebRequest(baseUrl + "/api/street-dice/" + tableId + "/presence/heartbeat", "POST");
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(
            JsonUtility.ToJson(new OnlineWalletRequest { playerId = playerId, playerSessionToken = token })));
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        onlineHeartbeatInFlight = false;
        if (request.responseCode != 401 && request.responseCode != 409) yield break;
        gameId = "";
        playerTokens.Clear();
        localDemo = true;
        realOnlineTable = false;
        mainOptions = true;
        startupScreen = StartupScreen.DieMenu;
        result = "Connection expired. Rejoin a new table.";
    }

    private void PlaySurfaceImpacts(float elapsed)
    {
        if (activeReplay == null) return;
        float release = replayIsLocal ? FirstPersonDiceHand.ReleaseTime : 0;
        while (nextImpact < activeReplay.Impacts.Count && activeReplay.Impacts[nextImpact].Time <= elapsed - release)
        {
            var impact = activeReplay.Impacts[nextImpact++];
            if (impact.Surface == "Metal") PlayAudio(metalClip, Mathf.Lerp(0.28f, 1f, impact.Strength));
            else if (impact.Surface == "Brick") TriggerBrickHaptic();
            else PlayAudio(rollClip, Mathf.Lerp(0.25f, 0.75f, impact.Strength));
        }
    }

    private void TriggerBrickHaptic()
    {
        if (Time.unscaledTime - lastBrickHapticAt < 0.12f) return;
        lastBrickHapticAt = Time.unscaledTime;
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

    private void ResolveCeeLoTable(int a, int b, int c)
    {
        var roll = EvaluateLocalCeeLo(a, b, c);
        result = roll.Message;
        tutorialDetail = roll.Message;
        if (roll.Outcome == "Reroll") { rollState = RollState.FadeWindow; return; }
        if (!bankerCeeLoReady)
        {
            if (roll.Outcome == "AutomaticWin" || roll.Outcome == "AutomaticLoss")
            {
                bool wins = roll.Outcome == "AutomaticWin";
                foreach (var seat in ceeLoSeats) TransferCash(wins ? seat : ceeLoBanker, wins ? ceeLoBanker : seat, shotAmount);
                FinishCeeLoRound(wins);
                return;
            }
            bankerCeeLoReady = true;
            bankerCeeLoRank = roll.Rank;
            bankerCeeLo = "Banker point " + roll.Point;
            shooterId = ceeLoSeats[0];
            catcherId = ceeLoBanker;
            rollState = RollState.FadeWindow;
            nextBotAt = Time.time + 3;
            return;
        }
        int comparison = roll.Rank.CompareTo(bankerCeeLoRank);
        if (comparison != 0) TransferCash(comparison > 0 ? ceeLoBanker : shooterId, comparison > 0 ? shooterId : ceeLoBanker, shotAmount);
        ceeLoSeatIndex++;
        if (ceeLoSeatIndex >= ceeLoSeats.Count) { FinishCeeLoRound(false); return; }
        shooterId = ceeLoSeats[ceeLoSeatIndex];
        rollState = RollState.FadeWindow;
        nextBotAt = Time.time + 3;
    }

    private void FinishCeeLoRound(bool bankerWin)
    {
        shooterId = ceeLoBanker;
        catcherId = shooterId == "p1" ? "p2" : "p1";
        bankerCeeLoReady = false;
        shotCommitted = false;
        awaitingShootChoice = true;
        rollState = RollState.WaitingForShot;
        if (bankerWin) streak++; else streak = 0;
        ApplyDiceColor();
    }

    private void CreateGroundMoney()
    {
        foreach (int denomination in WagerAmounts)
        {
            var texture = Resources.Load<Texture2D>(denomination == 20 ? "Money/iplay-prop-note" : "Money/iplay-note-" + denomination);
            if (texture == null) { Debug.LogError("Missing bill texture: " + denomination); continue; }
            var material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = texture;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 16;
            texture.mipMapBias = -0.65f;
            material.color = Color.white;
            billMaterials[denomination] = material;
        }
        var positions = new[] { new Vector3(-0.75f, 0, -2.15f), new Vector3(-1.5f, 0, -0.6f),
            new Vector3(-1.7f, 0, -1.75f), new Vector3(1.5f, 0, -0.6f),
            new Vector3(1.7f, 0, -1.75f) };
        for (int pile = 0; pile < positions.Length; pile++)
        {
            var root = new GameObject("Play money stack " + pile);
            root.transform.position = positions[pile] + Vector3.up * (DiceRestY - DiceWorldScale * 0.5f + 0.003f);
            moneyPiles.Add(root);
            var shadow = CreateDieShadow("Cash contact shadow");
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = -Vector3.up * 0.002f;
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadow.transform.localScale = new Vector3(0.48f, 0.22f, 1);
        }
        RefreshGroundMoney();
    }

    private static List<BillGroup> BillGroupsForAmount(int amount)
    {
        var bills = new List<BillGroup>();
        for (int i = WagerAmounts.Length - 1; i >= 0; i--)
        {
            int count = amount / WagerAmounts[i];
            if (count <= 0) continue;
            for (int note = 0; note < count; note++) bills.Add(new BillGroup(WagerAmounts[i], 1));
            amount -= WagerAmounts[i] * count;
        }
        return bills;
    }

    private int GroundWager(string player)
    {
        if (mainOptions) return 36;
        int amount = AcceptedWagerStake(player) + OfferedWagerStake(player);
        if (gameMode == GameMode.CeeLo && shotCommitted)
        {
            int next = bankerCeeLoReady ? ceeLoSeatIndex : 0;
            if (player == ceeLoBanker) amount = shotAmount * (ceeLoSeats.Count - next);
            else if (ceeLoSeats.IndexOf(player) >= next) amount = shotAmount;
        }
        else if ((shotCommitted || awaitingShootChoice) && (player == shooterId || player == catcherId)) amount += shotAmount;
        foreach (var bet in cashBets)
            if (!bet.Settled && (player == bet.Player || player == shooterId)) amount += bet.Amount;
        return amount;
    }

    private void RefreshGroundMoney()
    {
        PlaceOpponentMoney();
        for (int pile = 0; pile < moneyPiles.Count; pile++)
        {
            int amount = GroundWager(DemoShooterOrder[pile]);
            if (displayedMoney.TryGetValue(pile, out int previous) && previous == amount) continue;
            displayedMoney[pile] = amount;
            var root = moneyPiles[pile];
            root.SetActive(amount > 0);
            var bills = BillGroupsForAmount(amount);
            // Child zero is the contact shadow; grow the reusable paper pool only when needed.
            for (int note = root.transform.childCount - 1; note < bills.Count; note++)
            {
                var paper = new GameObject("Worn folded prop note");
                paper.transform.SetParent(root.transform, false);
                paper.transform.localPosition = new Vector3(note * 0.055f, note * 0.0015f, note * 0.025f);
                paper.transform.localRotation = Quaternion.Euler(0, (pile % 2 == 0 ? -12f : 12f) + (note % 5 - 2) * 3f, 0);
                var vertices = new Vector3[18];
                var uv = new Vector2[18];
                var indices = new List<int>();
                for (int i = 0; i < 9; i++) for (int j = 0; j < 2; j++)
                {
                    int n = i * 2 + j;
                    float u = i / 8f;
                    vertices[n] = new Vector3((u - 0.5f) * 0.46f, Mathf.Abs(u - 0.5f) * 0.012f + Mathf.Sin(u * 9 + note) * 0.002f, (j - 0.5f) * 0.196f);
                    uv[n] = new Vector2(u, j);
                    if (i < 8 && j == 0) indices.AddRange(new[] { n, n + 1, n + 2, n + 2, n + 1, n + 3 });
                }
                var mesh = new Mesh { vertices = vertices, uv = uv, triangles = indices.ToArray() };
                mesh.RecalculateNormals();
                paper.AddComponent<MeshFilter>().sharedMesh = mesh;
                paper.AddComponent<MeshRenderer>();
            }
            for (int note = 0; note < root.transform.childCount - 1; note++)
            {
                var paper = root.transform.GetChild(note + 1).gameObject;
                paper.SetActive(note < bills.Count);
                if (note >= bills.Count) continue;
                var group = bills[note];
                paper.name = "$" + group.Denomination + " iPlay prop note";
                paper.GetComponent<MeshRenderer>().sharedMaterial = billMaterials[group.Denomination];
            }
        }
    }

    private void PlaceOpponentMoney()
    {
        if (moneyPiles.Count != DemoShooterOrder.Length || Camera.main == null) return;
        float groundY = DiceRestY - DiceWorldScale * 0.5f + 0.003f;
        var ground = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        for (int i = 1; i < moneyPiles.Count; i++)
        {
            var ray = Camera.main.ViewportPointToRay(OpponentMoneyViewports[i - 1]);
            if (ground.Raycast(ray, out float distance)) moneyPiles[i].transform.position = ray.GetPoint(distance);
        }
    }

    private void QueueMoneyTransfer(string from, string to, int amount)
    {
        if (!moneyAnimationEnabled || mainOptions || confirmLeave || moneyPiles.Count != DemoShooterOrder.Length) return;
        if (Array.IndexOf(DemoShooterOrder, from) < 0 || Array.IndexOf(DemoShooterOrder, to) < 0) return;
        moneyTransfers.Enqueue(new MoneyTransfer(from, to, amount));
        if (!moneyTransferRunning) moneyTransferRoutine = StartCoroutine(PlayMoneyTransfers());
    }

    private void ClearMoneyTransfers()
    {
        if (moneyTransferRoutine != null) StopCoroutine(moneyTransferRoutine);
        moneyTransferRoutine = null;
        moneyTransferRunning = false;
        moneyTransfers.Clear();
        foreach (var bill in activeMoneyBills)
            if (bill != null) { bill.SetActive(false); Destroy(bill); }
        activeMoneyBills.Clear();
    }

    private IEnumerator PlayMoneyTransfers()
    {
        moneyTransferRunning = true;
        while (moneyTransfers.Count > 0) yield return AnimateMoneyTransfer(moneyTransfers.Dequeue());
        moneyTransferRunning = false;
        moneyTransferRoutine = null;
    }

    private IEnumerator AnimateMoneyTransfer(MoneyTransfer transfer)
    {
        int fromIndex = Array.IndexOf(DemoShooterOrder, transfer.From);
        int toIndex = Array.IndexOf(DemoShooterOrder, transfer.To);
        if (fromIndex < 0 || toIndex < 0) yield break;
        var groups = BillGroupsForAmount(transfer.Amount);
        var flying = new List<GameObject>();
        var starts = new List<Vector3>();
        var ends = new List<Vector3>();
        for (int i = 0; i < groups.Count; i++)
        {
            var template = moneyPiles[0].transform.childCount > 1 ? moneyPiles[0].transform.GetChild(1).gameObject : null;
            if (template == null) yield break;
            var group = groups[i];
            var bill = Instantiate(template);
            activeMoneyBills.Add(bill);
            bill.name = "Settling $" + group.Denomination;
            bill.transform.SetParent(null, true);
            bill.SetActive(true);
            bill.GetComponent<MeshRenderer>().sharedMaterial = billMaterials[group.Denomination];
            int row = i / 5;
            int rowStart = row * 5;
            int rowCount = Mathf.Min(5, groups.Count - rowStart);
            float column = i - rowStart - (rowCount - 1) * 0.5f;
            Vector3 offset = new Vector3(column * 0.09f, i * 0.001f, row * 0.035f);
            starts.Add(moneyPiles[fromIndex].transform.position + Vector3.up * 0.035f + offset);
            ends.Add(moneyPiles[toIndex].transform.position + Vector3.up * 0.04f + offset);
            flying.Add(bill);
        }
        PlayAudio(moneyPullClip, 0.85f);
        float started = Time.unscaledTime;
        while (Time.unscaledTime - started < Mathf.Max(0.35f, moneyTransferDuration))
        {
            float duration = Mathf.Max(0.35f, moneyTransferDuration);
            float t = Mathf.Clamp01((Time.unscaledTime - started) / duration);
            float travel = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - 0.16f) / 0.84f));
            for (int i = 0; i < flying.Count; i++)
            {
                float arc = Mathf.Sin(travel * Mathf.PI) * 0.42f + Mathf.SmoothStep(0, 0.16f, Mathf.Min(t / 0.16f, 1));
                Vector3 position = Vector3.Lerp(starts[i], ends[i], travel) + Vector3.up * arc;
                flying[i].transform.position = position;
                Quaternion ground = Quaternion.Euler(0, fromIndex * 29 + i * 11, 0);
                Vector3 towardCamera = Camera.main == null ? Vector3.up : (Camera.main.transform.position - position).normalized;
                Quaternion readable = Quaternion.FromToRotation(Vector3.up, towardCamera);
                flying[i].transform.rotation = Quaternion.Slerp(ground, readable, Mathf.Sin(t * Mathf.PI) * 0.9f);
                flying[i].transform.localScale = Vector3.one * Mathf.Lerp(1f, 1.28f, Mathf.Sin(t * Mathf.PI));
            }
            yield return null;
        }
        PlayAudio(moneyLandClip, 0.72f);
        yield return new WaitForSecondsRealtime(0.16f);
        foreach (var bill in flying)
        {
            activeMoneyBills.Remove(bill);
            Destroy(bill);
        }
    }

    private static AudioClip CreateMoneyClip(string name, float duration, bool landing)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        var samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = landing ? Mathf.Exp(-t * 28f) : Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
            float paper = HashSigned(i, landing ? 73 : 59, 17) * (landing ? 0.28f : 0.12f);
            float scrape = Mathf.Sin(2f * Mathf.PI * (landing ? 145f : 82f) * t) * (landing ? 0.05f : 0.025f);
            samples[i] = (paper + scrape) * envelope;
        }
        var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (pregameSkin != null) Destroy(pregameSkin);
        if (pregameThumbTexture != null) Destroy(pregameThumbTexture);
        if (voiceFaceIcon != null) Destroy(voiceFaceIcon);
        if (wagerDieIcon != null) Destroy(wagerDieIcon);
        ClearMoneyTransfers();
        if (handPreviews != null) foreach (var preview in handPreviews) if (preview != null) { preview.Release(); Destroy(preview); }
        if (dicePreviews != null) foreach (var preview in dicePreviews) if (preview != null) { preview.Release(); Destroy(preview); }
        if (skyTexture != null) { skyTexture.Release(); Destroy(skyTexture); }
    }
}
