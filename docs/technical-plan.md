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

The prototype exposes the voice entitlement gate now. If Vivox issuer, key, and domain are missing, the endpoint returns a `501` configuration response instead of pretending voice is ready. If those values are present, it still returns `501` until production token signing is wired, unless `StreetDice:AllowDevVoiceToken` or `STREET_DICE_ALLOW_DEV_VOICE_TOKEN=true` is explicitly enabled for local-only testing.

## MVP Backend Models

Implemented prototype models:

- `StreetDiceGameState`
- `StreetDicePlayer`
- `DiceRoll`
- `SideBet`
- `RollResolution`

Voice entitlement gate is implemented for the prototype; production Vivox signing still depends on the real signer used by the main card-game voice stack.

Implemented phases:

- `Lobby`
- `ComeOut`
- `Point`
- `ShooterDecision`
- `GameOver`

## MVP API Shape

Implemented REST endpoints:

```text
GET  /health
POST /api/street-dice/create
POST /api/street-dice/{gameId}/join
POST /api/street-dice/{gameId}/dice-color
POST /api/street-dice/{gameId}/shot
POST /api/street-dice/{gameId}/roll
POST /api/street-dice/{gameId}/fade
POST /api/street-dice/{gameId}/side-bet
POST /api/street-dice/{gameId}/decision/run-same
POST /api/street-dice/{gameId}/decision/double-up
POST /api/street-dice/{gameId}/bots/fill
POST /api/street-dice/{gameId}/bots/advance
POST /api/street-dice/{gameId}/voice/access-token
GET  /api/street-dice/{gameId}
```

## MVP Verification

Automated tests currently cover:

- Come-out `7/11` wins.
- Come-out `2/3/12` loses and keeps dice.
- Point establishment.
- Point hit wins.
- Seven-out loses.
- Seven-out after point passes dice to the Catcher.
- Fade/Catch nullifies roll.
- Faded roll does not resolve side bets.
- After 3 fades, additional fades increase shooter momentum.
- Run Same keeps previous shot amount.
- Double Up doubles next shot only.
- Full streak activates hot dice color state.
- Normal dice color cannot be red/orange.

Run:

```powershell
& 'C:\Users\uncle\.dotnet\dotnet.exe' test IPlayStreetDice.sln
.\tools\verify-street-dice-local.ps1 -StartServer
```
