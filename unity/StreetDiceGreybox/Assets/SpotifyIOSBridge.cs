#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Mirrors SpotifyAndroidBridge.cs -- same idea, iOS side. Spotify's iOS App
// Remote SDK (SPTAppRemote) is Objective-C/Swift; these extern declarations
// expect a small native shim (a .mm file added directly into
// Assets/Plugins/iOS/, which Unity's iOS build compiles straight into the
// generated Xcode project -- reference implementation in
// docs/spotify-integration-setup.md) exposing plain C functions that wrap
// SPTAppRemote and report back through UnitySendMessage, the same callback
// pattern as the Android side. Written to the real documented SDK API as
// closely as I could from training knowledge, but unverified: there's no
// Xcode toolchain in this environment to compile or run it against, and App
// Remote needs a real device regardless -- it doesn't run in the Simulator
// or the Unity Editor.
public sealed class SpotifyIOSBridge : MonoBehaviour, ISpotifyPlaybackBridge
{
    [DllImport("__Internal")] private static extern void _SpotifyConnect(string clientId, string redirectUri, string callbackTarget);
    [DllImport("__Internal")] private static extern void _SpotifyDisconnect();
    [DllImport("__Internal")] private static extern void _SpotifyPlay(string trackUri, double positionMilliseconds);
    [DllImport("__Internal")] private static extern void _SpotifyPause();
    [DllImport("__Internal")] private static extern void _SpotifyResume();
    [DllImport("__Internal")] private static extern void _SpotifySeekTo(double positionMilliseconds);
    [DllImport("__Internal")] private static extern void _SpotifySkipNext();
    [DllImport("__Internal")] private static extern void _SpotifySkipPrevious();

    private Action<bool> pendingConnectCallback;

    public bool IsAvailable => Application.platform == RuntimePlatform.IPhonePlayer;
    public bool IsConnected { get; private set; }
    public event Action<string, double, bool> PlayerStateChanged;

    public void Connect(string clientId, string redirectUri, Action<bool> onConnected)
    {
        if (!IsAvailable) { onConnected?.Invoke(false); return; }
        if (gameObject.name != "SpotifyIOSBridge") gameObject.name = "SpotifyIOSBridge";
        pendingConnectCallback = onConnected;
        _SpotifyConnect(clientId, redirectUri, gameObject.name);
    }

    public void Disconnect() { _SpotifyDisconnect(); IsConnected = false; }
    public void Play(string trackUri, double positionMilliseconds) => _SpotifyPlay(trackUri, positionMilliseconds);
    public void Pause() => _SpotifyPause();
    public void Resume() => _SpotifyResume();
    public void SeekTo(double positionMilliseconds) => _SpotifySeekTo(positionMilliseconds);
    public void SkipNext() => _SpotifySkipNext();
    public void SkipPrevious() => _SpotifySkipPrevious();

    // Called by the native shim via UnitySendMessage(gameObject.name, "OnConnectResult", "true"/"false").
    private void OnConnectResult(string success)
    {
        IsConnected = success == "true";
        pendingConnectCallback?.Invoke(IsConnected);
        pendingConnectCallback = null;
    }

    // Called by the native shim's SPTAppRemotePlayerStateDelegate every
    // time Spotify's own state changes, via
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
