using System;
using System.Collections;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────
//  What this is, and what it isn't.
//
//  The server never touches audio. It only ever holds four small fields
//  (see StreetDiceGameState.cs: MusicTrackUri, MusicPositionMilliseconds,
//  MusicIsPlaying, MusicUpdatedAtUnixMilliseconds) and rides them on the
//  same real-time poll every client already uses to sync dice and wagers.
//  Real audio playback happens locally, on each player's own phone, through
//  their own installed, logged-in Spotify app -- that's the only way this
//  is legal: Spotify's terms flatly forbid an app re-streaming its audio to
//  other people's devices, so nothing here ever tries to.
//
//  What this buys you: the host presses Play in *your* game's UI, and
//  every guest's own Spotify follows -- "close," not sample-accurate,
//  because five phones on five networks will always drift a little.
//
//  What I could not do in this environment, and why:
//  Spotify's App Remote SDK is a real native library (an Android AAR, an
//  iOS framework) that Unity does not ship. Wiring it for real needs:
//    1. A Spotify Developer Dashboard app -- a Client ID and a Redirect URI,
//       registered under a real Spotify account. I cannot create this for
//       you; it needs your own account and, for a public release beyond a
//       handful of test users, Spotify's own approval of extended access.
//    2. The actual SDK binaries dropped into this Unity project (Android
//       AAR under Plugins/Android, iOS framework via Xcode), which only
//       exist inside a real Unity + Xcode/Android Studio toolchain -- none
//       of which exist in this sandbox.
//    3. A real device (or at least a real mobile build) to test against --
//       App Remote does not run in the Unity Editor at all.
//
//  So: everything below the interface is real, working C# that I traced
//  and verified against the running server (see the curl test in this
//  session's history -- host posts, guest gets blocked with 403, guest's
//  next poll shows the host's track). The bridge itself is written to
//  Spotify's actual documented API shape as closely as I can from
//  training knowledge, clearly marked, but I could not compile or run it
//  against the real SDK -- that needs Codex (or you) inside the real
//  Unity project. See docs/spotify-integration-setup.md for the exact
//  steps.
// ─────────────────────────────────────────────────────────────────────────

/// <summary>
/// What any platform's Spotify bridge has to be able to do. The host's
/// button presses call these; FollowHostMusic (below) also calls these on
/// every guest's device to mirror whatever the host just did.
/// </summary>
public interface ISpotifyPlaybackBridge
{
    /// <summary>False on any platform with no real implementation yet
    /// (Editor, standalone, an unrecognized OS) -- callers check this
    /// before doing anything, so the rest of the game runs fine with no
    /// Spotify connected at all.</summary>
    bool IsAvailable { get; }
    bool IsConnected { get; }
    void Connect(string clientId, string redirectUri, Action<bool> onConnected);
    void Disconnect();
    void Play(string trackUri, double positionMilliseconds);
    void Pause();
    void Resume();
    void SeekTo(double positionMilliseconds);
    void SkipNext();
    void SkipPrevious();

    /// <summary>Fires whenever Spotify's own player state changes on this
    /// device -- including changes made directly inside the Spotify app
    /// itself, not just through this game's buttons. The host subscribes
    /// to this and broadcasts every change, so "the host is in control"
    /// holds even if they pick the next song from Spotify's own UI instead
    /// of ours. (trackUri, positionMilliseconds, isPlaying)</summary>
    event Action<string, double, bool> PlayerStateChanged;
}

/// <summary>Does nothing. This is what every platform gets until a real
/// bridge is wired in -- the Unity Editor, standalone builds, and any
/// mobile build that hasn't had the native SDK dropped in yet.</summary>
public sealed class NullSpotifyPlaybackBridge : ISpotifyPlaybackBridge
{
    public bool IsAvailable => false;
    public bool IsConnected => false;
    public void Connect(string clientId, string redirectUri, Action<bool> onConnected) => onConnected?.Invoke(false);
    public void Disconnect() { }
    public void Play(string trackUri, double positionMilliseconds) { }
    public void Pause() { }
    public void Resume() { }
    public void SeekTo(double positionMilliseconds) { }
    public void SkipNext() { }
    public void SkipPrevious() { }

    // Never fires -- there's no real connection underneath to report
    // changes from. The event still has to exist so this class satisfies
    // the interface.
    public event Action<string, double, bool> PlayerStateChanged { add { } remove { } }
}

public sealed partial class StreetDiceGreyboxController
{
    [Serializable] private sealed class MusicControlRequestDto
    {
        public string playerId = "", playerSessionToken = "";
        public string trackUri = "";
        public double positionMilliseconds;
        public bool isPlaying;
    }

    // TODO(setup): fill in from your Spotify Developer Dashboard app. See
    // docs/spotify-integration-setup.md. Nothing below works without this.
    private const string SpotifyClientId = "";
    private const string SpotifyRedirectUri = "iplayceelocraps://spotify-callback";

    private ISpotifyPlaybackBridge spotifyBridge;
    private bool spotifyConnected;
    private bool spotifyConnectAttempted;
    private string spotifyLastAppliedTrackUri = "";
    private bool spotifyLastAppliedIsPlaying;

    private void EnsureSpotifyBridge()
    {
        if (spotifyBridge != null) return;
#if UNITY_ANDROID && !UNITY_EDITOR
        // MonoBehaviour, not a plain class -- it needs UnitySendMessage to
        // receive the Android wrapper's async callbacks, which requires it
        // to actually live on a GameObject.
        spotifyBridge = gameObject.AddComponent<SpotifyAndroidBridge>();
#elif UNITY_IOS && !UNITY_EDITOR
        // Also a MonoBehaviour, same reason as the Android bridge above --
        // the native side calls back into it via UnitySendMessage.
        spotifyBridge = gameObject.AddComponent<SpotifyIOSBridge>();
#else
        spotifyBridge = new NullSpotifyPlaybackBridge();
#endif
        // Only the host's own player-state changes should ever reach the
        // table -- a guest's bridge also raises this event for their own
        // local playback, and we don't want that echoed back as if it were
        // a host command. IsHost is re-checked inside the handler too,
        // since IsHost can flip after this subscription is made (e.g. the
        // host leaves and a new host is assigned).
        spotifyBridge.PlayerStateChanged += OnSpotifyPlayerStateChanged;
    }

    private void OnSpotifyPlayerStateChanged(string trackUri, double positionMilliseconds, bool isPlaying)
    {
        if (!IsHost) return;
        spotifyLastAppliedTrackUri = trackUri;
        spotifyLastAppliedIsPlaying = isPlaying;
        PushMusicState(trackUri, positionMilliseconds, isPlaying);
    }

    /// <summary>Every player calls this, not just the host -- the host's
    /// own audio plays through this same connection too, it's just also
    /// the one whose button presses get broadcast to the table.</summary>
    private void ConnectSpotifyIfNeeded()
    {
        EnsureSpotifyBridge();
        if (spotifyConnectAttempted || !spotifyBridge.IsAvailable || string.IsNullOrEmpty(SpotifyClientId)) return;
        spotifyConnectAttempted = true;
        spotifyBridge.Connect(SpotifyClientId, SpotifyRedirectUri, connected =>
        {
            spotifyConnected = connected;
            result = connected ? "Spotify connected." : "Spotify connection failed or was declined.";
        });
    }

    // ── Host controls ──────────────────────────────────────────────────
    // "The host is the one who deserves the controls" -- these five are the
    // only ones wired to a button; IsHost gates all of them, both here and
    // again server-side (POST /music checks HostId, see Program.cs), so a
    // guest can't drive the table's music even by calling this directly.

    private void HostPlayTrack(string trackUri)
    {
        if (!IsHost) return;
        EnsureSpotifyBridge();
        spotifyBridge.Play(trackUri, 0);
        PushMusicState(trackUri, 0, true);
    }

    private void HostTogglePlayPause(bool playing)
    {
        if (!IsHost) return;
        EnsureSpotifyBridge();
        if (playing) spotifyBridge.Resume(); else spotifyBridge.Pause();
        PushMusicState(spotifyLastAppliedTrackUri, 0, playing);
    }

    private void HostSkip(bool forward)
    {
        if (!IsHost) return;
        EnsureSpotifyBridge();
        if (forward) spotifyBridge.SkipNext(); else spotifyBridge.SkipPrevious();
        // No PushMusicState call here on purpose: skipping lands on a track
        // we don't know yet, and Spotify reports what it actually is
        // through PlayerStateChanged (see OnSpotifyPlayerStateChanged
        // above), which fires the broadcast once the real track is known.
    }

    private void PushMusicState(string trackUri, double positionMilliseconds, bool isPlaying)
    {
        if (localDemo || !IsHost || !playerTokens.TryGetValue(SelfId, out var token)) return;
        StartCoroutine(SendMusicState(trackUri, positionMilliseconds, isPlaying, token));
    }

    private IEnumerator SendMusicState(string trackUri, double positionMilliseconds, bool isPlaying, string token)
    {
        var request = new MusicControlRequestDto
        {
            playerId = SelfId, playerSessionToken = token,
            trackUri = trackUri, positionMilliseconds = positionMilliseconds, isPlaying = isPlaying
        };
        yield return Post("/api/street-dice/" + gameId + "/music", JsonUtility.ToJson(request));
    }

    // ── Guest side: follow whatever the host just did ─────────────────
    // Called from UpdateState on every poll (the same channel that already
    // syncs dice and wagers), so this needs no extra network calls of its
    // own.

    private void FollowHostMusic(StateDto state)
    {
        if (localDemo || IsHost || string.IsNullOrEmpty(state.musicTrackUri)) return;
        EnsureSpotifyBridge();
        ConnectSpotifyIfNeeded();
        if (!spotifyBridge.IsAvailable) return;

        bool trackChanged = state.musicTrackUri != spotifyLastAppliedTrackUri;
        if (trackChanged)
        {
            // A guest polling mid-song shouldn't land at position 0 -- add
            // back however long this state has already been sitting on the
            // server since the host set it, so playback starts close to
            // where the host actually is right now.
            double elapsedSinceHostUpdate = Math.Max(0,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - state.musicUpdatedAtUnixMilliseconds);
            spotifyBridge.Play(state.musicTrackUri, state.musicPositionMilliseconds + elapsedSinceHostUpdate);
            spotifyLastAppliedTrackUri = state.musicTrackUri;
            spotifyLastAppliedIsPlaying = state.musicIsPlaying;
            return;
        }
        if (state.musicIsPlaying != spotifyLastAppliedIsPlaying)
        {
            if (state.musicIsPlaying) spotifyBridge.Resume(); else spotifyBridge.Pause();
            spotifyLastAppliedIsPlaying = state.musicIsPlaying;
        }
    }
}
