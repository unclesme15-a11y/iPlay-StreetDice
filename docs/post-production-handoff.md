# Post-Production Handoff

Snapshot taken 2026-09-19 against `6964ff1` (tip of both `main` and this branch).
Nothing was uncommitted or unpushed at that point — this file captures what is
left to do next, not recovered work.

This sandbox has no `dotnet` and no Unity Editor installed, so nothing below
was compiled or run. Findings are based on reading the code/docs only. Treat
line numbers as of `6964ff1`.

## P0 - Dice rolls are not actually server-authoritative

`README.md` and `docs/technical-plan.md` both promise: the backend decides
the dice values, clips/UI just sell it. That is not what the code does today.

- `server/src/IPlayStreetDice.Server/Program.cs:87-94` (`POST
  /api/street-dice/{gameId}/roll`) takes `Die1`/`Die2` straight from the
  client's JSON body and feeds them into `engine.Roll(new DiceRoll(request.Die1,
  request.Die2))` with no server-side randomness at all.
- Compare with `StreetDiceGameEngine.AdvanceBotAction` (`Core/StreetDiceGameEngine.cs`
  around line 184), which correctly calls `random.Next(1, 7)` server-side for
  bot rolls.
- Net effect: any human player can force any roll outcome (always come-out
  win, never seven-out, etc.) by editing the request body. This blocks any
  real-money or cash-equivalent side-bet use, and it's the concrete item
  behind roadmap Phase 6's "Security and abuse controls."

Suggested shape of the fix (for Codex, not applied here):

- Keep `StreetDiceGameEngine.Roll(DiceRoll roll)` accepting an explicit
  `DiceRoll` — the engine-level xUnit tests
  (`server/tests/IPlayStreetDice.Tests/StreetDiceGameEngineTests.cs`) call
  this directly with fixed values on purpose, and should keep working
  unmodified.
- Change the **HTTP** `/roll` endpoint so it generates the dice itself
  (e.g. via `RandomNumberGenerator` or `Random.Shared`, same idea as the bot
  path) instead of trusting `request.Die1`/`request.Die2`.
- If a deterministic-roll path is still needed for manual/local testing over
  HTTP, gate it the same way the voice endpoint already gates dev tokens:
  `StreetDice:AllowDevVoiceToken` / `STREET_DICE_ALLOW_DEV_VOICE_TOKEN` is the
  existing pattern (`Program.cs:140-143`) — mirror it for something like
  `StreetDice:AllowClientSuppliedRoll`, defaulting to off.
- Verify with `dotnet test IPlayStreetDice.sln` before pushing; this touches
  money-moving code (`Credit`/`Debit` in `StreetDiceGameEngine`), so it needs
  the existing rule-contract tests green, plus a new test asserting the HTTP
  endpoint ignores/rejects client-supplied dice in the default configuration.

## P1 - Phase 4: real Kling clips

Roadmap Phase 4 lists five clips still needed: shooter ready, come-out roll,
point roll, catcher fade, hot dice activation. Prompts are already drafted in
`artifacts/kling/street-dice-prompts.md` and the environment plate is locked
(`docs/visual-camera-plan.md` - "low ground bodega closed roll-up door").
This is generation + human curation work (Kling), not something that can be
scripted headlessly - next actionable step is just running those prompts and
dropping the approved clips into `Assets/Resources/Environments`.

Also noted in `docs/visual-camera-plan.md:181`: the next asset-quality jump
is swapping the current procedural dice mesh for scanned/professionally
authored dice models - code already supports it (color selection, hot-streak
override), it's an asset swap, not a code change.

## P2 - Phase 5: multiplayer feel

Server currently has no endpoints for table talk indicators or player
reactions - only side bets exist server-side (`POST
/api/street-dice/{gameId}/side-bet`). Before Unity UI work starts here,
Codex will need to decide whether reactions/table-talk are purely
client-side (no new state to sync) or need a server contract like side bets
do. Spectator-style camera cuts (roadmap Phase 5) are Unity-only and can
start independently once the real clips from P1 exist to cut to.

## P3 - Production voice token signing

`docs/technical-plan.md:72` says it outright: production Vivox signing
"depends on the real signer used by the main card-game voice stack" - a
different repo, not in this session's scope. Two separate blockers, don't
conflate them:

1. Real Vivox issuer/key/domain credentials (secrets - must go through env
   vars or a secrets manager, never committed to this repo).
2. The actual token-signing algorithm, ported from the card-game repo's
   implementation once it's available.

The gate itself (`Program.cs:131-174`) is already correct and intentionally
returns `501` until both are in place - nothing to fix there, just work to
wire in once the dependency is available.

## P4 - Phase 6: production readiness checklist

- **Commercial asset audit**: `Assets/RRFreelance/` (first-person hand pack)
  is paid Asset Store content and is gitignored on purpose
  (`docs/hand-pack-integration.md`). Confirm the license covers the final
  commercial build/distribution before shipping, not just local dev use.
- **Legal review**: the game models real wagering (chip wallet, side bets,
  Double Up) on street dice. Roadmap Phase 6 already flags "Legal review
  before any real-money or cash-equivalent direction" - do not treat P0's
  fix as clearing that; it only closes the fairness hole, not the legal one.
- **Mobile performance / device testing**: no profiling or device-matrix
  testing exists in the repo yet - first pass would be running the existing
  Unity greybox scene on a target device once real dice assets (P1) land.
