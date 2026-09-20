using System;
using System.Collections.Generic;
using IPlay.Demo;
using UnityEngine;

public sealed partial class StreetDiceGreyboxController
{
    private string wagerTarget = "";
    private int wagerStage, draftNumber, draftAmount;
    private WagerOutcome draftOutcome;
    private float wagerStageAt, nextOfferAt;
    private readonly Dictionary<string, int> offerPage = new Dictionary<string, int>();
    private readonly Dictionary<string, Vector2> offerSwipeStart = new Dictionary<string, Vector2>();
    private Texture2D fadeDisc;
    private Texture2D wagerDieIcon;
    private Texture2D impactBetIcon, blankBetDie, betBackArrow;
    private Font digitalBetFont;
    private readonly Dictionary<int, Texture2D> betBills = new Dictionary<int, Texture2D>();
    private bool wagerOverlayOpen;
    private bool addOnOverlayMode;
    private int selectedAddOnSourceId;
    private int armedOfferId;
    private float armedOfferAt;
    private readonly Dictionary<int, GameObject> groundAmountDice = new Dictionary<int, GameObject>();
    private Material groundAmountBodyMaterial, groundAmountFaceMaterial;
    private RenderTexture wagerOpenIcon, wagerClosedIcon;

    private void ResetWagerDraft()
    {
        wagerTarget = "";
        wagerStage = draftAmount = draftNumber = 0;
        selectedAddOnSourceId = 0;
        addOnOverlayMode = false;
    }

    private void SetWagerStage(int stage)
    {
        wagerStage = stage;
        wagerStageAt = Time.unscaledTime;
        PlayAudio(rollClip, 0.16f);
    }

    private static string WagerLabel(WagerOutcome outcome, int number) => outcome == WagerOutcome.Crap
        ? number == 0 ? "CRAP\n2/3/12" : "CRAP\n" + number
        : "HIT\n" + number;

    private void DrawBettingToggle()
    {
        if (impactBetIcon == null) impactBetIcon = Resources.Load<Texture2D>("UI/impact-dice-bet-v1");
        var die = new Rect(9, 5, 79, 73);
        bool available = (BettingWindowOpen || CanRequestPointAddOn) && !rolling;
        GUI.enabled = available && !drawerOpen;
        if (GUI.Button(die, new GUIContent("", "Toggle betting overlay"), GUIStyle.none))
        {
            wagerOverlayOpen = !wagerOverlayOpen;
            ResetWagerDraft();
            if (wagerOverlayOpen) { wagerTarget = shooterId; SetWagerStage(1); }
            PlayAudio(rollClip, 0.16f);
        }
        GUI.enabled = !drawerOpen;
        var previous = GUI.color;
        GUI.color = available ? Color.white : new Color(1, 1, 1, 0.45f);
        GUI.DrawTexture(die, impactBetIcon, ScaleMode.ScaleToFit, true);
        var spray = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 23,
            fontStyle = FontStyle.Bold };
        spray.normal.textColor = new Color(0.93f, 0.16f, 0.11f);
        GUI.Label(new Rect(15, 71, 69, 33), "BET", spray);
        GUI.color = previous;
    }

    private void DrawBettingOverlay()
    {
        if ((!BettingWindowOpen && !CanRequestPointAddOn) || rolling || gameMode != GameMode.Craps)
        {
            wagerOverlayOpen = false;
            ResetWagerDraft();
            return;
        }
        bool interact = GUI.enabled && !onlineWagerRequestInFlight;
        foreach (string id in DemoShooterOrder)
        {
            if (id == SelfId || (!localDemo && !Array.Exists(onlinePlayers, p => p.id == id && !p.hasLeft))) continue;
            DrawOpponentBetDice(id, SeatHudRect(id), interact);
        }
        if (wagerStage == 3) DrawWagerBillSelection(interact);
        GUI.enabled = interact;
    }

    private void DrawOpponentBetDice(string id, Rect seat, bool interact)
    {
        const float size = 62f, gap = 68f;
        bool selected = wagerTarget == id;
        float x = seat.x, y = seat.y + 53f;
        int stage = selected ? wagerStage : 1;
        int pointNumber = localDemo ? wagerBook.Point : int.TryParse(point, out int currentPoint) ? currentPoint : 0;
        GUI.enabled = interact;
        if (DrawDigitalBetDie(new Rect(x, y, size, size), "", true))
        {
            if (selected && stage > 1) SetWagerStage(stage - 1);
            else { wagerOverlayOpen = false; ResetWagerDraft(); }
        }
        if (stage == 1)
        {
            GUI.enabled = interact && BettingWindowOpen && id != shooterId && catcherId != SelfId && pointNumber != 0;
            if (DrawDigitalBetDie(new Rect(x + gap, y, size, size), "BET"))
            { wagerTarget = id; draftOutcome = WagerOutcome.Hit; SetWagerStage(2); }
            GUI.enabled = interact && id != catcherId && shooterId != SelfId &&
                (BettingWindowOpen || (id == shooterId && CanRequestPointAddOn));
            if (DrawDigitalBetDie(new Rect(x + 2 * gap, y, size, size), "CRAP"))
            { wagerTarget = id; draftOutcome = WagerOutcome.Crap; SetWagerStage(2); }
        }
        else if (stage == 2)
        {
            GUI.enabled = interact && BettingWindowOpen;
            if (DrawDigitalBetDie(new Rect(x + gap, y, size, size),
                pointNumber == 0 ? "2/3/12" : pointNumber.ToString()))
            { draftNumber = pointNumber; SetWagerStage(3); }
            int paired = WagerBook.GroupMate(pointNumber);
            GUI.enabled = interact && paired != 0 &&
                (BettingWindowOpen || EligiblePairedSource(paired) != null);
            if (DrawDigitalBetDie(new Rect(x + 2 * gap, y, size, size),
                paired == 0 ? "--" : paired.ToString()))
            { draftNumber = paired; SetWagerStage(3); }
        }
        GUI.enabled = interact;
    }

    private WagerOffer EligiblePairedSource(int number)
    {
        foreach (var offer in ActiveWagerOffers)
            if (offer.Status == WagerStatus.Accepted && offer.SourceOfferId == 0 &&
                offer.From == SelfId && offer.To == shooterId && offer.Outcome == WagerOutcome.Crap &&
                WagerBook.GroupMate(offer.Number) == number &&
                !HasLiveAddOn(offer.Id, WagerAddOnKind.PairedNumber)) return offer;
        return null;
    }

    private void DrawWagerBillSelection(bool interact)
    {
        float center = UiWidth * 0.5f;
        float lockY = UiHeight * 0.38f;
        GUI.enabled = false;
        DrawAmountLock(new Rect(center - 42f, lockY, 84f, 99f), 0, false, -1f,
            draftOutcome, draftNumber);
        float billWidth = 124f, gap = 29f;
        float left = center - (billWidth * WagerAmounts.Length + gap * (WagerAmounts.Length - 1)) * 0.5f;
        for (int i = 0; i < WagerAmounts.Length; i++)
        {
            int amount = WagerAmounts[i];
            GUI.enabled = interact && WagerFunds(SelfId) - ActiveWagerExposure(SelfId) >= amount;
            if (DrawBetBill(new Rect(left + i * (billWidth + gap), lockY + 128f, billWidth, 82f), amount))
            {
                if (BettingWindowOpen)
                    OfferWager(SelfId, wagerTarget, draftOutcome, draftNumber, amount);
                else
                {
                    var source = EligiblePairedSource(draftNumber);
                    if (source != null) OfferPointAddOnWithAmount(source, WagerAddOnKind.PairedNumber, amount);
                }
                draftAmount = draftNumber = 0;
                SetWagerStage(1);
            }
        }
        GUI.enabled = interact;
    }

    private bool HasLiveAddOn(int sourceId, WagerAddOnKind kind)
    {
        foreach (var offer in ActiveWagerOffers)
            if (offer.SourceOfferId == sourceId && offer.AddOnKind == kind &&
                (offer.Status == WagerStatus.Offered || offer.Status == WagerStatus.Accepted)) return true;
        return false;
    }

    private string FirstWagerTarget()
    {
        foreach (string id in DemoShooterOrder)
            if (id != SelfId && CanComposeWagerAgainst(id) &&
                (localDemo || Array.Exists(onlinePlayers, player => player.id == id && !player.hasLeft))) return id;
        return "";
    }

    private bool CanComposeWagerAgainst(string id) => id != SelfId &&
        ((BettingWindowOpen && (shooterId == SelfId || catcherId != SelfId || id == shooterId)) ||
            (!BettingWindowOpen && id == shooterId && CanRequestPointAddOn));

    private string WagerTargetName(string id) => localDemo ? "Opponent " + Array.IndexOf(DemoShooterOrder, id) :
        Array.Find(onlinePlayers, p => p.id == id)?.name ?? id;

    private bool DrawDigitalBetDie(Rect rect, string label, bool back = false)
    {
        if (blankBetDie == null) blankBetDie = Resources.Load<Texture2D>("UI/digital-display-die-v1");
        bool pressed = GUI.Button(rect, new GUIContent("", back ? "Back" : label), GUIStyle.none);
        var previous = GUI.color;
        GUI.color = GUI.enabled ? Color.white : new Color(1, 1, 1, 0.4f);
        GUI.DrawTexture(rect, blankBetDie, ScaleMode.ScaleToFit, true);
        if (back)
        {
            if (betBackArrow == null) betBackArrow = MakeBetBackArrow();
            GUI.DrawTexture(new Rect(rect.x + rect.width * 0.2f, rect.y + rect.height * 0.19f,
                    rect.width * 0.6f, rect.height * 0.62f),
                betBackArrow, ScaleMode.ScaleToFit, true);
        }
        else
        {
            if (digitalBetFont == null) digitalBetFont = Resources.Load<Font>("UI/BarlowCondensed-SemiBold");
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
                font = digitalBetFont, fontStyle = FontStyle.Bold,
                fontSize = Mathf.RoundToInt(rect.width * (label.Length > 5 ? 0.16f : label.Length > 3 ? 0.25f : 0.34f)) };
            style.normal.textColor = new Color(0.15f, 0.88f, 1f, GUI.enabled ? 0.36f : 0.1f);
            GUI.Label(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), label, style);
            style.normal.textColor = new Color(0.7f, 0.97f, 1f, GUI.enabled ? 1f : 0.45f);
            GUI.Label(rect, label, style);
        }
        GUI.color = previous;
        return pressed;
    }

    private static Texture2D MakeBetBackArrow()
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "iPlay digital back arrow" };
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            var p = new Vector2(x, y);
            float distance = Mathf.Min(DistanceToBetSegment(p, new Vector2(14, 32), new Vector2(52, 32)),
                Mathf.Min(DistanceToBetSegment(p, new Vector2(14, 32), new Vector2(34, 13)),
                    DistanceToBetSegment(p, new Vector2(14, 32), new Vector2(34, 51))));
            float alpha = Mathf.Clamp01((4.5f - distance) / 2.5f);
            texture.SetPixel(x, y, new Color(0.55f, 0.94f, 1f, alpha));
        }
        texture.Apply(false, true);
        return texture;
    }

    private static float DistanceToBetSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 line = b - a;
        return Vector2.Distance(point, a + line * Mathf.Clamp01(Vector2.Dot(point - a, line) / line.sqrMagnitude));
    }

    private bool DrawBetBill(Rect rect, int amount)
    {
        if (!betBills.TryGetValue(amount, out Texture2D bill))
        {
            bill = Resources.Load<Texture2D>(amount == 20 ? "Money/iplay-prop-note" : "Money/iplay-note-" + amount);
            betBills[amount] = bill;
        }
        bool pressed = GUI.Button(rect, new GUIContent("", "$" + amount), GUIStyle.none);
        var previous = GUI.color;
        GUI.color = GUI.enabled ? Color.white : new Color(1, 1, 1, 0.4f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, rect.height - 22), bill, ScaleMode.ScaleToFit, true);
        GUI.color = previous;
        var label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            fontSize = 18, fontStyle = FontStyle.Bold };
        GUI.Label(new Rect(rect.x, rect.y + rect.height - 22, rect.width, 22), "$" + amount, label);
        return pressed;
    }

    private void DrawWagerSeat(string id, float x, float y, bool interact)
    {
        if (gameMode != GameMode.Craps) return;
        EnsureWagerIcons();
        EnsureWagerDieIcon();
        bool composing = wagerTarget == id && BettingWindowOpen;
        bool canCompose = BettingWindowOpen && shooterId != SelfId &&
            (catcherId != SelfId || id == shooterId);
        GUI.enabled = interact && canCompose;
        bool tapped = GUI.Button(new Rect(x, y + 56, 48, 48),
            new GUIContent("", "Open bets for " + id), GUIStyle.none);
        Color previous = GUI.color;
        GUI.color = canCompose ? Color.white : new Color(1, 1, 1, 0.5f);
        GUI.DrawTexture(new Rect(x - 4, y + 52, 56, 56), wagerDieIcon, ScaleMode.ScaleToFit, true);
        GUI.color = previous;
        if (canCompose && tapped)
        {
            if (composing) ResetWagerDraft();
            else { ResetWagerDraft(); wagerTarget = id; SetWagerStage(1); }
        }
        GUI.enabled = interact;
        if (composing) DrawWagerComposer(id, x + 54, y + 56, interact);
    }

    private void DrawCenteredFade(bool interact)
    {
        if (gameMode != GameMode.Craps || catcherId != SelfId || !shotCommitted) return;
        if (!localDemo && (pendingRemoteFadeSeconds <= 0f || remoteReplayInProgress)) return;
        GUI.enabled = interact && rolling && !fadeInProgress && rollState != RollState.Locked && rollState != RollState.Resolving;
        if (DrawFadeButton(new Rect(UiWidth * 0.5f + 82, UiHeight - 152, 70, 70))) StartCoroutine(Fade());
        GUI.enabled = interact;
    }

    private Vector2 GroundWagerAnchor(string player)
    {
        int index = Array.IndexOf(DemoShooterOrder, player);
        if (index < 0 || index >= moneyPiles.Count) return new Vector2(UiWidth * 0.5f, UiHeight * 0.7f);
        Vector3 screen = Camera.main.WorldToScreenPoint(moneyPiles[index].transform.position);
        float x = (screen.x - UiOrigin.x) / UiScale;
        float y = (Screen.height - screen.y - UiOrigin.y) / UiScale;
        return new Vector2(Mathf.Clamp(x, 70, UiWidth - 70),
            Mathf.Clamp(y, 200, UiHeight - 45));
    }

    private void UpdateGroundOfferBills()
    {
        if (moneyPiles.Count != DemoShooterOrder.Length) return;
        var live = new HashSet<int>();
        var totals = new Dictionary<string, int>();
        foreach (var offer in gameMode == GameMode.Craps ? ActiveWagerOffers : Array.Empty<WagerOffer>())
        {
            if (offer.Status != WagerStatus.Offered && offer.Status != WagerStatus.Accepted) continue;
            totals[offer.From] = totals.TryGetValue(offer.From, out int count) ? count + 1 : 1;
        }
        var perRecipient = new Dictionary<string, int>();
        foreach (var offer in gameMode == GameMode.Craps ? ActiveWagerOffers : Array.Empty<WagerOffer>())
        {
            if (offer.Status != WagerStatus.Offered && offer.Status != WagerStatus.Accepted) continue;
            int seat = Array.IndexOf(DemoShooterOrder, offer.From);
            if (seat < 0) continue;
            int index = perRecipient.TryGetValue(offer.From, out int count) ? count : 0;
            perRecipient[offer.From] = index + 1;
            live.Add(offer.Id);
            string key = "ground:" + offer.From;
            int page = offerPage.TryGetValue(key, out int saved)
                ? Mathf.Clamp(saved, 0, totals[offer.From] - 1) : 0;
            if (index != page)
            {
                if (groundAmountDice.TryGetValue(offer.Id, out GameObject hiddenDie)) hiddenDie.SetActive(false);
                continue;
            }
            if (!groundAmountDice.TryGetValue(offer.Id, out GameObject amountDie))
            {
                amountDie = MakeGroundAmountDie(offer.Amount);
                groundAmountDice[offer.Id] = amountDie;
            }
            amountDie.SetActive(true);
            amountDie.transform.position = GroundOfferDiePosition(seat);
            if (Camera.main != null)
            {
                Vector3 towardCamera = (Camera.main.transform.position - amountDie.transform.position).normalized;
                amountDie.transform.rotation = Quaternion.LookRotation(
                    -towardCamera, Vector3.up) *
                    Quaternion.Euler(0f, 15f, 0f);
                Transform face = amountDie.transform.Find("Upright digital amount face");
                face.position = amountDie.transform.position + towardCamera * 0.142f;
                face.rotation = Quaternion.LookRotation(towardCamera, Vector3.up);
                Transform amountText = amountDie.transform.Find("Digital amount $" + offer.Amount);
                amountText.position = amountDie.transform.position + towardCamera * 0.147f;
                amountText.rotation = Quaternion.LookRotation(-towardCamera, Vector3.up);
            }
        }
        var stale = new List<int>();
        foreach (var pair in groundAmountDice)
            if (!live.Contains(pair.Key)) { Destroy(pair.Value); stale.Add(pair.Key); }
        foreach (int id in stale)
            groundAmountDice.Remove(id);
    }

    private Vector3 GroundOfferDiePosition(int seat)
    {
        Vector3 pile = moneyPiles[seat].transform.position;
        float outward = pile.x < 0f ? -1f : 1f;
        return pile + new Vector3(outward * 0.20f, 0.125f, -0.02f);
    }

    private void ClearGroundOfferVisuals()
    {
        foreach (var die in groundAmountDice.Values) Destroy(die);
        groundAmountDice.Clear();
    }

    private GameObject MakeGroundAmountDie(int amount)
    {
        if (groundAmountBodyMaterial == null)
        {
            groundAmountBodyMaterial = new Material(Shader.Find("Unlit/Color"));
            groundAmountBodyMaterial.color = new Color(0.025f, 0.09f, 0.14f);
        }
        if (groundAmountFaceMaterial == null)
        {
            groundAmountFaceMaterial = new Material(Shader.Find("Sprites/Default"));
            groundAmountFaceMaterial.mainTexture = Resources.Load<Texture2D>("UI/digital-display-die-v1");
        }
        var root = new GameObject("Ground digital amount die $" + amount);
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Low digital die body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        body.GetComponent<Collider>().enabled = false;
        body.GetComponent<MeshRenderer>().sharedMaterial = groundAmountBodyMaterial;
        var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = "Upright digital amount face";
        face.transform.SetParent(root.transform, false);
        face.transform.localPosition = new Vector3(0f, 0f, -0.13f);
        face.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        face.transform.localScale = new Vector3(0.205f, 0.205f, 1f);
        var faceMesh = Instantiate(face.GetComponent<MeshFilter>().sharedMesh);
        var uv = faceMesh.uv;
        for (int i = 0; i < uv.Length; i++)
            uv[i] = new Vector2(Mathf.Lerp(0.18f, 0.82f, uv[i].x), Mathf.Lerp(0.20f, 0.82f, uv[i].y));
        faceMesh.uv = uv;
        face.GetComponent<MeshFilter>().sharedMesh = faceMesh;
        face.GetComponent<Collider>().enabled = false;
        face.GetComponent<MeshRenderer>().sharedMaterial = groundAmountFaceMaterial;
        if (digitalBetFont == null) digitalBetFont = Resources.Load<Font>("UI/BarlowCondensed-SemiBold");
        var textObject = new GameObject("Digital amount $" + amount);
        textObject.transform.SetParent(root.transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, -0.135f);
        var label = textObject.AddComponent<TextMesh>();
        label.text = "$" + amount;
        label.font = digitalBetFont;
        label.fontSize = 96;
        label.characterSize = 0.012f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = new Color(0.72f, 0.97f, 1f);
        textObject.GetComponent<MeshRenderer>().sharedMaterial = digitalBetFont.material;
        return root;
    }

    private Rect GroundLockRect(WagerOffer offer)
    {
        Vector2 anchor = GroundWagerAnchor(offer.From);
        return new Rect(anchor.x - 32f, anchor.y - 92f, 65f, 76f);
    }

    private Rect GroundOfferPagerRect(string recipient)
    {
        Vector2 anchor = GroundWagerAnchor(recipient);
        return new Rect(anchor.x - 39f, anchor.y - 119f, 79f, 22f);
    }

    private void DrawGroundWagerLocks(bool interact)
    {
        if (gameMode != GameMode.Craps || rolling || mainOptions) return;
        foreach (string sender in DemoShooterOrder)
        {
            var offers = new List<WagerOffer>();
            foreach (var candidate in ActiveWagerOffers)
                if (candidate.From == sender &&
                    (candidate.Status == WagerStatus.Offered || candidate.Status == WagerStatus.Accepted)) offers.Add(candidate);
            if (offers.Count == 0) continue;
            string key = "ground:" + sender;
            int pageCount = offers.Count;
            int page = offerPage.TryGetValue(key, out int saved) ? Mathf.Clamp(saved, 0, pageCount - 1) : 0;
                var offer = offers[page];
                bool accepted = offer.Status == WagerStatus.Accepted;
                Rect lockBase = GroundLockRect(offer);
                float lockAge = accepted && wagerAcceptedAt.TryGetValue(offer.Id, out float at)
                    ? Time.unscaledTime - at : -1f;
                float settle = !accepted ? 0f : lockAge < 0f ? 1f :
                    Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((lockAge - 0.10f) / 0.25f));
                float bounce = accepted && lockAge >= 0.35f && lockAge < 0.48f
                    ? Mathf.Sin((lockAge - 0.35f) / 0.13f * Mathf.PI) * -3f : 0f;
                var lockRect = new Rect(lockBase.x, lockBase.y + 37f * settle + bounce,
                    lockBase.width, lockBase.height);
                bool canAccept = interact && offer.To == SelfId && !accepted && !onlineWagerRequestInFlight &&
                    (offer.SourceOfferId != 0 ? phase == "Point" : Time.unscaledTimeAsDouble <
                        (shooterId == SelfId ? AcceptanceDeadline : OfferDeadline));
                GUI.enabled = canAccept;
                bool tapped = DrawAmountLock(lockRect, offer.Amount, accepted, lockAge, offer.Outcome, offer.Number);
                GUI.enabled = interact;
                if (armedOfferId == offer.Id && Time.unscaledTime - armedOfferAt < 2f && !accepted)
                {
                    var old = GUI.color;
                    GUI.color = new Color(0.3f, 0.9f, 1f, 0.95f);
                    GUI.DrawTexture(new Rect(lockRect.x + 15f, lockRect.yMax - 5f, 36f, 3f), Texture2D.whiteTexture);
                    GUI.color = old;
                }
                if (tapped && armedOfferId == offer.Id && Time.unscaledTime - armedOfferAt < 2f)
                {
                    if (AcceptWager(offer.Id, SelfId)) PlayAudio(lockClip, 0.65f);
                    armedOfferId = 0;
                }
                else if (tapped)
                { armedOfferId = offer.Id; armedOfferAt = Time.unscaledTime; PlayAudio(rollClip, 0.13f); }
            if (pageCount <= 1) continue;
            Rect pager = GroundOfferPagerRect(sender);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
            style.normal.textColor = new Color(0.76f, 0.94f, 1f);
            var arrow = new GUIStyle(GUIStyle.none) { alignment = TextAnchor.MiddleCenter,
                fontSize = 17, fontStyle = FontStyle.Bold };
            arrow.normal.textColor = new Color(0.76f, 0.94f, 1f);
            if (GUI.Button(new Rect(pager.x, pager.y, 22f, pager.height), "<", arrow))
            { offerPage[key] = (page - 1 + pageCount) % pageCount; armedOfferId = 0; }
            GUI.Label(new Rect(pager.x + 22f, pager.y, 35f, pager.height),
                (page + 1) + "/" + pageCount, style);
            if (GUI.Button(new Rect(pager.x + 57f, pager.y, 22f, pager.height), ">", arrow))
            { offerPage[key] = (page + 1) % pageCount; armedOfferId = 0; }
            TrackOfferSwipe(key, pager, page, pageCount, interact);
        }
        GUI.enabled = interact;
    }

    private void TrackOfferSwipe(string key, Rect swipe, int page, int pageCount, bool interact)
    {
        if (!interact) return;
        var current = Event.current;
        if (current.type == EventType.MouseDown && swipe.Contains(current.mousePosition))
            offerSwipeStart[key] = current.mousePosition;
        else if (current.type == EventType.MouseUp && offerSwipeStart.TryGetValue(key, out Vector2 start))
        {
            offerSwipeStart.Remove(key);
            float delta = current.mousePosition.x - start.x;
            if (Mathf.Abs(delta) >= 32f && swipe.Contains(current.mousePosition))
            {
                offerPage[key] = Mathf.Clamp(page + (delta < 0 ? 1 : -1), 0, pageCount - 1);
                current.Use();
            }
        }
    }

    private bool DrawFadeButton(Rect rect)
    {
        if (fadeDisc == null)
        {
            fadeDisc = new Texture2D(128, 128, TextureFormat.RGBA32, false) { name = "Fade circular control" };
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float radius = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64, 64));
                Color color = radius > 59 ? new Color(0.78f, 0.8f, 0.82f) : new Color(0.12f, 0.13f, 0.14f);
                color.a = Mathf.Clamp01(63 - radius);
                fadeDisc.SetPixel(x, y, color);
            }
            fadeDisc.Apply(false, true);
        }
        bool inside = (Event.current.mousePosition - rect.center).sqrMagnitude <= rect.width * rect.width * 0.25f;
        bool enabled = GUI.enabled;
        GUI.enabled = enabled && (inside || GUIUtility.hotControl != 0);
        bool pressed = GUI.Button(rect, new GUIContent("", "Fade this roll"), GUIStyle.none);
        GUI.enabled = enabled;
        Color previous = GUI.color;
        GUI.color = enabled ? Color.white : new Color(1, 1, 1, 0.45f);
        GUI.DrawTexture(rect, fadeDisc);
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, fontStyle = FontStyle.Bold };
        style.normal.textColor = new Color(0.95f, 0.13f, 0.11f);
        GUI.Label(rect, "FADE", style);
        GUI.color = previous;
        return pressed && inside && enabled;
    }

    private void DrawWagerComposer(string target, float x, float y, bool interact)
    {
        float spread = Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - wagerStageAt) / 0.22f));
        bool ready = spread >= 0.98f;
        GUI.enabled = interact && ready && BettingWindowOpen && !onlineWagerRequestInFlight;
        if (wagerStage == 1)
        {
            int pointNumber = localDemo ? wagerBook.Point : int.TryParse(point, out int currentPoint) ? currentPoint : 0;
            bool allowHit = target != shooterId && catcherId != SelfId && pointNumber != 0;
            var outcomes = new List<(WagerOutcome outcome, int number)>();
            if (allowHit)
            {
                outcomes.Add((WagerOutcome.Hit, pointNumber));
                outcomes.Add((WagerOutcome.Hit, WagerBook.GroupMate(pointNumber)));
            }
            if (target != catcherId)
            {
                outcomes.Add((WagerOutcome.Crap, pointNumber));
                if (pointNumber != 0)
                    outcomes.Add((WagerOutcome.Crap, WagerBook.GroupMate(pointNumber)));
            }
            for (int i = 0; i < outcomes.Count; i++)
            {
                var choice = outcomes[i];
                string label = choice.outcome == WagerOutcome.Crap
                    ? "CRAP\n" + (choice.number == 0 ? "2/3/12" : choice.number.ToString())
                    : "HIT\n" + choice.number;
                if (DrawOutcomeDie(new Rect(x - 30 * (1 - spread) + (i % 2) * 72 * spread,
                    y + (i / 2) * 48 * spread, 68, 44), label))
                { draftNumber = choice.number; draftOutcome = choice.outcome; SetWagerStage(2); }
            }
        }
        else
        {
            DrawSelectedOutcomeDie(new Rect(x, y, 68, 44), WagerLabel(draftOutcome, draftNumber));
            if (wagerStage == 2)
            {
                for (int i = 0; i < WagerAmounts.Length; i++)
                {
                    int amount = WagerAmounts[i];
                    GUI.enabled = interact && ready && WagerFunds(SelfId) - ActiveWagerExposure(SelfId) >= amount;
                    if (GUI.Button(new Rect(x + (i % 2) * 72 * spread, y + 48 + (i / 2) * 48 * spread, 68, 44), "$" + amount))
                    { draftAmount = amount; SetWagerStage(3); }
                }
            }
            else if (DrawAmountLock(new Rect(x + 72, y - 16, 72, 88), draftAmount, false, -1f,
                draftOutcome, draftNumber))
            {
                var offer = OfferWager(SelfId, target, draftOutcome, draftNumber, draftAmount);
                if (offer != null) { ResetWagerDraft(); PlayAudio(lockClip, 0.65f); }
            }
        }
        GUI.enabled = interact;
    }

    private bool DrawOutcomeDie(Rect rect, string label)
    {
        bool pressed = GUI.Button(rect, new GUIContent("", label.Replace('\n', ' ')), GUIStyle.none);
        DrawSelectedOutcomeDie(rect, label);
        return pressed;
    }

    private void DrawSelectedOutcomeDie(Rect rect, string label)
    {
        EnsureWagerDieIcon();
        Color previous = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, wagerDieIcon, ScaleMode.ScaleToFit, true);
        GUI.color = new Color(0, 0, 0, 0.38f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold, fontSize = label.Contains("2/3/12") ? 12 :
                label.StartsWith("CRAP") ? 14 : 16 };
        style.normal.textColor = Color.white;
        GUI.Label(rect, label, style);
        GUI.color = previous;
    }

    private static Rect LockVisualRect(Rect hitRect, float elapsed)
    {
        if (elapsed < 0 || elapsed >= 0.18f) return hitRect;
        float press = Mathf.Sin(Mathf.PI * elapsed / 0.18f);
        float inset = press * 0.035f;
        return new Rect(hitRect.x + hitRect.width * inset, hitRect.y + hitRect.height * inset,
            hitRect.width * (1 - 2 * inset), hitRect.height * (1 - 2 * inset));
    }

    private bool DrawAmountLock(Rect rect, int amount, bool closed, float elapsed = -1f,
        WagerOutcome? outcome = null, int number = 0)
    {
        EnsureWagerIcons();
        bool pressed = GUI.Button(rect, new GUIContent("", closed ? "Wager locked" : "Lock $" + amount), GUIStyle.none);
        // Only artwork moves; the interaction rectangle and wager state never animate.
        rect = LockVisualRect(rect, closed && elapsed >= 0.35f ? elapsed - 0.35f : -1f);
        bool enabled = GUI.enabled;
        GUI.enabled = true;
        Color previousColor = GUI.color;
        if (closed && elapsed >= 0f && elapsed < 0.18f)
        {
            float close = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - 0.04f) / 0.14f));
            GUI.color = new Color(1f, 1f, 1f, 1f - close);
            GUI.DrawTexture(rect, wagerOpenIcon, ScaleMode.ScaleToFit, true);
            GUI.color = new Color(1f, 1f, 1f, close);
            GUI.DrawTexture(rect, wagerClosedIcon, ScaleMode.ScaleToFit, true);
        }
        else GUI.DrawTexture(rect, closed ? wagerClosedIcon : wagerOpenIcon, ScaleMode.ScaleToFit, true);
        GUI.color = previousColor;
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold, fontSize = 13 };
        bool crap = outcome == WagerOutcome.Crap;
        var title = new Rect(rect.x, rect.y + rect.height * 0.41f, rect.width, 16);
        var target = new Rect(rect.x, rect.y + rect.height * (crap ? 0.60f : 0.54f), rect.width, 20);
        string targetText = crap && number == 0 ? "2/3/12" : number.ToString();
        style.normal.textColor = Color.black;
        if (outcome.HasValue)
        {
            if (crap) GUI.Label(title, "CRAP", style);
            if (crap || number != 0)
            {
                style.fontSize = crap ? number == 0 ? 12 : 18 : 22;
                GUI.Label(target, targetText, style);
            }
        }
        style.normal.textColor = new Color(0.95f, 0.96f, 0.93f);
        if (outcome.HasValue)
        {
            style.fontSize = 13;
            if (crap) GUI.Label(title, "CRAP", style);
            if (crap || number != 0)
            {
                style.fontSize = crap ? number == 0 ? 12 : 18 : 22;
                GUI.Label(target, targetText, style);
            }
        }
        GUI.enabled = enabled;
        return pressed;
    }

    private void EnsureWagerIcons()
    {
        if (wagerOpenIcon != null) return;
        var source = Resources.Load<Texture2D>("UI/wager-locks-keyed");
        var shader = Resources.Load<Shader>("UI/WagerLockKey");
        if (source == null || shader == null) throw new InvalidOperationException("Wager lock artwork is missing.");
        var material = new Material(shader);
        wagerOpenIcon = RenderWagerLock(source, material, new Vector4(112f / 1536, 32f / 1024, 648f / 1536, 980f / 1024));
        wagerClosedIcon = RenderWagerLock(source, material, new Vector4(786f / 1536, 32f / 1024, 660f / 1536, 980f / 1024));
        Destroy(material);
    }

    private void EnsureWagerDieIcon()
    {
        if (wagerDieIcon != null) return;
        var source = Resources.Load<Texture2D>("UI/wager-die-photo");
        if (source == null) throw new InvalidOperationException("Photographic wager die is missing.");
        const int size = 512;
        var target = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        Graphics.Blit(source, target);
        RenderTexture.active = target;
        wagerDieIcon = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Photographic wager die" };
        wagerDieIcon.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);
        var pixels = wagerDieIcon.GetPixels32();
        var visited = new bool[pixels.Length];
        var queue = new int[pixels.Length];
        int head = 0, tail = 0;
        bool Backdrop(int index) => Mathf.Max(pixels[index].r, pixels[index].g, pixels[index].b) < 90;
        void Add(int index)
        {
            if (visited[index] || !Backdrop(index)) return;
            visited[index] = true;
            queue[tail++] = index;
        }
        for (int i = 0; i < size; i++)
        {
            Add(i); Add((size - 1) * size + i); Add(i * size); Add(i * size + size - 1);
        }
        // Only edge-connected dark pixels are removed; the die's black pips stay opaque.
        while (head < tail)
        {
            int index = queue[head++], x = index % size, y = index / size;
            if (x > 0) Add(index - 1);
            if (x < size - 1) Add(index + 1);
            if (y > 0) Add(index - size);
            if (y < size - 1) Add(index + size);
            pixels[index].a = 0;
        }
        wagerDieIcon.SetPixels32(pixels);
        wagerDieIcon.Apply(false, true);
    }

    private static RenderTexture RenderWagerLock(Texture source, Material material, Vector4 crop)
    {
        var texture = new RenderTexture(256, 384, 0, RenderTextureFormat.ARGB32) { name = "Wager lock cutout" };
        texture.Create();
        material.SetVector("_Crop", crop);
        var previousTarget = RenderTexture.active;
        Graphics.Blit(source, texture, material);
        RenderTexture.active = previousTarget;
        return texture;
    }

}
