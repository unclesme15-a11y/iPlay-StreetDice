# Launch Features -- Approved 2026-09-28

The owner picked these from a "what takes this from a 6-8 to a 10/10" review.
None are built yet. Everything follows the host rule in
`2026-09-27-rank-host-and-rewards.md`.

Considered and **not** chosen for now: online Cee-lo, recorded dice-call voice
lines, player cards with rank titles, seasons/leaderboards, a cosmetic shop,
and a store trailer. Don't build these without asking.

## 1. Find a game (public parties)

The fix for a new player with no friends online landing on an empty screen.

- The host chooses **Public** or **Private** when creating a party. Private
  works exactly like today: join by code, and only the host can share the
  invite.
- "Find a game" lists public parties with open seats, showing the **host's
  level** (which sets the party's max bet and unlocks), seats filled out of 5,
  and whether a shot is live.
- Quick join puts the player in the best open public party. If none exists,
  offer to host a new public one.
- Public parties still follow the host rule, seat hold and one-bankroll rules.

## 2. Modern UI switch (before launch)

Every screen is currently drawn with Unity's oldest UI system (IMGUI,
`OnGUI`), where each button is placed by hand-typed pixel numbers. That's why
visual tweaks take many rounds and screens need checking on every phone shape.

- Move menus, HUD and drawer to a layout-based system (uGUI or UI Toolkit) that
  scales to any screen.
- Keep the exact iPlay look (metal plates, bill photos, lock art, fonts). This
  is a rebuild of the plumbing, not a redesign.
- Do it after the game plays correctly on real phones, not first. Changing UI
  and fixing gameplay at the same time makes bugs hard to trace.

## 3. Level-up moment

- When a player crosses into a new level: screen flash, crowd reaction
  (see #6), the new level and what it unlocks (dice color, bet cap, $50/$100)
  revealed on screen.
- Smaller version for each of the 5 steps inside a level.
- Everyone at the party sees it, with the player's name, so ranking up is a
  public moment.
- Never interrupt a live roll; show it after the shot resolves.

## 4. Daily challenges

- 3 challenges a day per player, drawn from a pool, e.g. "Hit a point of 4,"
  "Win 3 side bets," "Shoot 10 times," "Win a shot of $50 or more."
- Each pays bonus XP (tune alongside `RankLadder.cs`), plus a bonus for
  finishing all 3.
- Reset at the same daily boundary the XP daily bonus uses (UTC day).
- Tracked server-side from real results, never trusted from the app.
- Shown on the Game Stats page and as a small toast when one completes.

## 5. Share-a-clip

- After a hot streak, a big win, or a level-up, one tap exports a short
  vertical replay (roughly 10-15 seconds) of the dice, with the iPlay logo.
  Built for TikTok and Instagram.
- The server already records every roll's motion frames
  (`PhysicalRollTransport`), so the replay can be re-rendered exactly.
- Share through the phone's normal share sheet. No account linking needed.
- Free marketing: every shared clip is an ad for the game.

## 6. Reactive crowd

- A crowd around the game that reacts to what happens: cheers and a surge on
  hot dice and big wins, groans on a seven-out, anticipation while the dice
  are in the air.
- Crowd size scales with the host's level (already approved).
- Footage is being sourced by Claude with Kling / Veo / Runway and lands in
  `docs/` for owner approval. Build the hooks (events the crowd reacts to),
  then drop the approved footage in.
