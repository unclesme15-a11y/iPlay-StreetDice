# Rank, Host and Rewards -- Agreed Design

Owner-approved decisions from the 2026-09-27/28 design sessions. "Table" in code
means one game session of up to 5 players -- the same thing the owner calls a
"party." Ads, bankroll and seat hold live in
`2026-09-28-ads-bankroll-and-seat-hold.md`.

## The host rule (applies to everything below)

- The first real player to join is the host (`StreetDiceGameState.HostId`).
- **Everything rank-related at a party is the host's level, full stop.** A
  Level 3 host gives every seat Level 3 benefits. A Level 1 host caps every seat
  at Level 1, even a Level 5 guest. Not additive, not "whichever is higher."
- **When the host leaves:** the party keeps the departing host's level for the
  rest of the party (`LockedTableLevel`), and the host role passes to the
  highest-ranked player still seated (earliest seat breaks a tie). The host's
  controls that pass are music and invites (owner confirmed).
- If the host's seat is only on hold (phone died, ad break), the next-highest
  rank covers as host and the original host gets the role back when they
  return. Not built yet -- see the seat-hold section of the ads/bankroll doc.
- Only the host can share or re-share the party invite.

## Built (server-enforced, tested)

| Level | Max bet | Total XP to reach | Grinder (~3 h/day) | Casual (~30 min/day) | Unlocks |
| --- | --- | --- | --- | --- | --- |
| 1 (start) | $100 | 0 | -- | -- | White dice |
| 2 | $250 (placeholder) | 12,000 | ~5 days | ~10 days | Green dice |
| 3 | $500 | 42,000 | ~2 weeks | ~5 weeks | Black dice, $50 and $100 notes |
| 4 | $750 (placeholder) | 112,000 | ~6 weeks | ~3 months | Red and blue dice with white pips |
| 5 (top) | $1,000 | 272,000 | ~3 months | ~7 months | Custom RGB color wheel |

Owner set the level 1/3/5 caps and the pacing: a "junior NBA 2K" grind, Level 2
in about 5 days for an extreme grinder and 10 for a daily casual, getting
harder every level, with a longer climb to the top. Dice-color and color-wheel
unlocks are approved but **not built** (see below).

### XP formula (numbers tunable in `RankLadder.cs`)

- **No daily cap.** Grinders are never stopped.
- **Every signed-in player seated at the party** earns 8 XP per finished shot,
  so a full 5-player party levels as fast as a 1-on-1.
- **Shooter or catcher:** +8 more.
- **Winning the shot:** +12, but only the first 3 wins against the same
  opponent each day (stops friends trading wins on purpose).
- **Money won:** +1 per $10, up to 20 per shot.
- **Daily bonus:** your first 15 shots each day are worth 5x. This is what lets
  a 30-minute daily player keep roughly half a grinder's pace instead of a
  sixth, without capping anyone.
- **Five steps inside every level** with a progress bar on Game Stats (Level 2,
  Step 3/5), so there's always a step coming even when a level takes weeks.
- The day counts above assume about 40 finished shots per hour at a party.
  That's an estimate: measure real shots per hour in beta, then retune the
  numbers in `RankLadder.cs`.

### Other built pieces

- Real accounts (username + password); logins survive server restarts.
- **Stakes stack like cash:** tapping a bill adds it to the pile, up to the
  party's cap, with a Clear button. $50/$100 join the picker once the host is
  Level 3+ *and* the note art exists in `Resources/Money` (not generated yet).
  **Owner has a close-up screenshot of stacked bills that shows exactly how
  the pile should look -- match it once it's uploaded.**
- **Trophy notes:** a player below Level 3 who wins a shot of $50 or more
  against a Level 3+ opponent keeps one note as a trophy (the $100 if the shot
  was $100+, otherwise the $50). Both must be signed in. Shown on Game Stats.
  Like all $50/$100 use, it only shows up at a Level 3+ host's party -- it's
  bragging rights, not an exclusive unlock.
- Online parties require signing in (owner decision; client flow not built
  yet). Offline play vs AI stays open to everyone.

## Approved, not built yet

- Dice colors level up (table above). Everyone starts with white only. The
  Level 4 red must be visibly different from the hot-dice orange-red.
- Level 5 color wheel in settings, showing the real RGB numbers.
- Crowd size scales with level (host-driven). Crowd footage is being sourced
  separately (Kling / Veo / Runway) and will land in `docs/` for approval.

## Still open

Nothing on rank. Tune the XP numbers after beta measures real shots per hour.

## Other locked decisions

- No dice sale. Losing the shot is Shoot or Pass only. All buttons use the iPlay
  style, never default Unity.
- Bills: realistic "regular" faces with iPlay PROP MONEY / NOT LEGAL TENDER
  markings and names removed, except the owner's custom $1 and $20.
- Hot dice: built on the existing red dice with red pips, with glow light,
  emissive pulse and a visible-but-not-overwhelming fire trail.
- Music controls sit directly on the in-game Options drawer, not a sub-tab.
  Game Stats is its own page reached from Options.
