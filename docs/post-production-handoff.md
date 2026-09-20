# Post-Production Handoff

Originally snapshotted 2026-09-19 against `6964ff1`. On 2026-09-19/20, a large
amount of real local work (Codex's, never previously pushed) landed on `main`
at `6b5a80a` and was merged into this branch at `a94da6a` - see "2026-09-20
merge" below for what that actually contained and how P0/P3 below were
superseded. The rest of this document's P1/P2/P4-P9 findings still apply
except where noted.

This sandbox has `dotnet` (installed via apt mid-session) but no Unity Editor,
so server-side claims below are compiled/tested/HTTP-verified; Unity-side
claims are reading-only.

## 2026-09-20 merge: Codex's real feature work landed

`main` had been stuck at an old commit because Codex's local work was too
large to upload through GitHub's web UI (a Unity `Library/` cache folder
mistakenly included, ~4GB) - not because the work didn't exist. Once pushed
properly through git (respecting `.gitignore`), it turned out to be
substantial and well-tested: **101 passing tests**, covering:

- A full server-owned physical dice-roll lifecycle (`/roll/prepare` →
  `/roll/fade` → `/roll/commit`), replacing the old `/roll` endpoint (now
  `410 Gone`). This independently found and fixed the exact same
  client-trusted-dice vulnerability P0 below describes, via a more complete
  fix than the one applied here on 2026-09-19 - **that earlier fix
  (`DiceRollFairness.cs`) has been removed as superseded.**
- A real Vivox token signer (`VivoxTokenSigner.cs`) that additionally
  includes a Unity Gaming Services environment ID in the identity URIs - a
  requirement this session's own signer (`VivoxAccessTokenGenerator.cs`,
  also removed as superseded) didn't know about and would have silently
  failed against a real Vivox account.
- A peer-to-peer wager system (replacing side-bets), a 5-second dice-sale
  auction, a 20-second reconnect-grace window for dropped connections, and
  per-game request locking.

This session's persistence layer (snapshot/restore across restarts) and
Docker packaging were merged forward and updated to match the richer game
state, then re-verified end-to-end (real HTTP server, real game, hard `kill
-9`, restart, confirmed state and session tokens survived) against the
merged code. **Scope note**: persistence covers enduring state (players,
balances, phase, history) but deliberately not the few-seconds-long in-flight
negotiations (an open dice-sale auction, a pending wager offer, a roll
mid-flight) - a restart loses at most a few seconds of that ceremony, never a
player's money or seat. See `Core/Persistence/GameSnapshot.cs`.

The stale `serverAuthoritativeRolls: false` in the `/health` response (a
leftover from before the physical-roll system existed) was corrected to
`true`.

Merge result: 103 passing tests (101 Codex's + 2 new persistence tests),
clean build, pushed to `claude/amazing-hopper-k9u8qv` at `a94da6a`.

## Decisions locked in (2026-09-19)

- **Wagering model: convert to non-cash virtual currency.** Chips/wallet
  balances are to have no real-world value, no cash-out, and no way to
  purchase them with real money. This is what makes a store release
  possible without gambling licensing. Everywhere the game or docs currently
  imply real value needs auditing against this — see P5 below.
- **Target platform order: mobile first, Steam later.** Revised 2026-09-19
  after the store-policy research in P6 below: **Apple App Store first**
  (viable via a 17+ age rating). **Google Play deprioritized** - its policy
  language reads as an outright ban on this genre, not just an age-gate, and
  virtual currency does not clearly avoid it. Amazon Appstore and Samsung
  Galaxy Store are viable secondary Android targets (same APK/AAB as a
  Google Play build) since neither shares Google's stricter stance. Steam-
  specific work (Steamworks SDK, SteamPipe upload, depot config) stays out
  of scope until Apple ships.

## P0 - Dice rolls are not actually server-authoritative — SUPERSEDED, see 2026-09-20 merge note above

`README.md` and `docs/technical-plan.md` both promise: the backend decides
the dice values, clips/UI just sell it. That was not what the code did as of
`6964ff1` — it now is.

**Fix applied**: `Core/DiceRollFairness.cs` (new) generates the roll
server-side with `RandomNumberGenerator` by default. The `/roll` endpoint
(`Program.cs`) only honors client-supplied `Die1`/`Die2` when
`StreetDice:AllowClientSuppliedRoll` (config) or
`STREET_DICE_ALLOW_CLIENT_SUPPLIED_ROLL=true` (env var) is explicitly set —
same pattern as the existing `AllowDevVoiceToken` gate, defaulting to off.
`RollRequest.Die1`/`Die2` are now optional (`int?`) since a normal client no
longer sends them at all.

Verified, not just written:
- `dotnet build IPlayStreetDice.sln` - clean, 0 warnings/errors.
- `dotnet test IPlayStreetDice.sln` - 34/34 passing (the original 30 engine
  tests unchanged and green, plus 4 new `DiceRollFairnessTests`).
- Live HTTP smoke test: ran the real server, opened a shot, and repeatedly
  called `POST /roll` with a rigged `die1:1, die2:1` payload (what a cheating
  client would send) - the server ignored it and returned independently
  random totals every time, confirming the fix holds at the HTTP boundary,
  not just in the unit test. Separately confirmed the opt-in dev override
  (`STREET_DICE_ALLOW_CLIENT_SUPPLIED_ROLL=true`) still honors client dice
  exactly, for local/manual testing.

Original writeup for reference, still accurate as history:

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

`StreetDiceGameEngine.Roll(DiceRoll roll)` itself was left unchanged on
purpose — the engine-level xUnit tests still call it directly with fixed
values, which is the correct way to keep the rule contract deterministically
testable. Only the HTTP boundary needed to stop trusting client input.

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

## P3 - Production voice token signing — SUPERSEDED, see 2026-09-20 merge note above

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

## P6 - Store and regional gambling-policy findings (researched 2026-09-19)

This is genre-classification risk, not a missing-config-file problem - it
determines whether the app can be listed at all on a given store, not just
what rating it gets. Researched live via web search on 2026-09-19; store
policies change, so re-verify against the current published guidelines at
actual submission time rather than trusting this snapshot. None of this is
legal advice.

- **Apple App Store - viable, chosen as the first target.** Apple doesn't
  ban simulated gambling outright; it requires a **17+ age rating** for
  "frequent or intense" simulated gambling (12+ only applies to "infrequent
  or mild," which this game's core loop - shooter/catcher wagering, side
  bets, streaks, hot dice - probably doesn't qualify for). South Korea
  additionally requires a government Rating Classification Number (RCN) for
  that content tier.
  ([AppleInsider](https://appleinsider.com/articles/19/08/20/app-store-shakeup-limits-simulated-gambling-to-users-aged-17),
  [Apple Developer](https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions/))
- **Google Play - deprioritized, likely blocks this genre outright.**
  Google's own policy text: "Apps must not provide simulated gambling
  content (for example, social casino apps; apps with virtual slot
  machines)... [games] where there is no opportunity to win real money or
  prizes based on the outcome of the game." That describes this game's
  genre directly. A April 2025 policy update specifically closed the
  "virtual currency isn't real value" reading - Google now treats
  virtual-currency games/items as having real-world value for gambling-
  policy purposes, which undercuts the P5 virtual-currency decision as a
  fix for Google Play specifically (it still stands for Apple/legal
  purposes).
  ([Play Console Help](https://support.google.com/googleplay/android-developer/answer/9877032?hl=en),
  [Gummicube](https://www.gummicube.com/blog/google-play-developer-policy-changes-real-money-gambling/))
- **Amazon Appstore - viable secondary Android target.** Amazon's real-money
  gambling licensing requirements explicitly do not apply to simulated
  gambling using currency with no value - more permissive than Google here.
  ([Amazon Developer Policy Center](https://developer.amazon.com/docs/policy-center/restricted-content.html))
- **Samsung Galaxy Store - viable secondary Android target.** Samsung's
  distribution guide bars apps that promote/enable real-money gambling but
  allows gambling-themed play without betting real cash/currency.
  ([Samsung Developer](https://developer.samsung.com/galaxy-store/distribution-guide.html))
- **International - flagged high-risk regions, not a full country-by-country
  audit** (scope chosen 2026-09-19: broad launch, flag risk rather than
  clear every country up front):
  - **EU**: Belgium and the Netherlands have precedent treating paid loot-
    box/gambling-style mechanics as regulated gambling. A broader EU
    Digital Fairness Act, expected late 2025/early 2026, may restrict these
    mechanics for minors EU-wide.
  - **Brazil**: a 2025 child-safety law bans loot-box-style sales to minors
    starting March 2026.
  - **UK**: the ASA has moved to active enforcement against app listings
    that don't disclose gambling-like mechanics.
  - **Australia**: 2024 rules require stricter age classification for games
    with simulated gambling; enforcement on major app stores has reportedly
    been inconsistent so far, which is a compliance gap to close, not a
    green light to rely on.
  - **US**: no federal loot-box law; regulation is state-by-state and
    mostly hasn't reached this category yet - currently the most permissive
    major market.
  ([blog.promise.legal](https://blog.promise.legal/lootbox-regulation-2026-game-studios/),
  [The Conversation](https://theconversation.com/loot-boxes-are-still-rife-in-kids-mobile-games-despite-ban-on-gambling-like-features-266226))

## P4 - Phase 6: production readiness checklist

- **Commercial asset audit**: `Assets/RRFreelance/` (first-person hand pack)
  is paid Asset Store content and is gitignored on purpose
  (`docs/hand-pack-integration.md`). Confirm the license covers the final
  commercial build/distribution before shipping, not just local dev use.
- **Legal review**: decided 2026-09-19 - going virtual-currency-only
  specifically to avoid needing gambling licensing/legal review as a
  precondition for a store release. This still needs a plain read-through by
  someone (not necessarily a lawyer, but ideally one) confirming nothing in
  the shipped copy/UI implies cash value, once P5's audit below is done.
- **Mobile performance / device testing**: no profiling or device-matrix
  testing exists in the repo yet - first pass would be running the existing
  Unity greybox scene on a target device once real dice assets (P1) land.

## P5 - Mobile store readiness (App Store + Google Play, decided 2026-09-19)

Checked directly against the repo - none of this exists yet, all of it is
open:

- **No bundle identifier is set.** `unity/StreetDiceGreybox/ProjectSettings/ProjectSettings.asset`
  has `applicationIdentifier: {}` and `overrideDefaultApplicationIdentifier: 0` -
  i.e. no reverse-domain app ID (e.g. `com.iplay.streetdice`) configured for
  any platform. Required before either store will accept a build.
- **Product name still says "Craps."** `ProjectSettings.asset:16` -
  `productName: iPlay Cee-lo & Craps`. This directly contradicts the game's
  own stated positioning in `README.md` ("Street dice foundation, not casino
  craps") and is worth a naming decision before it becomes the App
  Store/Play Store listing name - flagging, not changing, since that's a
  branding call.
- **No mobile input handling yet.** `unity/StreetDiceGreybox/Packages/manifest.json`
  has no `com.unity.inputsystem` (or equivalent touch input) package - the
  greybox controller was built for desktop testing (mouse/keyboard), not
  touch.
- **No in-app purchase package.** No `com.unity.purchasing` in the manifest.
  Not required for the virtual-currency-only model itself, but worth
  deciding now: if there's ever a legitimate real-money purchase (e.g.
  cosmetic dice skins, a "starter chip pack" that's clearly cosmetic/no
  cash-out), that needs StoreKit (iOS) / Google Play Billing wired in
  deliberately, reviewed against each store's IAP rules - don't bolt it on
  late.
- **Backend hosting — Docker exists, live deployment doesn't.** `server/Dockerfile`,
  `docker-compose.yml`, and periodic-snapshot persistence
  (`Core/Persistence/`) were added and verified 2026-09-19/20: built the real
  image, ran it, killed the container, started a fresh one against the same
  named volume, confirmed a game and its player session survived. What's
  still missing is an actual place to run that container 24/7, reachable by
  a phone over the internet - a cloud host (Fly.io, Render, Azure App
  Service, etc.) needs to be chosen and paid for; this repo doesn't pick one.
- **Compliance copy audit — done, came back clean.** Checked 2026-09-20:
  no code anywhere implements a real-money purchase or cash-out path (no
  `com.unity.purchasing`, no billing code, confirmed by grep). The team's
  own `docs/decisions/2026-08-19-founding-table-pass-opportunity.md`
  independently reached the same "play-only, no cash value, cannot be
  purchased, withdrawn, transferred, or redeemed" position already. There's
  also a real, working 18+ self-attestation age gate in the startup flow
  (`unity/StreetDiceGreybox/Assets/StreetDiceStartup.cs`, `DrawAdultGate()`)
  that already says "Play money only." No copy changes were needed.
- **Privacy Policy / Terms of Use / Support page — drafted and published.**
  A working draft grounded in what the app actually does (anonymous player
  ID, no email/password, Vivox voice via Unity Gaming Services, no
  purchases) is live at https://claude.ai/artifact/Qm6GkeYCcDEgeyFkmBrGUy
  and committed at `legal-site/index.html`. It has clearly marked
  placeholders (support email, legal entity name, jurisdiction, liability
  clause) that must be filled in - and the liability/jurisdiction language
  specifically should be reviewed by a lawyer - before this is used as the
  real Privacy Policy/Support URL in any store listing.
