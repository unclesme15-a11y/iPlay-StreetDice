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

## Style rules (decided 2026-09-21)

- **Style A — Metal Plate is the house style.** Brushed-metal panel, cyan
  glow edge, marker font. Every screen that is **not in-game** uses it.
  Drawn by `DrawMetalPlate` / `DrawMetalButton` / `DrawFlowChoice`.
- **Style B — Wall Text is in-game only.** Bare marker-font text painted
  onto the scene: the countdown numbers, "COME OUT" on the roll-up door,
  and similar diegetic text. It is *retired as a menu style* — no
  out-of-game screen should use it.
- **Style C — Unity Default** (raw grey `GUI.Button`) is always wrong. It's
  an unstyled placeholder wherever it appears.

### The logo

The official iPlay Cee-lo & Craps mark belongs on the **majority of
screens**, resized as needed to fit a given layout. Drawn by
`DrawBrandLogo(widthScale)` — pass a scale below 1 where the top-right
corner is tight.

## Applied 2026-09-21

- `DrawFlowChoice` now draws a metal plate behind its label, which
  converts every menu button to Style A in one move: mode menus, online
  menu, join table, code entry, profile, advanced settings, and every
  BACK button. Layout rects unchanged. Verified by inspection that
  `DrawFlowChoice` is used only in `StreetDiceStartup.cs` and never
  in-game, so the wall text is untouched.
- `DrawBrandLogo` extracted; logo added to the adult gate and under-18
  screens, which had none.
- Credits screen moved off Style C (raw Unity buttons) to Style A.

None of the above is verified in the Unity Editor — no Editor is
available in the environment these changes were made in.

### Still missing the logo

`StreetDicePlayExperience.cs` draws Global Settings and Credits, and
neither carries the mark. Their top-right corner is already occupied
(Exit button on Global Settings), so the logo there needs a size and
position picked against a real render rather than guessed.

## Group 1 — Startup flow

Source screenshots: `artifacts/unity-smoke/startup/`. Code:
`unity/StreetDiceGreybox/Assets/StreetDiceStartup.cs`.

| ID | Screen | Style | Notes | Verdict |
| --- | --- | --- | --- | --- |
| S1 | App icon splash (Intro) | — | Logo only, no chrome. | KEEP |
| S2 | Adult gate (18+ / Under 18) | A | Carries the "Play money only" line. Logo added. | KEEP |
| S3 | Under-18 denied | A | Dead end + Exit. Logo added. | KEEP |
| S4 | Die menu (main hub) | mixed | 3D die + gear icon + red text EXIT. Three treatments on one screen — open. | open |
| S5 | Craps mode menu | B → A | Play vs AI / Online, both live. | THEME done |
| S6 | Cee-Lo mode menu | B → A | Online greyed — no online Cee-Lo backend exists. | THEME done |
| S7 | Online Craps — form version | A | Name + Server + Host Table + Table code + Join. | MERGE — open |
| S8 | Online Craps — list version | B → A | Name + Host Table / Join Table. This is the live one in code. | MERGE — open |
| S9 | Join Table (Enter Code / saved table) | B → A | "The Jungle" shown greyed. | THEME done |
| S10 | Enter Code (table code field) | B → A | | THEME done |
| S11 | Exit confirmation | A | Same dialog from every entry point — consistent. | KEEP |
| S12 | Global Settings | A | Drawn in `StreetDicePlayExperience.cs`. Still no logo. | logo open |
| S13 | Advanced Settings (server address) | B → A | | THEME done |
| S14 | Credits | C → A | Restyled 2026-09-20. Still no logo. | logo open |
| S15 | Profile | B → A | Save Name + GOOGLE / APPLE / EMAIL CODE, all three greyed out (not implemented). **No screenshot exists.** | open |

### Open on Group 1

- **S7 + S8 merge**: same screen, two layouts, and parts of both are
  wanted. S8 is what the code actually draws today (`DrawOnlineMenu`);
  S7's extra piece is the inline **Server address** field, which
  currently lives one level deeper in Advanced Settings (S13). Needs a
  call on which parts survive.
- **S4**: three button treatments on one screen.
- **S12 / S14 logo**: top-right is occupied; needs a size/position picked
  against a real render.
- **S15**: three sign-in options are greyed placeholders. Keep them
  visible as "coming soon", or hide until they work?

## Group 2 — In-game loop

Not yet reviewed. Source: `artifacts/unity-smoke/readiness/`,
`betting/`, `sale/`, `add-ons/`.

## Group 3 — Wagers, fade, hot dice, dice sale

Not yet reviewed.
