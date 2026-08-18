# iPlay Street Dice

Separate repo for the iPlay street dice game concept.

This is not the iPlay card-game repo. It shares the broader iPlay identity, but it should be planned and built as its own game because the rules, table flow, clips, and UI are different.

## Core Direction

- Street dice foundation, not casino craps.
- First-person real-life Kling-style clips with Unity dice and UI overlays.
- Up to 5 players at a time: first-person shooter at bottom, two players per side or a top-side opponent arrangement depending on scene.
- Voice chat belongs in this game.
- The slap game does not need voice chat.
- Server-authoritative rolls and payouts. Video/clip timing sells the action, but the backend decides the actual dice values.

## Signature Mechanics

- Shooter must always be shooting against someone.
- Opposing player is the Catcher.
- Fade/Catch is an active iPlay defensive button that can stop a roll before it counts.
- Faded roll means no result, no payout, no side-bet resolution, and shooter shoots again.
- After 3 fades, repeated fades build shooter momentum.
- Streak meter is central. Full streak activates hot dice mode.
- Player dice colors: black, white, green, blue.
- Red/orange dice are reserved only for full streak/hot dice mode.

## Repo Map

- `docs/game-rules.md` - current rules contract.
- `docs/visual-camera-plan.md` - camera, player layout, magnifier, and Kling/Unity overlay plan.
- `docs/technical-plan.md` - backend, Unity, voice, and fairness architecture.
- `docs/roadmap.md` - build order.
- `server/` - ASP.NET Core backend prototype and xUnit rule tests.
- `unity/` - future Unity project or client source.
- `tools/` - future verification/build tools.
- `artifacts/` - generated review material and private references.

## Current Status

Backend prototype is implemented under `server/`.

Run verification:

```powershell
& 'C:\Users\uncle\.dotnet\dotnet.exe' test IPlayStreetDice.sln
```

Current backend supports deterministic Street Dice rule testing, table creation, joining, opening a Shooter/Catcher shot, rolling fixed dice values, Fade/Catch, side bets, Run Same, Double Up, streak, momentum, and hot dice state.
