# Screen Review

Working doc for the screen-by-screen design pass. Screens are reviewed in
groups. Each screen has a stable ID so it can be referred to in one word.

## How to use this

Reply with one line per screen. Four verdicts:

| Verdict | Means |
| --- | --- |
| `KEEP` | Fine as is. |
| `KILL` | This screen shouldn't exist anymore. Remove it from the flow. |
| `THEME` | Right screen, wrong look. Restyle it to the target style. |
| `MERGE` | Duplicate of another screen. Collapse them (name the survivor). |

Examples: `S7 MERGE into S8` · `S13 THEME` · `S5 KEEP` · `S1 KILL`

Add free text after the verdict for anything specific.

## Style vocabulary

Three looks are currently in use. "Doesn't follow the theme" means a screen
is using one of these where it should be using another.

- **Style A — Metal Plate**: brushed-metal panel, cyan glow edge, marker
  font. Drawn by `DrawMetalButton` / the settings panel rows.
  Used by: Adult Gate, Global Settings, Exit dialog, in-game buttons.
- **Style B — Wall Text**: bare marker-font text directly on the asphalt
  background, no panel or chrome. Drawn by `DrawFlowChoice`.
  Used by: Craps/Cee-Lo mode menus, Join Table, Code Entry, Advanced Settings.
- **Style C — Unity Default**: raw grey Unity `GUI.Button`, no custom
  styling. This one is always wrong — it's an unstyled placeholder.

## Group 1 — Startup flow

Source screenshots: `artifacts/unity-smoke/startup/`. Code:
`unity/StreetDiceGreybox/Assets/StreetDiceStartup.cs`.

| ID | Screen | Style | Notes | Verdict |
| --- | --- | --- | --- | --- |
| S1 | App icon splash (Intro) | — | Logo only, no chrome. | |
| S2 | Adult gate (18+ / Under 18) | A | Carries the "Play money only" line. | |
| S3 | Under-18 denied | A | Dead end + Exit. | |
| S4 | Die menu (main hub) | mixed | 3D die + small gear icon + red text EXIT. Three different button treatments on one screen. | |
| S5 | Craps mode menu | B | Play vs AI / Online, both live. | |
| S6 | Cee-Lo mode menu | B | Online greyed — no online Cee-Lo backend exists. | |
| S7 | Online Craps — form version | A | Name + Server + Host Table + Table code + Join. Duplicate of S8. | |
| S8 | Online Craps — list version | B | Name + Host Table / Join Table only. Duplicate of S7. | |
| S9 | Join Table (Enter Code / saved table) | B | "The Jungle" shown greyed. | |
| S10 | Enter Code (table code field) | B | | |
| S11 | Exit confirmation | A | Same dialog from every entry point — consistent. | |
| S12 | Global Settings | A | Hand colour, fade style, dice colour, sound, tutorial, credits. | |
| S13 | Advanced Settings (server address) | B | Reached from Global Settings (A) — style changes mid-flow. | |
| S14 | Credits | C → A | Was raw Unity buttons; restyled to A on 2026-09-20, not yet verified in Editor. | |

### Open questions on Group 1

- **S7 vs S8**: two different Online Craps layouts exist. One is almost
  certainly a stale earlier iteration. Which survives?
- **A/B inconsistency**: the flow crosses between Metal Plate and Wall Text
  at several points (S2→S5, S12→S13). Should one style win outright, or is
  the split intentional (e.g. panels for dialogs, bare text for menus)?
- **S4**: three button treatments on a single screen (3D die faces, metal
  gear icon, red text EXIT).

## Group 2 — In-game loop

Not yet reviewed. Source: `artifacts/unity-smoke/readiness/`,
`betting/`, `sale/`, `add-ons/`.

## Group 3 — Wagers, fade, hot dice, dice sale

Not yet reviewed.
