#if UNITY_ANDROID
using System;
using UnityEngine;

// Real Android App Remote SDK calls (SpotifyAppRemote.connect, getPlayerApi()
// .play/.pause/.resume/.skipNext/.skipPrevious/.seekTo) are Java, with a
// callback-based connect flow -- not something C# can call directly. This
// expects a small companion Android library, com.iplay.spotifybridge.
// SpotifyBridge, that wraps those calls behind plain static methods and
// reports back through UnitySendMessage. The reference Kotlin for that
// wrapper is in docs/spotify-integration-setup.md -- written to the real
// documented SDK API as closely as I could from training knowledge, but
// unverified: there's no Android toolchain in this environment to compile
// or run it against. Needs a real device to test at all; App Remote does
// not run in the Unity Editor.
public sealed class SpotifyAndroidBridge : MonoBehaviour, ISpotifyPlaybackBridge
{
    private const string BridgeClass = "com.iplay.spotifybridge.SpotifyBridge";
    private AndroidJavaObject bridge;
    private Action<bool> pendingConnectCallback;

    public bool IsAvailable => Application.platform == RuntimePlatform.Android;
    public bool IsConnected { get; private set; }
    public event Action<string, double, bool> PlayerStateChanged;

    public void Connect(string clientId, string redirectUri, Action<bool> onConnected)
    {
        if (!IsAvailable) { onConnected?.Invoke(false); return; }
        if (gameObject.name != "SpotifyAndroidBridge") gameObject.name = "SpotifyAndroidBridge";
        pendingConnectCallback = onConnected;
        using var activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
        bridge = new AndroidJavaObject(BridgeClass);
        // gameObject.name is the UnitySendMessage target; OnConnectResult /
        // OnPlayerStateChanged below are the methods it calls back into.
        bridge.Call("connect", activity, clientId, redirectUri, gameObject.name);
    }

    public void Disconnect() { bridge?.Call("disconnect"); IsConnected = false; }
    public void Play(string trackUri, double positionMilliseconds) => bridge?.Call("play", trackUri, (long)positionMilliseconds);
    public void Pause() => bridge?.Call("pause");
    public void Resume() => bridge?.Call("resume");
    public void SeekTo(double positionMilliseconds) => bridge?.Call("seekTo", (long)positionMilliseconds);
    public void SkipNext() => bridge?.Call("skipNext");
    public void SkipPrevious() => bridge?.Call("skipPrevious");

    // Called by the Android wrapper via UnitySendMessage(gameObject.name, "OnConnectResult", "true"/"false").
    private void OnConnectResult(string success)
    {
        IsConnected = success == "true";
        pendingConnectCallback?.Invoke(IsConnected);
        pendingConnectCallback = null;
    }

    // Called by the Android wrapper's Player API subscription every time
    // Spotify's own state changes -- a song ending, someone tapping skip
    // inside the Spotify app itself, etc -- via
    // UnitySendMessage(gameObject.name, "OnPlayerStateChanged", json), json
    // shaped like {"trackUri":"spotify:track:...","positionMilliseconds":12345.0,"isPlaying":true}.
    [Serializable]
    private sealed class PlayerStatePayload
    {
        public string trackUri = "";
        public double positionMilliseconds;
        public bool isPlaying;
    }

    private void OnPlayerStateChanged(string json)
    {
        var payload = JsonUtility.FromJson<PlayerStatePayload>(json);
        if (payload == null) return;
        PlayerStateChanged?.Invoke(payload.trackUri, payload.positionMilliseconds, payload.isPlaying);
    }
}
#endif
