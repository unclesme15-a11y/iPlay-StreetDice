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
  highest-ranked player still seated (earliest seat breaks a tie). The owner
  said "the voice controls go to a higher rank"; read as the host's controls
  (music, invites), since hosts have no separate voice controls. Confirm if
  that was meant literally.
- Only the host can share or re-share the party invite.

## Built (server-enforced, tested)

| Level | Max bet | XP to reach | Unlocks |
| --- | --- | --- | --- |
| 1 (start) | $100 | 0 | White dice |
| 2 | $250 (placeholder) | 200 | Green dice |
| 3 | $500 | 1,000 | Black dice, $50 and $100 notes |
| 4 | $750 (placeholder) | 2,500 | Red and blue dice with white pips |
| 5 (top) | $1,000 | 5,000 | Custom RGB color wheel |

Owner set the level 1/3/5 caps. Dice-color and color-wheel unlocks are
approved but **not built** (see below).

### XP formula (owner: "a combo of all"; numbers tunable in `RankLadder.cs`)

- +10 XP for every finished shot you're in, shooter or catcher, win or lose.
- +15 XP for winning a shot, but only your first 3 wins against the same
  opponent each day.
- +1 XP per $10 won on the shot, up to 50 per shot.
- 500 XP daily cap. At the cap, Level 5 takes at least 10 days.
- Only signed-in players earn XP. Live-verified: a catcher winning $20 earned
  27 XP (10 + 15 + 2); the shooter who lost earned 10.

### Other built pieces

- Real accounts (username + password); logins survive server restarts.
- **Stakes stack like cash:** tapping a bill adds it to the pile, up to the
  party's cap, with a Clear button. $50/$100 join the picker once the host is
  Level 3+ *and* the note art exists in `Resources/Money` (not generated yet).
  **Owner has a close-up screenshot of stacked bills that shows exactly how
  the pile should look -- match it once it's uploaded.**
- Hustled $50/$100 notes are only usable at a Level 3+ host's party. It's a
  trophy, not an exclusive unlock.

## Approved, not built yet

- Dice colors level up (table above). Everyone starts with white only. The
  Level 4 red must be visibly different from the hot-dice orange-red.
- Level 5 color wheel in settings, showing the real RGB numbers.
- Crowd size scales with level (host-driven). Crowd footage is being sourced
  separately (Kling / Veo / Runway) and will land in `docs/` for approval.

## Still open (ask the owner)

1. How does a player actually "hustle" a $50/$100 note?
2. Do online parties require signing in? (Needed once money follows the
   account -- see the ads/bankroll doc.)

## Other locked decisions

- No dice sale. Losing the shot is Shoot or Pass only. All buttons use the iPlay
  style, never default Unity.
- Bills: realistic "regular" faces with iPlay PROP MONEY / NOT LEGAL TENDER
  markings and names removed, except the owner's custom $1 and $20.
- Hot dice: built on the existing red dice with red pips, with glow light,
  emissive pulse and a visible-but-not-overwhelming fire trail.
- Music controls sit directly on the in-game Options drawer, not a sub-tab.
  Game Stats is its own page reached from Options.
