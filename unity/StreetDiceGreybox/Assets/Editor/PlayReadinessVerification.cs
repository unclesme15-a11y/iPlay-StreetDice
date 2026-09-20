using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayReadinessVerification
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static IEnumerator routine;
    private static int frame = -1;
    private static double deadline;
    private static EditorWindow gameView;
    static PlayReadinessVerification() { EditorApplication.update += Tick; }

    public static void Start()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.VerifyReadiness", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartMoneyCapture()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.CaptureMoney", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartDiceColorCapture()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.CaptureDiceColors", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartBettingCapture()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.CaptureBetting", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartSaleCapture()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.CaptureSale", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartAddOnCapture()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.CaptureAddOns", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartOnlinePhysical()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.VerifyOnlinePhysical", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartRealOnlineSeats()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.VerifyRealOnlineSeats", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartOnlineCatcherFade()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.VerifyOnlineCatcherFade", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartStartup()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.VerifyStartup", true);
        EditorApplication.isPlaying = true;
    }

    public static void StartStartupOnline()
    {
        StreetDiceDemoBuild.PrepareAndroidWithoutBuilding();
        gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        gameView.position = new Rect(30, 30, 1280, 760);
        gameView.Show();
        SessionState.SetBool("StreetDice.VerifyStartupOnline", true);
        EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (SessionState.GetBool("StreetDice.VerifyReadiness", false))
        {
            SessionState.SetBool("StreetDice.VerifyReadiness", false);
            deadline = EditorApplication.timeSinceStartup + 180;
            routine = Verify();
        }
        if (SessionState.GetBool("StreetDice.CaptureMoney", false))
        {
            SessionState.SetBool("StreetDice.CaptureMoney", false);
            deadline = EditorApplication.timeSinceStartup + 45;
            routine = CaptureMoney();
        }
        if (SessionState.GetBool("StreetDice.CaptureDiceColors", false))
        {
            SessionState.SetBool("StreetDice.CaptureDiceColors", false);
            deadline = EditorApplication.timeSinceStartup + 45;
            routine = CaptureDiceColors();
        }
        if (SessionState.GetBool("StreetDice.CaptureBetting", false))
        {
            SessionState.SetBool("StreetDice.CaptureBetting", false);
            deadline = EditorApplication.timeSinceStartup + 45;
            routine = CaptureBetting();
        }
        if (SessionState.GetBool("StreetDice.CaptureSale", false))
        {
            SessionState.SetBool("StreetDice.CaptureSale", false);
            deadline = EditorApplication.timeSinceStartup + 45;
            routine = CaptureSale();
        }
        if (SessionState.GetBool("StreetDice.CaptureAddOns", false))
        {
            SessionState.SetBool("StreetDice.CaptureAddOns", false);
            deadline = EditorApplication.timeSinceStartup + 45;
            routine = CaptureAddOns();
        }
        if (SessionState.GetBool("StreetDice.VerifyOnlinePhysical", false))
        {
            SessionState.SetBool("StreetDice.VerifyOnlinePhysical", false);
            deadline = EditorApplication.timeSinceStartup + 70;
            routine = VerifyOnlinePhysical();
        }
        if (SessionState.GetBool("StreetDice.VerifyRealOnlineSeats", false))
        {
            SessionState.SetBool("StreetDice.VerifyRealOnlineSeats", false);
            deadline = EditorApplication.timeSinceStartup + 75;
            routine = VerifyRealOnlineSeats();
        }
        if (SessionState.GetBool("StreetDice.VerifyOnlineCatcherFade", false))
        {
            SessionState.SetBool("StreetDice.VerifyOnlineCatcherFade", false);
            deadline = EditorApplication.timeSinceStartup + 70;
            routine = VerifyOnlineCatcherFade();
        }
        if (SessionState.GetBool("StreetDice.VerifyStartup", false))
        {
            SessionState.SetBool("StreetDice.VerifyStartup", false);
            deadline = EditorApplication.timeSinceStartup + 90;
            routine = VerifyStartup();
        }
        if (SessionState.GetBool("StreetDice.VerifyStartupOnline", false))
        {
            SessionState.SetBool("StreetDice.VerifyStartupOnline", false);
            deadline = EditorApplication.timeSinceStartup + 90;
            routine = VerifyStartupOnline();
        }
        if (routine == null || frame == Time.frameCount) return;
        frame = Time.frameCount;
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Readiness verification timed out.");
            if (!routine.MoveNext()) { routine = null; EditorApplication.Exit(0); }
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }

    private static object Get(object c, string field) => c.GetType().GetField(field, Flags).GetValue(c);
    private static void Set(object c, string field, object value) => c.GetType().GetField(field, Flags).SetValue(c, value);
    private static object Call(object c, string method, params object[] args) => c.GetType().GetMethod(method, Flags).Invoke(c, args);
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }

    private static IEnumerator VerifyStartup()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Startup controller missing");
        var field = c.GetType().GetField("startupScreen", Flags);
        Action<string> screen = name =>
        {
            field.SetValue(c, Enum.Parse(field.FieldType, name));
            Call(c, "UpdateStartupExperience");
        };
        Set(c, "mainOptions", true);
        string output = Path.GetFullPath("../../artifacts/unity-smoke/startup");
        Directory.CreateDirectory(output);
        screen("Intro");
        Set(c, "introStarted", Time.unscaledTime - 1.4f);
        yield return null;
        CaptureGameView(Path.Combine(output, "01-intro.png"));
        screen("AdultGate");
        yield return null;
        CaptureGameView(Path.Combine(output, "02-adult-gate.png"));
        screen("AdultDenied");
        yield return null;
        CaptureGameView(Path.Combine(output, "02b-under-18.png"));
        screen("DieMenu");
        yield return null;
        yield return null;
        CaptureGameView(Path.Combine(output, "03-die-menu.png"));
        screen("ModeMenu");
        Set(c, "gameMode", GameMode.Craps);
        yield return null;
        CaptureGameView(Path.Combine(output, "03b-craps-modes.png"));
        screen("OnlineMenu");
        yield return null;
        CaptureGameView(Path.Combine(output, "03c-online-screen.png"));
        screen("JoinMenu");
        yield return null;
        CaptureGameView(Path.Combine(output, "03d-join-options.png"));
        screen("CodeEntry");
        yield return null;
        CaptureGameView(Path.Combine(output, "03e-code-entry.png"));
        screen("ModeMenu");
        Set(c, "gameMode", GameMode.CeeLo);
        yield return null;
        CaptureGameView(Path.Combine(output, "03f-ceelo-modes.png"));
        screen("DieMenu");
        Check(Get(c, "staticMenuDie") is Texture2D, "Reference die image not loaded");
        Check(Get(c, "settingsGear") is Texture2D, "Settings gear not loaded");
        Check(GameObject.Find("iPlay Die Menu Camera") == null, "Stationary menu still created a 3D camera");
        var phone = CaptureStartupPhoneLayouts(c, output, screen);
        while (phone.MoveNext()) yield return null;
        Call(c, "OpenGlobalSettings");
        Check(field.GetValue(c).ToString() == "GlobalSettings", "Settings did not open");
        yield return null;
        CaptureGameView(Path.Combine(output, "04-global-settings.png"));
        screen("ServerSettings");
        yield return null;
        CaptureGameView(Path.Combine(output, "04a-advanced-server.png"));
        screen("GlobalSettings");
        Set(c, "showCredits", true);
        yield return null;
        CaptureGameView(Path.Combine(output, "04b-credits.png"));
        Set(c, "showCredits", false);
        Call(c, "ReturnToDieMenu");
        Check(field.GetValue(c).ToString() == "DieMenu", "Settings did not return to die");
        yield return null;
        Set(c, "exitConfirmation", true);
        yield return null;
        CaptureGameView(Path.Combine(output, "03g-exit-confirmation.png"));
        Set(c, "exitConfirmation", false);
        Call(c, "SelectMenuFace", 0);
        Check(field.GetValue(c).ToString() == "ModeMenu", "Craps face did not open modes");
        Check(Get(c, "gameMode").ToString() == "Craps", "Craps face chose wrong game");
        screen("DieMenu");
        yield return null;
        Call(c, "SelectMenuFace", 1);
        Check(field.GetValue(c).ToString() == "ModeMenu" && Get(c, "gameMode").ToString() == "CeeLo",
            "Cee-lo face did not open Cee-lo modes");
        var clicks = VerifyStartupClicks(c, screen, field);
        while (clicks.MoveNext()) yield return null;
        Debug.Log("STARTUP FLOW PASSED: intro, age gate, reference die, separate mode and online screens, and global settings");
    }

    private static IEnumerator VerifyStartupClicks(StreetDiceGreyboxController c, Action<string> screen, FieldInfo field)
    {
        Set(c, "mainOptions", true);
        screen("AdultGate");
        yield return null;
        var click = ClickStartup(c, GetUiWidth(c) / 2f - 125f, GetUiHeight(c) * 0.29f + 219f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "DieMenu", "Age confirmation click did not open die menu");

        float size = Mathf.Min(GetUiHeight(c) * 0.92f, GetUiWidth(c) * 0.57f);
        float dieX = (GetUiWidth(c) - size) / 2f, dieY = (GetUiHeight(c) - size) / 2f + 11f;
        click = ClickStartup(c, dieX + size * 0.25f, dieY + size * 0.65f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "ModeMenu" && Get(c, "gameMode").ToString() == "Craps",
            "Craps die face click did not open Craps modes");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.57f + 30f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "OnlineMenu", "Online click did not open its own screen");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.36f + 23f);
        while (click.MoveNext()) yield return null;
        var typing = TypeStartup(c, 'X');
        while (typing.MoveNext()) yield return null;
        Check(((string)Get(c, "playerName")).Contains("X"), "Name field did not accept typed input");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.68f + 27f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "JoinMenu", "Join click did not open join choices");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.60f + 30f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "JoinMenu", "Disabled Jungle button was clickable");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.43f + 30f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "CodeEntry", "Enter Code click did not open code screen");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.46f + 23f);
        while (click.MoveNext()) yield return null;
        typing = TypeStartup(c, '7');
        while (typing.MoveNext()) yield return null;
        Check(((string)Get(c, "joinCode")).Contains("7"), "Table code field did not accept typed input");
        for (int i = 0; i < 3; i++)
        {
            click = ClickStartup(c, 85f, GetUiHeight(c) - 54f);
            while (click.MoveNext()) yield return null;
            Check(field.GetValue(c).ToString() == new[] { "JoinMenu", "OnlineMenu", "ModeMenu" }[i],
                "Back click did not return one screen");
        }
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.40f + 30f);
        while (click.MoveNext()) yield return null;
        Check(!(bool)Get(c, "mainOptions") && Get(c, "gameMode").ToString() == "Craps",
            "Craps AI click did not start play");

        Set(c, "mainOptions", true);
        screen("DieMenu");
        yield return null;
        click = ClickStartup(c, dieX + size * 0.72f, dieY + size * 0.65f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "ModeMenu" && Get(c, "gameMode").ToString() == "CeeLo",
            "Cee-lo die face click did not open Cee-lo modes");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.57f + 30f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "ModeMenu", "Disabled Cee-lo online button was clickable");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.40f + 30f);
        while (click.MoveNext()) yield return null;
        Check(!(bool)Get(c, "mainOptions") && Get(c, "gameMode").ToString() == "CeeLo",
            "Cee-lo AI click did not start play");
        Set(c, "mainOptions", true);
        screen("DieMenu");
        yield return null;
        float gearX = dieX + size + 28f + 32f, gearY = GetUiHeight(c) * 0.46f + 32f;
        click = ClickStartup(c, gearX, gearY);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "GlobalSettings", "Settings gear click did not open settings");
        click = ClickStartup(c, GetUiWidth(c) / 2f, 120f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "ServerSettings", "Advanced click did not open server settings");
        click = ClickStartup(c, 85f, GetUiHeight(c) - 54f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "GlobalSettings", "Advanced Back did not return to settings");
        click = ClickStartup(c, GetUiWidth(c) / 2f - 340f, 120f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "DieMenu", "Settings Back did not return to die");
        Debug.Log("STARTUP CLICKS PASSED: age, die faces, modes, text fields, join choices, disabled modes, back, settings, and AI starts");
    }

    private static float GetUiWidth(StreetDiceGreyboxController c) =>
        (float)c.GetType().GetProperty("UiWidth", Flags).GetValue(c);

    private static float GetUiHeight(StreetDiceGreyboxController c) =>
        (float)c.GetType().GetProperty("UiHeight", Flags).GetValue(c);

    private static IEnumerator ClickStartup(StreetDiceGreyboxController c, float x, float y)
    {
        var position = new Vector2(x, y);
        Call(c, "QueueStartupTestEvent", new Event { type = EventType.MouseDown, mousePosition = position, button = 0 });
        for (int i = 0; i < 3; i++) yield return null;
        Call(c, "QueueStartupTestEvent", new Event { type = EventType.MouseUp, mousePosition = position, button = 0 });
        for (int i = 0; i < 3; i++) yield return null;
    }

    private static IEnumerator TypeStartup(StreetDiceGreyboxController c, char value)
    {
        Call(c, "QueueStartupTestEvent", new Event { type = EventType.KeyDown, character = value, keyCode = KeyCode.None });
        for (int i = 0; i < 3; i++) yield return null;
        Call(c, "QueueStartupTestEvent", new Event { type = EventType.KeyUp, character = value, keyCode = KeyCode.None });
        for (int i = 0; i < 3; i++) yield return null;
    }

    private static IEnumerator VerifyStartupOnline()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Startup controller missing");
        var field = c.GetType().GetField("startupScreen", Flags);
        Action<string> screen = name => field.SetValue(c, Enum.Parse(field.FieldType, name));
        Set(c, "mainOptions", true);
        Set(c, "gameMode", GameMode.Craps);
        Set(c, "baseUrl", "http://127.0.0.1:5108");
        Set(c, "playerName", "");
        screen("OnlineMenu");
        yield return null;
        var click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.36f + 23f);
        while (click.MoveNext()) yield return null;
        foreach (char letter in "Host")
        {
            var typing = TypeStartup(c, letter);
            while (typing.MoveNext()) yield return null;
        }
        Check((string)Get(c, "playerName") == "Host", "Host name was not typed into the field");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.55f + 27f);
        while (click.MoveNext()) yield return null;
        float timeout = Time.realtimeSinceStartup + 12f;
        while (string.IsNullOrEmpty((string)Get(c, "gameId")) || string.IsNullOrEmpty((string)Get(c, "localPlayerId")))
        {
            Check(Time.realtimeSinceStartup < timeout, "Host did not create and join a table");
            yield return null;
        }
        string tableId = (string)Get(c, "gameId"), hostId = (string)Get(c, "localPlayerId");

        Set(c, "mainOptions", true);
        Set(c, "playerName", "");
        Set(c, "joinCode", "");
        screen("OnlineMenu");
        yield return null;
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.36f + 23f);
        while (click.MoveNext()) yield return null;
        foreach (char letter in "Guest")
        {
            var typing = TypeStartup(c, letter);
            while (typing.MoveNext()) yield return null;
        }
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.68f + 27f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "JoinMenu", "Join screen did not open");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.43f + 30f);
        while (click.MoveNext()) yield return null;
        Check(field.GetValue(c).ToString() == "CodeEntry", "Code screen did not open");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.46f + 23f);
        while (click.MoveNext()) yield return null;
        foreach (char letter in tableId)
        {
            var typing = TypeStartup(c, letter);
            while (typing.MoveNext()) yield return null;
        }
        Check((string)Get(c, "joinCode") == tableId, "Typed table code did not match host code");
        click = ClickStartup(c, GetUiWidth(c) * 0.5f, GetUiHeight(c) * 0.64f + 27f);
        while (click.MoveNext()) yield return null;
        timeout = Time.realtimeSinceStartup + 12f;
        while ((string)Get(c, "localPlayerId") == hostId || (string)Get(c, "gameId") != tableId)
        {
            Check(Time.realtimeSinceStartup < timeout, "Guest did not join the hosted table through the screen");
            yield return null;
        }
        Check((string)Get(c, "playerName") == "Guest", "Joined player name changed");
        Debug.Log("STARTUP ONLINE PASSED: typed Host and Guest names, hosted table, typed code, joined second seat");
    }

    private static IEnumerator CaptureStartupPhoneLayouts(StreetDiceGreyboxController c, string output, Action<string> screen)
    {
        var assembly = typeof(EditorWindow).Assembly;
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = sizesType.BaseType.GetProperty("instance", all).GetValue(null);
        var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
        string groupName = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : "Standalone";
        var group = sizesType.GetMethod("GetGroup", all).Invoke(sizes, new[] { Enum.Parse(groupType, groupName) });
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        var modeType = assembly.GetType("UnityEditor.GameViewSizeType");
        var constructor = sizeType.GetConstructor(all, null, new[] { modeType, typeof(int), typeof(int), typeof(string) }, null);
        var selection = gameView.GetType().GetProperty("selectedSizeIndex", all);
        int previous = (int)selection.GetValue(gameView);
        try
        {
            foreach (var resolution in new[] { new Vector2Int(1280, 720), new Vector2Int(2340, 1080) })
            {
                int index = (int)group.GetType().GetMethod("GetTotalCount", all).Invoke(group, null);
                var size = constructor.Invoke(new object[] { Enum.Parse(modeType, "FixedResolution"), resolution.x, resolution.y,
                    "iPlay startup " + resolution });
                group.GetType().GetMethod("AddCustomSize", all).Invoke(group, new[] { size });
                selection.SetValue(gameView, index);
                gameView.Repaint();
                for (int i = 0; i < 60 && (Screen.width != resolution.x || Screen.height != resolution.y); i++)
                    yield return null;
                Check(Screen.width == resolution.x && Screen.height == resolution.y,
                    "Startup phone size did not apply: expected " + resolution + ", got " + Screen.width + "x" + Screen.height);
                screen("DieMenu");
                yield return null;
                CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-die-menu.png"));
                screen("AdultGate");
                yield return null;
                CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-adult-gate.png"));
                Set(c, "gameMode", GameMode.Craps);
                foreach (var flow in new[] { "ModeMenu", "OnlineMenu", "JoinMenu", "CodeEntry" })
                {
                    screen(flow);
                    yield return null;
                    CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-" + flow + ".png"));
                }
                Set(c, "gameMode", GameMode.CeeLo);
                screen("ModeMenu");
                yield return null;
                CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-cee-lo-modes.png"));
            }
        }
        finally { selection.SetValue(gameView, previous); gameView.Repaint(); screen("DieMenu"); }
        for (int i = 0; i < 8; i++) yield return null;
    }

    private static IEnumerator CaptureMoney()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        for (int i = 0; i < 5; i++) yield return null;
        string output = Path.GetFullPath("../../artifacts/unity-smoke/readiness");
        Directory.CreateDirectory(output);
        CaptureGameView(Path.Combine(output, "photo-money-ground.png"));
        Call(c, "QueueMoneyTransfer", "p2", "p1", 20);
        float payoutAt = Time.realtimeSinceStartup + 0.55f;
        while (Time.realtimeSinceStartup < payoutAt) yield return null;
        CaptureGameView(Path.Combine(output, "photo-money-transfer.png"));
        Debug.Log("MONEY PHOTO CAPTURE PASSED: ground and in-flight bill captures");
    }

    private static IEnumerator CaptureDiceColors()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Dice color capture controller missing");
        Call(c, "StartLocalDemo");
        Set(c, "botWagersEnabled", false);
        Call(c, "EnsureHandPreviews");
        VerifyDiceSelectionPixels(c);
        string output = Path.GetFullPath("../../artifacts/unity-smoke/dice-colors");
        Directory.CreateDirectory(output);
        var colors = (Color[])typeof(StreetDiceGreyboxController)
            .GetField("DiceColors", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        string[] names = { "white", "black", "green", "blue" };
        var previews = (RenderTexture[])Get(c, "dicePreviews");
        var dieA = (GameObject)Get(c, "dieA");
        var dieB = (GameObject)Get(c, "dieB");
        var regulars = (Dictionary<GameObject, GameObject>)Get(c, "regularDiceVisuals");
        var hotDice = (Dictionary<GameObject, GameObject>)Get(c, "hotDiceVisuals");
        var skyCamera = (Camera)Get(c, "skyCamera");
        var skyTexture = (RenderTexture)Get(c, "skyTexture");
        float normalZoom = skyCamera.orthographicSize;
        Set(c, "rolling", true);
        Set(c, "skyCamVisible", true);
        Set(c, "hotForCurrentThrow", false);
        Set(c, "streak", 0f);
        dieA.transform.position = new Vector3(-0.23f, -1.05f, -1f);
        dieB.transform.position = new Vector3(0.23f, -1.05f, -1f);
        foreach (var die in new[] { dieA, dieB }) die.SetActive(true);
        for (int i = 0; i < colors.Length; i++)
        {
            Set(c, "selectedDiceColor", colors[i]);
            Call(c, "ApplyDiceColor");
            Call(c, "LockDieToValue", dieA, 3);
            Call(c, "LockDieToValue", dieB, 5);
            foreach (var die in new[] { dieA, dieB })
            {
                Check(regulars[die].activeSelf && !hotDice[die].activeSelf,
                    "Regular dice were not displayed for " + names[i]);
                var material = regulars[die].GetComponentInChildren<Renderer>().material;
                Check(Vector4.Distance(material.GetColor("_BodyColor"), colors[i]) < 0.001f,
                    "Live dice did not use the selected " + names[i] + " color");
            }
            CaptureRenderTexture(previews[i], Path.Combine(output, "model-" + names[i] + ".png"));
            skyCamera.orthographicSize = normalZoom;
            skyCamera.Render();
            yield return null;
            CaptureGameView(Path.Combine(output, "game-" + names[i] + ".png"));
            skyCamera.orthographicSize = 0.32f;
            skyCamera.Render();
            CaptureRenderTexture(skyTexture, Path.Combine(output, "dice-" + names[i] + ".png"));
        }
        Set(c, "streak", 10f);
        Set(c, "hotForCurrentThrow", true);
        Call(c, "ApplyDiceColor");
        Call(c, "LockDieToValue", dieA, 3);
        Call(c, "LockDieToValue", dieB, 5);
        Check(hotDice[dieA].activeSelf && hotDice[dieB].activeSelf &&
            !regulars[dieA].activeSelf && !regulars[dieB].activeSelf,
            "Hot dice did not replace the selected regular dice");
        skyCamera.orthographicSize = 0.32f;
        skyCamera.Render();
        CaptureRenderTexture(skyTexture, Path.Combine(output, "dice-hot.png"));
        skyCamera.orthographicSize = normalZoom;
        skyCamera.Render();
        yield return null;
        CaptureGameView(Path.Combine(output, "game-hot.png"));
        Call(c, "StartLocalDemo");
        Set(c, "rolling", false);
        Set(c, "skyCamVisible", false);
        Set(c, "hotForCurrentThrow", false);
        Call(c, "ApplyDiceColor");
        Set(c, "botWagersEnabled", false);
        Call(c, "CommitShoot");
        foreach (string sender in new[] { "p4", "bot-5" })
        {
            var offer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", sender, "p1", IPlay.Demo.WagerOutcome.Crap, 0, 5);
            Check(offer != null, "Name placement offer missing for " + sender);
            Rect name = (Rect)Call(c, "SeatHudRect", sender);
            Rect wagerLock = (Rect)Call(c, "GroundLockRect", offer);
            Check(name.y + 26f < wagerLock.y, "Lower opponent name touches its lock: " + sender);
        }
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "lower-names.png"));
        Debug.Log("DICE COLOR CAPTURE PASSED: four live regular colors, distinct preview pixels, and separate hot dice; game and inspection-zoom captures saved.");
    }

    private static IEnumerator CaptureBetting()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        Call(c, "StartLocalDemo");
        Call(c, "PassLocalDice");
        Call(c, "CommitShoot");
        Check((bool)Get(c, "shotCommitted") && (string)Get(c, "shooterId") != "p1",
            "Betting capture requires another shooter");
        Set(c, "wagerOverlayOpen", true);
        Set(c, "wagerTarget", (string)Get(c, "shooterId"));
        Set(c, "wagerStage", 1);
        string output = Path.GetFullPath("../../artifacts/unity-smoke/betting");
        Directory.CreateDirectory(output);
        for (int i = 0; i < 5; i++) yield return null;
        CaptureGameView(Path.Combine(output, "01-bet-crap.png"));
        ((IPlay.Demo.WagerBook)Get(c, "wagerBook")).Open((string)Get(c, "shooterId"), 10,
            Time.unscaledTimeAsDouble);
        Set(c, "draftOutcome", IPlay.Demo.WagerOutcome.Crap);
        Set(c, "wagerStage", 2);
        yield return null;
        CaptureGameView(Path.Combine(output, "02-number.png"));
        Set(c, "draftNumber", 10);
        Set(c, "wagerStage", 3);
        yield return null;
        CaptureGameView(Path.Combine(output, "03-bills.png"));
        var offered = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p1", (string)Get(c, "shooterId"),
            IPlay.Demo.WagerOutcome.Crap, 10, 20);
        Check(offered != null, "Ground offer could not be created");
        Set(c, "wagerOverlayOpen", false);
        yield return null;
        CaptureGameView(Path.Combine(output, "04-open-ground-lock.png"));
        Check((bool)Call(c, "AcceptWager", offered.Id, offered.To), "Ground offer could not be accepted");
        float settledAt = Time.realtimeSinceStartup + 0.55f;
        while (Time.realtimeSinceStartup < settledAt) yield return null;
        CaptureGameView(Path.Combine(output, "05-closed-ground-lock.png"));
        Debug.Log("BETTING PHOTO CAPTURE PASSED: opponent dice, bill selection and recipient ground lock");
    }

    private static IEnumerator CaptureSale()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        string output = Path.GetFullPath("../../artifacts/unity-smoke/sale");
        Directory.CreateDirectory(output);
        Call(c, "StartLocalDemo");
        Call(c, "SellCurrentDice");
        Check((string)Get(c, "phase") == "SellingDice", "Seller did not open the auction");
        yield return null;
        CaptureGameView(Path.Combine(output, "01-seller-waiting.png"));

        Call(c, "StartLocalDemo");
        Call(c, "CycleDemoShooter");
        Call(c, "SellCurrentDice");
        Check((string)Get(c, "phase") == "SellingDice" && (string)Get(c, "shooterId") == "p3",
            "Bot seller did not open the auction");
        yield return null;
        CaptureGameView(Path.Combine(output, "02-bid-pad.png"));
        Set(c, "saleBidDigits", "12");
        Call(c, "PlaceDiceSaleBid");
        Set(c, "localSaleClosesAt", Time.unscaledTime - 0.01f);
        Call(c, "ResolveLocalDiceSale");
        Check((string)Get(c, "shooterId") == "p1", "Winning bidder did not receive the dice");
        Check((string)Get(c, "catcherId") != "p3", "Seller was made the buyer's catcher");
        Check((int)Call(c, "Balance", "p1") == 988, "Buyer was not charged at sale settlement");
        Check((int)Call(c, "Balance", "p3") == 1012, "Seller was not paid at sale settlement");
        yield return null;
        CaptureGameView(Path.Combine(output, "03-sale-complete.png"));
        Debug.Log("SELL VISUAL CAPTURE PASSED: seller, bidder, payment and catcher exclusion");
    }

    private static IEnumerator CaptureAddOns()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        Call(c, "StartLocalDemo");
        Call(c, "CycleDemoShooter");
        Call(c, "CommitShoot");
        Set(c, "phase", "Point");
        Set(c, "point", "10");
        var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        book.Open((string)Get(c, "shooterId"), 10, Time.unscaledTimeAsDouble);
        var source = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p1", "p3", IPlay.Demo.WagerOutcome.Crap, 10, 5);
        Check(source != null, "Original point wager could not be offered");
        Check((bool)Call(c, "AcceptWager", source.Id, "p3"), "Original point wager was not accepted");
        book.Open((string)Get(c, "shooterId"), 10, Time.unscaledTimeAsDouble - 20);
        Set(c, "bettingClosesAt", Time.unscaledTime - 1f);
        Set(c, "wagerOverlayOpen", true);
        Set(c, "wagerTarget", (string)Get(c, "shooterId"));
        Set(c, "wagerStage", 1);
        string output = Path.GetFullPath("../../artifacts/unity-smoke/add-ons");
        Directory.CreateDirectory(output);
        yield return null;
        CaptureGameView(Path.Combine(output, "01-opponent-bet-dice.png"));
        Set(c, "draftOutcome", IPlay.Demo.WagerOutcome.Crap);
        Set(c, "wagerStage", 2);
        yield return null;
        CaptureGameView(Path.Combine(output, "02-paired-number.png"));
        Call(c, "OfferPointAddOn", source, IPlay.Demo.WagerAddOnKind.DoubleUp);
        Call(c, "OfferPointAddOn", source, IPlay.Demo.WagerAddOnKind.PairedNumber);
        Check(book.Offers.Count(offer => offer.SourceOfferId == source.Id &&
            offer.Status == IPlay.Demo.WagerStatus.Offered) == 2, "Add-on requests were not independent");
        Debug.Log("ADD-ON VISUAL CAPTURE PASSED: paired number uses opponent digital dice");
    }

    [Serializable] private sealed class RealSeatFixture
    {
        public string playerId, playerSessionToken;
    }

    [Serializable] private sealed class PreparedFixture { public string rollId; }

    private static IEnumerator VerifyRealOnlineSeats()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        const string server = "http://127.0.0.1:5134";
        Set(c, "baseUrl", server);
        c.StartCoroutine((IEnumerator)Call(c, "CreateRealTable"));
        float timeout = Time.realtimeSinceStartup + 10f;
        while (((string)Get(c, "gameId")).Length == 0 ||
            ((System.Collections.IDictionary)Get(c, "playerTokens")).Count == 0)
        {
            Check(Time.realtimeSinceStartup < timeout, "Real host could not claim one table seat");
            yield return null;
        }
        string table = (string)Get(c, "gameId");
        Check((string)Get(c, "localPlayerId") == "p1", "Host did not receive first-person seat");
        Check(((System.Collections.IDictionary)Get(c, "playerTokens")).Count == 1,
            "Host incorrectly claimed another player's seat");

        string url = server + "/api/street-dice/" + table;
        using var join = FixturePost(url + "/join-real", "{\"playerName\":\"Guest\"}");
        var joinOperation = join.SendWebRequest();
        while (!joinOperation.isDone) yield return null;
        Check(join.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Guest could not join the host table");
        var guest = JsonUtility.FromJson<RealSeatFixture>(join.downloadHandler.text);
        Check(guest.playerId == "p2", "Guest did not receive the second seat");
        timeout = Time.realtimeSinceStartup + 4f;
        while (((Array)Get(c, "onlinePlayers")).Length < 2)
        {
            Check(Time.realtimeSinceStartup < timeout, "Host did not see guest seat");
            yield return null;
        }
        Call(c, "PassLocalDice");
        timeout = Time.realtimeSinceStartup + 4f;
        while ((string)Get(c, "shooterId") != "p2")
        {
            Check(Time.realtimeSinceStartup < timeout, "Pass did not hand dice to guest");
            yield return null;
        }
        using var shot = FixturePost(url + "/shot", "{\"shooterId\":\"p2\",\"shooterSessionToken\":\"" +
            guest.playerSessionToken + "\",\"catcherId\":\"p1\",\"amount\":20}");
        var shotOperation = shot.SendWebRequest();
        while (!shotOperation.isDone) yield return null;
        Check(shot.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Guest could not open a covered shot");
        float waitUntil = Time.realtimeSinceStartup + 10f;
        while (Time.realtimeSinceStartup < waitUntil) yield return null;
        using var heartbeat = FixturePost(url + "/presence/heartbeat", "{\"playerId\":\"p2\",\"playerSessionToken\":\"" +
            guest.playerSessionToken + "\"}");
        var heartbeatOperation = heartbeat.SendWebRequest();
        while (!heartbeatOperation.isDone) yield return null;
        Check(heartbeat.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Guest heartbeat did not preserve its seat");
        waitUntil = Time.realtimeSinceStartup + 6f;
        while (Time.realtimeSinceStartup < waitUntil) yield return null;
        using var prepare = FixturePost(url + "/roll/prepare", "{\"shooterId\":\"p2\",\"playerSessionToken\":\"" +
            guest.playerSessionToken + "\",\"power\":0.5,\"aim\":0}");
        var prepareOperation = prepare.SendWebRequest();
        while (!prepareOperation.isDone) yield return null;
        Check(prepare.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Guest could not prepare a server roll");
        var prepared = JsonUtility.FromJson<PreparedFixture>(prepare.downloadHandler.text);
        waitUntil = Time.realtimeSinceStartup + 1.5f;
        while (Time.realtimeSinceStartup < waitUntil) yield return null;
        using var commit = FixturePost(url + "/roll/commit", "{\"shooterId\":\"p2\",\"playerSessionToken\":\"" +
            guest.playerSessionToken + "\",\"rollId\":\"" + prepared.rollId + "\"}");
        var commitOperation = commit.SendWebRequest();
        while (!commitOperation.isDone) yield return null;
        Check(commit.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Guest could not commit a server roll");
        timeout = Time.realtimeSinceStartup + 5f;
        while ((int)Get(c, "lastSeenCommittedRoll") < 1)
        {
            Check(Time.realtimeSinceStartup < timeout, "Host did not receive guest's committed dice");
            yield return null;
        }
        Check((bool)Get(c, "remoteReplayInProgress"), "Host did not begin guest's dice replay");
        Check(((Vector3)Call(c, "RelativeRemoteThrowOffset", "p2")).z > 0f,
            "Guest roll did not enter from its remote side");
        for (int i = 0; i < 3; i++) yield return null;
        string output = Path.GetFullPath("../../artifacts/unity-smoke/readiness");
        Directory.CreateDirectory(output);
        CaptureGameView(Path.Combine(output, "real-online-guest-roll.png"));
        Debug.Log("REAL ONLINE SEATS PASSED: host claimed one seat, guest joined, dice passed, and guest's server roll reached host for fixed-camera replay. Editor only; no APK built.");
    }

    private static UnityEngine.Networking.UnityWebRequest FixturePost(string url, string body)
    {
        var request = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private static IEnumerator VerifyOnlinePhysical()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        Set(c, "baseUrl", "http://127.0.0.1:5134");
        Set(c, "mainOptions", false);
        c.StartCoroutine((IEnumerator)Call(c, "CreateTable"));
        float timeout = Time.realtimeSinceStartup + 10f;
        while (((string)Get(c, "gameId")).Length == 0 || ((System.Collections.IDictionary)Get(c, "playerTokens")).Count < 4)
        {
            Check(Time.realtimeSinceStartup < timeout, "Online fixture could not create and join the table");
            yield return null;
        }
        var seats = (Array)Get(c, "mics");
        for (int i = 1; i < seats.Length; i++)
        {
            var seat = seats.GetValue(i);
            var root = (GameObject)seat.GetType().GetProperty("Root").GetValue(seat);
            Check(!root.activeSelf, "Prototype 3D microphone occluded the online scene");
        }
        c.StartCoroutine((IEnumerator)Call(c, "OpenShot"));
        timeout = Time.realtimeSinceStartup + 5f;
        while (!(bool)Get(c, "shotCommitted"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Online fixture could not open a covered shot: "
                + Get(c, "result") + " shooter=" + Get(c, "shooterId") + " catcher=" + Get(c, "catcherId"));
            yield return null;
        }
        timeout = Time.realtimeSinceStartup + 5f;
        while (!(bool)Get(c, "onlineWagerWindowKnown"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Online fixture did not receive the server betting window");
            yield return null;
        }
        var tokens = (System.Collections.IDictionary)Get(c, "playerTokens");
        string gameId = (string)Get(c, "gameId");
        string offerJson = JsonUtility.ToJson(new RemoteWagerOfferDto
        {
            fromId = "p3", playerSessionToken = (string)tokens["p3"], toId = "p1",
            outcome = "Crap", amount = 5
        });
        using (var offerRequest = new UnityEngine.Networking.UnityWebRequest(
            "http://127.0.0.1:5134/api/street-dice/" + gameId + "/wager/offer", "POST"))
        {
            offerRequest.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(offerJson));
            offerRequest.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            offerRequest.SetRequestHeader("Content-Type", "application/json");
            var offered = offerRequest.SendWebRequest();
            while (!offered.isDone) yield return null;
            Check(offerRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
                "Remote seat could not offer a paired CRAP wager: " + offerRequest.error);
            int id = JsonUtility.FromJson<RemoteWagerResponseDto>(offerRequest.downloadHandler.text).offer.id;
            timeout = Time.realtimeSinceStartup + 3f;
            while (!HasOnlineWager(c, id, "Offered"))
            {
                Check(Time.realtimeSinceStartup < timeout, "Unity did not display the server's incoming CRAP offer");
                yield return null;
            }
            string wagerOutput = Path.GetFullPath("../../artifacts/unity-smoke/readiness");
            Directory.CreateDirectory(wagerOutput);
            for (int i = 0; i < 3; i++) yield return null;
            CaptureGameView(Path.Combine(wagerOutput, "online-incoming-crap-lock.png"));
            Call(c, "AcceptWager", id, "p1");
            timeout = Time.realtimeSinceStartup + 3f;
            while (!HasOnlineWager(c, id, "Accepted"))
            {
                Check(Time.realtimeSinceStartup < timeout, "Tapping the incoming Unity lock did not accept the server wager");
                yield return null;
            }
            for (int i = 0; i < 3; i++) yield return null;
            CaptureGameView(Path.Combine(wagerOutput, "online-accepted-crap-lock.png"));
        }
        timeout = Time.realtimeSinceStartup + 20f;
        while (Time.unscaledTimeAsDouble < (double)Get(c, "onlineShooterDeadline"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Online shooter acceptance window never closed");
            yield return null;
        }
        Set(c, "throwPower", 0.6f);
        Set(c, "throwAim", -0.2f);
        Set(c, "throwLeadIn", 0.24f);
        c.StartCoroutine((IEnumerator)Call(c, "RollCurrentMode"));
        timeout = Time.realtimeSinceStartup + 5f;
        while (Get(c, "serverLaunch") == null)
        {
            Check(Time.realtimeSinceStartup < timeout, "Normal online roll did not use server prepare");
            yield return null;
        }
        Check(Get(c, "serverReplay") == null, "Server faces were presented during the fade window");
        string output = Path.GetFullPath("../../artifacts/unity-smoke/readiness");
        Directory.CreateDirectory(output);
        CaptureGameView(Path.Combine(output, "online-physical-launch.png"));
        timeout = Time.realtimeSinceStartup + 12f;
        while (Get(c, "serverReplay") == null)
        {
            Check(Time.realtimeSinceStartup < timeout, "Server commit did not produce a physical replay");
            yield return null;
        }
        CaptureGameView(Path.Combine(output, "online-physical-replay.png"));
        while ((RollState)Get(c, "rollState") != RollState.Locked)
        {
            Check(Time.realtimeSinceStartup < timeout, "Online physical dice never reached roll lock");
            yield return null;
        }
        CaptureGameView(Path.Combine(output, "online-physical-locked.png"));
        while ((bool)Get(c, "rolling"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Normal online physical roll did not finish");
            yield return null;
        }
        int a = (int)Get(c, "die1"), b = (int)Get(c, "die2");
        Check(a is >= 0 and <= 6 && b is >= 0 and <= 6, "Server dice faces were not applied");
        using var request = UnityEngine.Networking.UnityWebRequest.Get("http://127.0.0.1:5134/api/street-dice/" + Get(c, "gameId"));
        var readback = request.SendWebRequest();
        while (!readback.isDone) yield return null;
        Check(request.result == UnityEngine.Networking.UnityWebRequest.Result.Success, "Online state could not be read back");
        Check(!request.downloadHandler.text.Contains("\"pendingRoll\":{", StringComparison.Ordinal),
            "Pending public roll was not cleared after commit");
        Debug.Log("ONLINE PHYSICAL PASSED: Unity displayed and accepted an authenticated peer lock, waited for the server deadline, then used prepare/commit and applied server faces. Local Editor fixture only.");
    }

    private static IEnumerator VerifyOnlineCatcherFade()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        Set(c, "baseUrl", "http://127.0.0.1:5134");
        Set(c, "mainOptions", false);
        c.StartCoroutine((IEnumerator)Call(c, "CreateTable"));
        float timeout = Time.realtimeSinceStartup + 10f;
        while (((System.Collections.IDictionary)Get(c, "playerTokens")).Count < 4)
        {
            Check(Time.realtimeSinceStartup < timeout, "Catcher fixture could not join four seats");
            yield return null;
        }
        string gameId = (string)Get(c, "gameId");
        var tokens = (System.Collections.IDictionary)Get(c, "playerTokens");
        string passJson = JsonUtility.ToJson(new RemotePlayerActionDto
        {
            playerId = "p1", playerSessionToken = (string)tokens["p1"]
        });
        using (var pass = new UnityEngine.Networking.UnityWebRequest(
            "http://127.0.0.1:5134/api/street-dice/" + gameId + "/pass", "POST"))
        {
            pass.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(passJson));
            pass.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            pass.SetRequestHeader("Content-Type", "application/json");
            var passed = pass.SendWebRequest();
            while (!passed.isDone) yield return null;
            Check(pass.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
                "Initial shooter could not pass clockwise to the catcher: " + pass.error);
        }
        Set(c, "shooterId", "p2");
        Set(c, "catcherId", "p1");
        Set(c, "shotAmount", 20);
        c.StartCoroutine((IEnumerator)Call(c, "OpenShot"));
        timeout = Time.realtimeSinceStartup + 5f;
        while (!(bool)Get(c, "shotCommitted") || (string)Get(c, "shooterId") != "p2")
        {
            Check(Time.realtimeSinceStartup < timeout, "Catcher fixture could not open p2's shot: " + Get(c, "result"));
            yield return null;
        }
        timeout = Time.realtimeSinceStartup + 5f;
        while (!(bool)Get(c, "onlineWagerWindowKnown"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Catcher fixture did not receive the server betting window");
            yield return null;
        }
        timeout = Time.realtimeSinceStartup + 20f;
        while (Time.unscaledTimeAsDouble < (double)Get(c, "onlineShooterDeadline"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Remote shooter acceptance window never closed");
            yield return null;
        }
        string prepareJson = JsonUtility.ToJson(new RemotePrepareDto
        {
            shooterId = "p2",
            playerSessionToken = (string)tokens["p2"],
            power = 0.6f
        });
        using var prepare = new UnityEngine.Networking.UnityWebRequest(
            "http://127.0.0.1:5134/api/street-dice/" + gameId + "/roll/prepare", "POST");
        prepare.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(prepareJson));
        prepare.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        prepare.SetRequestHeader("Content-Type", "application/json");
        var launched = prepare.SendWebRequest();
        while (!launched.isDone) yield return null;
        Check(prepare.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Remote shooter fixture could not prepare physical roll: " + prepare.error);
        timeout = Time.realtimeSinceStartup + 1.5f;
        while (string.IsNullOrEmpty((string)Get(c, "pendingRemoteRollId")))
        {
            Check(Time.realtimeSinceStartup < timeout, "Catcher Unity did not discover the public pending roll");
            yield return null;
        }
        Check((bool)Get(c, "rolling") && Get(c, "serverReplay") == null,
            "Catcher UI had no fade window or leaked the replay");
        c.StartCoroutine((IEnumerator)Call(c, "Fade"));
        timeout = Time.realtimeSinceStartup + 8f;
        while (!(bool)Get(c, "klingCatchActive"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Catcher FADE did not launch the hand-only Kling clip: " + Get(c, "result"));
            yield return null;
        }
        for (int i = 0; i < 4; i++) yield return null;
        string output = Path.GetFullPath("../../artifacts/unity-smoke/readiness");
        Directory.CreateDirectory(output);
        CaptureGameView(Path.Combine(output, "online-catcher-fade.png"));
        while ((bool)Get(c, "fadeInProgress"))
        {
            Check(Time.realtimeSinceStartup < timeout, "Online catcher fade did not complete");
            yield return null;
        }
        using var state = UnityEngine.Networking.UnityWebRequest.Get("http://127.0.0.1:5134/api/street-dice/" + gameId);
        var readback = state.SendWebRequest();
        while (!readback.isDone) yield return null;
        Check(state.result == UnityEngine.Networking.UnityWebRequest.Result.Success,
            "Catcher state readback failed");
        string body = state.downloadHandler.text;
        Check(body.Contains("\"fadeCount\":1", StringComparison.Ordinal)
            && body.Contains("\"shooterId\":\"p2\"", StringComparison.Ordinal)
            && body.Contains("\"pendingRoll\":null", StringComparison.Ordinal),
            "Online catcher fade did not preserve the shooter and clear the roll");
        Debug.Log("ONLINE CATCHER FADE PASSED: p1 discovered p2's pending roll, used roll-ID fade, played hand-only Kling, kept shooter and wager unchanged. Local Editor fixture only.");
    }

    [Serializable] private sealed class RemotePrepareDto
    {
        public string shooterId, playerSessionToken;
        public float power, aim;
        public bool leftHanded;
    }
    [Serializable] private sealed class RemotePlayerActionDto
    {
        public string playerId, playerSessionToken;
    }
    [Serializable] private sealed class RemoteWagerOfferDto
    {
        public string fromId, playerSessionToken, toId, outcome;
        public int number, amount;
    }
    [Serializable] private sealed class RemoteWagerResponseDto { public RemoteWagerIdDto offer; }
    [Serializable] private sealed class RemoteWagerIdDto { public int id; }

    private static bool HasOnlineWager(object controller, int id, string status)
    {
        var offers = (System.Collections.IEnumerable)Get(controller, "onlineWagerOffers");
        foreach (var offer in offers)
            if ((int)offer.GetType().GetProperty("Id").GetValue(offer) == id &&
                offer.GetType().GetProperty("Status").GetValue(offer).ToString() == status) return true;
        return false;
    }

    private static IEnumerator Verify()
    {
        yield return null;
        var c = UnityEngine.Object.FindAnyObjectByType<StreetDiceGreyboxController>();
        Check(c != null, "Runtime bootstrap missing");
        int activeListeners = 0;
        foreach (var listener in UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            if (listener.isActiveAndEnabled) activeListeners++;
        Check(activeListeners == 1 && Camera.main.GetComponent<AudioListener>() != null,
            "Gameplay must have exactly one active audio listener on the main camera");
        Debug.Log("AUDIO ROUTING PASSED: one active listener attached to the gameplay camera. Audible device output is not covered.");
        VerifyCatchGrades();
        VerifySkyCamFraming();
        Set(c, "moneyAnimationEnabled", false);
        Set(c, "botWagersEnabled", false);
        string output = Path.GetFullPath("../../artifacts/unity-smoke/readiness");
        Directory.CreateDirectory(output);
        for (int i = 0; i < 5; i++) yield return null;
        var cameraLifecycle = VerifySkyCameraLifecycle(c);
        while (cameraLifecycle.MoveNext()) yield return null;
        var fadeSettings = VerifyFadeSettings(c, output);
        while (fadeSettings.MoveNext()) yield return null;
        CaptureGameView(Path.Combine(output, "main-options.png"));
        var moneyCleanup = VerifyMoneySessionCleanup(c);
        while (moneyCleanup.MoveNext()) yield return null;
        Call(c, "StartLocalDemo");
        VerifyDiceCalling(c);
        VerifyBotMicPresentation(c);
        VerifyCompactPhaseHud(c);
        Call(c, "CommitShoot");
        for (int i = 0; i < 3; i++) yield return null;
        VerifyIPlayFont(c);
        int shooterSeconds = (int)Call(c, "CurrentBettingCountdown", Time.unscaledTimeAsDouble);
        Check(shooterSeconds == 15, "Shooter did not receive a continuous 15-second countdown");
        double shooterDeadline = (double)Call(c, "CurrentBettingDeadline");
        Check((int)Call(c, "CurrentBettingCountdown", shooterDeadline - 14d) == 14 &&
            (int)Call(c, "CurrentBettingCountdown", shooterDeadline - 5d) == 5 &&
            (int)Call(c, "CurrentBettingCountdown", shooterDeadline) == 0,
            "Shooter countdown did not track the full 15-second deadline");
        CaptureGameView(Path.Combine(output, "betting-countdown-shooter-15.png"));
        var countdownBook = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        countdownBook.Open("p1", 0, Time.unscaledTimeAsDouble - 0.62d);
        for (int i = 0; i < 2; i++) yield return null;
        Check((int)Call(c, "CurrentBettingCountdown", Time.unscaledTimeAsDouble) == 15,
            "Shooter countdown changed digits before a real second elapsed");
        CaptureGameView(Path.Combine(output, "betting-countdown-shooter-15-approach.png"));
        Set(c, "shooterId", "p3");
        Set(c, "catcherId", "p2");
        Set(c, "phase", "Point");
        Set(c, "point", "10");
        Call(c, "OpenBettingWindow");
        int playerSeconds = (int)Call(c, "CurrentBettingCountdown", Time.unscaledTimeAsDouble);
        Check(playerSeconds == 10, "Non-shooter did not receive a 10-second countdown");
        double playerDeadline = (double)Call(c, "CurrentBettingDeadline");
        Check((int)Call(c, "CurrentBettingCountdown", playerDeadline - 9d) == 9 &&
            (int)Call(c, "CurrentBettingCountdown", playerDeadline - 1d) == 1 &&
            (int)Call(c, "CurrentBettingCountdown", playerDeadline) == 0,
            "Non-shooter countdown did not track the 10-second public deadline");
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "betting-countdown-player-10.png"));
        Set(c, "wagerOverlayOpen", true);
        Set(c, "wagerTarget", "p4");
        Call(c, "SetWagerStage", 1);
        Set(c, "wagerStageAt", Time.unscaledTime - 1);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "wager-outcomes.png"));
        Set(c, "draftOutcome", IPlay.Demo.WagerOutcome.Hit);
        Call(c, "SetWagerStage", 2);
        Set(c, "wagerStageAt", Time.unscaledTime - 1);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "wager-numbers.png"));
        Set(c, "draftNumber", 4);
        Call(c, "SetWagerStage", 3);
        Set(c, "wagerStageAt", Time.unscaledTime - 1);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "wager-amounts.png"));
        foreach (string field in new[] { "wagerOpenIcon", "wagerClosedIcon" })
        {
            var target = (RenderTexture)Get(c, field);
            var lockPrevious = RenderTexture.active;
            RenderTexture.active = target;
            var lockPixels = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            lockPixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            lockPixels.Apply();
            Check(lockPixels.GetPixel(0, 0).a < 0.01f, "Lock background is not transparent");
            Check(lockPixels.GetPixel(128, 120).a > 0.95f, "Lock body disappeared during background removal");
            UnityEngine.Object.DestroyImmediate(lockPixels);
            RenderTexture.active = lockPrevious;
        }
        var controls = VerifyWagerControls(c, output);
        while (controls.MoveNext()) yield return null;
        var opponents = VerifyOpponentResponses(c);
        while (opponents.MoveNext()) yield return null;
        var lowFunds = VerifyLowFunds(c);
        while (lowFunds.MoveNext()) yield return null;
        foreach (int amount in new[] { 1, 5, 10, 20 })
        {
            Call(c, "StartLocalDemo");
            Call(c, "SelectMainWager", amount);
            Check((int)Get(c, "shotAmount") == amount, "Wager preset not selected");
            Call(c, "CommitShoot");
            Call(c, "SelectMainWager", amount == 20 ? 1 : 20);
            Check((int)Get(c, "shotAmount") == amount, "Live wager was changed");
            Set(c, "selectedSideWager", amount);
            Call(c, "AddCashBet", "p3", "ComeOutLoss");
            Call(c, "RefreshGroundMoney");
            Check((int)Call(c, "GroundWager", "p1") == amount * 2, "Ground wager does not include matching side stake");
            var piles = (List<GameObject>)Get(c, "moneyPiles");
            Check(piles[1].transform.GetChild(1).name == "$" + amount + " iPlay prop note", "Wrong denomination on ground");
            Call(c, "LeaveLocalGame");
            var balances = (Dictionary<string, int>)Get(c, "cash");
            Check(balances["p1"] == 1000 - amount * 2 && balances["p2"] == 1000 + amount && balances["p3"] == 1000 + amount, "Denomination forfeit mismatch");
        }
        Check(((Dictionary<int, Material>)Get(c, "billMaterials")).Count == 4, "Missing denomination texture");
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        var timedBets = (System.Collections.IList)Get(c, "cashBets");
        int timedBetCount = timedBets.Count;
        Set(c, "bettingClosesAt", Time.unscaledTime - 0.1f);
        Call(c, "AddCashBet", "p3", "ComeOutLoss");
        Check(timedBets.Count == timedBetCount, "Side bet was accepted after the betting timer closed");
        Call(c, "StartLocalDemo");
        Call(c, "SelectMainWager", 3);
        Check((int)Get(c, "shotAmount") == 20, "Unsupported preset accepted");
        Call(c, "CommitShoot");
        Call(c, "ResolveLocalRoll", 7);
        var doubleUp = (IEnumerator)Call(c, "DoubleUp");
        while (doubleUp.MoveNext()) yield return null;
        Set(c, "shotCommitted", true);
        Call(c, "RefreshGroundMoney");
        var doublePile = ((List<GameObject>)Get(c, "moneyPiles"))[0];
        int individualTwenties = 0;
        foreach (Transform paper in doublePile.transform)
            if (paper.gameObject.activeSelf && paper.name == "$20 iPlay prop note") individualTwenties++;
        Check((int)Get(c, "shotAmount") == 40 && individualTwenties == 2, "Double Up did not show two individual twenties");
        Call(c, "StartLocalDemo");
        Check((bool)Get(c, "awaitingShootChoice"), "Shoot/pass choice missing");
        Call(c, "PassLocalDice");
        Check((string)Get(c, "shooterId") != "p1", "Pass did not hand off");
        Check(!(bool)Get(c, "shotCommitted"), "Pass committed wager");
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        Call(c, "ResolveLocalRoll", 10);
        Call(c, "PassLocalDice");
        Check((string)Get(c, "shooterId") == "p1" && (string)Get(c, "point") == "10", "Pass abandoned point");
        Call(c, "AddCashBet", "p3", "MissPointGroup");
        Call(c, "LeaveLocalGame");
        var cash = (Dictionary<string, int>)Get(c, "cash");
        Check(cash["p1"] == 970 && cash["p2"] == 1020 && cash["p3"] == 1010, "Forfeit payout mismatch");
        Call(c, "LeaveLocalGame");
        Check(cash["p1"] == 970, "Double charged leave");
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        Call(c, "BeginShake", new Vector2(Screen.width * 0.5f, Screen.height * 0.3f), -2);
        Check(!(bool)Get(c, "shakeHeld"), "Throw started before the betting timer closed");
        Set(c, "bettingClosesAt", Time.unscaledTime - 0.1f);
        var inputBook = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        inputBook.Open("p1", 0, Time.unscaledTimeAsDouble - 11);
        Call(c, "BeginShake", new Vector2(Screen.width * 0.5f, Screen.height * 0.3f), -2);
        Check(!(bool)Get(c, "shakeHeld"), "Throw bypassed shooter acceptance countdown");
        inputBook.Open("p1", 0, Time.unscaledTimeAsDouble - 16);
        Call(c, "BeginShake", new Vector2(Screen.width * 0.5f, Screen.height * 0.3f), -2);
        Check((bool)Get(c, "shakeHeld"), "Hold did not start shake");
        Call(c, "CancelShake");
        Check(!(bool)Get(c, "shakeHeld"), "Cancelled touch stuck");
        var gestureOrigin = new Vector2(Screen.width * 0.5f, Screen.height * 0.3f);
        Call(c, "BeginShake", gestureOrigin, -2);
        Set(c, "edgeDrag", true);
        Call(c, "OnApplicationPause", true);
        Check(!(bool)Get(c, "shakeHeld") && (int)Get(c, "touchId") == -1 && !(bool)Get(c, "edgeDrag"), "Pause retained unfinished input");
        Call(c, "EndShake", gestureOrigin + Vector2.up * Screen.height * 0.3f);
        Call(c, "BeginShake", gestureOrigin, -2);
        Check(!(bool)Get(c, "rolling") && !(bool)Get(c, "shakeHeld"), "Paused input started a throw");
        Call(c, "OnApplicationPause", false);
        Call(c, "EndShake", gestureOrigin + Vector2.up * Screen.height * 0.3f);
        Check(!(bool)Get(c, "rolling"), "Stale release threw dice after resume");
        Call(c, "BeginShake", gestureOrigin, -2);
        Check((bool)Get(c, "shakeHeld"), "Resume blocked a fresh gesture");
        Set(c, "edgeDrag", true);
        Call(c, "OnApplicationFocus", false);
        Check(!(bool)Get(c, "shakeHeld") && !(bool)Get(c, "edgeDrag"), "Focus loss retained unfinished input");
        Check(cash["p1"] == 1000, "Input interruption changed the player's money");
        Debug.Log("INTERRUPTION INPUT PASSED: pause/focus cancel shake and edge swipe; paused/stale release ignored; fresh gesture accepted after resume. Callback simulation, not physical phone coverage.");
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        Call(c, "ResolveLocalRoll", 10);
        Call(c, "OpenBettingWindow");
        var liveOffer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p3", "p4", IPlay.Demo.WagerOutcome.Hit, 4, 10);
        Check(liveOffer != null, "Legal spectator HIT 4 offer failed");
        Check((bool)Call(c, "AcceptWager", liveOffer.Id, "p4"), "Recipient could not accept offer");
        Check((int)Call(c, "GroundWager", "p3") == 10 && (int)Call(c, "GroundWager", "p4") == 10, "Accepted stakes missing from ground");
        var liveBook = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        liveBook.Open("p1", 10, Time.unscaledTimeAsDouble - 16);
        liveBook.BeginRoll(Time.unscaledTimeAsDouble);
        Call(c, "ResolveLocalRoll", 4);
        Check((int)Call(c, "Balance", "p3") == 1010 && (int)Call(c, "Balance", "p4") == 990, "Spectator payout used the wrong counterparties");
        Check((string)Get(c, "point") == "10", "Grouped wager changed shooter point");
        Call(c, "ResolveCashSideBets", 4);
        Check((int)Call(c, "Balance", "p3") == 1010, "Spectator wager paid twice");
        Call(c, "StartLocalDemo");
        foreach (bool left in new[] { false, true })
        foreach (bool snap in new[] { false, true })
        {
            Set(c, "leftHanded", left);
            Set(c, "snapStyle", snap);
            Call(c, "SelectThrowHand");
            c.PrepareRollPreviewForEditor();
            for (int i = 0; i <= 90; i++)
            {
                c.SampleRollPresentation(i / 60f);
                Check(Camera.main.WorldToViewportPoint(c.ThrowWrist.position).y <= -0.099f, "Wrist escaped crop");
            }
            c.SampleRollPresentation(FirstPersonDiceHand.ReleaseTime - 0.00001f);
            var dice = (GameObject[])Get(c, "replayDice");
            Vector3 held = dice[0].transform.position;
            c.SampleRollPresentation(FirstPersonDiceHand.ReleaseTime);
            Check(Vector3.Distance(held, dice[0].transform.position) < 0.001f, "Dice teleported at release");
        }
        Set(c, "leftHanded", false);
        Set(c, "snapStyle", false);
        Call(c, "SelectThrowHand");
        ThrowMotionVerification.Verify(c, Camera.main);
        Set(c, "shooterId", "p1");
        int wallHits = 0;
        foreach (float power in new[] { 0.2f, 0.5f, 1f })
        foreach (float aim in new[] { -1f, 0f, 1f })
        {
            Set(c, "throwPower", power);
            Set(c, "throwAim", aim);
            c.PrepareRollPreviewForEditor(713);
            var replay = (DicePhysicsReplay)Get(c, "activeReplay");
            foreach (var impact in replay.Impacts)
            {
                Check(impact.Strength >= 0 && impact.Strength <= 1, "Impact strength outside normalized range");
                if (impact.Surface == "Metal" || impact.Surface == "Brick") wallHits++;
            }
        }
        Check(wallHits > 0, "Strong throws never hit a wall");
        Set(c, "throwAim", 0f);
        Set(c, "throwPower", 0.15f);
        c.PrepareRollPreviewForEditor(713);
        Vector3 weakEarly = ((DicePhysicsReplay)Get(c, "activeReplay")).Sample(0.15f, 0).position;
        Set(c, "throwPower", 1f);
        c.PrepareRollPreviewForEditor(713);
        Vector3 strongEarly = ((DicePhysicsReplay)Get(c, "activeReplay")).Sample(0.15f, 0).position;
        Check(strongEarly.z > weakEarly.z + 0.35f, "Swipe power did not change early forward travel");
        Set(c, "throwPower", 0.5f);
        Set(c, "throwAim", -1f);
        c.PrepareRollPreviewForEditor(713);
        Vector3 leftEarly = ((DicePhysicsReplay)Get(c, "activeReplay")).Sample(0.15f, 0).position;
        Set(c, "throwAim", 1f);
        c.PrepareRollPreviewForEditor(713);
        Vector3 rightEarly = ((DicePhysicsReplay)Get(c, "activeReplay")).Sample(0.15f, 0).position;
        Check(rightEarly.x > leftEarly.x + 0.25f, "Swipe aim did not change early lateral travel");
        Debug.Log("GESTURE PHYSICS PASSED: weak and strong launches separate early; left and right aim diverge in the expected direction.");
        Set(c, "throwPower", 0.5f);
        Set(c, "throwAim", 0f);
        Set(c, "rolling", false);
        Set(c, "gameMode", GameMode.CeeLo);
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        Call(c, "ResolveLocalCeeLo", 2, 2, 4);
        Check((string)Get(c, "shooterId") != "p1", "Cee-lo banker never yielded to players");
        for (int i = 0; i < 4; i++) Call(c, "ResolveLocalCeeLo", 4, 5, 6);
        Check((bool)Get(c, "awaitingShootChoice"), "Cee-lo round did not finish");
        Check(cash["p1"] == 920, "Cee-lo banker did not settle all four opponents");
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        Call(c, "ResolveLocalCeeLo", 2, 2, 4);
        Call(c, "LeaveLocalGame");
        Check(cash["p1"] == 920, "Leaving banker did not forfeit all pending opponents");
        Set(c, "gameMode", GameMode.Craps);
        Call(c, "StartLocalDemo");
        Call(c, "CommitShoot");
        c.PrepareRollPreviewForEditor();
        c.SampleRollPresentation(c.RollPreviewDuration);
        Set(c, "rolling", false);
        Call(c, "UpdateSkyCam");
        var sky = (Camera)Get(c, "skyCamera");
        foreach (string field in new[] { "dieAShadow", "dieBShadow", "dieCShadow" })
        {
            var shadow = (GameObject)Get(c, field);
            Check((sky.cullingMask & (1 << shadow.layer)) != 0, "Overhead camera excludes dice contact shadows");
            Check(shadow.GetComponent<Collider>() == null, "Visual contact shadow has a physical collider");
        }
        sky.Render();
        var previous = RenderTexture.active;
        RenderTexture.active = sky.targetTexture;
        var image = new Texture2D(768, 384, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 768, 384), 0, 0);
        image.Apply();
        var pixels = image.GetPixels32();
        int min = 255, max = 0;
        foreach (var pixel in pixels) { min = Math.Min(min, pixel.r); max = Math.Max(max, pixel.r); }
        Check(max - min > 35, "Sky cam is blank");
        File.WriteAllBytes(Path.Combine(output, "sky-cam.png"), image.EncodeToPNG());
        var contactRenderers = new List<Renderer>();
        foreach (string field in new[] { "dieAShadow", "dieBShadow", "dieCShadow" })
            contactRenderers.Add(((GameObject)Get(c, field)).GetComponent<Renderer>());
        try
        {
            foreach (var renderer in contactRenderers) renderer.enabled = false;
            sky.Render();
            RenderTexture.active = sky.targetTexture;
            image.ReadPixels(new Rect(0, 0, 768, 384), 0, 0);
            image.Apply();
            var withoutShadows = image.GetPixels32();
            int darkerPixels = 0;
            for (int i = 0; i < pixels.Length; i++)
                if (withoutShadows[i].r + withoutShadows[i].g + withoutShadows[i].b > pixels[i].r + pixels[i].g + pixels[i].b + 9) darkerPixels++;
            Check(darkerPixels >= 8, "Dice contact shadows do not visibly darken the overhead pavement");
            Debug.Log("CONTACT SHADOW PIXELS PASSED: " + darkerPixels + " overhead pixels visibly darkened by contact shadows.");
        }
        finally
        {
            foreach (var renderer in contactRenderers) renderer.enabled = true;
            sky.Render();
        }
        RenderTexture.active = previous;
        UnityEngine.Object.Destroy(image);
        bool wasRolling = (bool)Get(c, "rolling");
        Set(c, "rolling", true);
        Set(c, "skyCamVisible", true);
        Set(c, "skyResultVisible", false);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "sky-display.png"));
        Set(c, "skyResultDuration", 5f);
        Call(c, "BeginSkyResultHold", 6, 4, null);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "roll-result-static.png"));
        Set(c, "skyResultDuration", 0.8f);
        Call(c, "ResetDiceToShooter");
        Set(c, "rolling", wasRolling);
        Check(!(bool)Get(c, "skyCamVisible") && !(bool)Get(c, "skyResultVisible") && Get(c, "activeReplay") == null,
            "Sky display and dice did not clear/reset together");
        Set(c, "moneyAnimationEnabled", true);
        Set(c, "moneyTransferDuration", 3f);
        Call(c, "TransferCash", "p1", "p2", 40);
        for (int i = 0; i < 36; i++) yield return null;
        Check((bool)Get(c, "moneyTransferRunning"), "Money transfer animation did not run");
        var flyingBill = GameObject.Find("Settling $20");
        int flyingTwenties = 0;
        foreach (var candidate in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (candidate.name == "Settling $20") flyingTwenties++;
        Check(flyingBill != null && flyingTwenties == 2, "Two individual settlement bills missing");
        var pilesForTransfer = (List<GameObject>)Get(c, "moneyPiles");
        CaptureGameView(Path.Combine(output, "money-transfer.png"));
        Debug.Log("Money transfer height=" + flyingBill.transform.position.y + " source=" + pilesForTransfer[0].transform.position.y);
        Check(flyingBill.transform.position.y > pilesForTransfer[0].transform.position.y + 0.05f, "Settlement bill never lifted from pavement");
        Set(c, "moneyTransferDuration", 0.35f);
        float finishDeadline = Time.unscaledTime + 1f;
        while ((bool)Get(c, "moneyTransferRunning") && Time.unscaledTime < finishDeadline) yield return null;
        Check(!(bool)Get(c, "moneyTransferRunning"), "Money transfer animation did not finish");
        for (int i = 0; i < 5; i++) yield return null;
        CaptureGameView(Path.Combine(output, "in-game.png"));
        Call(c, "StartLocalDemo");
        float originalStreak = (float)Get(c, "streak");
        Set(c, "streak", 5f);
        Call(c, "ApplyDiceColor");
        for (int i = 0; i < 2; i++) yield return null;
        CaptureGameView(Path.Combine(output, "hot-meter-half.png"));
        Set(c, "streak", 10f);
        Call(c, "ApplyDiceColor");
        for (int i = 0; i < 2; i++) yield return null;
        CaptureGameView(Path.Combine(output, "hot-meter-full.png"));
        c.StartCoroutine((IEnumerator)Call(c, "AnimateDiceRoll", 3, 4, null));
        float smokeDeadline = Time.realtimeSinceStartup + 6f;
        bool capturedSmoke = false;
        while ((bool)Get(c, "rolling") && Time.realtimeSinceStartup < smokeDeadline)
        {
            var trails = (Dictionary<GameObject, TrailRenderer>)Get(c, "hotSmokeTrails");
            if (trails.Count >= 2 && trails.Values.Any(trail => trail.emitting && trail.positionCount > 2))
            {
                CaptureGameView(Path.Combine(output, "hot-dice-smoke-in-flight.png"));
                capturedSmoke = true;
                break;
            }
            yield return null;
        }
        Check(capturedSmoke, "Hot dice smoke did not follow the moving dice");
        while ((bool)Get(c, "rolling") && Time.realtimeSinceStartup < smokeDeadline) yield return null;
        Check(!(bool)Get(c, "rolling"), "Hot roll did not finish");
        Set(c, "streak", originalStreak);
        Call(c, "ApplyDiceColor");
        for (int style = 0; style < 9; style++)
        {
            Call(c, "StartLocalDemo");
            Set(c, "botWagersEnabled", false);
            Call(c, "CommitShoot");
            Set(c, "fadeCount", style % 3);
            Set(c, "catcherId", "p1");
            Set(c, "selectedFadeStyle", style % 3);
            Set(c, "selectedHandSkin", style / 3);
            var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
            book.Open("p1", 0, Time.unscaledTimeAsDouble - 16);
            book.BeginRoll(Time.unscaledTimeAsDouble);
            Set(c, "rolling", true);
            var a = (GameObject)Get(c, "dieA");
            var b = (GameObject)Get(c, "dieB");
            a.transform.position = new Vector3(-0.13f, -1.05f, 0f);
            b.transform.position = new Vector3(0.13f, -1.05f, 0f);
            a.SetActive(true); b.SetActive(true);
            c.StartCoroutine((IEnumerator)Call(c, "Fade"));
            float started = Time.unscaledTime;
            bool captured = false;
            var motionFiles = new List<string>();
            var motionTimes = new List<double>();
            long lastMotionFrame = -1;
            string motionFolder = Path.Combine(output, "motion-" + style);
            if (style < 3) Directory.CreateDirectory(motionFolder);
            while ((bool)Get(c, "fadeInProgress"))
            {
                Check(Time.unscaledTime - started < (style < 3 ? 7f : 3f), "Fade never completed");
                Check(!(bool)typeof(StreetDiceGreyboxController).GetProperty("CanGesture", Flags).GetValue(c), "Throw allowed during fade");
                Check((Camera.main.cullingMask & (1 << 30)) != 0, "Fade hid the environment layer");
                Check(!a.activeSelf && !b.activeSelf && !((GameObject)Get(c, "dieC")).activeSelf, "Dice remained visible after the fade killed the roll");
                if ((bool)Get(c, "klingCatchActive"))
                {
                    var motionPlayer = (UnityEngine.Video.VideoPlayer)Get(c, "catchVideo");
                    if (style < 3) motionPlayer.playbackSpeed = 0.25f;
                    if (style < 3 && motionPlayer.frame >= 0 && motionPlayer.frame != lastMotionFrame)
                    {
                        lastMotionFrame = motionPlayer.frame;
                        string frameFile = Path.Combine(motionFolder, "frame-" + motionFiles.Count.ToString("D3") + ".png");
                        motionFiles.Add(frameFile);
                        motionTimes.Add(motionPlayer.time);
                        CaptureGameView(frameFile);
                    }
                    Check(((Camera)Get(c, "skyCamera")).cullingMask == (1 << 31), "Fade display includes gameplay layers");
                }
                if (!captured && (bool)Get(c, "klingCatchActive") &&
                    ((UnityEngine.Video.VideoPlayer)Get(c, "catchVideo")).time > (style % 3 == 0 ? 0.15 : 0.36))
                {
                    if ((bool)Get(c, "klingCatchActive"))
                    {
                        var player = (UnityEngine.Video.VideoPlayer)Get(c, "catchVideo");
                        Check(player.isPrepared && player.frame > 0, "Kling fade capture has no decoded video frame");
                        string expected = new[] { "catch-tap-kling", "catch-plant-kling", "catch-wave-kling" }[style % 3];
                        Check(player.clip.name == expected, "Catcher's selected fade style was ignored");
                    }
                    CaptureGameView(Path.Combine(output, "fade-" + style + ".png"));
                    captured = true;
                }
                yield return null;
            }
            Check(captured, "Fade had no visible animation interval");
            ((UnityEngine.Video.VideoPlayer)Get(c, "catchVideo")).playbackSpeed = 1;
            Check(!(bool)Get(c, "rolling") && !(bool)Get(c, "skyCamVisible"), "Fade did not reset dice and display");
            Check(!book.CanOffer(Time.unscaledTimeAsDouble) && book.CanRoll(Time.unscaledTimeAsDouble),
                "Fade reopened betting or delayed the immediate rethrow");
            if (style < 3)
            {
                Check(motionFiles.Count >= 5, "Too few frames to review the live fade");
                var manifest = new List<string> { "ffconcat version 1.0" };
                for (int i = 0; i < motionFiles.Count; i++)
                {
                    manifest.Add("file '" + Path.GetFileName(motionFiles[i]) + "'");
                    double duration = i + 1 < motionFiles.Count ? System.Math.Max(1d / 60, motionTimes[i + 1] - motionTimes[i]) : 0.25;
                    manifest.Add("duration " + duration.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                }
                manifest.Add("file '" + Path.GetFileName(motionFiles[motionFiles.Count - 1]) + "'");
                File.WriteAllLines(Path.Combine(motionFolder, "frames.ffconcat"), manifest);
                Debug.Log("FADE MOTION CAPTURE style=" + style + " frames=" + motionFiles.Count + " clipTimestamped=true");
            }
        }
        Debug.Log("HAND-ONLY FADE PASSED: all three selected styles across three shades, all dice hidden throughout fade, overhead excludes gameplay layers, decoded video frames, throw lock and immediate rethrow without a new betting window. Normal roll display remains separately tested.");
        var fadeControls = VerifyFadeControls(c, output);
        while (fadeControls.MoveNext()) yield return null;
        var videoClock = VerifyKlingClock(c);
        while (videoClock.MoveNext()) yield return null;
        for (int i = 0; i < 5; i++) yield return null;
        Set(c, "drawerOpen", true);
        Set(c, "drawerAmount", 1f);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "drawer.png"));
        Set(c, "drawerPage", "Voice & Sound");
        yield return null;
        CaptureGameView(Path.Combine(output, "voice-sound-switches.png"));
        Set(c, "drawerPage", "Options");
        for (int i = 0; i < 5; i++) yield return null;
        Set(c, "confirmLeave", true);
        for (int i = 0; i < 5; i++) yield return null;
        CaptureGameView(Path.Combine(output, "leave-confirmation.png"));
        var phoneLayouts = VerifyPhoneLayouts(c, output);
        while (phoneLayouts.MoveNext()) yield return null;
        Debug.Log("READINESS PASSED: release-only sky display, exact environment pavement crop, static roll result and synchronized dice reset; individual bill visuals, transfer animation queue, four bill textures and wager presets, normalized metal impact volume, matched ground stakes, Double Up bills, shoot/pass, live-point guard, forfeit balances, idempotent leave, hold/cancel, both hands/styles, release continuity, 180 roll cases and nonblank sky camera. No APK built.");
    }

    private static IEnumerator PointerClick(StreetDiceGreyboxController c, Vector2 position)
    {
        QueuePointer(c, position, EventType.MouseDown);
        for (int i = 0; i < 3; i++) yield return null;
        QueuePointer(c, position, EventType.MouseUp);
        float until = Time.unscaledTime + 0.3f;
        while (Time.unscaledTime < until) yield return null;
    }

    private static void VerifyBotMicPresentation(StreetDiceGreyboxController c)
    {
        var seats = (Array)Get(c, "mics");
        try
        {
            Call(c, "SetPrototypeSeatMarkersVisible", true);
            for (int i = 1; i < seats.Length; i++)
            {
                object seat = seats.GetValue(i);
                string id = (string)seat.GetType().GetProperty("PlayerId").GetValue(seat);
                var root = (GameObject)seat.GetType().GetProperty("Root").GetValue(seat);
                Check((bool)Call(c, "IsBotSeat", id) && !root.activeSelf,
                    "Local computer seat displayed a microphone marker");
            }
            object catcher = seats.GetValue(1);
            var talkUntil = catcher.GetType().GetField("talkUntil", BindingFlags.Instance | BindingFlags.NonPublic);
            float before = (float)talkUntil.GetValue(catcher);
            Call(c, "PulseMic", "p2", 2f);
            Check((float)talkUntil.GetValue(catcher) == before, "Bot microphone pulsed in local play");

            Set(c, "localDemo", false);
            Call(c, "SetPrototypeSeatMarkersVisible", true);
            Check(!(bool)Call(c, "IsBotSeat", "p2") && (bool)Call(c, "IsBotSeat", "bot-2"),
                "Online seat identity did not follow human/bot player IDs");
            for (int i = 1; i < seats.Length; i++)
            {
                object seat = seats.GetValue(i);
                string id = (string)seat.GetType().GetProperty("PlayerId").GetValue(seat);
                var root = (GameObject)seat.GetType().GetProperty("Root").GetValue(seat);
                Check(root.activeSelf != (bool)Call(c, "IsBotSeat", id),
                    "Online seat microphone did not match human/bot identity");
            }
            Debug.Log("BOT VOICE PRESENTATION PASSED: local bots have no mic marker or pulse; online human markers remain visible while bot markers stay hidden.");
        }
        finally
        {
            Set(c, "localDemo", true);
            Call(c, "SetPrototypeSeatMarkersVisible", false);
        }
    }

    private static void VerifyCompactPhaseHud(StreetDiceGreyboxController c)
    {
        bool previousTutorial = (bool)Get(c, "tutorialMode");
        string previousPoint = (string)Get(c, "point");
        try
        {
            Set(c, "tutorialMode", false);
            Set(c, "point", "-");
            Check((string)Call(c, "PhaseHudLabel") == "", "Come-out instruction leaked outside tutorial mode");
            Set(c, "point", "10");
            Check((string)Call(c, "PhaseHudLabel") == "", "Point label leaked into ordinary play");
            Set(c, "tutorialMode", true);
            Check((string)Call(c, "PhaseHudLabel") == "POINT 10", "Tutorial point label was lost");
            Set(c, "point", "-");
            Check((string)Call(c, "PhaseHudLabel") == "COME OUT", "Tutorial come-out label was lost");
            Debug.Log("COMPACT HUD PASSED: ordinary play hides the point label; tutorial mode retains full labels.");
        }
        finally
        {
            Set(c, "tutorialMode", previousTutorial);
            Set(c, "point", previousPoint);
        }
    }

    private static void VerifyIPlayFont(StreetDiceGreyboxController c)
    {
        var expected = Resources.Load<Font>("UI/PermanentMarker-Regular");
        var skin = (GUISkin)Get(c, "pregameSkin");
        Check(expected != null && skin != null, "iPlay graffiti font skin was not created");
        Check(skin.font == expected && skin.label.font == expected && skin.button.font == expected &&
            skin.box.font == expected && skin.toggle.font == expected,
            "Live UI text does not consistently use the iPlay graffiti font");
        Debug.Log("IPLAY FONT PASSED: live labels, buttons, boxes and toggles share the Permanent Marker graffiti face.");
    }

    private static IEnumerator VerifyMoneySessionCleanup(StreetDiceGreyboxController c)
    {
        bool previousAnimation = (bool)Get(c, "moneyAnimationEnabled");
        float previousDuration = (float)Get(c, "moneyTransferDuration");
        var active = (HashSet<GameObject>)Get(c, "activeMoneyBills");
        var queue = (System.Collections.ICollection)Get(c, "moneyTransfers");
        try
        {
            Set(c, "moneyAnimationEnabled", true);
            Set(c, "moneyTransferDuration", 10f);
            Call(c, "StartLocalDemo");
            Call(c, "TransferCash", "p1", "p3", 20);
            Call(c, "TransferCash", "p3", "p4", 10);
            yield return null;
            Check(active.Count > 0 && queue.Count > 0, "Payout cleanup fixture did not start and queue transfers");
            var oldBills = new List<GameObject>(active);
            Call(c, "StartLocalDemo");
            Check(active.Count == 0 && queue.Count == 0 && !(bool)Get(c, "moneyTransferRunning"), "New session retained old payout work");
            foreach (var bill in oldBills) Check(bill == null || !bill.activeSelf, "Old payout bill remains visible after reset");
            var cash = (Dictionary<string, int>)Get(c, "cash");
            foreach (int balance in cash.Values) Check(balance == 1000, "Presentation cleanup changed new-session balances");
            Call(c, "TransferCash", "p1", "p3", 20);
            yield return null;
            Call(c, "LeaveLocalGame");
            Check(active.Count == 0 && queue.Count == 0 && !(bool)Get(c, "moneyTransferRunning"), "Leave retained payout animation");
            Check(cash["p1"] == 980 && cash["p3"] == 1020, "Leaving reversed settled money");
            Call(c, "StartLocalDemo");
            Set(c, "moneyTransferDuration", 0.35f);
            Call(c, "TransferCash", "p3", "p1", 5);
            float deadline = Time.realtimeSinceStartup + 3;
            while ((bool)Get(c, "moneyTransferRunning") && Time.realtimeSinceStartup < deadline) yield return null;
            Check(!(bool)Get(c, "moneyTransferRunning") && active.Count == 0 && queue.Count == 0, "Fresh payout failed after session cleanup");
            Check(cash["p1"] == 1005 && cash["p3"] == 995, "Fresh payout settled incorrectly");
            Debug.Log("PAYOUT SESSION CLEANUP PASSED: active and queued bills cleared on reset/leave, settled balances preserved, fresh payout completes.");
        }
        finally
        {
            Call(c, "ClearMoneyTransfers");
            Set(c, "moneyAnimationEnabled", previousAnimation);
            Set(c, "moneyTransferDuration", previousDuration);
        }
    }

    private static IEnumerator VerifySkyCameraLifecycle(StreetDiceGreyboxController c)
    {
        var camera = (Camera)Get(c, "skyCamera");
        bool previousMenu = (bool)Get(c, "mainOptions");
        bool previousVisible = (bool)Get(c, "skyCamVisible");
        bool previousRolling = (bool)Get(c, "rolling");
        bool previousFade = (bool)Get(c, "fadeInProgress");
        int renders = 0;
        Camera.CameraCallback rendered = source => { if (source == camera) renders++; };
        Camera.onPostRender += rendered;
        try
        {
            Set(c, "mainOptions", false);
            Set(c, "rolling", false);
            Set(c, "fadeInProgress", false);
            Set(c, "skyCamVisible", false);
            for (int i = 0; i < 5; i++) yield return null;
            Check(!camera.enabled && renders == 0, "Hidden overhead camera still renders");
            Set(c, "skyCamVisible", true);
            for (int i = 0; i < 5; i++) yield return null;
            Check(!camera.enabled && renders == 0, "Stale overhead flag rendered between rolls");
            Set(c, "rolling", true);
            for (int i = 0; i < 5; i++) yield return null;
            Check(camera.enabled && renders > 0, "Visible overhead camera did not render");
            Set(c, "mainOptions", true);
            yield return null;
            int before = renders;
            for (int i = 0; i < 5; i++) yield return null;
            Check(!camera.enabled && renders == before, "Main settings still render the hidden overhead camera");
            Set(c, "mainOptions", false);
            Set(c, "rolling", false);
            for (int i = 0; i < 5; i++) yield return null;
            Check(!camera.enabled && renders == before, "Overhead display persisted after the roll");
            Set(c, "skyCamVisible", false);
            for (int i = 0; i < 5; i++) yield return null;
            Check(!camera.enabled && renders == before, "Dismissed overhead display kept rendering");
            Debug.Log("SKY CAMERA LIFECYCLE PASSED: actual render callbacks only during rolls; zero overhead renders between rolls, in settings and after dismissal.");
        }
        finally
        {
            Camera.onPostRender -= rendered;
            Set(c, "mainOptions", previousMenu);
            Set(c, "skyCamVisible", previousVisible);
            Set(c, "rolling", previousRolling);
            Set(c, "fadeInProgress", previousFade);
        }
    }

    private static void VerifySkyCamFraming()
    {
        var step = typeof(StreetDiceGreyboxController).GetMethod("StepSkyCamSize", BindingFlags.Static | BindingFlags.NonPublic);
        float Sample(float current, float required, float dt) => (float)step.Invoke(null, new object[] { current, required, dt });
        float reference = 0;
        foreach (int fps in new[] { 30, 60, 90, 120 })
        {
            float size = 0.7f;
            for (int frame = 0; frame < fps / 10; frame++)
            {
                float previous = size;
                size = Sample(size, 0.18f, 1f / fps);
                Check(size >= 0.18f && size <= previous, "Sky cam zoom overshot its framing bounds");
            }
            if (reference == 0) reference = size;
            else Check(Mathf.Abs(size - reference) < 0.00001f, "Sky cam zoom depends on frame rate");
            Check(Sample(size, 1.2f, 1f / fps) >= 1.2f, "Sky cam clipped a spreading roll");
        }
        Check(Sample(0.7f, 0.18f, 0) == 0.7f, "Sky cam zoom moved without elapsed time");
        Debug.Log("SKY FRAMING PASSED: 30/60/90/120 Hz zoom equivalence, no inward overshoot, immediate widening for separated dice.");
    }

    private static void VerifyDiceCalling(StreetDiceGreyboxController c)
    {
        var clips = (AudioClip[])Get(c, "diceCallClips");
        Check(clips != null && clips.Length == 13, "Dice calling clip bank was not initialized");
        bool available = (bool)Get(c, "diceCallingAvailable");
        if (!available)
        {
            Call(c, "SetDiceCalling", true);
            Check(!(bool)Get(c, "diceCalling"), "Dice Calling activated without the iPlay number recordings");
            Debug.Log("DICE CALLING GATED: iPlay reference is preserved; number calling remains unavailable until matching recordings are supplied.");
            return;
        }
        for (int value = 1; value <= 12; value++)
            Check(clips[value] != null && clips[value].length > 0.1f, "Missing spoken dice call " + value);
        bool previous = (bool)Get(c, "diceCalling");
        bool hadPreference = PlayerPrefs.HasKey("StreetDice.DiceCalling");
        int saved = PlayerPrefs.GetInt("StreetDice.DiceCalling", 1);
        try
        {
            Call(c, "SetDiceCalling", false);
            Check(!(bool)Get(c, "diceCalling") && PlayerPrefs.GetInt("StreetDice.DiceCalling", 1) == 0,
                "Dice Calling did not turn off and save");
            Call(c, "SetDiceCalling", true);
            Check((bool)Get(c, "diceCalling") && PlayerPrefs.GetInt("StreetDice.DiceCalling", 0) == 1,
                "Dice Calling did not turn on and save");
        }
        finally
        {
            Set(c, "diceCalling", previous);
            if (hadPreference) PlayerPrefs.SetInt("StreetDice.DiceCalling", saved);
            else PlayerPrefs.DeleteKey("StreetDice.DiceCalling");
            PlayerPrefs.Save();
        }
        Debug.Log("DICE CALLING PASSED: twelve spoken numbers and persisted on/off setting.");
    }

    private static IEnumerator VerifyFadeSettings(StreetDiceGreyboxController c, string output)
    {
        int previous = (int)Get(c, "selectedFadeStyle");
        bool hadPreference = PlayerPrefs.HasKey("StreetDice.FadeStyle");
        int saved = PlayerPrefs.GetInt("StreetDice.FadeStyle", 0);
        int oldHand = (int)Get(c, "selectedHandSkin");
        bool oldTutorial = (bool)Get(c, "tutorialMode");
        float oldVolume = (float)Get(c, "effectsVolume");
        bool hadTutorial = PlayerPrefs.HasKey("StreetDice.Tutorial");
        int savedTutorial = PlayerPrefs.GetInt("StreetDice.Tutorial", 0);
        bool hadVolume = PlayerPrefs.HasKey("StreetDice.Effects");
        float savedVolume = PlayerPrefs.GetFloat("StreetDice.Effects", 0.7f);
        Color oldDice = (Color)Get(c, "selectedDiceColor");
        string[] colorKeys = { "StreetDice.HandSkin", "StreetDice.DiceColor" };
        bool[] hadColors = { PlayerPrefs.HasKey(colorKeys[0]), PlayerPrefs.HasKey(colorKeys[1]) };
        int[] savedColors = { PlayerPrefs.GetInt(colorKeys[0], 0), PlayerPrefs.GetInt(colorKeys[1], 0) };
        Set(c, "mainOptions", true);
        var screenField = c.GetType().GetField("startupScreen", Flags);
        screenField.SetValue(c, Enum.Parse(screenField.FieldType, "LegacyPregame"));
        try
        {
            float width = (float)typeof(StreetDiceGreyboxController).GetProperty("UiWidth", Flags).GetValue(c);
            for (int i = 0; i < 3; i++)
            {
                var click = PointerClick(c, new Vector2(width / 2 - 150 + i * 200 + 95, 225));
                while (click.MoveNext()) yield return null;
                Check((int)Get(c, "selectedHandSkin") == i && PlayerPrefs.GetInt(colorKeys[0], -1) == i, "Hand swatch did not select and save its shade");
            }
            var colors = (Color[])typeof(StreetDiceGreyboxController).GetField("DiceColors", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            for (int i = 0; i < colors.Length; i++)
            {
                var click = PointerClick(c, new Vector2(width / 2 - 150 + i * 150 + 55, 443));
                while (click.MoveNext()) yield return null;
                Check((Color)Get(c, "selectedDiceColor") == colors[i] && PlayerPrefs.GetInt(colorKeys[1], -1) == i, "Dice swatch did not select and save its color");
            }
            for (int i = 0; i < 3; i++)
            {
                var click = PointerClick(c, new Vector2(width / 2 - 150 + (i + 0.5f) * 600 / 3, 366));
                while (click.MoveNext()) yield return null;
                Check((int)Get(c, "selectedFadeStyle") == i, "Fade settings tap selected the wrong style");
                if (i > 0) Check(PlayerPrefs.GetInt("StreetDice.FadeStyle", -1) == i, "Fade selection was not persisted");
            }
            Set(c, "tutorialMode", false);
            var tutorialClick = PointerClick(c, new Vector2(width / 2 + 305, 547));
            while (tutorialClick.MoveNext()) yield return null;
            Check((bool)Get(c, "tutorialMode") && PlayerPrefs.GetInt("StreetDice.Tutorial", 0) == 1, "Pregame tutorial selection did not save");
            Set(c, "effectsVolume", 0f);
            var volumeClick = PointerClick(c, new Vector2(width / 2 - 30, 547));
            while (volumeClick.MoveNext()) yield return null;
            float volume = (float)Get(c, "effectsVolume");
            Check(volume > 0.5f && volume <= 1f && Mathf.Approximately(volume, PlayerPrefs.GetFloat("StreetDice.Effects", -1)), "Pregame sound slider did not change and save");
            Call(c, "SetEffectsVolume", -2f);
            Check((float)Get(c, "effectsVolume") == 0f, "Volume lower clamp failed");
            Call(c, "SetEffectsVolume", 2f);
            Check((float)Get(c, "effectsVolume") == 1f, "Volume upper clamp failed");
            Debug.Log("PREGAME SOUND/TUTORIAL PASSED: pointer controls, saved values and volume bounds.");
            CaptureGameView(Path.Combine(output, "fade-main-settings.png"));
            VerifyDiceSelectionPixels(c);
            Debug.Log("FADE SETTINGS PASSED: Tap/Plant/Wave pointer selection beside hand appearance; changed selection persisted.");
            Debug.Log("COLOR SWATCHES PASSED: pointer selection and persistence for all three hand shades and four dice colors.");
        }
        finally
        {
            Set(c, "selectedFadeStyle", previous);
            Set(c, "tutorialMode", oldTutorial);
            Set(c, "effectsVolume", oldVolume);
            if (hadTutorial) PlayerPrefs.SetInt("StreetDice.Tutorial", savedTutorial); else PlayerPrefs.DeleteKey("StreetDice.Tutorial");
            if (hadVolume) PlayerPrefs.SetFloat("StreetDice.Effects", savedVolume); else PlayerPrefs.DeleteKey("StreetDice.Effects");
            Set(c, "selectedHandSkin", oldHand); Call(c, "ApplyHandSkin");
            Set(c, "selectedDiceColor", oldDice); Call(c, "ApplyDiceColor");
            for (int i = 0; i < colorKeys.Length; i++)
                if (hadColors[i]) PlayerPrefs.SetInt(colorKeys[i], savedColors[i]); else PlayerPrefs.DeleteKey(colorKeys[i]);
            if (hadPreference) PlayerPrefs.SetInt("StreetDice.FadeStyle", saved); else PlayerPrefs.DeleteKey("StreetDice.FadeStyle");
            PlayerPrefs.Save();
        }
    }

    private static void VerifyDiceSelectionPixels(StreetDiceGreyboxController c)
    {
        var previews = (RenderTexture[])Get(c, "dicePreviews");
        Check(previews != null && previews.Length == 4, "Four dice preview textures required");
        var means = new Color[4];
        var previousTarget = RenderTexture.active;
        try
        {
            for (int i = 0; i < previews.Length; i++)
            {
                var target = previews[i];
                Check(target != null && target.IsCreated(), "Missing dice selection render target");
                var pixels = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
                try
                {
                    RenderTexture.active = target;
                    pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                    pixels.Apply();
                    int count = 0;
                    Color sum = Color.clear;
                    foreach (var pixel in pixels.GetPixels())
                    {
                        if (pixel.a < 0.9f) continue;
                        count++;
                        sum += pixel;
                    }
                    float coverage = count / (float)(target.width * target.height);
                    Check(coverage > 0.08f && coverage < 0.8f, "Dice preview blank or incorrectly framed: " + i);
                    means[i] = sum / count;
                }
                finally { UnityEngine.Object.Destroy(pixels); }
            }
        }
        finally { RenderTexture.active = previousTarget; }
        Check(means[0].grayscale > means[1].grayscale + 0.1f, "White and black dice previews indistinguishable");
        Check(means[2].g > means[2].r + 0.04f && means[2].g > means[2].b + 0.04f, "Green dice preview lost its color");
        Check(means[3].b > means[3].r + 0.04f && means[3].b > means[3].g + 0.04f, "Blue dice preview lost its color");
        Call(c, "EnsureHandPreviews");
        Check(ReferenceEquals(previews, Get(c, "dicePreviews")), "Selection previews were rebuilt instead of cached");
        Debug.Log("DICE PREVIEWS PASSED: four nonblank framed renders, distinct body colors and reused cache.");
    }

    private static IEnumerator VerifyPhoneLayouts(StreetDiceGreyboxController c, string output)
    {
        var assembly = typeof(EditorWindow).Assembly;
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = sizesType.BaseType.GetProperty("instance", all).GetValue(null);
        var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
        string groupName = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : "Standalone";
        var group = sizesType.GetMethod("GetGroup", all).Invoke(sizes, new[] { Enum.Parse(groupType, groupName) });
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        var modeType = assembly.GetType("UnityEditor.GameViewSizeType");
        var constructor = sizeType.GetConstructor(all, null, new[] { modeType, typeof(int), typeof(int), typeof(string) }, null);
        var selection = gameView.GetType().GetProperty("selectedSizeIndex", all);
        int previous = (int)selection.GetValue(gameView);
        try
        {
            foreach (var resolution in new[] { new Vector2Int(1280, 720), new Vector2Int(2340, 1080), new Vector2Int(2400, 1080) })
            {
                int index = (int)group.GetType().GetMethod("GetTotalCount", all).Invoke(group, null);
                var size = constructor.Invoke(new object[] { Enum.Parse(modeType, "FixedResolution"), resolution.x, resolution.y, "iPlay verification " + resolution });
                group.GetType().GetMethod("AddCustomSize", all).Invoke(group, new[] { size });
                selection.SetValue(gameView, index);
                gameView.Repaint();
                for (int i = 0; i < 12; i++) yield return null;
                Check(Screen.width == resolution.x && Screen.height == resolution.y, "Game View did not apply requested phone resolution: " + Screen.width + "x" + Screen.height);
                var pregame = VerifyFadeSettings(c, output);
                while (pregame.MoveNext()) yield return null;
                CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-pregame.png"));
                var locks = VerifyPhoneWagerLocks(c, output, resolution);
                while (locks.MoveNext()) yield return null;
                Call(c, "StartLocalDemo"); Set(c, "botWagersEnabled", false); Call(c, "CommitShoot");
                Set(c, "confirmLeave", false); Set(c, "drawerOpen", false); Set(c, "drawerAmount", 0f);
                Set(c, "shooterId", "p3"); Set(c, "catcherId", "p2");
                Set(c, "phase", "Point"); Set(c, "point", "10"); Call(c, "OpenBettingWindow");
                Set(c, "draftNumber", 10); Set(c, "draftOutcome", IPlay.Demo.WagerOutcome.Hit);
                Set(c, "wagerTarget", "p4"); Call(c, "SetWagerStage", 2); Set(c, "wagerStageAt", Time.unscaledTime - 1);
                for (int i = 0; i < 5; i++) yield return null;
                string prefix = "phone-" + resolution.x + "x" + resolution.y;
                CaptureGameView(Path.Combine(output, prefix + "-bets.png"));
                var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
                book.Open("p3", 10, Time.unscaledTimeAsDouble - 16); book.BeginRoll(Time.unscaledTimeAsDouble);
                Set(c, "rolling", true); Set(c, "rollState", RollState.FadeWindow); Set(c, "fadeCount", 1);
                c.StartCoroutine((IEnumerator)Call(c, "Fade"));
                float captureDeadline = Time.realtimeSinceStartup + 5;
                while (Time.realtimeSinceStartup < captureDeadline &&
                    (!(bool)Get(c, "klingCatchActive") || ((UnityEngine.Video.VideoPlayer)Get(c, "catchVideo")).time < 0.36)) yield return null;
                Check((bool)Get(c, "klingCatchActive"), "Phone fade did not reach capture frame");
                CaptureGameView(Path.Combine(output, prefix + "-sky-layout.png"));
                while ((bool)Get(c, "fadeInProgress") && Time.realtimeSinceStartup < captureDeadline) yield return null;
                Check(!(bool)Get(c, "fadeInProgress"), "Phone fade did not finish");
                Set(c, "drawerOpen", true); Set(c, "drawerAmount", 1f);
                for (int i = 0; i < 3; i++) yield return null;
                CaptureGameView(Path.Combine(output, prefix + "-drawer.png"));
                Debug.Log("PHONE LAYOUT CAPTURED " + resolution.x + "x" + resolution.y + "; layout fixtures, not device performance evidence.");
            }
        }
        finally { selection.SetValue(gameView, previous); gameView.Repaint(); }
        for (int i = 0; i < 8; i++) yield return null;
        var inset = new Rect(80, 32, Screen.width - 112, Screen.height - 48);
        Set(c, "safeAreaOverride", (Rect?)inset);
        try
        {
            Set(c, "drawerOpen", false); Set(c, "drawerAmount", 0f); Set(c, "confirmLeave", false);
            var controls = VerifyWagerControls(c, output);
            while (controls.MoveNext()) yield return null;
            var origin = (Vector2)typeof(StreetDiceGreyboxController).GetProperty("UiOrigin", Flags).GetValue(c);
            Check(Vector2.Distance(origin, new Vector2(80, 16)) < 0.01f, "Safe area did not offset the UI origin");
            CaptureGameView(Path.Combine(output, "safe-area-wager.png"));
            var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
            book.Open("p1", 0, Time.unscaledTimeAsDouble - 16);
            Set(c, "bettingClosesAt", Time.unscaledTime - 16);
            Call(c, "BeginShake", new Vector2(inset.x - 1, inset.y + inset.height * 0.3f), -2);
            Check(!(bool)Get(c, "shakeHeld"), "Cutout-area touch started a throw");
            Call(c, "BeginShake", new Vector2(inset.center.x, inset.y + inset.height * 0.3f), -2);
            Check((bool)Get(c, "shakeHeld"), "Safe-area center touch failed to start shake");
            Call(c, "CancelShake");
            Set(c, "drawerOpen", true); Set(c, "drawerAmount", 1f);
            for (int i = 0; i < 3; i++) yield return null;
            CaptureGameView(Path.Combine(output, "safe-area-drawer.png"));
            Debug.Log("SAFE AREA PASSED: asymmetric inset, actual wager pointer workflow, outside touch rejection and inside shake. Simulated inset, not physical-device verification.");
        }
        finally { Set(c, "safeAreaOverride", null); }
    }

    private static void VerifyCatchGrades()
    {
        var source = new Texture2D(2, 1, TextureFormat.RGBA32, false, true) { filterMode = FilterMode.Point };
        source.SetPixels(new[] { new Color(0.2f, 0.2f, 0.2f, 1), new Color(0.4f, 0.25f, 0.15f, 1) });
        source.Apply();
        var target = new RenderTexture(2, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        var pixels = new Texture2D(2, 1, TextureFormat.RGBA32, false, true);
        var background = new Material(Resources.Load<Shader>("Fades/CatchVideo"));
        var previous = RenderTexture.active;
        Color neutral = Color.clear;
        float previousHand = 0;
        try
        {
            var grades = new[] { Vector4.one, new Vector4(1.6f, 1.75f, 1.85f, 1), new Vector4(2.7f, 3.2f, 3.6f, 1) };
            for (int i = 0; i < grades.Length; i++)
            {
                background.SetVector("_SkinGrade", grades[i]);
                Graphics.Blit(source, target, background);
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 2, 1), 0, 0); pixels.Apply();
                Color pavement = pixels.GetPixel(0, 0), hand = pixels.GetPixel(1, 0);
                if (i == 0) neutral = pavement;
                Check(Vector4.Distance(pavement, neutral) < 0.01f, "Hand shade tinted neutral pavement");
                Check(hand.grayscale > previousHand + 0.04f, "Kling hand grades are not distinct");
                previousHand = hand.grayscale;
            }
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(pixels);
            UnityEngine.Object.DestroyImmediate(background);
            target.Release(); UnityEngine.Object.DestroyImmediate(target);
        }
        Debug.Log("CATCH GRADES PASSED: three distinct hand grades and neutral pavement unchanged. Synthetic color samples, not full-frame segmentation proof.");
    }

    private static IEnumerator VerifyFadeControls(StreetDiceGreyboxController c, string output)
    {
        Call(c, "StartLocalDemo");
        Set(c, "botWagersEnabled", false);
        Call(c, "CommitShoot");
        Set(c, "shooterId", "p3");
        Set(c, "catcherId", "p1");
        Set(c, "rolling", true);
        Set(c, "rollState", RollState.FadeWindow);
        var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        book.Open("p3", 0, Time.unscaledTimeAsDouble - 16);
        book.BeginRoll(Time.unscaledTimeAsDouble);
        for (int i = 0; i < 4; i++) yield return null;
        CaptureGameView(Path.Combine(output, "fade-circular-control.png"));
        float uiWidth = (float)typeof(StreetDiceGreyboxController).GetProperty("UiWidth", Flags).GetValue(c);
        float uiHeight = (float)typeof(StreetDiceGreyboxController).GetProperty("UiHeight", Flags).GetValue(c);
        var fadeRect = new Rect(uiWidth * 0.5f + 82, uiHeight - 152, 70, 70);
        var corner = PointerClick(c, fadeRect.min + new Vector2(4, 4));
        while (corner.MoveNext()) yield return null;
        Check(!(bool)Get(c, "fadeInProgress"), "FADE corner tap triggered outside circle");
        Set(c, "catcherId", "p2");
        var spectator = PointerClick(c, fadeRect.center);
        while (spectator.MoveNext()) yield return null;
        Check(!(bool)Get(c, "fadeInProgress"), "Spectator triggered FADE");
        Set(c, "shooterId", "p1");
        var shooter = PointerClick(c, fadeRect.center);
        while (shooter.MoveNext()) yield return null;
        Check(!(bool)Get(c, "fadeInProgress"), "Shooter triggered FADE");
        Set(c, "shooterId", "p3"); Set(c, "catcherId", "p1");
        var center = PointerClick(c, fadeRect.center);
        while (center.MoveNext()) yield return null;
        Check((bool)Get(c, "fadeInProgress"), "Catcher center tap did not trigger FADE");
        var repeated = PointerClick(c, fadeRect.center);
        while (repeated.MoveNext()) yield return null;
        float deadline = Time.unscaledTime + 3;
        while ((bool)Get(c, "fadeInProgress") && Time.unscaledTime < deadline) yield return null;
        Check(!(bool)Get(c, "fadeInProgress") && (int)Get(c, "fadeCount") == 1, "Repeated FADE tap duplicated or stalled fade");
        Debug.Log("FADE POINTER PASSED: circular corner rejection, catcher center activation, shooter/spectator exclusion and repeated-tap protection. Desktop input only.");
    }

    private static IEnumerator VerifyKlingClock(StreetDiceGreyboxController c)
    {
        Call(c, "StartLocalDemo");
        Set(c, "botWagersEnabled", false);
        Call(c, "CommitShoot");
        Set(c, "fadeCount", 1);
        Set(c, "catcherId", "p1"); Set(c, "selectedFadeStyle", 1);
        Set(c, "rolling", true);
        Set(c, "rollState", RollState.FadeWindow);
        var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        book.Open("p1", 0, Time.unscaledTimeAsDouble - 16);
        book.BeginRoll(Time.unscaledTimeAsDouble);
        c.StartCoroutine((IEnumerator)Call(c, "Fade"));
        float deadline = Time.realtimeSinceStartup + 5;
        while (!(bool)Get(c, "klingCatchActive") && Time.realtimeSinceStartup < deadline) yield return null;
        Check((bool)Get(c, "klingCatchActive"), "Kling clock fixture did not start");
        var player = (UnityEngine.Video.VideoPlayer)Get(c, "catchVideo");
        while (player.time < 0.08 && Time.realtimeSinceStartup < deadline) yield return null;
        Check(player.time < 0.3, "Kling clock fixture missed the hand animation interval");
        player.Pause();
        for (int i = 0; i < 3; i++) yield return null;
        var die = (GameObject)Get(c, "dieA");
        double pausedAt = player.time;
        float pauseUntil = Time.realtimeSinceStartup + 0.2f;
        while (Time.realtimeSinceStartup < pauseUntil)
        {
            Check(!die.activeSelf, "Dice reappeared during paused hand-only fade");
            Check(System.Math.Abs(player.time - pausedAt) < 0.001, "Paused hand clip advanced");
            yield return null;
        }
        player.Play();
        deadline = Time.realtimeSinceStartup + 3;
        while ((bool)Get(c, "fadeInProgress") && Time.realtimeSinceStartup < deadline) yield return null;
        Check(!(bool)Get(c, "fadeInProgress") && !(bool)Get(c, "klingCatchActive"), "Kling fade failed to resume and finish");
        Check(Mathf.Approximately(((Camera)Get(c, "skyCamera")).aspect, 2), "Kling fade did not restore camera aspect");
        Check(!book.CanOffer(Time.unscaledTimeAsDouble) && book.CanRoll(Time.unscaledTimeAsDouble),
            "Kling fade reopened betting or delayed rethrow");
        Debug.Log("KLING CLOCK PASSED: paused hand clip stays paused with dice hidden; resume completes fade, restores camera and allows immediate rethrow.");
    }

    private static void QueuePointer(StreetDiceGreyboxController c, Vector2 position, EventType type)
    {
        float scale = (float)typeof(StreetDiceGreyboxController).GetProperty("UiScale", Flags).GetValue(c);
        Vector2 origin = (Vector2)typeof(StreetDiceGreyboxController).GetProperty("UiOrigin", Flags).GetValue(c);
        var method = typeof(EditorGUIUtility).GetMethod("QueueGameViewInputEvent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Check(method != null, "Unity Game View input queue unavailable");
        method.Invoke(null, new object[] { new Event { type = type, mousePosition = origin + position * scale, button = 0 } });
    }

    private static Vector2 IncomingLock(StreetDiceGreyboxController c, string recipient, int index = 0)
    {
        var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        var offer = book.Offers.Where(item => item.To == recipient &&
            item.Status is IPlay.Demo.WagerStatus.Offered or IPlay.Demo.WagerStatus.Accepted).Skip(index).First();
        return ((Rect)Call(c, "GroundLockRect", offer)).center;
    }

    private static IEnumerator VerifyWagerControls(StreetDiceGreyboxController c, string output)
    {
        Call(c, "StartLocalDemo");
        Set(c, "botWagersEnabled", false);
        Call(c, "CommitShoot");
        Set(c, "shooterId", "p3");
        Set(c, "catcherId", "p2");
        Set(c, "phase", "Point"); Set(c, "point", "10");
        Call(c, "OpenBettingWindow");
        var openOverlay = PointerClick(c, new Vector2(49, 42));
        while (openOverlay.MoveNext()) yield return null;
        Check((bool)Get(c, "wagerOverlayOpen") && (int)Get(c, "wagerStage") == 1,
            "Wall Bet die did not open the digital wager overlay");
        var seat = (Rect)Call(c, "SeatHudRect", "p4");
        Vector2 bet = new Vector2(seat.x + 99, seat.y + 84);
        Vector2 crap = new Vector2(seat.x + 167, seat.y + 84);
        float uiWidth = (float)typeof(StreetDiceGreyboxController).GetProperty("UiWidth", Flags).GetValue(c);
        float uiHeight = (float)typeof(StreetDiceGreyboxController).GetProperty("UiHeight", Flags).GetValue(c);
        float firstBill = uiWidth * 0.5f - (124f * 4 + 29f * 3) * 0.5f + 62f;
        Vector2 bill20 = new Vector2(firstBill + 3f * 153f, uiHeight * 0.38f + 169f);
        foreach (var position in new[] { bet, crap, bill20 })
        {
            var click = PointerClick(c, position);
            while (click.MoveNext()) yield return null;
        }
        var book = (IPlay.Demo.WagerBook)Get(c, "wagerBook");
        Check(book.Offers.Count == 1, "Bill click did not create exactly one offer");
        var offer = book.Offers[0];
        Check(offer.Number == 4 && offer.Amount == 20 && offer.To == "p4", "Pointer controls changed the selected wager");
        Call(c, "AcceptWager", offer.Id, "p4");
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "wager-locked-outgoing.png"));

        var crapChoice = PointerClick(c, crap);
        while (crapChoice.MoveNext()) yield return null;
        var crapFour = PointerClick(c, crap);
        while (crapFour.MoveNext()) yield return null;
        Check((IPlay.Demo.WagerOutcome)Get(c, "draftOutcome") == IPlay.Demo.WagerOutcome.Crap
            && (int)Get(c, "draftNumber") == 4, "Point-10 CRAP 4 choice lost its number");
        var crapAmount = PointerClick(c, bill20);
        while (crapAmount.MoveNext()) yield return null;
        Check(book.Offers.Count == 2 && book.Offers[1].Outcome == IPlay.Demo.WagerOutcome.Crap
            && book.Offers[1].Number == 4, "Point-10 CRAP 4 offer lost its number");
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "wager-numbered-crap-outgoing.png"));

        Call(c, "StartLocalDemo"); Call(c, "CommitShoot");
        Set(c, "phase", "Point"); Set(c, "point", "10");
        Call(c, "OpenBettingWindow");
        var crapTenOffer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p2", "p1", IPlay.Demo.WagerOutcome.Crap, 10, 10);
        var crapFourOffer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p2", "p1", IPlay.Demo.WagerOutcome.Crap, 4, 5);
        Check(crapTenOffer != null && crapFourOffer != null && crapTenOffer.Number == 10
            && crapFourOffer.Number == 4, "Point group CRAP offers were not distinct");
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "wager-numbered-crap-incoming.png"));

        Call(c, "StartLocalDemo"); Call(c, "CommitShoot");
        var incoming = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p3", "p1", IPlay.Demo.WagerOutcome.Crap, 0, 10);
        Check(incoming != null, "Incoming wager fixture failed");
        var firstLockCenter = IncomingLock(c, "p1");
        var acceptClick = PointerClick(c, firstLockCenter);
        while (acceptClick.MoveNext()) yield return null;
        Check(incoming.Status == IPlay.Demo.WagerStatus.Offered, "First lock tap should only arm acceptance");
        var confirmClick = PointerClick(c, firstLockCenter);
        while (confirmClick.MoveNext()) yield return null;
        Check(incoming.Status == IPlay.Demo.WagerStatus.Accepted, "Second recipient lock tap did not accept wager");
        var repeatClick = PointerClick(c, firstLockCenter);
        while (repeatClick.MoveNext()) yield return null;
        Check((int)Call(c, "AcceptedWagerStake", "p1") == 10, "Repeated lock click doubled the stake");
        CaptureGameView(Path.Combine(output, "wager-locked-incoming.png"));
        Call(c, "StartLocalDemo"); Call(c, "CommitShoot");
        var senders = new[] { "p3", "p4", "p2", "bot-5" };
        var four = new List<IPlay.Demo.WagerOffer>();
        foreach (var sender in senders)
        {
            var nextOffer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", sender, "p1", IPlay.Demo.WagerOutcome.Crap, 0, 5);
            Check(nextOffer != null, "Four-lock offer failed for " + sender);
            four.Add(nextOffer);
        }
        for (int i = 0; i < 3; i++) yield return null;
        var senderDice = (Dictionary<int, GameObject>)Get(c, "groundAmountDice");
        Check(four.All(offer => senderDice.TryGetValue(offer.Id, out GameObject die) &&
            die.activeInHierarchy && Mathf.Abs(die.transform.position.x) > 1.3f),
            "Offers did not appear beside their senders");
        CaptureGameView(Path.Combine(output, "wager-four-ground-locks.png"));
        var select = PointerClick(c, IncomingLock(c, "p1", 3));
        while (select.MoveNext()) yield return null;
        Check(four[3].Status == IPlay.Demo.WagerStatus.Offered, "Fourth ground lock accepted on first tap");
        select = PointerClick(c, IncomingLock(c, "p1", 3));
        while (select.MoveNext()) yield return null;
        Check(four[3].Status == IPlay.Demo.WagerStatus.Accepted, "Fourth ground lock did not accept");
        for (int i = 0; i < 3; i++) Check(four[i].Status == IPlay.Demo.WagerStatus.Offered, "Wrong ground lock accepted");
        Debug.Log("WAGER POINTER PASSED: outcome, amount, offer, recipient acceptance and repeated-click stake invariance through OnGUI events. Physical touch delivery is not covered.");
    }

    private static IEnumerator VerifyPhoneWagerLocks(StreetDiceGreyboxController c, string output, Vector2Int resolution)
    {
        float uiWidth = (float)typeof(StreetDiceGreyboxController).GetProperty("UiWidth", Flags).GetValue(c);
        var hitRect = new Rect(uiWidth * 0.5f - 180, 100, 72, 80);
        var visualMethod = typeof(StreetDiceGreyboxController).GetMethod("LockVisualRect", Flags | BindingFlags.Static);
        for (int i = 0; i <= 24; i++)
        {
            var visual = (Rect)visualMethod.Invoke(null, new object[] { hitRect, i / 120f });
            Check(hitRect.Contains(visual.min) && visual.xMax <= hitRect.xMax && visual.yMax <= hitRect.yMax, "Lock feedback escaped its fixed hit area");
            Check(visual.width >= hitRect.width * 0.92f && visual.height >= hitRect.height * 0.92f, "Lock feedback shrank amount excessively");
        }
        Check((Rect)visualMethod.Invoke(null, new object[] { hitRect, 0.18f }) == hitRect, "Lock feedback did not settle exactly");
        foreach (int amount in new[] { 1, 5, 10, 20 })
        {
            Call(c, "StartLocalDemo");
            Set(c, "botWagersEnabled", false);
            Set(c, "drawerOpen", false); Set(c, "drawerAmount", 0f); Set(c, "confirmLeave", false);
            Call(c, "CommitShoot");
            var offer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p3", "p1", IPlay.Demo.WagerOutcome.Crap, 0, amount);
            Check(offer != null && offer.Amount == amount, "Phone denomination offer failed: " + amount);
            string prefix = "phone-" + resolution.x + "x" + resolution.y + "-lock-" + amount;
            for (int i = 0; i < 2; i++) yield return null;
            var groundDice = (Dictionary<int, GameObject>)Get(c, "groundAmountDice");
            Check(groundDice.TryGetValue(offer.Id, out GameObject amountDie), "Offer amount die missing");
            Check(amountDie.transform.position.x < -0.85f, "Offer amount die entered the roll lane");
            Check(amountDie.transform.Find("Low digital die body").localScale.y >= 0.2f,
                "Offer amount die is lying flat");
            Set(c, "wagerOverlayOpen", false);
            yield return null;
            Check(groundDice.ContainsKey(offer.Id) && amountDie.activeInHierarchy,
                "Ground offer disappeared when betting overlay closed");
            CaptureGameView(Path.Combine(output, prefix + "-open.png"));
            var accept = PointerClick(c, IncomingLock(c, "p1"));
            while (accept.MoveNext()) yield return null;
            Check(offer.Status == IPlay.Demo.WagerStatus.Offered, "Phone lock accepted on first tap");
            accept = PointerClick(c, IncomingLock(c, "p1"));
            while (accept.MoveNext()) yield return null;
            Check(offer.Status == IPlay.Demo.WagerStatus.Accepted, "Phone lock did not accept $" + amount);
            Check((int)Call(c, "AcceptedWagerStake", "p1") == amount, "Wrong phone accepted stake: " + amount);
            var feedback = (Dictionary<int, float>)Get(c, "wagerAcceptedAt");
            Check(feedback.ContainsKey(offer.Id), "Accepted wager has no visual feedback timestamp");
            float acceptedAt = feedback[offer.Id];
            CaptureGameView(Path.Combine(output, prefix + "-accepting.png"));
            var repeat = PointerClick(c, IncomingLock(c, "p1"));
            while (repeat.MoveNext()) yield return null;
            Check((int)Call(c, "AcceptedWagerStake", "p1") == amount, "Closed phone lock charged twice: " + amount);
            Call(c, "AcceptWager", offer.Id, "p1");
            Check(feedback[offer.Id] == acceptedAt, "Duplicate acceptance restarted lock feedback");
            for (int i = 0; i < 2; i++) yield return null;
            CaptureGameView(Path.Combine(output, prefix + "-closed.png"));
        }
        Call(c, "StartLocalDemo");
        Set(c, "botWagersEnabled", false);
        Call(c, "CommitShoot");
        var phoneFour = new List<IPlay.Demo.WagerOffer>();
        foreach (string sender in new[] { "p3", "p4", "p2", "bot-5" })
        {
            var nextOffer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", sender, "p1", IPlay.Demo.WagerOutcome.Crap, 0, 5);
            Check(nextOffer != null, "Phone four-lock offer failed for " + sender);
            phoneFour.Add(nextOffer);
        }
        for (int i = 0; i < 3; i++) yield return null;
        var fourDice = (Dictionary<int, GameObject>)Get(c, "groundAmountDice");
        Check(phoneFour.All(offer => fourDice.TryGetValue(offer.Id, out GameObject die) &&
            die.activeInHierarchy && Mathf.Abs(die.transform.position.x) > 1.3f),
            "Offers did not stay in their senders' areas");
        var placementTargets = new[]
        {
            new Vector2(0.14f, 0.275f), new Vector2(0.15f, 0.13f),
            new Vector2(0.78f, 0.275f), new Vector2(0.74f, 0.13f)
        };
        var piles = (List<GameObject>)Get(c, "moneyPiles");
        for (int i = 0; i < phoneFour.Count; i++)
        {
            Vector3 placed = Camera.main.WorldToViewportPoint(piles[i + 1].transform.position);
            Check(Vector2.Distance(placed, placementTargets[i]) < 0.01f,
                "Opponent cash missed its marked area at " + resolution + " seat " + (i + 1));
            Check(Mathf.Abs(piles[i + 1].transform.position.y - piles[0].transform.position.y) < 0.001f,
                "Opponent cash lifted above the roll surface at " + resolution + " seat " + (i + 1));
            Check(Mathf.Abs(Vector3.Dot(piles[i + 1].transform.GetChild(0).forward, Vector3.up)) > 0.99f,
                "Cash contact shadow is not flat on the pavement at " + resolution + " seat " + (i + 1));
            Check(Mathf.Abs(Mathf.DeltaAngle(0f, piles[i + 1].transform.GetChild(1).localEulerAngles.y)) <= 18.1f,
                "First bill turned away from a flat street layout at " + resolution + " seat " + (i + 1));
            Rect name = (Rect)Call(c, "SeatHudRect", phoneFour[i].From);
            Rect wagerLock = (Rect)Call(c, "GroundLockRect", phoneFour[i]);
            Check(name.y + 26f < wagerLock.y,
                "Opponent name touches wager lock at " + resolution + " seat " + (i + 1));
        }
        CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-four-ground-locks.png"));
        var fourth = PointerClick(c, IncomingLock(c, "p1", 3));
        while (fourth.MoveNext()) yield return null;
        Check(phoneFour[3].Status == IPlay.Demo.WagerStatus.Offered, "Phone fourth lock accepted on first tap");
        fourth = PointerClick(c, IncomingLock(c, "p1", 3));
        while (fourth.MoveNext()) yield return null;
        Check(phoneFour[3].Status == IPlay.Demo.WagerStatus.Accepted, "Phone fourth lock missed at " + resolution);
        for (int i = 0; i < 3; i++) Check(phoneFour[i].Status == IPlay.Demo.WagerStatus.Offered, "Phone four-lock selection leaked at " + resolution);

        Call(c, "StartLocalDemo"); Call(c, "CommitShoot");
        Set(c, "phase", "Point"); Set(c, "point", "10");
        Call(c, "OpenBettingWindow");
        var crapTen = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p2", "p1", IPlay.Demo.WagerOutcome.Crap, 10, 10);
        var crapFour = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", "p2", "p1", IPlay.Demo.WagerOutcome.Crap, 4, 5);
        Check(crapTen != null && crapFour != null && crapTen.Number == 10 && crapFour.Number == 4,
            "Phone numbered CRAP offers rejected at " + resolution);
        for (int i = 0; i < 3; i++) yield return null;
        CaptureGameView(Path.Combine(output, "phone-" + resolution.x + "x" + resolution.y + "-crap-10-4.png"));
        Rect pairPager = (Rect)Call(c, "GroundOfferPagerRect", "p2");
        var pairNext = PointerClick(c, new Vector2(pairPager.x + 68f, pairPager.y + 11f));
        while (pairNext.MoveNext()) yield return null;
        Check(((Dictionary<string, int>)Get(c, "offerPage"))["ground:p2"] == 1,
            "Paired-number offer could not be reached beside its sender at " + resolution);

        Call(c, "StartLocalDemo");
        Check(((Dictionary<int, float>)Get(c, "wagerAcceptedAt")).Count == 0, "New session retained old lock feedback");
        Debug.Log("PHONE LOCKS PASSED " + resolution + ": all four amounts accepted via pointer; closed-lock repeat is idempotent; open/closed captures saved.");
    }

    private static IEnumerator VerifyOpponentResponses(StreetDiceGreyboxController c)
    {
        int accepted = 0, declined = 0;
        foreach (string recipient in new[] { "p3", "p4", "p2", "bot-5" })
        {
            Call(c, "StartLocalDemo"); Call(c, "CommitShoot");
            Set(c, "phase", "Point"); Set(c, "point", "10");
            Call(c, "OpenBettingWindow");
            Set(c, "botWagersEnabled", true);
            Set(c, "nextOfferAt", Time.unscaledTime + 1000f);
            Set(c, "nextBotAt", Time.time + 1000f);
            string from = recipient == "p3" ? "p4" : "p3";
            bool againstCatcher = recipient == "p2";
            var offer = (IPlay.Demo.WagerOffer)Call(c, "OfferWager", from, recipient,
                againstCatcher ? IPlay.Demo.WagerOutcome.Hit : IPlay.Demo.WagerOutcome.Crap, againstCatcher ? 4 : 10, 10);
            Check(offer != null, "Bot response fixture was rejected");
            var pending = (Dictionary<int, float>)Get(c, "botOfferResponseAt");
            Check(pending.ContainsKey(offer.Id), "Bot response was not scheduled");
            float deadline = Time.unscaledTime + 4f;
            while (pending.ContainsKey(offer.Id) && Time.unscaledTime < deadline) yield return null;
            Check(!pending.ContainsKey(offer.Id), "Bot response stayed pending after its deadline");
            int stake = (int)Call(c, "AcceptedWagerStake", recipient);
            if (offer.Status == IPlay.Demo.WagerStatus.Accepted) { accepted++; Check(stake == 10, "Bot acceptance missing stake"); }
            else { declined++; Check(stake == 0, "Declined bot offer reserved recipient money"); }
            Check((int)Call(c, "Balance", recipient) == 1000, "Bot decision paid an unsettled wager");
            float settledAt = Time.unscaledTime + 0.15f;
            while (Time.unscaledTime < settledAt) yield return null;
            Check((int)Call(c, "AcceptedWagerStake", recipient) == stake, "Bot decision repeated itself");
        }
        Set(c, "botWagersEnabled", false);
        Debug.Log("BOT RESPONSES PASSED: all four live profiles scheduled and resolved one decision, reserved only accepted stakes, no early payout or repeated acceptance. Observed accepted=" + accepted + " declined=" + declined);
    }

    private static IEnumerator VerifyLowFunds(StreetDiceGreyboxController c)
    {
        Call(c, "StartLocalDemo");
        var money = (Dictionary<string, int>)Get(c, "cash");
        foreach (var seat in new List<string>(money.Keys)) money[seat] = 0;
        money["p1"] = 100; money["p4"] = 20;
        Call(c, "SelectMainWager", 20); Call(c, "CommitShoot");
        Check((bool)Get(c, "shotCommitted") && (string)Get(c, "catcherId") == "p4", "Funded alternative catcher was not selected");
        Call(c, "ResolveLocalRoll", 7);
        Check(money["p1"] == 120 && money["p4"] == 0, "Low-bankroll settlement incorrect");
        var same = (IEnumerator)Call(c, "RunSame");
        while (same.MoveNext()) yield return null;
        Check(!(bool)Get(c, "shotCommitted") && (string)Get(c, "phase") == "ShooterDecision", "Run Same committed an uncovered wager");
        var twice = (IEnumerator)Call(c, "DoubleUp");
        while (twice.MoveNext()) yield return null;
        Check((int)Get(c, "shotAmount") == 20 && !(bool)Get(c, "shotCommitted"), "Double Up mutated an uncovered wager");
        Check(!(bool)Call(c, "CanCover", -1), "Negative wager passed coverage check");
        for (int i = 0; i < 200; i++) Call(c, "PassLocalDice");
        int total = 0; foreach (int balance in money.Values) { Check(balance >= 0, "Passing produced negative funds"); total += balance; }
        Check(total == 120 && !(bool)Get(c, "shotCommitted"), "Repeated passes moved money or committed a shot");

        var modeType = Get(c, "gameMode").GetType();
        Set(c, "gameMode", Enum.Parse(modeType, "CeeLo"));
        Call(c, "StartLocalDemo");
        money = (Dictionary<string, int>)Get(c, "cash");
        foreach (var seat in new List<string>(money.Keys)) money[seat] = 0;
        money["p1"] = 10; money["p3"] = 5; money["p4"] = 5;
        Call(c, "SelectMainWager", 5); Call(c, "CommitShoot");
        Check((bool)Get(c, "shotCommitted"), "Cee-lo banker could not cover two funded opponents");
        Check(((List<string>)Get(c, "ceeLoSeats")).Count == 2, "Broke Cee-lo opponents joined a wager");
        Call(c, "ResolveCeeLoTable", 4, 5, 6);
        Check(money["p1"] == 20 && money["p3"] == 0 && money["p4"] == 0, "Cee-lo paid unfunded opponents");
        Set(c, "gameMode", Enum.Parse(modeType, "Craps"));
        Call(c, "StartLocalDemo");
        Debug.Log("LOW FUNDS PASSED: alternate catcher, guarded Run Same/Double Up, 200 cash-neutral passes, funded-only Cee-lo opponents and conserved balances.");
    }

    private static float MeshSurfaceDistance(Mesh mesh, Matrix4x4 world, Vector3 point)
    {
        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++) vertices[i] = world.MultiplyPoint3x4(vertices[i]);
        var triangles = mesh.triangles;
        float best = float.PositiveInfinity;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var a = vertices[triangles[i]];
            var b = vertices[triangles[i + 1]];
            var c = vertices[triangles[i + 2]];
            var normal = Vector3.Cross(b - a, c - a);
            float area = normal.sqrMagnitude;
            if (area > 1e-15f)
            {
                var projected = point - normal * (Vector3.Dot(point - a, normal) / area);
                if (Vector3.Dot(Vector3.Cross(b - a, projected - a), normal) >= 0f &&
                    Vector3.Dot(Vector3.Cross(c - b, projected - b), normal) >= 0f &&
                    Vector3.Dot(Vector3.Cross(a - c, projected - c), normal) >= 0f)
                    best = Mathf.Min(best, (point - projected).sqrMagnitude);
            }
            best = Mathf.Min(best, SegmentDistanceSquared(point, a, b), SegmentDistanceSquared(point, b, c), SegmentDistanceSquared(point, c, a));
        }
        return Mathf.Sqrt(best);
    }

    private static float SegmentDistanceSquared(Vector3 p, Vector3 a, Vector3 b)
    {
        var edge = b - a;
        float t = edge.sqrMagnitude < 1e-15f ? 0f : Mathf.Clamp01(Vector3.Dot(p - a, edge) / edge.sqrMagnitude);
        return (p - a - edge * t).sqrMagnitude;
    }

    private static void CaptureGameView(string path)
    {
        if (gameView == null) gameView = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        RenderTexture target = null;
        for (var type = gameView.GetType(); type != null && target == null; type = type.BaseType)
            foreach (var field in type.GetFields(Flags))
                if (field.FieldType == typeof(RenderTexture)) target = field.GetValue(gameView) as RenderTexture;
        if (target == null) { Debug.LogWarning("GameView capture unavailable: " + path); return; }
        var old = RenderTexture.active;
        RenderTexture.active = target;
        var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        if (SystemInfo.graphicsUVStartsAtTop)
        {
            var pixels = image.GetPixels32();
            var flipped = new Color32[pixels.Length];
            for (int row = 0; row < target.height; row++)
                Array.Copy(pixels, row * target.width, flipped, (target.height - 1 - row) * target.width, target.width);
            image.SetPixels32(flipped);
        }
        image.Apply();
        File.WriteAllBytes(path, image.EncodeToPNG());
        RenderTexture.active = old;
        UnityEngine.Object.Destroy(image);
    }

    private static void CaptureRenderTexture(RenderTexture target, string path)
    {
        var previous = RenderTexture.active;
        try
        {
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            try
            {
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally { UnityEngine.Object.Destroy(image); }
        }
        finally { RenderTexture.active = previous; }
    }
}
