# Technical Plan

## Architecture

Use a server-authoritative model.

Backend owns:

- Table creation.
- Seat assignment.
- Shooter/Catcher assignment.
- Roll state.
- Dice result generation.
- Fade/Catch acceptance window.
- Side bet state.
- Payout/score.
- Streak and momentum.
- Voice room entitlement.

Unity owns:

- Presentation.
- Touch controls.
- Dice visuals and timing.
- Kling/live-action clip playback.
- HUD overlays.
- Magnifier display.
- Audio/voice client.

## Fairness

Do not let client video determine dice values.

Recommended roll flow:

1. Shooter taps Roll.
2. Backend opens fade window and prepares authoritative roll.
3. Catcher may send Fade/Catch during allowed window.
4. Backend locks either:
   - `Faded`, no result.
   - `Counted`, with dice values.
5. Unity animates to match backend result.

## Voice

Street Dice should have table voice chat.

Voice channel should be table-scoped, similar to the card game:

```text
iplay.streetdice.table.{gameId}
```

Voice access should require:

- Player is seated.
- Player has valid seat token/session.
- Player is still in the current table.

## MVP Backend Models

Suggested models:

- `GameState`
- `Player`
- `DiceRoll`
- `SideBet`
- `Shot`
- `FadeAttempt`
- `StreakState`
- `VoiceSeatSession`

Suggested phases:

- `Lobby`
- `ChooseShooter`
- `OpenShot`
- `ComeOut`
- `PointEstablished`
- `FadeWindow`
- `RollResolving`
- `Payout`
- `ShooterDecision`
- `GameOver`

## MVP API Shape

Potential REST endpoints:

```text
POST /api/street-dice/create
POST /api/street-dice/{gameId}/join
POST /api/street-dice/{gameId}/start
POST /api/street-dice/{gameId}/shot
POST /api/street-dice/{gameId}/catch
POST /api/street-dice/{gameId}/roll
POST /api/street-dice/{gameId}/fade
POST /api/street-dice/{gameId}/side-bet
POST /api/street-dice/{gameId}/decision/run-same
POST /api/street-dice/{gameId}/decision/double-up
GET  /api/street-dice/{gameId}
POST /api/street-dice/{gameId}/voice/access-token
```

## MVP Verification

Automated tests should cover:

- Come-out `7/11` wins.
- Come-out `2/3/12` loses and keeps dice.
- Point establishment.
- Point hit wins.
- Seven-out loses.
- Fade/Catch nullifies roll.
- Faded roll does not resolve side bets.
- After 3 fades, additional fades increase shooter momentum.
- Run Same keeps previous shot amount.
- Double Up doubles next shot only.
- Full streak activates hot dice color state.
- Normal dice color cannot be red/orange.
