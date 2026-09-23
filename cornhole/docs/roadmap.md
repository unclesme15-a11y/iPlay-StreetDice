# Roadmap

## Phase 1: Rules And Look Lock

- Approve the rules contract (`game-rules.md`) and settle what's left in `open-decisions.md`.
- Generate and lock the **Thrower View** still in Kling (text-to-image first; don't spend on video yet).
- Lock the matching Far Board Cam and Near Board Cam stills from the same setting.

## Phase 2: Scoring Backend

- Cornhole rules engine: innings, alternating throws, cancellation scoring, target score from Match Setup (default 21), bust/skunk toggles.
- Unit tests for each scoring example in `game-rules.md`.
- Seeded bag physics on the server, plus a frame-path transport (copy the Street Dice pattern).

## Phase 3: Unity Greybox

- Invisible boards lined up on the locked plates.
- First-person hand + swipe throw + bag flight.
- Board-cam cut after release.
- Match Setup screen, HUD, bag counters, inning summary.
- Local practice mode against a placeholder bot, no video yet.

## Phase 4: Video Opponents

- Generate one character's clip set (idle, step-up, throw, 3 reactions) with Kling image-to-video from the locked plate.
- Add the release-frame JSON and the Unity bag hand-off.
- Second and third characters once the first one looks right.

## Phase 5: Multiplayer

- 2v2 seats, human/bot mixing, avatar selection.
- Voice.
- Reconnect / bot takeover.
