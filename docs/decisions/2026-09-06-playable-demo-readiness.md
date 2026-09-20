# iPlay Cee-lo & Craps: Unity demo decisions

## Current implementation

- The approved closed-door, single-pavement photographic bodega plate is unchanged.
- A required top-of-screen RenderTexture camera shows the same physical dice objects from above. It frames all active dice and holds their settled poses; it is not an independent roll or a numeric substitute.
- Normal HUD is quiet. Tutorial explanations remain opt-in.
- Main options contain three hand shades and white/black/green/blue dice. Hot dice remain red/orange with the credited Geug mesh.
- The in-game side drawer contains money/bets, Classic/Snap throw style, left/right hand, tutorial, voice/sound, and Leave Game pinned last.
- Hold on the clear lower-center pavement to shake. Swipe upward and release to throw. Distance changes bounded throw strength; horizontal movement changes aim. A short tap or cancelled touch does not throw. No Roll button.
- Committing a craps shot opens a visible 10-second side-betting countdown. Throw input remains locked until zero, late side bets are rejected, and every unresolved point roll opens a fresh window.
- Classic has no snap. Snap is a separate selectable animation immediately after release, before hand withdrawal. Both constrain the wrist below the viewport. Snap pose still needs the user's visual acceptance against the Lonewolf253 reference.
- Throw Style remains a compact Classic/Snap selector; it does not include a thumbnail or small animation preview.
- Shoot/Pass appears before commitment and between settled rounds. Passing leaves the player available to side bet. No mid-point pass or shot reset.
- Quitting a committed shot counts as a crap/forfeit loss. It settles the main wager and live side bets, clears the point and streak, and does not manufacture a dice face result. Repeated leave does not charge twice.
- A leaving side bettor forfeits their open side bets. A leaving main opponent forfeits the main wager; other accepted side bets are not silently erased.
- Cash uses small stacks of bent, textured prop notes on the pavement, separate from the environment image. Piles now match wager exposure using $1/$5/$10/$20 denominations, not a bill-for-bill wallet. Exact balances remain in the HUD/drawer. See `2026-09-06-currency-set.md` for the portrait revision and wager presets.
- Collision-only pavement, closed door and adjacent brick bound roll replay. Impact events distinguish pavement, metal and brick. Audio is synthesized prototype audio, not location recordings.
- Offline Cee-lo now settles a banker round across four opponents, rather than stopping after one comparison.

## Validation and delivery boundaries

- `PlayReadinessVerification.Start` prepares Android settings, enters Unity play mode, exercises cash settlement, Shoot/Pass, touch cancellation, hands, styles, physical roll presentation, sky-camera pixels, and captures the actual Game View.
- `ThrowMotionVerification` covers six faces, two/three dice, five seats and three aspect ratios (180 cases), wrist crop, release continuity, hand exit, snap movement and settled roll lock.
- Android preparation configures landscape, ARM64, IL2CPP and minimum API 26. It does not call BuildPipeline.
- No APK build is authorized or performed by this work.
- This is an OFFLINE, PLAY-MONEY demo. All other seats are computer-controlled test opponents. Human profile pictures and actual speaking indicators require multiplayer identity and voice integration.
- Voice & Sound explicitly reports voice unavailable in the offline demo. Its effects slider works; the microphone control is disabled rather than pretending to connect. Existing server Vivox token signing and real Unity voice-channel connection are NOT production-ready.
- Legacy server-control preview methods are not exposed in the player UI. The backend remains a prototype: persistent accounts, real matchmaking, hardened roll authority, durable wagering, reconnect and real voice need a separate integration pass before online testing.
- Unity editor/desktop tests do not replace Android touchscreen, GPU, audio and performance testing after an explicitly approved APK build.

## Reference motion

- Main street snap reference: https://www.youtube.com/watch?v=lKNMuFg26WY&t=142s
- Alternative first-person reference: https://www.youtube.com/shorts/oDj5lQ64b4Q
- These videos are animation references, not assets bundled in the game.

## Generated assets

Built-in image generation was used for the prop money, not for replacing the Kling alley:

- `Assets/Resources/Money/iplay-prop-note.png`: worn US-style twenty-dollar prop note, flat orthographic scan, cotton fibers, muted engraved printing, creases, small 'iPlay PROP MONEY' and 'NOT LEGAL TENDER' text; no padding, shadows or perspective. Applied to curved paper meshes.
- `Assets/Resources/Environments/bodega-pavement-exact-crop.png` is not generated. It is a literal square crop of the pavement pixels in the approved environment and is the active sky-camera surface.

Generated source images are preserved in the project artifacts. The earlier generated pavement experiment is archived and not packaged in Unity Resources.
