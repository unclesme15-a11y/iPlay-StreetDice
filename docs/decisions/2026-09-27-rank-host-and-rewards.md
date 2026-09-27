# Rank, Host and Rewards -- Agreed Design

Owner-approved decisions from the 2026-09-27 design sessions. "Table" in code
means one game session of up to 5 players -- the same thing the owner calls a
"party."

## The host rule (applies to everything below)

- The first real player to join is the host (`StreetDiceGameState.HostId`).
- **Everything rank-related at a table is the host's level, full stop.** A
  Level 3 host gives every seat Level 3 benefits. A Level 1 host caps every seat
  at Level 1, even a Level 5 guest. Not additive, not "whichever is higher."
- The host alone controls the table's music (Spotify).

## Built (server-enforced, tested)

| Level | Max bet | Unlocks |
| --- | --- | --- |
| 1 (start) | $100 | -- |
| 2 | $250 (placeholder) | -- |
| 3 | $500 | $50 and $100 notes |
| 4 | $750 (placeholder) | -- |
| 5 (top) | $1,000 | -- |

- Owner set levels 1, 3 and 5. Levels 2 and 4 are placeholders.
- Leveling is total shot wins (5 / 15 / 30 / 50). Also a placeholder -- the
  owner hasn't chosen how XP is earned.
- Real accounts (username + password), not device-local rank.
- Hustled $50/$100 notes: a lower rank can hold one as a flex. It is only
  usable at a table whose host is Level 3+. It's a trophy, not an exclusive
  unlock (everyone at a Level 3+ table can already use $50/$100).

## Approved, not built yet

- **Dice colors level up.** Everyone starts with white dice only.
- **Level 4: blue and red dice with white pips.** The Level 4 red must be a
  visibly different shade from the hot-dice orange-red so nobody mistakes it
  for hot dice.
- **Level 5 (top): custom color wheel in settings** showing the real RGB
  numbers, for a truly custom die.
- **Crowd size scales with level** -- bigger crowd at a higher-level table
  (host-driven, like everything else). Crowd footage itself is being sourced
  separately (Kling / Veo / Runway) and will land in `docs/` for approval.
- Per the host rule, all of the above follow the host: a Level 5 host's table
  gets the color wheel and the big crowd for everyone.

## Open questions (don't guess -- ask the owner)

1. Which dice colors unlock at Levels 2 and 3?
2. How is XP earned? (Wins are trivially farmable by two friends shooting $1
   at each other.)
3. How does a player actually "hustle" a $50/$100 note?
4. When the host leaves mid-game, who becomes host, and does the table's level
   change mid-game or stay locked?
5. How does a player stake above $20 when only $1/$5/$10/$20 notes exist
   below Level 3? (Stacking bills? Bigger presets?)

## Other locked decisions

- No dice sale. Losing the shot is Shoot or Pass only. All buttons use the iPlay
  style, never default Unity.
- Bills: realistic "regular" faces with iPlay PROP MONEY / NOT LEGAL TENDER
  markings and names removed, except the owner's custom $1 and $20.
- Hot dice: built on the existing red dice with red pips, with glow light,
  emissive pulse and a visible-but-not-overwhelming fire trail.
- Music controls sit directly on the in-game Options drawer, not a sub-tab.
  Game Stats is its own page reached from Options.
