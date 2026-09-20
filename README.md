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
- `docs/post-production-handoff.md` - current punch list of what's left.
- `docs/store-submission-checklists.md` - Apple/Amazon/Samsung/Vivox account setup steps.
- `legal-site/index.html` - Privacy Policy/Terms of Use/Support page, also published at https://claude.ai/artifact/Qm6GkeYCcDEgeyFkmBrGUy.
- `server/` - ASP.NET Core backend and xUnit rule tests. `Dockerfile` + `docker-compose.yml` for containerized deployment.
- `.github/workflows/` - CI (build + test on push/PR).
- `unity/StreetDiceGreybox/` - Unity greybox client project/source.
- `tools/` - local verification/build tools.
- `artifacts/` - generated review material and private references.

## Current Status

Run verification:

```powershell
& 'C:\Users\uncle\.dotnet\dotnet.exe' test IPlayStreetDice.sln
.\tools\verify-street-dice-local.ps1 -StartServer
```

The backend is server-authoritative end to end: a full physical dice-roll
lifecycle (`/roll/prepare` → `/roll/fade` → `/roll/commit`) where the server
generates the randomness and simulates the physics, a peer-to-peer wager
system, dice-sale auctions, a 20-second reconnect-grace window for dropped
connections, real Vivox voice token signing, and state that survives a
server restart or container redeploy (snapshotted to disk periodically and
on shutdown). 103 tests currently pass (`dotnet test IPlayStreetDice.sln`).

Unity greybox:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe' -batchmode -quit -projectPath .\unity\StreetDiceGreybox -logFile .\unity\compile.log
```

Kling prompts for first clip tests are in `artifacts/kling/street-dice-prompts.md`.
