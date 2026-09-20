# Grade A demo implementation

The complete ten-step Editor-demo objective is verified below. No APK build is authorized.

## Roll camera and intro direction (2026-09-17)

- The sky display is a roll-only display. It starts after release, remains during the moving dice and brief result hold, and clears with the dice reset. A stale visibility flag cannot render the overhead camera between rolls; the betting countdown and sky display cannot appear together in normal play. The earlier overlap was a forced verification fixture, not a gameplay path.
- The catcher-only FADE circle sits 26 GUI units closer to the bottom edge, with its existing size and horizontal placement retained.
- `docs/decisions/shared-iplay-intro-prompt.md` records the common metal-plate/cyan logo ident and the restrained Street Dice accent. The card-game client currently has a simpler 3.5-second logo fade/scale intro and already uses the exact same iPlay voice sting; the requested layered plate animation remains future work.

## Ten-step completion audit (2026-09-16)

The Grade A Editor demo scope below is complete. Later user direction supersedes the older goal wording in two places: FADE is hand-only because a faded roll is dead, and direct red/green ground locks replace the discarded lock-starburst launcher. Physical-device testing remains intentionally deferred until an APK is authorized; Vivox, avatars and the separate Claude start screen are outside this ten-step demo scope.

1. **Wager lock art:** complete. The keyed photographic sheet produces transparent red-open and green-closed locks for `$1`, `$5`, `$10` and `$20`. The all-denomination phone fixtures inspect and tap every state.
2. **Expanding mobile wager controls:** complete. A photographic die beneath each opponent opens that pair's eligible die-backed `HIT`/`CRAP` outcomes to the right, then `$1/$5/$10/$20`; the chosen outcome remains visible through amount selection.
3. **Recipient acceptance:** complete. A red amount-bearing lock appears above the proposer's ground bills, only the recipient can accept it, it becomes green, and duplicate taps cannot reserve or settle twice. The live authenticated Unity/server fixture passes.
4. **Bet timing:** complete. The server owns the ten-second public offer deadline and the shooter's additional five-second acceptance deadline. Boundary and expiry tests pass; the HUD displays the live numeric countdown.
5. **Three Kling fades:** complete under the current hand-only direction. Tap, Plant and Wave use the official watermark-free Kling outputs at the accepted fast timings. All nine style/shade combinations decode, hide dice for the full dead-roll presentation, preserve wagers and permit the same shooter to roll again.
6. **First-person hands:** complete for the demo. Three selectable shades, left/right throwing hand, Classic/Snap styles, hidden wrist, hold/shake/release, visible snap-before-exit and interruption cleanup are covered by pointer, persistence and motion checks at three phone aspect ratios.
7. **Roll presentation refinement:** complete for the demo. Imported regular/hot dice, gesture-dependent trajectories, server-owned Craps outcomes, exact pavement crop, contact shadows, metal impact levels, mandatory sky display and ground-origin result transition pass the 180-case and live-server verification gates.
8. **Physical money presentation:** complete. Individual `$1/$5/$10/$20` bills use the approved faces, ground piles remain readable, and queued full-bill transfers move between loser/winner areas with peel audio. Session cleanup and conserved-ledger checks pass.
9. **Offline opponent pacing:** complete. Opponents offer/respond to wagers, shoot, pass, fade and rotate through both games without stalled turns or negative cash. The final normal-speed run completed six Craps cycles across three shooters and six Cee-lo cycles across all five seats with `$5,000` conserved.
10. **Full demo validation:** complete for the non-APK scope. Backend `78/78`, graphics-enabled Unity readiness, authenticated online Craps prepare/fade/commit, 1280x720, 2340x1080 and 2400x1080 pointer/capture gates, and sustained timing all pass. Latest timing: mean `16.67ms`, p95 `19.53ms`, p99 `22.49ms`, maximum `36.29ms`, one frame above `33.34ms`, none above `50ms`.

Authoritative final logs: `artifacts/unity-smoke/unity-wager-final-clean-fades.log`, `artifacts/unity-smoke/unity-online-ground-lock-final.log`, `artifacts/unity-smoke/sustained-play-final-v2.log`, and `artifacts/unity-smoke/sustained-play.md`. The local HTTP contract gate also passed with an 81-frame committed server replay and shut its temporary server down. No APK was built.

## Shutter countdown and iPlay voice reference (2026-09-17)

- The roll result no longer draws an animated number. The overhead camera still holds on the resting dice, then clears with the dice reset.
- Only the betting countdown uses the graffiti number treatment. Each digit begins faint at the metal shutter and grows no larger than the earlier approved door-sized frame (190 GUI font units); it finishes translucent and readable without moving into the foreground.
- `Voice & Sound` now has a Dice Calling option. It remains disabled until matching iPlay-voice recordings for the values 1 through 12 are present under `Resources/Audio/DiceCalls/`. Once present, Craps calls the sum and Cee-lo calls the three individual die values. The supplied 1.48-second `02-iplay-dark-futuristic.mp3` is preserved in `artifacts/audio-reference/` as the voice reference, not substituted with a different synthesized voice or misrepresented as number calls.
- No APK was built.

## Graffiti HUD and role-specific countdown (2026-09-16)

- The Permanent Marker iPlay face now applies to the complete live GUI skin, including labels, buttons, boxes, toggles, wager controls, drawer text and pregame text. The tutorial checkmark remains a symbol rather than text.
- Removed the small black countdown panel. The initial implementation enlarged each digit toward the viewer and reused that animation for the roll result; the 2026-09-17 direction above supersedes both behaviors.
- The local player sees `15` through `1` when they are the shooter and `10` through `1` in every other role. Both are projections of the actual wager deadlines: the public offer deadline is ten seconds and only the shooter receives the five-second acceptance extension.
- `artifacts/unity-smoke/unity-graffiti-countdown-motion.log` passes the full graphics-enabled readiness suite, exact 15/10 role starts, intermediate deadline boundaries, expiry at zero, shared-font assertions and all three phone layouts. Captures: `betting-countdown-shooter-15.png`, `betting-countdown-shooter-15-approach.png`, and `betting-countdown-player-10.png`. No APK built.

## Live paired wager and main-bet safety update (2026-09-15)

- The backend now owns paired offer/accept wagers, role permissions, available funds, the 10-second offer window and 15-second shooter deadline. It keeps accepted wagers through FADE/no-count rolls and settles each accepted offer once on a counted roll or forfeit. Old solo `/side-bet` and unauthenticated `/bots/advance` now return 410, alongside retired face-posting roll/fade routes.
- Online Unity reads server offers, countdown and player balances rather than the offline `WagerBook`. Its lock taps send authenticated offer/accept requests; no accepted visual state is applied before a server response. Come-out locks display `CRAP 2/3/12`; point-phase CRAP locks retain their selected target (`CRAP 10` or `CRAP 4`). Under the current grouped rule those two CRAP targets share the same settlement outcome; any payout distinction still needs a rule decision.
- Opening, repeating or doubling a main wager now requires both shooter and catcher to cover it; unaffordable stakes are rejected before state changes. Backend suite: 76 passed. Local HTTP smoke passed an accepted $5 CRAP wager through FADE and counted commit, conserved $5,000, and checked the retired routes. Headless Unity Editor fixtures passed the normal online physical throw, an incoming server offer accepted through the Unity path, and catcher FADE after the server betting deadline.
- The graphics-enabled full readiness run passed, including three distinct Kling hand grades. Its headless predecessor failed the pixel-grade check because rendering was disabled, not because shade assets changed. A graphics-enabled online fixture also passed and captured `artifacts/unity-smoke/readiness/online-incoming-crap-lock.png` and `online-accepted-crap-lock.png`; inspected both. The come-out lock shows `CRAP 2/3/12` and $5, changing from red/open to green/closed after server acceptance. The existing point-phase `phone-1280x720-crap-10-4.png` visibly separates `CRAP 10` and `CRAP 4` on two locks. These are Editor captures, not device or final art approval.
- Independent five-phone sessions, remaining hand/fade realism, server-owned Cee-lo and real-device playback are still open. No APK built.

## Server-owned online Craps and ground cash visibility

- Normal online Unity Craps gestures now send only bounded power/aim/handedness to `/roll/prepare`, show the public launch in the held hand, and receive settled faces plus pose replay only from `/roll/commit` after the public fade time. The catcher polls public pending ID/time and uses `/roll/fade` before playing the hand-only Kling clip. The former client-face `/roll` and non-roll-ID `/fade` routes return 410. Online Cee-lo and independent multi-client replay are still unfinished; `/health` remains `serverAuthoritativeRolls:false` for the overall demo.
- A local server-backed Unity Editor run `unity/online-physical-live3.log` passed a covered shot through the normal Unity route and captured launch/replay. `unity/online-catcher-fade-live2.log` passed a p2 shooter/p1 catcher fade, preserved the shooter and money, and captured the Kling hand with no dice. Neither is a real-device or independent-player test. The selected hand clip still shows a Kling watermark and the sky-cam dice look bright; final visual approval is open.
- Large 3D prototype mic stands seen in the first online capture are now hidden in normal online play; the seat HUD keeps human mic/profile placeholders and bot AI labels. Real portraits/Vivox are not implemented.
- `tools/verify-street-dice-local.ps1` now exercises physical prepare/fade/commit, private pre-fade faces, conserved play-money balances, and 410 bypass rejection. It passed a live no-count throw with 107 replay frames. Backend 71 tests and Unity full readiness passed; deterministic point/group/seven-out cases stay in the backend test suite.
- A photo-preserving bill material and a forward shooter pile position now keep a full $20 bill visible left of the dice on the approved pavement. `unity/money-position-capture.log` passed ground and in-flight bill captures; inspected `photo-money-ground.png` and `photo-money-transfer.png`. Flat distant notes still read as strips at this low angle, and device/sound-quality approval remains open. No APK built.

## Numbered CRAP wagers

- User correction: `CRAP` alone does not show whether the active point-group offer targets `10` or `4`. The shared wager book now retains a point-phase CRAP target equal to the point or grouped mate instead of discarding it. Come-out CRAP remains target `2/3/12`. The outcome composer exposes `HIT 10`, `HIT 4`, `CRAP 10` and `CRAP 4` where role permissions allow.
- Under the already documented grouped rule, either `4` or `10` defeats both point-10 CRAP offers; `7` wins them. HIT settlement was not changed. Backend suite: 71 passed, including separate offer IDs/targets, both grouped hits, 7 and come-out target validation.
- Unity `readiness-numbered-crap-phones.log` passes the full suite, a pointer-selected and locked CRAP 4 wager, separate incoming CRAP 10/4 offers, and captures at 1280x720, 2340x1080 and 2400x1080. Inspected `phone-1280x720-crap-10-4.png`: both target numbers and different amounts are readable on their own locks without overlap. These are editor captures, not real-phone touch proof. No APK built.

## Lock starburst and quieter HIT labels

- Incoming wager launcher now uses four tilted miniature red lock cutouts on diagonal spokes behind the green center lock, inside the same fixed pointer rectangle. The first GUI-space rotation attempt scattered one cutout; the corrected local-space matrix keeps the four cutouts together. `unity/readiness-local-tilt-starburst.log` passes full readiness, including launcher/recipient pointer actions and three landscape phone layouts.
- Inspected the refreshed `artifacts/unity-smoke/readiness/phone-1280x720-four-lock-starburst.png`: the launcher no longer reads as a cardinal cross, stays separate from the first full-size offer lock, and all four CRAP locks remain legible. This is an editor capture, not final user approval or real-phone touch proof.
- Number-bearing HIT locks now display only the target number and dollar amount; CRAP locks display `CRAP` and the amount. Inspected the refreshed `wager-lock.png`: the HIT 4 target occupies the lock center without the extra HIT line. Wager rule state and acceptance are unchanged. No APK built.

## Physical roll launch alignment and transport

- Follow-up: Unity now decodes the live route's scalar `launchPoses` and can show each server-chosen starting rotation in the held hand. `unity/readiness-public-launch.log` passes the full suite and focused tests for malformed launch rejection, uninterrupted hand release, and exact position/rotation continuity into a committed replay fixture. `unity/readiness-server-snap.log` also passes the selected snap's visible interval, wrist crop and hand exit during server-pose replay. These use a synthetic committed-frame fixture, not yet an online round from a live server. No APK built.
- The legacy `/fade` path is now rejected while a physical roll is pending; otherwise it could increase fade momentum without cancelling the private result. The roll-ID fade path remains valid. Backend suite: 67 passed.
- Pre-fade **exact pose streaming is not automatically safe**: a modified client with early positions and velocities could simulate deterministic future physics and infer the landing while FADE is still permitted. The approved catcher timing/visibility rule is pending; do not claim final online outcome unpredictability from merely withholding face values. The old face-posting online path still needs replacement.
- Added Unity `ServerPhysicalRollReplay` to validate/decode scalar server commit frames, interpolate poses, map server pavement height into the Unity scene, and apply the same server rotation throughout replay. Editor checks verify regular and hot imported pip faces at rest, malformed frame rejection and interpolation; `unity/readiness-server-replay.log` passed.
- Initial server replay started 1.225m away from the right-hand release. Measured both hand release poses in Unity, then moved the BEPU start bodies to bounded, handedness-specific positions. `unity/readiness-server-origin-aligned.log` passes the complete readiness suite and measures 0.004m right / 0.016m left release-origin differences. This checks positions, not yet the public starting rotation's hand animation.
- Split server-chosen **public launch orientations** from the private spin seed. The public first pose is independent of the private seed; the private seed still determines velocities and final outcome. Backend tests cover both hand origins, seed separation and the 256-seed coarse face/no-count sample; full suite: 67 passed. Sharp-box contacts and broad fairness testing remain open.
- Live HTTP probe on port 5132 caught and fixed empty vector/quaternion objects in `/roll/prepare`. The route now returns only `rollId`, `fadeDeadline` and two scalar `launchPoses`; commit returned faces `6,1`, 78 replay frames and a first frame matching the prepared launch exactly. The probe server was stopped afterward. These are backend checks, not online Unity rounds.
- **Still incomplete:** Unity's online Craps path selects die faces on the client and posts the legacy `/roll`; Cee-lo uses client-selected faces too. No pre-fade streaming, catcher device timing, online replay delivery to other seats, public launch-orientation hand display, rounded dice shape or real-phone validation is proved. `/health` correctly remains `serverAuthoritativeRolls:false`. No APK built.

## Gesture trajectory evidence and online fairness gap

- Unity's `DicePhysicsReplay` launches unnumbered rigid bodies in a separate PhysX scene. The local swipe feeds power and aim into that replay; `unity/readiness-gesture-trajectory.log` passes same-seed comparisons proving a weak and strong throw separate early in forward travel and left/right aim diverge laterally. The full readiness suite passes.
- This is **presentation physics**, not authoritative result physics. `RollCurrentMode` picks dice faces on the client; online `Roll` posts those faces to `/api/street-dice/{gameId}/roll`, and the server currently accepts them as the outcome after checking the shooter session. A modified client could select winning faces. Cee-lo's online evaluation likewise receives client-picked faces.
- Step 7 and the online part of Step 10 remain incomplete until a server-owned gesture/physics outcome protocol is implemented: submit roll intent and gesture, keep a cancellable fade window, commit the server-owned result only when fade no longer applies, then replay that result on the client. The current visual face correction cannot be treated as proof of fairness. No APK built.
- The required unrevealed launch/commit protocol and failure gates are recorded in `docs/decisions/server-owned-roll-contract.md`. `/health` now reports `serverAuthoritativeRules: true` and `serverAuthoritativeRolls: false` rather than the misleading blanket claim. The 54 backend tests pass, and a live health probe returned those exact fields.
- Added `ServerDicePhysics` using BepuPhysics 2.4.0 on .NET 8. It simulates two or three physical dice with gesture-bounded power/aim, pavement/door/side-wall collision geometry, fixed 120Hz steps, settled top-face scoring and explicit simulation/pool disposal. Seven new tests cover fixed-seed counted throws, repeatability, gesture trajectory, invalid input and a 256-seed face/no-count sample. The full backend suite passes 61 tests.
- At the initial simulator check, the component was **not wired to an endpoint or Unity**. Its sharp box collision shape and first-pass contact tuning are not final die realism; the legacy `/roll` still trusts client-selected faces. Private pre-fade trajectory, phone replay, no-count behavior and broad statistical validation remain open. No APK built.
- Follow-up: `StreetDicePhysicalRoll` and authenticated `/roll/prepare`, `/roll/fade`, `/roll/commit` routes now hold the simulated result privately, cancel it only for the catcher before the deadline, and commit/settle exactly once. A 1.25-second human fade window is the current draft, pending user timing approval. Added scalar 60Hz pose frames for eventual phone replay. Four lifecycle tests cover withheld payout, duplicate prepare/commit, role/deadline checks, four fades with momentum and pending-shooter leave. Full backend suite: 65 passed.
- Live HTTP probe: early commit rejected; committed faces `2,3` established a point with 74 replay frames and total money conserved at 2,000; duplicate commit kept the same result. A separate probe rejected non-catcher and duplicate fades, returned `Faded`, preserved the shooter and conserved 2,000. These are two-seat backend probes, not online Unity validation.
- The legacy `/roll` and `/fade` routes remain callable and the current Unity online path still uses them. Therefore `/health` correctly remains `serverAuthoritativeRolls: false`; live online fairness and complete Step 7/10 are not achieved. No APK built.

## Compact live rule HUD

- Outside tutorial mode, the top-right rule label is empty on come-out/Cee-lo and shows only the established Craps point number. Tutorial mode retains `COME OUT`, `CEE-LO` and `POINT n` labels. The center betting countdown now shows only the remaining seconds in a narrower fixed-width field.
- `unity/readiness-compact-hud.log` passes helper assertions for come-out and point labels, plus full play-mode readiness. Inspected the refreshed `artifacts/unity-smoke/readiness/betting-timer.png`: the numeric countdown remains legible and does not overlap the seat or scene content.
- This is text reduction, not a finished timer visual or phone-device validation. The existing Editor indexing exception appears early but does not fail readiness. No APK built.

## Bot seat voice cues

- Live seat HUD now shows a small `AI` profile placeholder for bot seats instead of a muted microphone. The local demo treats every remote seat as a bot; the online prototype preserves microphone cues for human seats only.
- Bot pulse calls are ignored and 3D prototype mic markers remain hidden for bots. `unity/readiness-bot-profile-only.log` passes local and online identity assertions plus full play-mode readiness. Inspected the refreshed 1280x720 four-lock capture; bot symbols are gone without seat overlap.
- Follow-up corrected online identity: `p2` is a human catcher seat, while server-created computer IDs begin with `bot-`. `unity/readiness-bot-identity.log` passes those ID checks, marker checks and full readiness. The earlier static `p2` AI label was wrong for an online room.
- The `AI` square is not a final portrait or profile-picture system. Bot portraits, real online seat identities and physical-device voice presentation remain open. The existing Editor indexing exception still appears early in the log; it did not fail the readiness run. No APK built.

## Radial incoming-lock launcher

- The compact four-lock launcher now fans the existing red lock cutouts around a larger green center lock. Tapping still opens the same four incoming offers; wager rules and acceptance are unchanged.
- `unity/readiness-starburst-radial-live.log` passes the full play-mode readiness suite. Inspected the refreshed desktop and 1280x720 phone captures under `artifacts/unity-smoke/readiness/*four-lock-starburst.png`; neither overlaps the adjacent offer locks.
- The cluster is readable but visually resembles a cross more than a final starburst. User approval and further art refinement remain open. The first attempted batch-mode run exited before verification because it used `-quit`; only the later live-editor log is a passing readiness result. No APK built.

## Wager acceptance feedback

- Accepted locks now perform a restrained 180ms press-and-settle response, shrinking at most 7 percent before returning exactly to their original size. Green closed artwork and accepted wager state apply immediately. Amount placement follows the artwork; pointer bounds never move.
- Acceptance timestamps are per offer, are not restarted by duplicate acceptance, and clear with a new local session. Both human and bot acceptance use the same path.
- `unity/readiness-lock-feedback-verified.log` passes full readiness. Added sampled bounds/size/settle assertions, timestamp creation and duplicate invariance, session cleanup, and accepting-state captures to the all-denomination phone tests. All 12 denomination/resolution cases pass. Backend rerun: 54 passed.
- Inspected the 1280x720 $20 accepting capture: green closed state and amount remain visible. A static capture does not establish perceived motion quality; physical phone/user motion approval remains open. The initial `readiness-lock-feedback.log` failed because a test helper omitted static-method lookup; the corrected run is the authoritative result. No APK built.

## All-denomination phone wager locks

- Expanded readiness with `VerifyPhoneWagerLocks`: creates an actual incoming $1/$5/$10/$20 wager, captures the red open lock, accepts through OnGUI pointer input, verifies the accepted stake, re-taps the closed lock to prove stake invariance, then captures the green closed lock.
- Runs at 1280x720, 2340x1080 and 2400x1080. `unity/readiness-phone-wager-locks.log` passes all 12 cases and the full existing suite. Captures: `artifacts/unity-smoke/readiness/phone-<width>x<height>-lock-<amount>-open.png` and `-closed.png`.
- Visually inspected all eight open/closed 1280x720 captures: all amounts are legible within the lock, with recognizable red open and green closed states. This supersedes the older missing all-denomination closed-state gate. Larger-layout interaction is verified by the same test, not a claim of physical phone touch or user visual approval.
- Remaining: state-transition visual refinement and physical touch delivery when a device build is authorized. No runtime wager rules changed and no APK built. Kling hand-sheet retry remains pending user confirmation; no generation was resubmitted.

## Pregame control contrast

- Replaced the low-contrast default pregame slider with a cyan filled track and larger gold-edged thumb, keeping Unity's actual slider interaction. Tutorial uses a 36-unit outlined checkbox, visible checkmark and matching marker-font label; the entire labeled row remains selectable.
- Custom controls live in `StreetDicePregameControls.cs`; their generated thumb texture is cached and destroyed with the controller. Saved-setting behavior is unchanged.
- `unity/readiness-pregame-styled-controls.log` passes full readiness, including pointer/persistence/bounds checks for sound/tutorial at all three phone resolutions. Inspected checked-state desktop and 1280x720 captures for glyph rendering, contrast and overlap. Physical touch remains untested; no APK built.
- This resolves the previously recorded slider-contrast/default-toggle gaps. It does not finish overall menu visual treatment or hand realism.

## Pregame sound and tutorial settings

- Added functional sound volume and tutorial controls to the pregame menu. The in-game drawer uses the same setters; tutorial preference now survives restart, volume loads clamped to 0..1, and preferences are written only when a value changes rather than every sound-page repaint.
- Centered/enlarged the title and separated Credits into the footer to accommodate the settings row.
- `unity/readiness-pregame-controls.log` passes full readiness and actual pointer interaction with both new controls at all three phone resolutions. Tests check saved values and upper/lower volume bounds, restoring user preferences afterward. Inspected current desktop and 2400x1080 captures.
- Still needs visual styling: the sound slider is low contrast and the tutorial toggle retains the default GUI style. Hand realism and wider menu treatment remain unfinished. No APK built.

## Actual dice selection previews

- Pregame dice colors now display the imported gameplay die and its real recolor material rather than flat color rectangles. Four cached renders share the isolated hand-preview studio, avoiding a continuously active preview camera. Temporary cloned materials and final cached render targets are cleaned up.
- `unity/readiness-dice-picker-pixels.log` passes full readiness, actual selection/persistence at all three phone resolutions, nonblank/framing/color pixel assertions and cache reuse. Inspected the fresh 1280x720 menu capture.
- This completes the dice-thumbnail portion of the pregame redesign, not the whole visual design or ten-step objective. No APK built.

## Fade video compatibility

- Re-encoded the existing hand-only Kling runtime clips as constrained-baseline H.264, without reordered B-frames, with explicit limited-range BT.709 metadata. The supplied source clips had no color-primary metadata; BT.709 is an explicit conventional-HD assumption, not recovered source metadata. No new motion, dice, crop, speed change or color grade was introduced.
- Added `tools/normalize-fade-videos.ps1`. It preserves pre-conversion copies under `artifacts/video-compatibility`, validates every output before replacing runtime files, and checks dimensions, frame count, duration, color tags and strictly increasing equal presentation/decode timestamps. Unity metadata GUIDs are unchanged.
- Preserved 1440x1440 at 60fps: Tap 51 frames / 0.85s; Plant 44 / 0.733333s; Wave 46 / 0.766667s. Decoded-frame SSIM versus pre-conversion copies: 0.992340, 0.991138, 0.991433 respectively. Re-encoding is lossy, not byte-identical preservation.
- `unity/readiness-fade-encoding.log` passes the full readiness suite, including all nine hand/style combinations, hand-only video pause/resume and three phone layouts. No `Unexpected timestamp` or `Color primaries` warnings remain in that log. The separate pre-existing Editor indexing exception remains; this is not a globally warning-free certification.
- Inspected a fresh in-game wave capture: video hand and pavement render in the display, with dice absent during the fade. Physical phone playback and final visual approval remain unverified. No APK built.

## Pregame hand presentation and phone selection

- Refined the live pregame hand previews: clearer finger framing, higher-resolution cached renders, restrained reflections and warmer dark-skin correction. Wider rows retain functional selections and show White/Brown/Black labels. Full design evidence and limitations: `docs/decisions/dice-pregame-style.md`.
- `unity/readiness-pregame-phones.log` passes the full suite and newly repeated hand/dice/fade pointer-and-persistence checks at all three phone aspect ratios. Inspected the narrowest and widest menu captures. Backend: 54 passed.
- The reference concept is not the implemented screen; current model realism and menu materials still need work. Investigate WindowsMediaFoundation timestamp/color-primary warnings on all three hand-only fade clips next. No APK built.

## Interrupted mobile gestures

- Added the application-pause callback alongside focus-loss handling. Both clear an unfinished shake and edge swipe; the paused flag prevents a new throw gesture until resume. Existing released rolls and wager settlement are not changed by this input cleanup.
- `unity/readiness-interruption.log` passes simulated pause/focus cancellation, stale-release rejection, paused-input rejection, fresh gesture after resume and unchanged player funds, plus the complete existing readiness suite.
- These checks invoke Unity lifecycle callbacks in Editor; physical OS interruption delivery remains untested. No APK built.

## Sustained editor timing baseline

- Added frame-interval sampling to the sustained-play verifier after a five-second warmup, without screenshots or accelerated gameplay during measurement.
- `unity/sustained-play-measured.log` passes both game modes: six cycles each, multiple shooters, no 45-second progress stalls, nonnegative balances and conserved $5,000 totals. Latest detailed report: `artifacts/unity-smoke/sustained-play.md`.
- Sample: 9,378 frame intervals; mean 16.84ms, median 16.65ms, p95 21.61ms, p99 27.27ms, maximum 59.01ms. Thirty intervals exceeded 33.34ms and four exceeded 50ms.
- Environment: Unity 6000.4.11f1 Editor, 1334x750 Game View, RTX 5050 Laptop GPU, 60fps target. Includes editor overhead; these are frame intervals, not GPU timings or a standalone/mobile performance guarantee. No APK built.

## Overhead dice contact shadows

- Found that existing contact-shadow quads were on a layer excluded by the overhead camera. Moved them to the dice layer and removed their unnecessary physical colliders. Hand-only fades still exclude that layer.
- `unity/readiness-contact-pixels.log` passes camera inclusion and no-collider assertions, plus a rendered on/off comparison: 83 overhead pixels are measurably darkened by the shadows in the sample pair. The complete readiness suite passes.
- Inspected the refreshed sky-cam capture. This is a subtle contact improvement, not final approval of dice lighting or pavement realism. No APK built.

## Payout session boundaries

- Found that flying bills and queued payout coroutines survived leaving or resetting a session. Added explicit payout coroutine ownership and active-bill tracking, with immediate visual cleanup on leave, restart and controller destruction. Ledger settlement is unchanged.
- `unity/readiness-payout-cleanup.log` passes: real active plus queued transfers are cancelled on restart; old bills become invisible immediately; leave preserves already-settled balances; a fresh payout completes after cleanup. The full existing readiness suite also passes.
- This fixes cross-session visual leakage; it does not establish final bill-motion realism. No APK built.

## Sustained play and audio listener

- Added `SustainedPlayVerification`: normal-time real roll animations, betting windows, money animations and the existing opponent update loop. Local actions use the existing shoot/pass and hold/release methods; no direct outcome settlement or clock acceleration.
- Corrected an initial harness-only timing error: this editor enumerator needs explicit elapsed-time waits, not unprocessed WaitForSeconds objects. The initial failure is retained in `unity/sustained-play.log`.
- `unity/sustained-play-timing.log` passes; `artifacts/unity-smoke/sustained-play.md` records six Craps cycles across two shooters (123.5s) and six Cee-lo cycles across three shooters (32.8s). Every sampled frame preserves $5,000 total and nonnegative balances. This is a bounded session, not hours-long soak or exhaustive opponent coverage; cycles include possible fades.
- That run exposed a missing AudioListener on the runtime-created gameplay camera. Added it, plus a readiness assertion requiring exactly one active listener on that camera.
- `unity/readiness-audio-listener.log` passes audio routing and the complete existing readiness suite, without the repeated no-listener warnings. This establishes routing, not audible phone output or final sound realism.
- Refreshed backend tests: 54 pass. No APK built; full visual and physical-device gates remain open.

## Hidden overhead rendering eliminated

- The overhead camera starts disabled and is enabled in LateUpdate only when its display is visible outside main settings. This observes coroutine roll/fade state before camera rendering and avoids a second scene render while the display is hidden.
- `unity/readiness-camera-lifecycle.log` passes actual camera post-render callback checks: visible display renders; hidden, settings and dismissed states do not. The full readiness suite also passes, including decoded Kling fades and normal roll/reset behavior.
- Inspected the refreshed 1280x720 sky layout: display remains framed and unobstructed. This frame precedes hand entry and does not alone prove fade motion; separate decoded-video checks cover that path.
- This removes unnecessary rendering work; no phone FPS, GPU-time or battery improvement is claimed without device measurement. No APK built.

## Frame-independent sky cam

- Replaced fixed-per-frame zoom interpolation with elapsed-time damping. The camera widens immediately when the dice spread, then eases inward without cropping their required framing bounds.
- `unity/readiness-sky-framing.log` passes the complete existing readiness suite and focused 30/60/90/120 Hz equivalence, immediate widening, zero-time stability and inward overshoot checks. This is simulated timing coverage, not physical-phone performance evidence.
- Inspected the refreshed `sky-display.png`; the sample pair is contained in the overhead display. That editor fixture forces the display during a betting window and is not evidence of actual roll timing. The separate release/reset checks cover display lifetime.

## Hand model selection previews

- Replaced the three hand-color squares with cached renders of the actual RRFreelance hand model, using its three skin materials' albedo, tint and normal textures. Each preview is selectable and the selected shade is underlined.
- Preview rendering uses an isolated camera and layer; editor shader compilation completes before capture to avoid caching the cyan loading placeholder. Render targets are released on teardown.
- Visually checked `artifacts/unity-smoke/readiness/main-options.png`: three textured, distinct shades are visible. These are static model previews, not rotatable viewers; Kling fade videos are unchanged.
- `unity/readiness-hand-previews-final.log` records READINESS PASSED, including shade-selection persistence and the existing full demo checks. Physical mobile validation remains open. No APK built.

## Required delivery

1. Transparent realistic wager locks in four denominations, red open and green closed.
2. Mobile expanding outcome and amount controls, preserving the selected outcome.
3. Recipient acceptance and immutable locked offers using amount-bearing locks.
4. Public 10-second offers and private additional 5-second shooter acceptance.
5. Three hand-only Kling fade animations: Tap, Plant and Wave. Latest user direction removes dice from fades entirely; normal rolls retain the magnified dice display. Fade style is selectable alongside hand appearance in main settings.
6. Refined first-person hands, three shades, classic/snap, hidden wrist.
7. Dice, ground, light, shadow, audio, sky display and result transition refinement.
8. Individual readable bills and physical-looking payout transfers.
9. Believable offline opponent decisions and complete game pacing.
10. Full demo validation, aspect-ratio captures and performance evidence. Physical-phone verification requires a later authorized device build.

## Latest Direction: Hand-Only Fade

- User explicitly removed dice from fade presentation because the roll is already dead. This supersedes earlier hand/dice compositing, dice stopping trajectories and wave occlusion requirements in this progress history.
- Replaced the fade implementation with hand-only Kling playback. All three dice and the shooter hand are hidden as soon as the fade animation starts, and the fade camera excludes gameplay layers. Normal roll rendering is unchanged.
- Removed the legacy rig fade and foreground compositing code. Tap/Plant/Wave is now a saved main-settings selection beside hand appearance. The current catcher uses their chosen animation; the in-game catcher-only FADE action remains.
- `readiness-hand-only.log` passes actual settings pointer selection/persistence, nine style/shade cases with all dice inactive for the full fade, video-only camera layers, playback pause/resume, restored betting, normal roll checks, landscape captures and safe-area pointer/gesture tests.
- Inspected `fade-main-settings.png` and `fade-2.png`. Current video preview: `artifacts/unity-smoke/readiness/wave-hand-only.mp4`. Earlier dice-containing fade videos are historical, not current behavior. No APK built.

## Landscape Phone Layout Evidence

- Confirmed project orientation is landscape, with portrait rotation disabled. Added fixed-resolution Game View capture at 1280x720 (16:9), 2340x1080 (19.5:9), and 2400x1080 (20:9), asserting the actual Screen dimensions before each capture.
- Initial layout fixtures were incorrect (HIT 0 and a forced camera state during open betting). Corrected the selected outcome to HIT 10 and now run an actual planted Kling fade for the overhead capture.
- `readiness-phone-live.log` passes all three resolution checks, actual fade completion and the existing readiness suite. Captures include wagers, overhead and drawer for every resolution under `artifacts/unity-smoke/readiness/phone-*`.
- Inspected 16:9 overhead/drawer, 19.5:9 wagers and 20:9 overhead; the displayed controls and player labels do not overlap in those inspected states. Leave Game remains visible at the bottom of the drawer. This does not verify every state or physical touch sizes, safe-area cutouts, device decoding, or mobile GPU performance.
- No APK built. Full ten-step goal remains active.

## Latest visual direction and fade control

- User rejected the rendered catcher hand. The rig geometry tests below do not establish acceptable realism. Kling is now the required direction for the fade hand presentation.
- Generated a five-second Kling planted-hand preview; provenance and review are in `artifacts/kling/2026-09-13/catch-plant-review.md`. It is not integrated or approved. Tap and wave clips remain outstanding.
- Tightened the overhead framing without increasing world-space dice size. `readiness-close-sky.log` passes the existing checks; visual approval remains open.
- Replaced the rectangular FADE control with a 70-unit circular control, preserving catcher-only visibility and blocking input during an active fade. Click release must be inside the circle.
- `readiness-fade-control.log` passes the existing readiness suite. That suite does not yet directly exercise the new circular hit boundary or prove phone layout quality.
- No APK built; the full ten-step objective remains active.
- Follow-up: `readiness-fade-pointer.log` passes actual pointer checks for circle-corner rejection, catcher-center activation, shooter/spectator exclusion and repeated-tap protection. Increased seat spacing from 166 to 200 logical units to keep the larger circle clear of the next seat label. Physical phone verification remains outstanding.
- User now accepts the Kling clip look, but requires much faster motion and visible dice. A separately retimed preview asset is available; hand/dice compositing remains required.

## Current implementation work

### Kling tap and regular dice follow-up

- Added three runtime hand grades (light, medium, original dark) to the Kling clips, selected from the catcher's existing hand setting. This preserves the original photographic motion/detail; it is not three independently generated hand identities. Both the video and wave foreground share `CatchSkin.cginc` for consistent color/masking.
- Inspected light and medium planted-hand captures (`fade-1.png`, `fade-4.png`). Grades are visibly distinct and preserve creases; full-motion skin-edge/shadow matching and user visual approval remain open.
- `readiness-kling-gradechecks.log` passes focused GPU tests for three distinct grades, unchanged neutral pavement samples, and matching foreground/background skin colors, plus full existing readiness. Neutral test samples do not prove all warm pavement pixels remain unaffected. No extra Kling generation charge; no APK built.

- Added timestamped in-game motion capture for all three fades. Full-size frame encoding initially interfered with short playback; `readiness-kling-motion.log` failed its final still-capture gate. Recording now uses quarter-speed playback only in the first three test cases, then resets speed to 1; exported videos use source clip timestamps to restore intended speed, with a final 0.25-second review hold. Runtime watchdog allows the clip duration at the selected playback speed plus a margin, with a minimum three seconds.
- `readiness-kling-motion2.log` passes: 33 tap, 29 plant and 31 wave captured frames, plus existing checks. Encoded `tap-in-game.mp4`, `plant-in-game.mp4`, and `wave-in-game.mp4` under `artifacts/unity-smoke/readiness/`.
- Inspected a twelve-sample wave motion sheet: dice enter, stop, are occluded by the moving foreground hand, and remain after withdrawal. This is sampled visual review, not a claim of perfect every-frame matting or phone performance. White dice still need better shadow/lighting matching under the hand.

- Wave integration follow-up: imported the fast clip and added `Resources/Fades/WaveForeground.shader`, a clip-specific warm-color matte above the actual dice. Video pavement stays below the dice; the masked hand sits 0.25 world units above that surface, without a collider. Wave dice stop near the frame center beneath the sweeping hand.
- `readiness-kling-wave.log` passes decoded frames for all three Kling styles, dice stopping regions/support, wave foreground height, pointer permissions, video-clock synchronization and full existing readiness checks. Inspected `fade-2.png`: foreground thumb occludes the die beneath it. Single-frame inspection does not prove all blurred edges or matte frames are clean.
- All three demo fade styles now take the Kling playback path; legacy rig helpers remain unused and should be removed after migration coverage is finalized. Current Kling clips are dark shade only. Wave uses multiple sweeps, and mask quality/full-motion review, lighting and other hand shades remain open. No APK built.

- Wave source generated for 24 credits and reviewed in `artifacts/kling/2026-09-13/catch-wave-review.md`; retimed to about 0.75 seconds. It contains multiple hovering sweeps rather than the requested single back-and-forth. Foreground hand masking for dice underneath remains outstanding; do not claim the wave has been integrated.
- Video surface now stays hidden during preparation and until a decoded frame is available, preventing a loading rectangle in the main scene. `readiness-kling-loading.log` passes existing readiness and video-clock checks; device startup/decoder coverage remains open.

- Generated a dedicated three-second Kling tap source for 24 credits. Provenance, exact prompt and output URL are recorded in `artifacts/kling/2026-09-13/catch-tap-review.md`.
- Source contact-sheet inspection showed slower motion than prompted. Retimed it at 5x with a short final empty-pavement hold. Tap now uses the Kling video path; dice start moving at clip time 0.38 seconds, after the inspected hand withdrawal, then settle over 0.30 seconds.
- `readiness-kling-tap.log` passes decoded tap/plant video checks, die framing/support, input permissions, video pause/resume synchronization and existing readiness checks. Inspected the settled tap capture: hand has withdrawn and both dice remain visible. Full frame-by-frame tap entry verification across live throw trajectories remains outstanding.
- Regular dice shader now uses nonmetallic resin response, bounded smoothness and reduced albedo. Settled capture has more edge shading but still bright white faces; final realism approval is not claimed.
- Wave still needs a dedicated Kling clip. Tap and plant currently share only the dark hand shade. No APK built.

### Kling planted-hand integration

- Imported the retimed 0.733-second clip as `Resources/Fades/catch-plant-kling.mp4` and connected it to the Plant fade style using Unity VideoPlayer.
- The video pavement renders beneath the actual game dice, only in the overhead camera. The display becomes square for this clip, preserving its proportions. Dice decelerate over 0.30 seconds into the clear lower area and remain visible during the plant/withdrawal.
- Inspected `artifacts/unity-smoke/readiness/fade-1.png`: photographic hand and both game dice visible, with a clear gap. Dice highlights are too bright against the clip and require further tuning.
- `readiness-kling-framing.log` passes decoded video-frame checks, lower-region dice-center checks, pavement support, fade reset, and the existing readiness suite. These checks do not prove pixel-level hand clearance at every frame or final realism.
- The planted clip is dark shade only. Tap/wave still use the rejected rig; their Kling replacements and other skin shades remain required. Preloading, video-clock synchronization under load, failure handling and device playback still need refinement. No APK built.
- Playback synchronization follow-up: planted dice now use VideoPlayer.time rather than a separate frame-time accumulator. Completion follows the clip end event, with a three-second safety timeout and camera/video cleanup in a finally block. Camera framing is set before playback starts.
- `readiness-kling-clock.log` passes a real video pause/resume test: dice translation and rotation remain fixed during a 0.2-second pause inside the moving interval; resuming completes the fade, restores the wide camera aspect and reopens betting. Existing readiness checks also pass. This is Editor evidence, not mobile decoder or all-failure-path proof.

- Added a shared wager model compiled by the server and Unity. The live controller now integrates offer creation, acceptance, expiration, reserved funds, ground stakes, settlement transfers, forfeits and the extra shooter countdown.
- Live HUD now uses an expanding boxes control, separate HIT point/HIT grouped/CRAP choices, four amount choices, retained selected outcome, and amount-bearing lock. Incoming offers can be accepted; multiple offers can be paged. Lock rendering is explicitly a temporary silhouette pending the realistic sprites. Circular FADE styling and motion are still pending.
- Offline opponents create offers during the public window and respond to offers with a variable delay. Behavioral refinement remains pending.
- Outcome-specific hit bets preserve HIT 10 after a 4; the actual point wins all remaining hit wagers; seven resolves the opposing sides.
- Offers expire at 10 seconds for non-shooter recipients and 15 seconds for the shooter.
- Implementation default: one live offer per outcome per directed pair; other recipients accept within the first ten seconds. These were unresolved in the design discussion.
- Fade choreography above supersedes all earlier notes that describe grabbing, securing or touching dice.

No step is considered completed until its runtime behavior and visual output have been inspected.

## September 15 wager and fade refinement

- Opponent balances are omitted from shared server state; only the authenticated owner can query a wallet. The Unity seat HUD shows only local cash.
- The die under each opponent's name opens that pair's eligible bets. Choices slide right as die-backed HIT/CRAP options. A photographic 1/2/3-pip die replaces the earlier geometric launcher; its edge-connected black source background is removed at runtime without clearing enclosed pips. The in-game options opener uses the same die; the separate Claude start screen has not been imported.
- The ground lock starburst and extra launcher were removed. Red offered and green accepted wager locks appear directly above each bettor's ground bills, and the recipient taps the actual offered lock to accept. All four denominations and multiple senders passed phone-shaped pointer checks.
- Human voice-seat icon is now a face emitting mouth-level waves. FADE sits right of the central hand area with red lettering and its original opacity.
- The three completed Kling fade jobs supplied official watermark-free originals. Runtime clips were replaced from those clean originals using the previously accepted fast timing (51, 44, and 46 frames). No new paid Kling job or APK build was submitted.
- Backend tests: 78/78 passed. Graphics-enabled Unity readiness and all three phone-shaped lock checks passed in `artifacts/unity-smoke/unity-photo-die-clean-fades.log`. This is demo verification, not final mobile touch or visual approval.
- Five-seat clockwise turn order is unresolved: current server seven-out swaps the shooter and chosen catcher, but catcher selection is not forced to the next physical seat.

## Catcher animation pass

- Added `StreetDiceFadePresentation.cs` with tap, plant and wave timelines using the purchased hand rig. Current demo cycles the three styles across consecutive fades.
- The hand renders only in the overhead display (layer 31). Layer 30 remains reserved for the environment image. The wave framing crops the wrist outside the display.
- A fade prevents overlapping throws, interrupts the result replay, retains accepted wagers and opens a fresh betting window after the animation. No fake catcher mic pulse is triggered.
- `readiness-fades-layerfix.log` passes the existing checks plus all three fade intervals, input locking and betting-window restart. Backend tests remain 44/44 passing.
- Inspected runtime `fade-0.png`, `fade-1.png`, `fade-2.png`. The timing/framing is a first implementation, not final realism approval. Hand material lighting, smoothly grounded dice deceleration, collision-clearance measurement across the full animation, and bot-triggered fade behavior still need work. The overhead pavement remains too magnified/soft.
- No APK was built. The full ten-step goal remains incomplete.

## Catcher geometry and material refinement

- Catch dice now finish on a flat face and use rotation-dependent corner support to avoid passing through the pavement during deceleration.
- Catcher skin uses the same material tuning as the throwing hand, with separate reusable material ownership and three supported shades.
- Added occasional bot catch decisions in the roll timeline. Probability decreases after repeated fades and does not inspect the dice values. This still needs a dedicated bot integration/cadence test.
- `readiness-fades-nine.log` passes nine runtime cases (three styles x three shades), checking geometry-to-dice clearance above the circumscribed dice radius, pavement support, main environment visibility, input lock and fresh betting window. Captures are `fade-0.png` through `fade-8.png`; inspected tan wave and dark planted-hand captures.
- Geometry verification uses `BakeMesh(mesh, true)` with the renderer's world matrix. The earlier uncorrected bake applied the rig scale twice and produced oversized test geometry. Logged baked bounds now match the rendered hand's bounds. The test checks triangle surface distance when broad bounds overlap; it does not lower the clearance threshold.
- These cases cover the fixed test entry pose, not every possible live throw position or frame rate. Full fade timing/visual approval, contact shadows, readable overhead pavement, and broader entry-pose coverage remain open.

## Realistic wager lock integration

- Edited the approved blank lock sheet using the built-in image tool, preserving the metal/red-open/green-closed design and replacing only the baked checkerboard with a uniform removal key. Source: `Assets/Resources/UI/wager-locks-keyed.png`.
- `Resources/UI/WagerLockKey.shader` removes that key into two cached RGBA render textures at runtime. The source PNG is not claimed to have alpha. Runtime cutouts do have verified transparent background corners and opaque metal centers.
- Replaced flat lock silhouettes in the live wager control with the realistic cutouts and dynamically centered $1/$5/$10/$20 amounts. FADE control spacing adjusted to avoid overlap with the taller lock.
- `readiness-lock-art2.log` passes, including both lock-alpha checks and the nine fade checks. Captured all four open-lock denominations; inspected `wager-lock-20.png` at game scale with no checkerboard/magenta rectangle.
- Closed-lock gameplay capture, touch interaction, animation between lock states, and small-screen legibility remain to be verified before declaring the lock step finished.

## Wager pointer and closed-lock verification

- `VerifyWagerControls` now injects pointer events through Unity's native Game View input queue, following the editor's own input route. It does not invoke button callbacks or set wager stages directly. Initial synthetic repaint-event attempts failed and were removed from the runtime controller.
- Verified expanding outcome selection, $20 selection, exactly one HIT 4 offer to the intended opponent, recipient acceptance, and repeated-click stake invariance. This is desktop pointer delivery, not a physical phone test.
- `readiness-wager-renderfix.log` passes both the pointer workflow and the full readiness suite. Captured `wager-locked-outgoing.png` and `wager-locked-incoming.png`; inspected the accepted green $10 lock.
- Fixed an actual render-target leak during lock initialization that contaminated the cached lock artwork with miniature scene UI. Lock blitting now restores the previous target. Accepted locks remain visually bright/readable while their input is disabled.
- Remaining lock gates: all denominations in the closed state at small mobile viewports, state-transition polish, and physical touch delivery when a device build is authorized.

## Offline opponent decision pass

- Added shared `DemoOpponentPolicy`: four distinct preferred stakes, variable response delays, affordable-offer acceptance/decline, bankroll-sensitive caution, voluntary passes and bankroll-limited Double Up decisions. No future dice values are accepted by the policy API.
- Live bot offer amounts subtract existing exposure. Responses are considered once; expired/accepted entries are removed from the response scheduler. Disabling bot wagers now disables automatic responses as well as proposals.
- Bot shooter choices now include pass, Run Same and eligible Double Up instead of always following the same branch.
- Backend suite: 54 tests pass, including 20,000 affordable-offer samples, profile preferences, response timing, acceptance pressure, optional shooting, overflow-safe doubling and invalid inputs.
- `readiness-opponent-responses.log` passes all four live response profiles plus the full readiness suite. Observed three acceptances and one decline in this run. Verified only accepted recipient stakes are reserved, balances are not paid before settlement, and decisions do not repeat.
- Long-session bot pacing, pass/Double Up end-to-end paths, and adversarial low-bankroll turn progression remain to be validated. This does not complete the full opponent or ten-step objective.

## Low-bankroll turn progression

- Found and fixed: the next main wager could stall on an insolvent default catcher. Coverage now finds another funded opponent before a new shot; existing funded catchers remain selected.
- Run Same and Double Up validate funds inside the action, not only in the UI. Double Up rejects integer overflow. Removed UI assignments that could commit a shot after a failed action.
- Computer shooters pass when nobody can cover the wager instead of repeatedly attempting an uncovered Run Same.
- Cee-lo builds its round from funded opponents only and requires the banker to cover that actual participant count. Unfunded seats do not pay a wager they could not join.
- `readiness-lowfunds.log` passes: funded catcher replacement, rejected uncovered repeat/double actions, negative-amount rejection, 200 cash-neutral passes, two-opponent Cee-lo settlement and balance conservation. The full readiness suite also passes.
- These are targeted progression checks; extended autonomous match pacing and remaining visual/mobile gates are still open.

## Overhead pavement refinement

- Kept the original 260px square environment crop; disabled lossy default compression and automatic power-of-two resizing for that source.
- Corrected mapping from one crop across 3.5 world units to a finer 0.85-unit material scale. Extended the overhead-only floor so following the dice cannot expose the old floor edge.
- Added `PavementPhoto.shader` to preserve photographed illumination instead of relighting the crop as a plain Standard material. The surface supports live shadow attenuation; actual shadow strength still needs direct visual/performance validation.
- Initial mirror tiling produced an obvious repeated pattern in the runtime capture. Added smoothly blended deterministic sampling offsets of the same crop to reduce that repetition, without generating a replacement surface.
- `readiness-pavement-blend.log` passes the full readiness suite without shader compilation errors. Inspected the updated planted-hand overhead capture: sharper texture, darker photo-matched tone and reduced regular repetition.
- This uses four texture samples per fragment; mobile GPU timing and shadow quality remain open, so the rendering/performance step is not yet complete.

## Verification from current implementation

- Backend: 44 tests pass, including deadline boundaries, separate number settlement, point/7 outcomes, fade preservation, recipient permissions, funds and idempotent forfeits.
- Unity: `readiness-wagers.log` records a passing readiness run with actual spectator offer acceptance, stakes, payout balances and unchanged point, plus the existing 180 motion cases. The editor's existing Search indexing exception remains present before play-mode verification.
- Lock source: `artifacts/ui/locks/lock-states-blank-source.png` generated with the built-in image tool from the approved lock sheet. Blank centers permit dynamic denominations. This is RGB with baked checkerboard, so alpha extraction and visual verification are still required before shipping.
- `readiness-wager-hud.log` passes after the HUD replacement. Actual Game View captures `wager-outcomes.png`, `wager-amounts.png` and `wager-lock.png` verify stage composition. These captures do not prove touch interaction or final visual quality.
