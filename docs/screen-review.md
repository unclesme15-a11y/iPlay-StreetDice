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

### Sound moved off Global Settings — done 2026-09-22

Global Settings is reached before a table exists to hear the effect on,
so its Sound slider was adjusting a mix the player had no feedback for.
It was also a pure duplicate: the in-game "Voice & Sound" drawer page
already has a "Dice & Impact Volume" slider bound to the exact same
`effectsVolume` field (`StreetDicePlayExperience.cs`, `DrawDrawer()`).
Removed from Global Settings; the in-game one is unchanged and is now
the only one. Bottom band shrunk from 130px to 78px now that it holds
one row instead of two.

### Logo on Global Settings and Credits — done 2026-09-22

Sized against a real render. The IMGUI canvas is 1100x620
(`UiScale = max(0.45, min(width/1100, height/620))`), so every rect
resolves to fixed numbers regardless of device resolution.

At the default scale the mark is 275x138 and runs from y=8 to y=145,
which lands on top of the Exit button (y 90-138). `DrawBrandLogo(0.55f)`
gives 151x76 at x 933-1084, y 8-84 — clear of the button row by 6px and
clear of the credits scroll view by 148px horizontally.

Skipped on the legacy pregame screen, whose own title is already the
words "iPlay Cee-lo & Craps"; a mark there would duplicate it.

## Group 1 — Startup flow

Source screenshots: `artifacts/unity-smoke/startup/`. Code:
`unity/StreetDiceGreybox/Assets/StreetDiceStartup.cs`.

| ID | Screen | Style | Notes | Verdict |
| --- | --- | --- | --- | --- |
| S1 | App icon splash (Intro) | — | Logo only, no chrome. | KEEP |
| S2 | Adult gate (18+ / Under 18) | A | Carries the "Play money only" line. Logo added. | KEEP |
| S3 | Under-18 denied | A | Dead end + Exit. Logo added. | KEEP |
| S4 | Die menu (main hub) | mixed | Die faces bare, everything else Style A (gear, exit, PROFILE, Tutorial). Option 4 applied. | KEEP |
| S5 | Craps mode menu | B → A | Play vs AI / Online, both live. | THEME done |
| S6 | Cee-Lo mode menu | B → A | Online greyed — no online Cee-Lo backend exists. | THEME done |
| S7 | Online Craps — form version | A | Server field idea kept; rest of the layout came from S8. | MERGED into S8 |
| S8 | Online Craps — list version | A | Now the one screen: Server Address + Host Table + Join Table + Profile (conditional). | KEEP |
| S9 | Join Table (Enter Code / saved table) | B → A | "The Jungle" shown greyed. | THEME done |
| S10 | Enter Code (table code field) | B → A | | THEME done |
| S11 | Exit confirmation | A | Same dialog from every entry point — consistent. | KEEP |
| S12 | Global Settings | A | Drawn in `StreetDicePlayExperience.cs`. Logo added at 0.55 scale. Credits/Tutorial overlap fixed. | KEEP |
| S13 | Advanced Settings (server address) | B → A | | THEME done |
| S14 | Credits | C → A | Restyled 2026-09-20. Logo added. Was titled "Global Settings" — fixed. | KEEP |
| S15 | Profile | B → A | Save Name + GOOGLE / APPLE / EMAIL CODE, all three greyed out (not implemented). **No screenshot exists.** | open |

### Open on Group 1

- **S7 + S8 merge**: same screen, two layouts, and parts of both are
  wanted. S8 is what the code actually draws today (`DrawOnlineMenu`);
  S7's extra piece is the inline **Server address** field, which
  currently lives one level deeper in Advanced Settings (S13). Needs a
  call on which parts survive.
- **S4**: resolved, option 4 applied — see below.
- **S15**: three sign-in options are greyed placeholders. Keep them
  visible as "coming soon", or hide until they work?

## S7 + S8 merge — done 2026-09-22

Traced the actual navigation tree first (the code, not guesswork) to see
where Advanced Settings really lived. It turned out to answer its own
question: Advanced Settings is not a child of the Online screen at all --
it hangs off the gear icon on the die menu, a completely separate branch:
`Die Menu -> gear -> Global Settings -> Advanced -> Server Address`. The
old S7 mock only *looked* like Online had a Server field; in the live
code, that field was three taps away in an unrelated menu.

Decision: move Server Address onto the Online screen (`DrawOnlineMenu`),
since `ValidOnlineIdentity` was already silently checking it before
letting Host/Join work -- a bad address just greyed the buttons out with
no way to see or fix why, unless you already knew to go hunting under
the gear icon.

`DrawServerSettings` (S13, Advanced Settings) held nothing but this one
field, a Save button, and Back -- once the field moved, there was
nothing left there worth its own screen. Removed the "Advanced" button
from Global Settings. `StartupScreen.ServerSettings` and
`DrawServerSettings` are left in the code (unreachable from normal play
now) rather than deleted outright, because the Editor verification
harness (`PlayReadinessVerification.cs`) reflection-navigates straight
to that screen by name and captures `04a-advanced-server.png` from it;
deleting the screen would break that capture. Safe to remove properly
whenever that test is updated.

Also: `DrawFlowBack()` on the Online screen now saves the address too
(previously only Host/Join saved it), so an edited address isn't
silently lost if you type it and back out without hosting or joining.

Re-verified the full online screen layout -- title, field + its floating
label, Host Table, Join Table, Profile, Back -- against the 1100x620
canvas: no overlaps, nothing off-canvas. (First pass had the field's
label overlapping the title by 10px; moved the field from 0.28 to 0.32
of screen height to clear it.)

## S12 / S14 — global settings, fixed 2026-09-22

Three defects, found from a screenshot and confirmed by resolving every
rect on the screen against the 1100x620 canvas.

1. **Credits button drawn on top of the Tutorial switch.** The toggle
   occupied x+610..x+900, y 527..567; the Credits plate occupied
   x+650..x+900, y 545..589. They shared a 250x22 block, and Credits is
   drawn second, so it covered the word "Tutorial" and the lower half of
   the OFF/ON switch. The bottom band is now two clean rows: row 1
   (Sound + Tutorial) at y 512-552, row 2 (play-money line + Credits) at
   y 562-606, inside a band that runs 490-620.
2. **The Credits screen was titled "Global Settings".** Credits is not
   its own `StartupScreen`; it is a mode inside Global Settings toggled
   by `showCredits`, and the heading only tested `startupScreen`. It now
   tests `showCredits` first and reads "Credits". The heading also no
   longer shrinks to 20pt in credits mode — it stays at 30pt like every
   other screen title.
3. **Top button row fused into the first band.** The row ended at y=144
   and the Hand Color band starts at y=143, so the plate glow edges
   merged. Row moved to y=90, leaving a 5px gap.

Verified by resolving all 23 controls and 4 bands: no control-to-control
overlap, no control escaping its band, nothing off-canvas.

## S4 — the die menu, investigated 2026-09-22

### The interactive die was built and never switched on

`Assets/DieMenu.cs` (653 lines) is a faithful implementation of the
"Six-Sided Main Menu" artifact: quarter-turn swipes, flick shortcut,
tap-a-side-face-to-turn then tap-again-to-fire, arrow keys, number keys
1-6, idle drift, overshoot ease. Codex even replaced the artifact's demo
faces with this game's real ones in `Reset()` — Craps, Cee-lo, Global
Settings, Exit, and two non-selectable iPlay logo faces.

It has never run. It is a `MonoBehaviour`, and:

- `Scenes/StreetDiceDemo.unity` (125 lines) contains **no** attached
  scripts at all — `grep -o 'm_Script: {fileID' ` returns nothing.
- The script's GUID `5073ea4e3b16cb14d80b9850b7c17aa2` appears **0**
  times in the scene.
- It raycasts against a die with a `BoxCollider` to resolve which face
  was tapped (`hit.transform.InverseTransformDirection(hit.normal)`).
  No menu die model exists in the project. The only dice are
  `Resources/Dice/MacricioxRegularDie.prefab` and `GeugHotDie.prefab`,
  which are the in-game rolling dice.

Three orphaned shaders confirm the build was abandoned partway:
`Resources/Branding/MenuIvoryResin.shader`, `MenuFaceText.shader` and
`MenuSymbol.shader`, plus `menu-ivory-worn-albedo.png`. That is the
material set a 3D menu die needs — resin body, marker text, cyan
symbol. None is referenced from any `.cs` file.

Note the whole project is bootstrapped from
`StreetDiceGreyboxController.cs:128` via
`[RuntimeInitializeOnLoadMethod]`, which spawns the controller into an
empty scene and draws everything in IMGUI. Nothing is scene-authored, so
a scene-dependent MonoBehaviour like `DieMenu` was never going to fire.

### What actually runs

`DrawDieStartMenu()` in `StreetDiceStartup.cs` — a flat
`Resources/Branding/menu-die-static.png` drawn with
`ScaleMode.StretchToFill`, with two invisible `GUI.Button` rects laid
over the lettered faces.

**The hit boxes are correct.** Measured against the image: the CRAPS
face occupies 9%-46% horizontally and 43%-90% vertically; its rect is
`0.09 / 0.43 / 0.37 / 0.47` — an exact match. The CEE-LO face sits at
roughly 52%-92% / 42%-90%; its rect is `0.54 / 0.43 / 0.37 / 0.47`.
Since the texture is stretched to fill a square rect, image fractions map
1:1 to rect fractions, so this holds at every resolution.

Decision on record: the static die stays. `DieMenu.cs` and the three
menu shaders are dead code kept for reference only — if the 3D die is
ever revived it needs a scene-authored die GameObject with a collider,
which this project's runtime-bootstrap architecture does not currently
have anywhere to put.

### S4 resolved — option 4 applied 2026-09-22

Gear and exit now sit on their own Style A metal plates
(`Rect(controlX-5, controlY-17, 150, 84)` and
`Rect(controlX-5, gearPlate.yMax+20, 150, 66)`), matching PROFILE and
Tutorial. The die's two faces are left bare on purpose — a plate over
the hand-lettering would cover the thing that reads as clickable in the
first place. Plates sit 23px clear of the die's right edge, the same
gap the bare icons used to keep. The plate is also the tap target now
(not just the icon), which is a bigger, easier hit on a phone.

Re-verified: no overlaps among die / gear plate / exit plate / PROFILE /
Tutorial, nothing off-canvas.

| Control | Treatment | Reads as clickable? |
| --- | --- | --- |
| CRAPS face | bare, over the PNG | yes — real photo of hand-lettering |
| CEE-LO face | bare, over the PNG | yes — real photo of hand-lettering |
| Settings | Style A plate | yes |
| Exit | Style A plate | yes |
| PROFILE | Style A plate | yes |
| Tutorial | Style A switch | yes |

### Mobile tap on CRAPS / CEE-LO — already works, no fix needed

Confirmed 2026-09-22: those two faces are ordinary `GUI.Button` calls.
Unity's IMGUI (`OnGUI`) has always treated a phone tap identically to a
mouse click -- there is no separate "mobile" input path to wire up, no
`EventSystem` involved (that's a uGUI/Canvas concept; this project is
entirely `OnGUI`), and the whole screen already renders through the same
safe-area-aware `GUI.matrix` transform
(`StreetDiceGreyboxController.OnGUI -> DrawPlayExperience`) that the
rest of the game uses, so the hit rects land in the right place on any
phone regardless of notch or aspect ratio. Nothing here is blocked on
Codex.

### Tutorial Mode moved off Global Settings, onto this screen — done 2026-09-22

Own control now, not nested in a settings screen: `Rect(8, controlY+142,
240, 44)`, stacked below PROFILE in the left margin. The die fills
nearly the full screen height (570 of 620px at 0.92 * UiHeight), so
there is no usable space below it -- the left/right margins beside the
die (about 265px each) are the only open room, which is also where
PROFILE and the gear/exit controls already live. 44px tall for an easy
mobile tap target. Re-verified: no overlap with PROFILE, gear, exit, or
the die itself; nothing off-canvas.

## Group 2 — In-game loop

Not yet reviewed. Source: `artifacts/unity-smoke/readiness/`,
`betting/`, `sale/`, `add-ons/`.

## Group 3 — Wagers, fade, hot dice, dice sale

Not yet reviewed.


## Round 2 fixes — 2026-09-24

User feedback on the Group 1 review page.

**S5/S6/S9/S10 -- not a code issue.** Confirmed in the live code: all four
already call `DrawFlowChoice`, the same Style A plate function as
everything else. The confusion came from the review page itself --
Part B showed the old, pre-fix screenshots as the main image with only
a small caption saying the style had changed underneath, easy to skim
past. Fix going forward: when a screen's old screenshot is Style B but
the live code is Style A, show the reconstructed Style A version as the
primary image, not the stale capture.

**S4 die menu -- right column reordered to Profile, Settings, Exit.**
Three equal 150x54 plates stacked on the die's right margin (was two
icon plates on the right + PROFILE alone on the left):
- PROFILE moved from the left margin to the top of this stack.
- Settings is now a text plate ("SETTINGS") instead of a bare gear icon.
- Exit's icon is now inset 6px on every side (was 35/11px) -- fills
  most of its plate instead of floating in the middle of it.
Tutorial keeps its original spot in the left margin; only PROFILE moved
out of that side. Re-verified: no overlaps, nothing off-canvas, 23px
clear of the die's right edge same as before.

**S7+S8 (Online Craps) -- reordered to Profile, Host Table, Join Table,
Server Address.** Profile first (still only shown when no name is
saved), Server Address last since most players never touch it. Field's
floating label re-checked against Join Table above it and the Back
button below -- clear by 19.6px and 12.4px.

**S12 Global Settings -- Exit button removed, Credits moved to the left.**
Exit did `ReturnToDieMenu()` and then raised the quit dialog -- same
first step as Back, so it read as a duplicate control. Removed; Back
re-centered under the title (440-660 of a 1100-wide canvas, dead center
of the button band) rather than left-pinned with empty space where Exit
used to be. Credits and the "Offline demo | Play money" label swapped
sides -- Credits now left at its original width, the label now right-
aligned in the remaining space, edge-aligned with the band's right edge.

**S14 Credits -- "Creative Commons Attribution 4.0" cut from three
mentions to one.** It appeared under Regular Dice, again under Hot Dice,
and a third time as the license button's own label -- three statements
of the same fact. Each artist keeps their own one-line credit (CC BY
requires per-source attribution); the license itself is now named once,
as a one-line lead-in directly above the button that links to it.
Scroll content shortened from 580 to 470 to match the now-shorter text
(labels went from 4 lines to 2), so there's no dead scroll space at the
bottom.


## Round 3 fix — 2026-09-24

**S12 Global Settings -- "Offline demo | Play money" label removed.**
Credits re-centered in the bottom band now that it's the only thing
left in that row (425-675 of the 1100-wide canvas, dead center of the
70-1030 band), instead of sitting left-pinned with empty space where the
label used to be. Re-verified: Credits sits fully inside the band, no
overlaps.
