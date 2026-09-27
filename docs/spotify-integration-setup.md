# Spotify host-controls setup

What's built and verified vs. what still needs a real toolchain, and the
exact native code to drop in when you have one.

## What's already done (verified)

- **Server**: `HostId` on the game (first real player to join a table --
  same rule as `ShooterId`), and `POST /api/street-dice/{gameId}/music`
  (host-only, 403s anyone else). Ridden on the same
  `GET /api/street-dice/{gameId}` poll every client already uses -- no
  new sync channel. Live-tested with `dotnet run` + `curl`: host posts a
  track, guest's next poll shows it; guest posting gets 403.
- **Client C#** (`Assets/StreetDiceSpotify.cs`): the `ISpotifyPlaybackBridge`
  interface, the host-side push logic, and `FollowHostMusic` (every
  guest's poll loop, applies whatever the host just did). This is real,
  working C# -- it compiles against the interface and is exercised the
  same way the rest of the HUD is.
- **Client UI**: a "Music" page in the in-game drawer (Options → Music).
  Everyone sees a Connect button and "Now Playing." The host additionally
  gets Prev/Play-Pause/Next buttons; guests get a "the host controls
  this" note instead.

## What needs a real Unity + Xcode/Android Studio + Spotify account

Three things, none of which exist in this sandbox:

1. **A Spotify Developer Dashboard app.** Needs your own Spotify account.
2. **The native SDK wired into this Unity project** -- an Android AAR
   and an iOS framework Unity doesn't ship by default.
3. **A real device.** App Remote does not run in the Editor or the iOS
   Simulator.

### 1. Register the app

1. Go to Spotify's Developer Dashboard and create an app.
2. Add a Redirect URI: `iplayceelocraps://spotify-callback` (already
   hardcoded as `SpotifyRedirectUri` in `StreetDiceSpotify.cs` -- change
   both places together if you pick a different scheme).
3. Copy the **Client ID** into `SpotifyClientId` in `StreetDiceSpotify.cs`
   (currently blank -- nothing connects until this is filled in).
4. Under API access, add the Android package name and signing
   certificate SHA-1 (once you have a keystore), and the iOS bundle ID.
5. For anyone beyond a small test-user allowlist, Spotify requires
   requesting extended quota mode -- that's a manual approval on their
   side, budget time for it before a public release.

### 2. Android: drop in the App Remote SDK + this wrapper

Add Spotify's `spotify-app-remote` AAR (and `spotify-auth`) under
`Assets/Plugins/Android/`, per Spotify's Android SDK docs. Then add a
small Kotlin (or Java) wrapper class at
`com.iplay.spotifybridge.SpotifyBridge` -- `SpotifyAndroidBridge.cs`
already calls this exact class/method shape via `AndroidJavaObject`.
Reference implementation:

```kotlin
package com.iplay.spotifybridge

import android.app.Activity
import com.spotify.android.appremote.api.ConnectionParams
import com.spotify.android.appremote.api.Connector
import com.spotify.android.appremote.api.SpotifyAppRemote
import com.spotify.protocol.types.PlayerState
import com.unity3d.player.UnityPlayer
import org.json.JSONObject

class SpotifyBridge {
    private var appRemote: SpotifyAppRemote? = null

    fun connect(activity: Activity, clientId: String, redirectUri: String, callbackTarget: String) {
        val params = ConnectionParams.Builder(clientId)
            .setRedirectUri(redirectUri)
            .showAuthView(true)
            .build()

        SpotifyAppRemote.connect(activity, params, object : Connector.ConnectionListener {
            override fun onConnected(remote: SpotifyAppRemote) {
                appRemote = remote
                subscribeToPlayerState(callbackTarget)
                UnityPlayer.UnitySendMessage(callbackTarget, "OnConnectResult", "true")
            }

            override fun onFailure(error: Throwable) {
                UnityPlayer.UnitySendMessage(callbackTarget, "OnConnectResult", "false")
            }
        })
    }

    private fun subscribeToPlayerState(callbackTarget: String) {
        appRemote?.playerApi?.subscribeToPlayerState()?.setEventCallback { state: PlayerState ->
            val json = JSONObject()
            json.put("trackUri", state.track?.uri ?: "")
            json.put("positionMilliseconds", state.playbackPosition.toDouble())
            json.put("isPlaying", !state.isPaused)
            UnityPlayer.UnitySendMessage(callbackTarget, "OnPlayerStateChanged", json.toString())
        }
    }

    fun disconnect() {
        appRemote?.let { SpotifyAppRemote.disconnect(it) }
        appRemote = null
    }

    fun play(trackUri: String, positionMilliseconds: Long) {
        appRemote?.playerApi?.play(trackUri)
        if (positionMilliseconds > 0) appRemote?.playerApi?.seekTo(positionMilliseconds)
    }

    fun pause() = appRemote?.playerApi?.pause()
    fun resume() = appRemote?.playerApi?.resume()
    fun seekTo(positionMilliseconds: Long) { appRemote?.playerApi?.seekTo(positionMilliseconds) }
    fun skipNext() = appRemote?.playerApi?.skipNext()
    fun skipPrevious() = appRemote?.playerApi?.skipPrevious()
}
```

This is written to the App Remote SDK's documented API shape from
training knowledge -- it has not been compiled against the real AAR
(no Android toolchain here). Double-check method names/signatures
against whatever SDK version you pull in.

### 3. iOS: drop in the App Remote framework + this shim

Add `SpotifyiOS.framework` (from Spotify's iOS SDK) into the Xcode
project (Unity's iOS build lets you add frameworks via
`Assets/Plugins/iOS/`). Then add a small Objective-C++ shim at
`Assets/Plugins/iOS/SpotifyBridge.mm` -- `SpotifyIOSBridge.cs` already
calls these exact C function names via `[DllImport("__Internal")]`.
Reference implementation:

```objc
#import <SpotifyiOS/SpotifyiOS.h>

extern "C" {
    static NSString *UnitySendMessageTarget = nil;
    static SPTAppRemote *appRemote = nil;

    // Minimal delegate; wire this up as the real AppDelegate / a small
    // Objective-C class in a real project rather than a bare block, since
    // SPTAppRemoteDelegate / SPTAppRemotePlayerStateDelegate need an
    // actual object to hold strongly.
    @interface IPlaySpotifyDelegate : NSObject <SPTAppRemoteDelegate, SPTAppRemotePlayerStateDelegate>
    @end

    @implementation IPlaySpotifyDelegate
    - (void)appRemoteDidEstablishConnection:(SPTAppRemote *)appRemoteConn {
        [appRemoteConn.playerAPI subscribeToPlayerState:nil];
        UnitySendMessage([UnitySendMessageTarget UTF8String], "OnConnectResult", "true");
    }
    - (void)appRemote:(SPTAppRemote *)appRemoteConn didFailConnectionAttemptWithError:(NSError *)error {
        UnitySendMessage([UnitySendMessageTarget UTF8String], "OnConnectResult", "false");
    }
    - (void)playerStateDidChange:(id<SPTAppRemotePlayerState>)playerState {
        NSDictionary *payload = @{
            @"trackUri": playerState.track.URI ?: @"",
            @"positionMilliseconds": @(playerState.playbackPosition),
            @"isPlaying": @(!playerState.isPaused)
        };
        NSData *data = [NSJSONSerialization dataWithJSONObject:payload options:0 error:nil];
        NSString *json = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        UnitySendMessage([UnitySendMessageTarget UTF8String], "OnPlayerStateChanged", [json UTF8String]);
    }
    @end

    static IPlaySpotifyDelegate *delegate_ = nil;

    void _SpotifyConnect(const char *clientId, const char *redirectUri, const char *callbackTarget) {
        UnitySendMessageTarget = [NSString stringWithUTF8String:callbackTarget];
        SPTConfiguration *config = [[SPTConfiguration alloc]
            initWithClientID:[NSString stringWithUTF8String:clientId]
            redirectURL:[NSURL URLWithString:[NSString stringWithUTF8String:redirectUri]]];
        appRemote = [[SPTAppRemote alloc] initWithConfiguration:config logLevel:SPTAppRemoteLogLevelNone];
        delegate_ = [IPlaySpotifyDelegate new];
        appRemote.delegate = delegate_;
        [appRemote connect];
    }

    void _SpotifyDisconnect() { [appRemote disconnect]; }
    void _SpotifyPlay(const char *trackUri, double positionMilliseconds) {
        [appRemote.playerAPI play:[NSString stringWithUTF8String:trackUri] callback:nil];
    }
    void _SpotifyPause() { [appRemote.playerAPI pause:nil]; }
    void _SpotifyResume() { [appRemote.playerAPI resume:nil]; }
    void _SpotifySeekTo(double positionMilliseconds) {
        [appRemote.playerAPI seekToPosition:(NSInteger)positionMilliseconds callback:nil];
    }
    void _SpotifySkipNext() { [appRemote.playerAPI skipToNext:nil]; }
    void _SpotifySkipPrevious() { [appRemote.playerAPI skipToPrevious:nil]; }
}
```

Also register the `iplayceelocraps://` URL scheme in the Xcode project's
Info.plist (`CFBundleURLTypes`) and handle it in `UnityAppController` so
Spotify's login redirect makes it back into the app -- standard OAuth
redirect wiring, same as any other "Login with X" SDK on iOS.

This is written to SPTAppRemote's documented shape from training
knowledge -- unverified against a real compile (no Xcode here). Names
and exact delegate signatures may need small adjustments against
whatever SDK version you pull in.

### 4. Manual checklist

- [ ] Spotify Developer Dashboard app created, Client ID copied into
      `SpotifyClientId` in `StreetDiceSpotify.cs`
- [ ] Android package name + SHA-1 and iOS bundle ID registered on the
      Dashboard app
- [ ] `spotify-app-remote` + `spotify-auth` AARs in
      `Assets/Plugins/Android/`
- [ ] `com.iplay.spotifybridge.SpotifyBridge.kt` added to the Android
      plugin sources (or compiled into a small AAR of its own)
- [ ] `SpotifyiOS.framework` added via `Assets/Plugins/iOS/`
- [ ] `SpotifyBridge.mm` added to `Assets/Plugins/iOS/`
- [ ] `iplayceelocraps://` URL scheme registered in the iOS Info.plist
      and forwarded to `SPTAppRemote` in the app delegate
- [ ] Tested on a real Android device and a real iPhone (App Remote
      doesn't run in the Editor or Simulator)
- [ ] Extended quota mode requested on the Dashboard app before public
      release, if you expect more than a handful of test users

## The 3 questions this was built to answer

**Does Spotify need to be open on the phone?** Not visibly -- it doesn't
need to be the app someone's looking at. It does need to be *installed
and logged in*; App Remote wakes it in the background and talks to it
there. If it's been force-quit, the SDK will launch it for the connect
step.

**Is the login from the app?** Yes -- tapping Connect hands off to
Spotify's own official login screen (in-app browser or the Spotify app
itself), the same OAuth flow any "Login with Spotify" button uses. This
game never sees a password, only the access token Spotify hands back.

**Can the controls be on my app?** Yes -- that's exactly what App Remote
is for. Play/pause/skip/seek all happen without leaving iPlay; that's
the "Music" drawer page built above. What can't happen (Spotify's terms
forbid it) is piping the host's actual audio to the other four phones --
each guest's own Spotify plays it locally, kept in sync by the small
metadata broadcast described above.
