# Codex Handoff Prompt (final, 2026-09-28)

Paste everything below the line into Codex.

---

You are taking over iPlay Cee-lo & Craps (GitHub: unclesme15-a11y/iPlay-StreetDice):
a mobile street-dice game -- Unity client (`unity/StreetDiceGreybox`) plus a
server-authoritative ASP.NET Core backend (`server/`). Up to 5 players per party,
Vivox voice chat, server-rolled physical dice, play money only (never real money).

## Where to work

Work ONLY on branch `claude/amazing-hopper-k9u8qv`. It is far ahead of `main`;
never base work on `main`, never push to `main`. Read these first -- they are
the spec, and they win over anything you'd otherwise assume:

- `docs/decisions/2026-09-27-rank-host-and-rewards.md` -- host rule, rank ladder,
  XP, bet caps, dice-color unlocks, trophy notes
- `docs/decisions/2026-09-28-ads-bankroll-and-seat-hold.md` -- ads, one bankroll
  per account, seat hold, sign-in
- `docs/decisions/2026-09-28-launch-features.md` -- online Cee-lo, The Jungle,
  level-up moment, dice calls, music, share-a-clip, reactive crowd, modern UI
- `docs/spotify-integration-setup.md` -- host-controlled Spotify + search bar
- `docs/decisions/2026-09-27-fifty-hundred-dollar-notes.md` -- $50/$100 art prompts
- `docs/reference/stacked-bills-target.png` -- the approved bill-pile look
- `docs/screen-review.md` -- rounds 1-13: every change so far and why

## Rules that never change

1. **Everything rank-related at a party is the HOST's level, full stop** -- bet
   cap, $50/$100 unlock, dice colors, crowd size. Not additive, not "whichever is
   higher." A host who leaves keeps their level on the party; the host role
   (music, invites) passes to the highest-ranked player still seated. (Built --
   don't change it.)
2. **The only exception:** the broke-player refill ad pays half the player's
   OWN max bet per ad, and they can keep watching until they reach the HOST's
   max bet.
3. **Only the host can share or re-share a party invite.**
4. **No dice sale.** Losing the shot is Shoot or Pass only.
5. **Every button and screen uses the iPlay style** (metal plates, bill photos,
   lock art) -- never default Unity look.
6. **Music controls sit directly on the Options drawer**, never behind a sub-tab.
   Game Stats is its own page reached from Options.
7. **XP numbers live only in server `RankLadder.cs`.** Don't scatter them.
8. **Play money only.** Never sell play money or let it be cashed out.

## What already exists (don't rebuild)

Server (166 passing tests): craps engine with server-rolled physical dice, peer
wagers, persistence across restarts, accounts (sign up / sign in, logins survive
restarts), rank ladder (Level 1-5 caps $100/$250/$500/$750/$1,000), XP (everyone
seated earns per finished shot; shooter/catcher and winner earn more; first 15
shots a day earn 5x; no daily cap; five steps per level), host hand-off, trophy
notes, host-only music sync endpoint.

Client (never compiled -- see step 1): bill-stacking stake picker with a Clear
button, Game Stats page (sign in, level + 5-step progress bar, XP, trophy
notes), inline music controls, Spotify bridges (Android/iOS), hot dice.

## Priority order

### 1. Compile and verify
Open the Unity project and fix every compile error. Never compiled yet:
`StreetDiceAccount.cs`, `StreetDiceSpotify.cs`, `SpotifyAndroidBridge.cs`,
`SpotifyIOSBridge.cs`, plus recent edits to `StreetDicePlayExperience.cs` and
`StreetDiceGreyboxController.cs`. Run `Editor/PlayReadinessVerification.cs`.
Add a readiness check for bill stacking (tapping bills adds up, stops at the
cap, Clear resets). Screenshot: stake picker, Options drawer, Game Stats signed
out and signed in, hot dice.

### 2. Real phones
Test on a real Android phone and a real iPhone before building features.

### 3. Core systems (specs in the rank and ads/bankroll docs)
- a. **Sign-in required for online.** Reject `join-real` without a valid account
  token AND add the client sign-in prompt -- ship both together. Offline vs AI
  stays open to everyone.
- b. **One bankroll per account.** Money follows the account between parties
  (closes the leave-and-rejoin-for-$1,000 loophole). New accounts start at $1,000.
- c. **Seat hold until the party ends.** A dropped or ad-watching player's seat
  and money are held; the party skips them; rejoining with the code on the same
  account returns them to their seat. The host re-sharing the invite frees a
  held seat for someone new. While the HOST's seat is on hold, the next-highest
  rank covers as host and hands it back when they return. Ad breaks mark the
  player "away" so the 20-second disconnect timer doesn't remove them.
- d. **Broke-player refill, server side**, granted only through AppLovin MAX's
  server-side verification callback (never trusted from the app).
- e. **Dice color unlocks:** white at start, Level 2 green, Level 3 black,
  Level 4 red and blue with white pips (the red visibly different from hot
  dice), Level 5 a color wheel in settings showing real RGB numbers.
  Host-driven like everything else.
- f. **Rate-limit** `POST /api/accounts/login`.

### 4. Online Cee-lo
Cee-lo is offline only today. Build a server-authoritative Cee-lo engine next to
the craps one (3 server-rolled physical dice, same roll lifecycle, seats,
sessions, persistence), enable Online for Cee-lo, and apply every rule above
(host rule, sign-in, bankroll, seat hold, trophy notes, voice, host music). XP:
each banker-vs-player result counts as one finished shot.

### 5. The Jungle
Public parties -- a must for both Craps and Cee-lo. Private party (join by code)
or The Jungle (browse/quick-join open public parties showing host level, seats
filled, whether a shot is live; none open -> start one as host). No host Spotify
in The Jungle -- it plays the iPlay soundtrack.

### 6. Launch features (launch-features doc)
- **Street-dice speed (already fixed server-side 2026-09-28 -- keep it):** a
  betting countdown opens only at the come-out and when the point is set; the
  point rolls after that have no countdown (see `docs/game-rules.md`). Verify
  the client HUD matches. Replace the big countdown number with a **color
  countdown ring around the lock** once a lock is on screen: it drains around
  the lock through the 10-second propose window (green -> yellow -> red), and
  the shooter's ring keeps going through their extra 5 seconds to lock bets in.
- **Betting flow (built 2026-09-28 -- compile it, verify on phones; rules in
  `docs/game-rules.md`):**
  - Come-out overlay: no menu. A CRAP 2/3/12 lock twice the standard size
    (170x198 vs 85x99) center screen for every non-shooter, a NO BET tab beside
    it, and a glowing countdown line around it (`DrawComeOutOverlay` /
    `DrawCountdownLine` in `StreetDiceWagerHud.cs`). Press the lock -> bills ->
    the lock drops to the ground; NO BET = done. It stays up whether the bet
    menu is open or not -- BET never hides it.
  - Point set: the bet menu pops open by itself for non-shooters
    (`UpdateBetMenuAutoOpen`) -- the only automatic pop-up. It closes when the
    propose time ends; only locks stay. BET opens/closes it any time betting
    is allowed. "He don't hit 10" is the CRAP 10 lock.
  - Ends early when everyone's done: proposing, NO BET, or closing the menu at
    the point marks a player done (`WagerBook.MarkDone`, server `POST /wager/done`); when all
    non-shooters are done the countdown ends and the shooter keeps up to 5
    seconds only while a lock is waiting on them. Offline bots count as done
    2-6 seconds into the window.
  - Late bet: after the point window, a player with no live bet against the
    shooter can still propose CRAP point / paired number between rolls
    (`WagerBook.CanProposeLate`); it expires at the next throw if not taken.
  - A come-out 2/3/12 keeps the dice and starts a fresh come-out (already how
    the engine works).
- **Level-up moment:** flash, crowd reaction, new level + unlocks revealed;
  smaller version per step; everyone at the party sees it; never mid-roll.
- **Dice calls, Tutorial Mode only:** in real play the voice chat IS the dice
  calling. Remove the separate Dice Calling switch; Tutorial Mode controls it.
  Recordings come from the owner.
- **Music** (full spec in launch-features #5):
  - Menus: the owner's cousin's instrumentals.
  - The Jungle: the iPlay soundtrack, nobody controls it, synced so every phone
    plays the same song at the same moment.
  - The soundtrack plays from an in-game Bluetooth speaker prop at the spot:
    3D-positioned, filtered to sound like a portable speaker, pulsing to the
    music. (Spotify audio can't get this effect -- it doesn't run through the
    game.)
  - Private parties: the HOST chooses "Party music: Spotify / iPlay Soundtrack"
    in the Options drawer. Soundtrack = synced, from the speaker prop, host gets
    play/pause/skip. Spotify = host Spotify plus a search bar (Spotify Web API
    search -> `HostPlayTrack`). Only the host's choices count (already enforced
    server-side). No always-listening voice command -- "Hey Siri/Google, play
    ___ on Spotify" already syncs the party.
  - Free Spotify guests can't play a specific song (Premium only): check
    `canPlayOnDemand`; if false, play the closest thing Spotify allows (host's
    album/artist on shuffle) and offer a one-tap switch to the iPlay soundtrack.
  - Every player's own sound controls: music volume slider, "Mute party music"
    switch, and music that dips automatically while anyone talks on voice chat.
  - Audio files and signed permissions (a parent/guardian for each minor
    artist) come from the owner.
- **Share-a-clip:** one tap exports a short vertical replay (hot streak, big win,
  level-up) with the iPlay logo via the share sheet; re-render from the recorded
  roll frames.
- **Reactive crowd:** build the event hooks (hot dice, big wins, seven-out /
  1-2-3, dice in the air), crowd size by host level. Claude is sourcing the
  footage; drop it in once the owner approves.

### 7. Assets
- **$50/$100 notes:** generate from the exact prompts in the fifty-hundred doc
  into `Assets/Resources/Money/iplay-note-50.png` and `iplay-note-100.png`. The
  stake picker and ground piles pick them up automatically.
- **Bill pile:** must match `docs/reference/stacked-bills-target.png` -- a loose
  overlapping fan. Cap the fan at about 5 notes and stack extra notes on top, so
  big stakes read as a thick fanned stack, not a long line.

### 8. Spotify on real devices
Follow `docs/spotify-integration-setup.md` once the owner supplies a Spotify
Client ID.

### 8b. Ads -- AppLovin MAX (spec in the ads/bankroll doc)
Integrate AppLovin MAX mediation once the owner supplies the SDK key and ad unit
IDs (use MAX test mode until then; never ship test IDs):
- Banners on every non-game screen only (menus, online lobby, settings,
  credits) -- never inside a game or the in-game drawer; leave a clear strip so
  a banner never covers a button.
- Party ad break (full-screen, skippable after 15 seconds) for the WHOLE party
  at once: after every 2nd seven-out at the party, whoever threw it. The
  party's first 5 seven-outs always get their 2 breaks (no timer); after that
  a break needs at least 3 minutes since the last one ended, but never more
  than 3 seven-outs without one (about 12 breaks an hour). Server pauses the party (no rolls, timers frozen, everyone
  "away"), resumes when all ads close or after 35 seconds. Never counts a
  come-out 2/3/12; never mid-roll. Cee-lo: the same, counting every time the
  bank passes to a new banker instead of seven-outs.
- Rewarded ad when broke (player opts in): pays half the player's OWN max bet
  per ad, up to the HOST's max bet; granted via MAX server-side verification.
- Before any ad starts, mark the player "away" (seat hold) so the disconnect
  timer never removes them. Never show an ad mid-roll.
- Offline vs the computer uses the same ad rules.
- MAX's consent flow; iPhone tracking prompt text is in the ads doc.
- Extend the 18+ screen text with "No real money. Nothing of real value can be
  won." (Google's social casino requirement).
- No in-app purchases of any kind.

### 9. Modern UI switch -- last, before launch
Move menus, HUD and drawer off IMGUI (`OnGUI`) to uGUI or UI Toolkit, keeping the
exact iPlay look. Only after gameplay is verified on phones.

## Do NOT build (saved for later or owned by someone else)
Daily challenges (saved for after launch), player cards / rank titles,
seasons / leaderboards, cosmetic shop or any in-app purchase, store trailer,
crowd footage itself (Claude is sourcing it).

## Before every push
- `dotnet test` in `server/tests/IPlayStreetDice.Tests` passes (166/166 now,
  plus whatever you add).
- Unity compiles clean; the readiness run passes; screenshots of any UI change.
- Add a new round to `docs/screen-review.md` for each change: what, why, how
  verified.
- Push only to `claude/amazing-hopper-k9u8qv`.
- If something in the docs is unclear or contradicts itself, stop and ask the
  owner instead of guessing.
