using System;
using UnityEngine;

public sealed partial class StreetDiceGreyboxController
{
    private enum StartupScreen { Intro, AdultGate, AdultDenied, DieMenu, ModeMenu, OnlineMenu, JoinMenu, CodeEntry, Profile, GlobalSettings, ServerSettings, LegacyPregame }

    private const float IntroSeconds = 3.15f;
    private const string AdultAcceptedKey = "StreetDice.AdultAccess.Accepted";
    private const string ProfileNameKey = "iPlay.Profile.DisplayName";
    private StartupScreen startupScreen = StartupScreen.Intro;
    private StartupScreen profileReturnScreen = StartupScreen.DieMenu;
    private string profileDraft = "";
    private float introStarted;
    private bool introFirstClack, introSecondClack, introStrike, exitConfirmation, exitRequested;
    private Texture2D brandPlate, staticMenuDie, settingsGear, menuBrand, exitSign, metalPlate;
    private Material introPlateMaterial;
    private Font cardMenuDisplayFont;
    private AudioClip brandVoice, brandStrike, menuClack;

    private void InitializeStartupExperience()
    {
        brandPlate = Resources.Load<Texture2D>("Branding/iplay_app_icon");
        staticMenuDie = Resources.Load<Texture2D>("Branding/menu-die-static");
        menuBrand = Resources.Load<Texture2D>("Branding/iplay-ceelo-craps-brand-v1");
        settingsGear = Resources.Load<Texture2D>("UI/blue-steel-settings-v1");
        exitSign = Resources.Load<Texture2D>("UI/real-exit-sign-v1");
        metalPlate = Resources.Load<Texture2D>("UI/metal-plate-row-v1");
        cardMenuDisplayFont = Resources.Load<Font>("UI/BarlowCondensed-SemiBold");
        brandVoice = Resources.Load<AudioClip>("Audio/IPLAY_LOGO_VOICE_STING");
        brandStrike = Resources.Load<AudioClip>("Audio/IPLAY_CARD_SLAM_IMPACT_V2");
        var introShader = Resources.Load<Shader>("Branding/IntroPlate");
        if (brandPlate == null || staticMenuDie == null || menuBrand == null || settingsGear == null || exitSign == null || metalPlate == null ||
            cardMenuDisplayFont == null || brandVoice == null || brandStrike == null || introShader == null)
            throw new InvalidOperationException("The iPlay menu art, voice, impact, or shader is missing.");
        introPlateMaterial = new Material(introShader) { mainTexture = brandPlate };
        menuClack = CreateImpactClip("Menu die clack", 0.11f, 0.23f);
        playerName = PlayerPrefs.GetString(ProfileNameKey, "");
        profileDraft = playerName;
        startupScreen = Application.isEditor && Application.isBatchMode
            ? StartupScreen.LegacyPregame : StartupScreen.Intro;
        introStarted = Time.unscaledTime;
        UpdateStartupExperience();
    }

    private void UpdateStartupExperience()
    {
        if (!mainOptions || startupScreen != StartupScreen.Intro) return;
        float elapsed = Time.unscaledTime - introStarted;
        if (!introFirstClack && elapsed >= 0.20f) { introFirstClack = true; PlayAudio(menuClack, 0.46f); }
        if (!introSecondClack && elapsed >= 0.43f) { introSecondClack = true; PlayAudio(menuClack, 0.64f); }
        if (!introStrike && elapsed >= 1.27f)
        {
            introStrike = true;
            PlayAudio(brandStrike, 0.9f);
            PlayAudio(brandVoice, 0.9f);
        }
        if (elapsed >= IntroSeconds)
            startupScreen = PlayerPrefs.GetInt(AdultAcceptedKey, 0) == 1
                ? StartupScreen.DieMenu : StartupScreen.AdultGate;
    }

    private void DrawStartupExperience()
    {
        switch (startupScreen)
        {
            case StartupScreen.Intro: DrawLogoIntro(); break;
            case StartupScreen.AdultGate: DrawAdultGate(); break;
            case StartupScreen.AdultDenied: DrawAdultDenied(); break;
            case StartupScreen.DieMenu: DrawDieStartMenu(); break;
            case StartupScreen.ModeMenu: DrawModeMenu(); break;
            case StartupScreen.OnlineMenu: DrawOnlineMenu(); break;
            case StartupScreen.JoinMenu: DrawJoinMenu(); break;
            case StartupScreen.CodeEntry: DrawCodeEntry(); break;
            case StartupScreen.Profile: DrawProfile(); break;
            case StartupScreen.ServerSettings: DrawServerSettings(); break;
        }
    }

    private void DrawLogoIntro()
    {
        DrawStartupBackground(new Color(0.02f, 0.031f, 0.043f));
        float elapsed = Mathf.Max(0f, Time.unscaledTime - introStarted);
        float pulse = Mathf.Sin(Mathf.Clamp01((elapsed - 1.29f) / 0.45f) * Mathf.PI) * 0.52f;
        introPlateMaterial.SetFloat("_BaseLight", Mathf.Lerp(0.09f, 0.38f, Mathf.Clamp01(elapsed / 0.5f)));
        introPlateMaterial.SetFloat("_BorderReveal", Mathf.Clamp01((elapsed - 0.16f) / 0.78f));
        introPlateMaterial.SetFloat("_SymbolReveal", Mathf.Clamp01((elapsed - 0.58f) / 0.55f));
        introPlateMaterial.SetFloat("_Pulse", pulse);
        float strike = elapsed < 1.27f ? 0f : Mathf.Exp(-(elapsed - 1.27f) * 11f) * 0.055f;
        float size = Mathf.Min(UiHeight * 0.87f, UiWidth * 0.55f) * (0.95f + strike);
        var logical = new Rect((UiWidth - size) / 2f, (UiHeight - size) / 2f, size, size);
        if (Event.current.type == EventType.Repaint)
        {
            Graphics.DrawTexture(logical, brandPlate, introPlateMaterial);
        }
        float fade = Mathf.Clamp01((elapsed - 2.67f) / (IntroSeconds - 2.67f));
        if (fade > 0f)
        {
            var old = GUI.color;
            GUI.color = new Color(0.02f, 0.031f, 0.043f, fade);
            GUI.DrawTexture(new Rect(0, 0, UiWidth, UiHeight), Texture2D.whiteTexture);
            GUI.color = old;
        }
    }

    private void DrawAdultGate()
    {
        DrawPregameBackdrop();
        float center = UiWidth / 2f, top = UiHeight * 0.29f;
        DrawMetalPlate(new Rect(center - 380f, top, 760f, 172f));
        var heading = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 34 };
        GUI.Label(new Rect(center - 320f, top + 22f, 640f, 48f), "18+", heading);
        heading.fontSize = 24;
        GUI.Label(new Rect(center - 340f, top + 75f, 680f, 72f),
            "iPlay Cee-lo & Craps is for adults. Play money only.", heading);
        if (DrawMetalButton(new Rect(center - 240f, top + 192f, 225f, 54f), "I'm 18 or older"))
        {
            PlayerPrefs.SetInt(AdultAcceptedKey, 1);
            PlayerPrefs.Save();
            startupScreen = StartupScreen.DieMenu;
        }
        if (DrawMetalButton(new Rect(center + 15f, top + 192f, 225f, 54f), "Under 18"))
            startupScreen = StartupScreen.AdultDenied;
    }

    private void DrawAdultDenied()
    {
        DrawPregameBackdrop();
        var centered = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 27 };
        DrawMetalPlate(new Rect(UiWidth / 2f - 360f, UiHeight * 0.35f, 720f, 110f));
        GUI.Label(new Rect(UiWidth / 2f - 330f, UiHeight * 0.35f + 19f, 660f, 74f),
            "This game is for adults.", centered);
        if (DrawMetalButton(new Rect(UiWidth / 2f - 105f, UiHeight * 0.35f + 132f, 210f, 52f), "Exit")) QuitFromMenu();
    }

    private void DrawDieStartMenu()
    {
        DrawPregameBackdrop();
        float brandWidth = Mathf.Min(350f, UiWidth * 0.25f);
        GUI.DrawTexture(new Rect(UiWidth - brandWidth - 16f, 8f, brandWidth, brandWidth * 0.5f),
            menuBrand, ScaleMode.ScaleToFit, true);
        float size = Mathf.Min(UiHeight * 0.92f, UiWidth * 0.57f);
        var rect = new Rect((UiWidth - size) / 2f, (UiHeight - size) / 2f + 11f, size, size);
        GUI.DrawTexture(rect, staticMenuDie, ScaleMode.StretchToFill, true);
        if (!exitConfirmation)
        {
            if (GUI.Button(new Rect(rect.x + size * 0.09f, rect.y + size * 0.43f,
                size * 0.37f, size * 0.47f), GUIContent.none, GUIStyle.none)) SelectMenuFace(0);
            if (GUI.Button(new Rect(rect.x + size * 0.54f, rect.y + size * 0.43f,
                size * 0.37f, size * 0.47f), GUIContent.none, GUIStyle.none)) SelectMenuFace(1);

            float controlX = rect.xMax + 28f;
            float controlY = UiHeight * 0.46f;
            var gearButton = new Rect(controlX - 5f, controlY - 5f, 74f, 74f);
            GUI.DrawTexture(gearButton, settingsGear, ScaleMode.ScaleToFit, true);
            if (GUI.Button(gearButton, new GUIContent("", "Settings"), GUIStyle.none)) OpenGlobalSettings();
            var exitRect = new Rect(controlX - 8f, controlY + 78f, 80f, 44f);
            GUI.DrawTexture(exitRect, exitSign, ScaleMode.ScaleToFit, true);
            if (GUI.Button(exitRect, new GUIContent("", "Exit iPlay"), GUIStyle.none))
                exitConfirmation = true;
            if (DrawFlowChoice(new Rect(rect.x - 168f, controlY + 70f, 150f, 52f), "PROFILE"))
                OpenProfile(StartupScreen.DieMenu);
        }
        if (!exitConfirmation) return;
        DrawOpaquePanel(new Rect(UiWidth / 2f - 205, UiHeight / 2f - 92, 410, 184));
        GUI.Label(new Rect(UiWidth / 2f - 178, UiHeight / 2f - 72, 356, 42), "Exit iPlay?");
        if (DrawMetalButton(new Rect(UiWidth / 2f - 178, UiHeight / 2f + 14, 165, 47), "Stay")) exitConfirmation = false;
        if (DrawMetalButton(new Rect(UiWidth / 2f + 13, UiHeight / 2f + 14, 165, 47), "Exit")) QuitFromMenu();
    }

    private void DrawFlowBackground(string title)
    {
        DrawPregameBackdrop();
        float brandWidth = Mathf.Min(350f, UiWidth * 0.25f);
        GUI.DrawTexture(new Rect(UiWidth - brandWidth - 16f, 8f, brandWidth, brandWidth * 0.5f),
            menuBrand, ScaleMode.ScaleToFit, true);
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 38 };
        GUI.Label(new Rect(UiWidth * 0.18f, UiHeight * 0.14f, UiWidth * 0.64f, 60f), title, style);
    }

    private bool DrawFlowChoice(Rect rect, string title, bool available = true)
    {
        var old = GUI.color;
        GUI.color = available ? Color.white : new Color(0.62f, 0.66f, 0.68f, 0.75f);
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 30 };
        GUI.Label(rect, title, style);
        GUI.color = old;
        return available && GUI.Button(rect, new GUIContent("", title), GUIStyle.none);
    }

    private bool DrawFlowBack()
    {
        var rect = new Rect(28f, UiHeight - 78f, 138f, 48f);
        return DrawFlowChoice(rect, "BACK");
    }

    private bool ValidOnlineIdentity => !string.IsNullOrWhiteSpace(playerName) &&
        Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private void DrawFlowField(Rect rect, string label, ref string value, int maxLength)
    {
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 21 };
        GUI.Label(new Rect(rect.x, rect.y - 37f, rect.width, 30f), label, style);
        var fieldStyle = new GUIStyle(GUIStyle.none)
        {
            font = GUI.skin.label.font,
            fontSize = 26,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white },
            focused = { textColor = Color.white }
        };
        value = GUI.TextField(rect, value, maxLength, fieldStyle);
        var line = new Rect(rect.x, rect.yMax + 3f, rect.width, 2f);
        var old = GUI.color;
        GUI.color = new Color(0.15f, 0.78f, 0.92f, 0.55f);
        GUI.DrawTexture(line, Texture2D.whiteTexture);
        GUI.color = old;
    }

    private void DrawModeMenu()
    {
        DrawFlowBackground(gameMode == GameMode.Craps ? "CRAPS" : "CEE-LO");
        float x = UiWidth * 0.31f, w = UiWidth * 0.38f;
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.40f, w, 62f), "PLAY VS AI")) StartLocalDemo();
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.57f, w, 62f), "ONLINE", gameMode == GameMode.Craps))
            startupScreen = StartupScreen.OnlineMenu;
        if (DrawFlowBack()) startupScreen = StartupScreen.DieMenu;
    }

    private void DrawOnlineMenu()
    {
        DrawFlowBackground("ONLINE CRAPS");
        float x = UiWidth * 0.32f, w = UiWidth * 0.36f;
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.43f, w, 55f), "HOST TABLE", ValidOnlineIdentity))
        {
            SaveServerAddress();
            StartCoroutine(CreateRealTable());
        }
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.61f, w, 55f), "JOIN TABLE", ValidOnlineIdentity))
            startupScreen = StartupScreen.JoinMenu;
        if (string.IsNullOrWhiteSpace(playerName) &&
            DrawFlowChoice(new Rect(x, UiHeight * 0.76f, w, 46f), "PROFILE"))
            OpenProfile(StartupScreen.OnlineMenu);
        if (DrawFlowBack()) startupScreen = StartupScreen.ModeMenu;
    }

    private void OpenProfile(StartupScreen returnTo)
    {
        profileReturnScreen = returnTo;
        profileDraft = playerName;
        startupScreen = StartupScreen.Profile;
    }

    private void DrawProfile()
    {
        DrawFlowBackground("PROFILE");
        float x = UiWidth * 0.30f, w = UiWidth * 0.40f;
        DrawFlowField(new Rect(x, UiHeight * 0.32f, w, 46f), "DISPLAY NAME", ref profileDraft, 24);
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.44f, w, 48f), "SAVE NAME",
            !string.IsNullOrWhiteSpace(profileDraft)))
        {
            playerName = profileDraft.Trim();
            PlayerPrefs.SetString(ProfileNameKey, playerName);
            PlayerPrefs.Save();
            startupScreen = profileReturnScreen;
        }
        var label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18 };
        GUI.Label(new Rect(x, UiHeight * 0.57f, w, 32f), "ACCOUNT NOT CONNECTED", label);
        DrawFlowChoice(new Rect(x, UiHeight * 0.64f, w, 44f), "GOOGLE", false);
        DrawFlowChoice(new Rect(x, UiHeight * 0.73f, w, 44f), "APPLE", false);
        DrawFlowChoice(new Rect(x, UiHeight * 0.82f, w, 44f), "EMAIL CODE", false);
        if (DrawFlowBack()) startupScreen = profileReturnScreen;
    }

    private void DrawJoinMenu()
    {
        DrawFlowBackground("JOIN TABLE");
        float x = UiWidth * 0.31f, w = UiWidth * 0.38f;
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.43f, w, 60f), "ENTER CODE"))
            startupScreen = StartupScreen.CodeEntry;
        DrawFlowChoice(new Rect(x, UiHeight * 0.60f, w, 60f), "THE JUNGLE", false);
        if (DrawFlowBack()) startupScreen = StartupScreen.OnlineMenu;
    }

    private void DrawCodeEntry()
    {
        DrawFlowBackground("ENTER CODE");
        float x = UiWidth * 0.32f, w = UiWidth * 0.36f;
        DrawFlowField(new Rect(x, UiHeight * 0.46f, w, 46f), "TABLE CODE", ref joinCode, 32);
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.64f, w, 55f), "JOIN TABLE",
            ValidOnlineIdentity && !string.IsNullOrWhiteSpace(joinCode)))
        {
            SaveServerAddress();
            StartCoroutine(JoinRealTable());
        }
        if (DrawFlowBack()) startupScreen = StartupScreen.JoinMenu;
    }

    private void DrawServerSettings()
    {
        DrawFlowBackground("ADVANCED SETTINGS");
        float x = UiWidth * 0.25f, w = UiWidth * 0.5f;
        DrawFlowField(new Rect(x, UiHeight * 0.45f, w, 46f), "SERVER ADDRESS", ref baseUrl, 120);
        bool valid = Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        if (DrawFlowChoice(new Rect(x, UiHeight * 0.64f, w, 52f), "SAVE", valid))
        {
            SaveServerAddress();
            startupScreen = StartupScreen.GlobalSettings;
        }
        if (DrawFlowBack()) startupScreen = StartupScreen.GlobalSettings;
    }

    private void SaveServerAddress()
    {
        baseUrl = baseUrl.Trim().TrimEnd('/');
        PlayerPrefs.SetString("StreetDice.ServerUrl", baseUrl);
        PlayerPrefs.Save();
    }

    private void DrawStartupBackground(Color color)
    {
        var old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(0, 0, UiWidth, UiHeight), Texture2D.whiteTexture);
        GUI.color = old;
    }

    private void DrawPregameBackdrop()
    {
        if (pregameBackground == null) pregameBackground = Resources.Load<Texture2D>("UI/pregame-dice-background");
        GUI.DrawTexture(new Rect(0f, 0f, UiWidth, UiHeight), pregameBackground, ScaleMode.ScaleAndCrop);
    }

    private void DrawMetalPlate(Rect rect)
    {
        GUI.DrawTextureWithTexCoords(rect, metalPlate, new Rect(0.008f, 0.369f, 0.984f, 0.300f), true);
    }

    private bool DrawMetalButton(Rect rect, string label)
    {
        DrawMetalPlate(rect);
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 17 };
        GUI.Label(rect, label, style);
        return GUI.Button(rect, new GUIContent("", label), GUIStyle.none);
    }

    private void SelectMenuFace(int index)
    {
        if (startupScreen != StartupScreen.DieMenu || exitConfirmation) return;
        PlayAudio(menuClack, 0.54f);
        if (index == 0 || index == 1)
        {
            gameMode = index == 0 ? GameMode.Craps : GameMode.CeeLo;
            startupScreen = StartupScreen.ModeMenu;
        }
        else if (index == 2)
        {
            OpenGlobalSettings();
        }
        else if (index == 3) exitConfirmation = true;
    }

    private void OpenGlobalSettings()
    {
        startupScreen = StartupScreen.GlobalSettings;
        showCredits = false;
    }

    private void ReturnToDieMenu()
    {
        showCredits = false;
        startupScreen = StartupScreen.DieMenu;
    }

    private void QuitFromMenu()
    {
        exitRequested = true;
        if (!Application.isEditor) Application.Quit();
    }
}
