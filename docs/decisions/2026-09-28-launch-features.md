# Launch Features -- Approved 2026-09-28

The owner picked these from a "what takes this from a 6-8 to a 10/10" review.
None are built yet. Everything follows the host rule in
`2026-09-27-rank-host-and-rewards.md`.

**Saved for later on purpose -- do not build:** daily challenges (the owner
wants something to add after launch), player cards with rank titles,
seasons/leaderboards, a cosmetic shop, and a store trailer.

## 1. Online Cee-lo

Cee-lo is half the game's name, but today it's offline only: the Online button
is switched off for Cee-lo (`StreetDiceStartup.cs`, `DrawModeMenu`), and the
server only has a rules evaluator (`CeeLoRules.Evaluate`), no online game
engine.

- Build a server-authoritative Cee-lo game engine alongside the craps one:
  server-rolled physical dice (3 dice), same prepare/commit roll lifecycle,
  same seats, sessions, reconnect and persistence.
- Everything cross-cutting applies unchanged: host rule (host's level sets the
  max bet and unlocks), host hand-off, sign-in for online, one bankroll per
  account, seat hold, trophy notes, $50/$100 unlock, voice chat, host music.
- XP: each banker-vs-player result counts as one finished shot (the banker and
  that player are the two "in the shot"; everyone seated earns seated XP).
  Keep the pacing targets in `RankLadder.cs`.
- Cee-lo gets its own public parties in The Jungle (#2).

## 2. The Jungle (public parties) -- a must for Craps AND Cee-lo

"The Jungle" is the owner's name for finding a game with strangers. It fixes a
new player with no friends online landing on an empty screen.

- Two ways to play online: a **private party** (today's flow: join by code,
  only the host can share or re-share the invite) or **The Jungle**.
- The Jungle lists open public parties for the chosen game (Craps or Cee-lo),
  showing the host's level (it sets the party's max bet and unlocks), seats
  filled out of 5, and whether a shot is live. Quick join drops the player into
  the best open one; if none, offer to start a new Jungle party as its host.
- All party rules still apply (host rule, seat hold, one bankroll, sign-in).
- **No host Spotify in The Jungle** -- strangers shouldn't have one person's
  music forced on them. The Jungle plays the iPlay soundtrack instead (see #5).

## 3. Level-up moment

- When a player crosses into a new level: screen flash, crowd reaction (#7),
  the new level and what it unlocks (dice color, bet cap, $50/$100) revealed.
- A smaller version for each of the 5 steps inside a level.
- Everyone at the party sees it with the player's name. Never interrupt a live
  roll; show it after the shot resolves.

## 4. Dice calls -- Tutorial Mode only

Spoken dice calls exist in code but are silent: they need recordings at
`Resources/Audio/DiceCalls/<value>` (see `StreetDicePlayExperience.cs`).

- **They play only while Tutorial Mode is on.** In real play the voice group
  chat *is* the dice calling -- players calling rolls to each other is the most
  real it gets, so the game stays out of the way.
- Remove the separate "Dice Calling" switch from Voice & Sound; Tutorial Mode
  controls it. Update `VerifyDiceCalling` in the readiness checks to match.
- Recordings (owner to provide): every number the game calls, plus the street
  names for craps (e.g. "Yo-leven") and Cee-lo (e.g. "4-5-6", trips). The
  owner decides the exact wording and voice.

## 5. Music

- **Menus:** the owner's cousin's instrumentals.
- **The Jungle:** the iPlay soundtrack (owner's songs), playing on each
  player's phone; volume in settings.
- **Private parties:** the host's Spotify, as already designed
  (`docs/spotify-integration-setup.md`), plus a **search bar** in the Options
  drawer's Music section so the host can pick songs without leaving the game.
  Search uses Spotify's Web API (App Remote alone can't search), then plays the
  chosen track for the whole party through the existing sync.
- **Voice requests:** "Hey Siri / Hey Google, play ___ on Spotify" already
  works with no extra code -- the host's Spotify changes and the game's
  existing player-state listener syncs the party. The search bar also gets
  the phone keyboard's built-in dictation mic for free. Do **not** build an
  always-listening "Spotify, play..." command: the mic already streams the
  group chat, so the game would be listening to every word of the party.
- **Rights:** the owner needs written permission from the cousin for the
  instrumentals, and must own or license every soundtrack song. Audio files
  come from the owner.

## 6. Share-a-clip

- After a hot streak, a big win, or a level-up, one tap exports a short
  vertical replay (roughly 10-15 seconds) of the dice with the iPlay logo,
  built for TikTok and Instagram, through the phone's normal share sheet.
- The server already records every roll's motion frames
  (`PhysicalRollTransport`), so the replay can be re-rendered exactly.

## 7. Reactive crowd

- Crowd reacts to what happens: cheers on hot dice and big wins, groans on a
  seven-out (or a 1-2-3 in Cee-lo), anticipation while the dice are in the air.
- Crowd size scales with the host's level (already approved).
- Footage is being sourced by Claude (Kling / Veo / Runway) and lands in
  `docs/` for owner approval. Build the event hooks; drop footage in once
  approved.

## 8. Modern UI switch (last, before launch)

Every screen is drawn with Unity's oldest UI system (IMGUI, `OnGUI`), where
each button is placed by hand-typed pixel numbers. Move menus, HUD and drawer
to a layout-based system (uGUI or UI Toolkit) that scales to any phone, keeping
the exact iPlay look. Do it after gameplay is verified on real phones, not
before -- changing UI and fixing gameplay at the same time makes bugs hard to
trace.
