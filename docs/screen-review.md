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

Started 2026-09-24. Source: `artifacts/unity-smoke/readiness/`,
`betting/`, `sale/`, `add-ons/` (skipping resolution duplicates,
per-denomination lock variants, and the physics-roll motion frame
sequences -- one representative capture per distinct screen/state).

### A real pattern, not yet a verdict

The whole in-game HUD uses its own button language -- small blue-outlined
rounded-square icon buttons (back arrow, CRAP, point number, DOUBLE,
PAIR 4) -- separate from the menu system's metal-plate Style A. That's
not automatically wrong the way Group 1's mix was: a live table needs
compact controls that don't block the dice, where a full menu screen
doesn't. The `BET` / choose-opponent panel is the exception and already
uses full Style A plates.

What *is* a real inconsistency: **picking a dollar amount looks
different in different places.**
- On the point-number bet screen (S20), amounts are photographed dollar
  bills -- $1/$5/$10/$20 -- the nicest, most thematic treatment in the
  game.
- On the dice-sale screen (S28, S32), the same four amounts are flat
  dark-grey boxes with plain white text -- no photo, no plate, no glow.
- On the dice-sale bid pad (S31), amounts are typed on a third
  treatment again -- thin-outlined boxes with plain digits, a phone
  keypad.

Same action, three looks. Worth a call once you've seen them side by
side.

| ID | Screen | Notes | Verdict |
| --- | --- | --- | --- |
| S16 | In-game HUD (baseline) | BET icon top-left, You/Shooter + balance + mic, 4 opponents, gear top-right, money + dice on the ground. | open |
| S17 | Options drawer (in-game) | Throw Style, Throwing Hand, Tutorial Mode, Voice & Sound, Rules, Leave Game. Full Style A. | open |
| S18 | Voice & Sound (drawer sub-page) | Mute Mic, Dice & Impact Volume, Dice Calling -- all greyed, offline demo. Style A. | open |
| S19 | Bet: choose opponent | "BET" + 4 opponent plates. Style A -- the one HUD screen that matches the menu style. | open |
| S20 | Bet: pick amount | Point number painted on the door (Style B, correct). Photographed $1/$5/$10/$20 bills. Blue icon buttons (back/CRAP) either side. | open |
| S21 | Wagers placed (4 locks) | Lock icons per opponent reading "CRAP 2/3/12 $5", radial opponent markers, hamburger icon top-left. | open |
| S22 | Point bet locked, with Double/Pair teaser | "COME OUT" painted on the door behind a live "CRAP 10" bet. Blue icon buttons. | open |
| S23 | Side bet: Double / Pair 4 | Same blue-icon-button language as S22. | open |
| S24 | Side bet confirmed (paired number) | Paired-number lock placed at $5; other opponents' controls greyed until it resolves. | open |
| S25 | Fade gesture in progress (Plant) | Picture-in-picture inset of a hand pressed to the pavement, FADE ring still visible. | open |
| S26 | Point number HUD badge | Small black rounded badge, plain sans digit -- different treatment than the big painted number in S20/S22. | open |
| S27 | Come-out / payout moment | "COME OUT" painted on the door, bills + dice on the ground, balance updated. | open |
| S28 | Hot dice + Shoot/Sell | Flame meter icon (full), flat grey $1/$5/$10/$20 boxes, Shoot / Sell buttons, red hot dice. | open |
| S29 | Leave game confirmation | Matches S11's exit dialog exactly -- Style A, consistent. | open |
| S30 | Dice sale: waiting for bidder | "WAITING FOR BIDDER" painted on the door, live bid amounts shown below it. | open |
| S31 | Dice sale: bid pad | "DICE FOR SALE" painted on the door, numeric keypad (1-9, C/0/back), BID button. | open |
| S32 | Dice sale: purchase complete | "You bought the dice for $12" painted on the door, same flat grey chip row as S28. | open |

## Group 3 — Wagers, fade, hot dice, dice sale

Folded into Group 2 above -- the screenshots didn't separate cleanly by
that line, so it made more sense to review them together.


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


## Group 2, round 1 -- 2026-09-26

User feedback on the initial Group 2 batch, plus a full spec for the
corner-BET wager flow (push BET top-left -> per-opponent digital dice ->
Hit/Crap -> point/paired number -> lock appears -> pick amount -> confirm).

### The wager/lock system was already almost entirely built

Traced it through `StreetDiceWagerHud.cs` before changing anything. Nearly
everything described was already live:
- `DrawBettingToggle` -- the corner BET icon, opens the overlay.
- `DrawOpponentBetDice` -- three "digital display" dice per opponent
  (back arrow, then two choices), stage-based: Hit/Crap, then point/paired
  number.
- `DrawAmountLock` -- the ground padlock, already cross-fades from the
  open icon to the closed icon on accept.
- **The red-to-green glow "gpt missed" was already built.** Checked the
  actual source art (`wager-locks-keyed.png`): the open-lock crop is
  red-glow, the closed-lock crop is green-glow, and `DrawAmountLock`
  already cross-fades between them over 0.18s when a wager is accepted.
  Nothing to add here.
- The "double tap to confirm" is a tap-then-confirm-within-2-seconds
  pattern (`armedOfferId`), not a literal double tap, but serves the same
  purpose already.

### What was actually fixed

- **"BET" -> "HIT"**: the stage-1 button read "BET" on screen while the
  enum underneath was already `WagerOutcome.Hit`, and the lock that
  followed already read "HIT". Just relabeled to match.
- **Lock text parity**: Crap wagers showed a "CRAP" title above the
  number; Hit wagers showed only the bare number, no title at all. Both
  now get their word.
- **Lock size**: enlarged ~30% on both the draft lock (84x99 -> 110x130)
  and the ground lock (65x76 -> 85x99) -- too small to read before.
  Adjusted the bill row and page-arrow positions that sit next to them
  so nothing overlaps at the new size.
- **Ground marker -> real bill**: the "amount confirmed" marker next to
  a ground lock used to be a small digital-display cube with a "$X"
  TextMesh on it -- the same look as the still-choosing bet-picker dice.
  Replaced with a single upright quad textured with the actual bill
  photo (same asset `DrawBetBill` uses to pick the amount), billboarded
  toward the camera. Matches "that's how a bet proposal looks, it's a
  picture of that."

### Dollar amounts and door text, unified

- **Bills for picking an amount, everywhere except the sale bid.** The
  Shoot/Sell amount row (was a raw `GUI.Toolbar` with "$1"/"$5"/"$10"/
  "$20" text tabs) now uses the same photographed-bill buttons as the
  bet screen, at a smaller HUD-appropriate size, with the selected one
  underlined in cyan.
- **Dice-sale bidding stays a number keypad** (per instruction) --
  arbitrary bid amounts don't fit fixed denominations.
- **"COME OUT" styling now shared.** "WAITING FOR BIDDER" / "DICE FOR
  SALE" / "[name] bought the dice for $X" were a small plain cyan
  `GUI.Label` -- default skin font, no animation, a completely separate
  implementation from `DrawDoorGhostNumber` (the graffiti-font, painted-
  on-the-door treatment "COME OUT" uses). All three now route through
  `DrawDoorGhostNumber`. Its font-scale was hardcoded to recognize the
  literal string "COME OUT"; generalized to scale by text length instead
  so longer phrases don't render at full size (verified it still
  resolves to the exact same size for "COME OUT" and for countdown
  digits as before).

### Raw Unity-default buttons removed from the live HUD

Swept every `GUI.Button` call with a plain text label across the
gameplay files. Fixed the ones actually reachable during play:
Shoot / Sell / Run Same / Double Up, and the online lobby's "COPY CODE"
button -- all converted to `DrawMetalButton`.

### Dead code found, not touched

Two more pieces of debug/superseded scaffolding, same situation as
`DieMenu.cs` (see S4 investigation above) -- built, never wired in:
- `DrawWagerSeat` / `DrawWagerComposer` -- an older tap-the-opponent-die
  wager composer, fully separate from the corner-BET-button flow that's
  actually live. Never called. Still has a raw `GUI.Button("$" + amount)`
  in it, not worth fixing since nothing reaches it.
- `DrawBottomControls` and everything it calls (`DrawDeterministicControls`,
  `DrawDiceSkinControls`, `DrawHandSkinControls`, `DiceSkinButton`) plus
  `DrawPlayerOverlays` -- a developer debug panel (forced dice rolls, hand
  skin swatches, "Bet Hit"/"Bet Miss"/"Fade/Catch" test buttons). Never
  called from anywhere in the live render path.

### Still open

S16 (in-game HUD baseline), S18 (Voice & Sound drawer page), and S29
(leave confirmation) were flagged for removal in the original batch --
unclear why, and all three looked correct on inspection. Needs your
read on what specifically should change.


## Group 2, round 2 -- 2026-09-27

### Hot dice pip contrast

The red pips looked flat/same-red-as-body in game, not matching
`hot-dice-faces.png`. Traced it: the material is untouched at runtime
(confirmed by an existing automated check, "Imported hot material was
overwritten", in `HotDiceAssetBuild.cs`) -- nothing recolors it. The
difference is lighting. That reference image was rendered in
`CaptureFaces()` under bright, even studio light (ambient 0.65, a 1.5-
intensity directional light from every angle). The actual table uses one
warm point light at 1.15 intensity for mood. A glossy material that gets
its pip-vs-body contrast mostly from specular highlights reads flat under
weaker, more diffuse light.

Fix: added a dedicated `Hot Dice Glow Light` (warm-orange point light,
starts at 0 intensity) that switches on to 1.6 only while the dice are
actually hot (`ApplyDiceColor`, the same place that already tracks hot
state). Doesn't touch the scene's overall mood -- the light is off the
rest of the time.

### Dice sale removed entirely

Per instruction: losing the shot is now Shoot or Pass, no auction.
Removed across the whole stack:

- **Server**: deleted `Core/StreetDiceSales.cs` (`SellDice`, `BidForDice`,
  `ResolveDiceSale`, the `DiceSale`/`DiceSaleBid` types, and the
  `ProtectedSaleComeOut` come-out-protection rule and its three call
  sites) and `tests/DiceSaleTests.cs` (4 tests). Removed the `/sell` and
  `/sell/bid` endpoints and the `DiceSale` field from game state. Kept
  `/pass` -- it already existed, fully implemented, just never wired to
  a button. 99/99 tests pass (was 103; the 4 removed were sale-specific).
- **Client**: `StreetDiceSell.cs` (230 lines of sale UI/logic) reduced to
  just `localTurnOrder`, which is used elsewhere for seat cycling. The
  Shoot/Sell buttons are now Shoot/Pass, calling the already-built
  `PassLocalDice()` -- which itself was dead code until now, exactly like
  `PassDice()` on the server. The bot AI's `DemoOpponentPolicy.Pass(...)`
  check was, ironically, wired to call `SellCurrentDice()`; now it calls
  `PassLocalDice()`, matching its own name.
- Reworked the online shooter-change reset: it used to call
  `ResetDiceToShooter()` only when a sale completed. Generalized to fire
  whenever `shooterId` actually changes between polls, which covers Pass
  and is arguably more correct than the old version (a normal seven-out
  handoff wasn't triggering it either).
- Reverted `DrawDoorGhostNumber`'s length-based font scaling back to the
  simple `text == "COME OUT"` check -- that generalization existed only
  to size the sale headlines, which no longer exist.
- Deleted the `CaptureSale()` editor routine and its
  `artifacts/unity-smoke/sale/` screenshots (S30-S32 no longer exist).
- Updated the in-game Rules page text, `docs/game-rules.md`,
  `docs/prebeta-readiness.md`, `docs/visual-approval-checklist.md`, and
  `README.md` (dropped from 103 to 99 tests). `docs/post-production-handoff.md`
  gets a dated note instead of a rewrite -- it's a historical record of
  what Codex built, not current-state documentation.

S30/S31/S32 (dice-sale screens from the Group 2 catalog) no longer exist.
Shoot/Pass uses the same `DrawMetalButton` styling as everything else in
the HUD -- never the raw default Unity look.

## Group 2, round 3 -- 2026-09-27

### Hot dice: all 3 editions combined

Built off the existing red-body/darker-red-pip reference art
(`hot-dice-faces.png`, unchanged) rather than a new color -- three
layered effects that were built separately across this round now all run
together whenever a die is hot:

1. **Glow light** -- `Hot Dice Glow Light`, a warm-orange point light
   that switches on to 1.6 intensity only while hot (see round 2's pip-
   contrast fix above).
2. **Emissive pulse on the die itself** -- `ApplyHotDiceGlow`, a sine-wave
   pulse on the material's emission, reusing the base color texture as
   the emission source so the red pips glow along with the body rather
   than washing out to a flat color. Defensively tries both glTF
   (`emissiveFactor`/`emissiveTexture`) and Unity Standard
   (`_EmissionColor`/`_EmissionMap`) property names, since this project's
   dice import through `com.unity.cloud.gltfast` and the exact shader
   this lands on couldn't be confirmed without a live Editor.
3. **Ember-colored fire trail** -- `UpdateHotSmoke`'s `TrailRenderer`
   gradient changed from grey smoke to warm amber-to-ember-to-dark-red,
   and widened (0.026 peak vs. the old value) so it reads clearly without
   overwhelming the dice themselves -- "noticeably visible but not too
   much," per instruction.

This is distinct from the separate Level 4 rank-reward red die discussed
in this round's rank-system conversation, which is a different, lighter
shade with white pips (existing auto-contrast logic) specifically so it
never gets confused with a die that's hot.

### Spotify host-controls-it

New capability: the host's Spotify drives what plays for the whole
table, following the shape the user asked for -- Spotify-only, everyone
logged into their own account, sync "close" not sample-accurate.

- **Server**: added `HostId` to game state (first real joiner becomes
  host, same rule as `ShooterId`) and
  `POST /api/street-dice/{gameId}/music` (host-only -- 403s anyone else),
  carrying track URI / position / playing state / a server timestamp.
  Rides the existing `GET /api/street-dice/{gameId}` poll -- no new sync
  channel. New `TableHostTests.cs` (2 tests); 101/101 passing overall.
  Live-verified with `dotnet run` + `curl`: host posts succeed and
  propagate to the guest's next poll, guest posts get 403.
- **Client**: `StreetDiceSpotify.cs` -- an `ISpotifyPlaybackBridge`
  interface (`Connect`/`Play`/`Pause`/`Resume`/`SeekTo`/`SkipNext`/
  `SkipPrevious`, plus a `PlayerStateChanged` event so a host picking a
  song from inside Spotify itself, not just this game's buttons, still
  broadcasts to the table), a `NullSpotifyPlaybackBridge` fallback for
  the Editor/unsupported platforms, and platform bridges
  (`SpotifyAndroidBridge.cs`, `SpotifyIOSBridge.cs`) that call into a
  native companion library this project doesn't yet include (see
  `docs/spotify-integration-setup.md` for the reference Kotlin/Obj-C++ to
  drop in). `FollowHostMusic` runs on every guest's poll and applies
  whatever the host's state says, compensating for however long the
  update has been sitting on the server so a guest joining mid-song
  doesn't restart it at 0.
- **UI**: new "Music" page in the in-game drawer (Options → Music).
  Everyone gets Connect + a "Now Playing" line; the host additionally
  gets Prev/Play-Pause/Next. Guests see a note that the host controls it.
  Uses the same `DrawMetalButton`/`DrawOpaquePanel` styling as the rest
  of the drawer.

**What's verified vs. not**: the server plumbing and the C# orchestration
logic are real and either live-tested (server) or straightforwardly
correct C# (client). The actual Spotify App Remote SDK calls in
`SpotifyAndroidBridge.cs`/`SpotifyIOSBridge.cs`, and the native
Kotlin/Objective-C++ wrapper code in `docs/spotify-integration-setup.md`,
are written to Spotify's documented SDK shape from training knowledge but
**could not be compiled or run** -- there's no Android/Xcode toolchain,
no Spotify Developer account, and no real device in this sandbox, and
App Remote doesn't run in the Unity Editor or iOS Simulator regardless.
`SpotifyClientId` in `StreetDiceSpotify.cs` is still blank -- nothing
connects until that's filled in from a real Spotify Dashboard app.

Answers to the three direct questions:
- **Does Spotify need to be open on the phone?** Not visibly open --
  installed and logged in is enough; App Remote wakes it in the
  background.
- **Is the login from the app?** Yes, Spotify's own official OAuth
  screen; this game never sees a password.
- **Can the controls be on my app?** Yes -- that's what the new Music
  drawer page is. What can't happen (Spotify's ToS forbids it) is piping
  the host's actual audio to other phones; each guest's own Spotify plays
  it locally, kept in sync by the small metadata broadcast above.

## Group 2, round 4 -- 2026-09-27

### Rank ladder: real accounts, bet cap, prestige bills

User-specified numbers: Level 1 (start) max bet $100, Level 3 $500, Level 5
(top) $1000 -- Level 2/4 filled in as a smooth ramp (250/750), clearly
marked as the easiest values to retune (`RankLadder.cs`). $50/$100 note
art unlocks at Level 3.

- **Real accounts, not device-local.** User's explicit call (over a
  simpler PlayerPrefs-only approach) -- `PlayerAccountStore.cs`:
  register/login with PBKDF2-hashed passwords (210k iterations, matches
  OWASP's current minimum), account session tokens using the exact same
  generation/fixed-time-compare pattern `StreetDiceTableStore` already
  uses for table sessions. Persisted the same way as table state --
  write-to-temp-then-move JSON, its own file so a table-state format
  change can never take accounts down with it. New endpoints:
  `POST /api/accounts/register`, `POST /api/accounts/login`,
  `GET /api/accounts/{id}`.
- **Wins-per-level is a placeholder XP metric.** Nothing in this
  conversation specified how leveling is earned; total shot wins was the
  simplest thing already trackable (hooked into `/roll/commit`, which
  already knows whether a resolution was a shooter win). Easy to swap for
  something else later -- it's one array in `RankLadder.cs`.
- **Host-propagation, corrected to be additive.** The first draft made
  the table's cap purely the host's level (matching "that is the high
  ranked player's dice game if he/she is the host" literally). Caught in
  review: that would also let a low host DOWNGRADE a guest below their
  own earned level, which contradicts "they will be able to bet more" --
  benefit language, not replacement. Fixed to
  `EffectiveBetCap(gameId, playerId) = MaxBetForLevel(max(hostLevel,
  thatPlayer'sOwnLevel))`: a high host lifts everyone up, nobody's own
  progress is ever taken away by a lower host. A generic, no-player-given
  call (e.g. a lobby display before anyone's identified as the actor)
  still reads as the host's level alone.
- **Enforced, not just displayed.** `/shot`, `/decision/run-same` and
  `/decision/double-up` all reject amounts over the requesting player's
  effective cap with 400 + the actual cap, rather than trusting the
  client. Live-verified with `dotnet run` + `curl`: registered an
  account, joined as host, confirmed `betCap: 100` on the table, a $500
  shot got rejected, a $100 shot went through.
- **Hustled prestige notes.** `PlayerAccount.HustledPrestigeNotes` is the
  holding data for "it's ok if a lower rank ends up with a $50 or $100,
  that's a flex" -- nothing yet decides how a note lands in there (that's
  a separate design question, flagged, not guessed at). What's built is
  `PrestigeNoteUsableBy`: a held note is usable if the table's host is
  Level 3+ *or* the holder reached Level 3 on their own account --
  matching "they wouldn't be able to use it unless they are in a party
  with a level 3+ host or they level up themselves" exactly, including
  that a personal level-up works even under a low-level host (the one
  case where an individual's own level matters on its own, separate from
  the additive host-boost rule above).
- 31 new tests (`RankLadderTests`, `PlayerAccountStoreTests`,
  `RankGatedTableTests`) plus the `ReconnectGraceTests` fix (was missing
  a `using` it happened not to need before `PlayerAccountStore` became a
  constructor dependency of `StreetDiceTableStore`). 132/132 passing.

### $50/$100 bill art: prompts only, not generated

No image-generation tool is available in this session. Corrected an
earlier misreading: the project's existing $1/$5/$10/$20 notes already
use realistic president portraits (Washington, Lincoln, Hamilton, and a
custom Tubman likeness on $20) with "iPlay PROP MONEY"/"NOT LEGAL
TENDER" markings -- this is the established, already-approved pattern
(`docs/decisions/2026-09-06-currency-set.md`), not something this round
introduced. `docs/decisions/2026-09-27-fifty-hundred-dollar-notes.md`
extends it with the same template for $50 (Grant) and $100 (Franklin),
keeping $1 and $20 on their existing custom portraits per instruction.
The prompts are written and ready; the two PNGs still need to be
generated by a session with image-gen access and dropped into
`Assets/Resources/Money/`.

### Not yet done

Client-side (Unity) wiring -- an actual login/register screen, and
extending `StreetDicePlayExperience.cs`'s `WagerAmounts` bill-selection
list to include $50/$100 when unlocked -- was intentionally left for a
follow-up rather than touched blind. There's no way to exercise it yet
(no client login flow exists to ever set `prestigeBillsUnlocked: true`
for a real player), and the existing bill-selection UI has a fixed
layout that's risky to widen without a live Unity Editor to check it in.
The crowd-size and dice-color-wheel reward ideas from the original
rank-system brainstorm are also still unbuilt -- this round only covers
what had concrete numbers attached (bet cap, prestige bill unlock).

## Group 2, round 5 -- 2026-09-27

### Music moved inline; login + Game Stats added

Per instruction: music controls no longer live behind their own drawer
tab. `DrawInlineMusicControls` renders the exact same connect/status/now-
playing/host-transport controls directly on the main Options page, right
under Voice & Sound -- removed the "Music" button and the separate
`drawerPage == "Music"` page entirely, no dead code left behind.

Also closed the gap flagged at the end of the last round ("no Unity
login screen yet"): a new `StreetDiceAccount.cs` gives the client an
actual account system --

- Sign in / Create Account form on the new **Game Stats** page (reached
  from a new Options button), using the same `DrawFlowField` styled
  text-input helper the startup flow already uses for player name/server
  address, plus `GUI.PasswordField` for the password.
- Logged-in state shows level, wins, wins to next level, this level's
  max bet, and whether $50/$100 notes are unlocked -- all real data from
  `GET /api/accounts/{id}`, not placeholders.
- Login persists across app restarts via `PlayerPrefs` (same pattern
  `baseUrl` already uses), and `JoinRealPlayer` now sends the saved
  `accountId`/`accountSessionToken` along with `join-real` -- this is
  the piece that actually turns on the bet cap and prestige-bill
  unlocks built last round for a real signed-in player, not just in
  tests.
- Account requests use their own `PostAccount` coroutine rather than the
  shared `Post`/`PostOpen` helpers, since those assume a table already
  exists and swallow the server's actual error text; sign-in failures
  now show the real message (e.g. "Incorrect username or password.")
  on the form itself.
- Server-side: `GET/POST /api/accounts/*` responses now send
  `winsUntilNextLevel: -1` at max level instead of a JSON `null` --
  Unity's `JsonUtility` doesn't reliably handle a null landing on a
  non-nullable int field, so this avoids relying on unverified behavior
  there. Live-verified with `dotnet run` + `curl`: register, log in
  (now-consistent response shape between login and profile fetch), wrong
  password correctly returns 401 with a readable error body. 132/132
  tests still pass (no test-visible behavior changed server-side this
  round, just the response shape).

## Group 2, round 6 -- 2026-09-27

### Corrected: the table's rank is the host's rank, full stop

Round 4 quietly changed the host-propagation rule to be additive --
`max(host's level, that player's own level)` -- reasoning that "they
will be able to bet more" implied a floor, never a downgrade. User
corrected this: "I want it all to be around the host... if a host is a
level 3 and everyone else is a level 1, everyone will get level 3
benefits as long as you are at a level 3 table." That's not additive --
the table's cap and prestige-bill unlock are simply the host's level,
period, even for a guest who personally outranks the host. Reverted
`EffectiveBetCap`/`PrestigeBillsUnlocked` to pure `HostLevel(gameId)`,
dropped the now-wrong `playerId` parameter and the `EffectiveLevel`
max-of-two helper, and swapped the test that had asserted the additive
behavior for one asserting the corrected one (a Level 5 guest still
plays at a Level 1 host's $100 cap). `PrestigeNoteUsableBy` keeps its own
separate own-level-3 path for a hustled note specifically -- that's a
distinct, explicitly-stated exception for that one flex mechanic, not
the general table cap. 132/132 tests pass; live-verified the reverted
default case with `dotnet run` + `curl`.

### Same correction, extended to the $50/$100 hustle rule

Asked to apply the same host-only correction to the prestige-note hustle
logic. `PrestigeNoteUsableBy` had kept a personal-level-3 exception ("or
they level up themselves") alongside the host check. Removed it --
`PrestigeNoteUsableBy` is now just "does this account hold the note, and
is the table's host Level 3+," nothing else. Worth being upfront about
the consequence: once bill unlock is purely host-driven, a hustled note
never grants its holder anything a table's other players don't already
get once the host clears Level 3 -- the note is a trophy/collectible,
not a live gameplay edge, since there's no scenario left where holding
one lets you do something a non-holder at the same table can't. Added a
test (`HoldingAHustledNote_DoesNotByItselfGrantAnythingAHostAlreadyUnlocksForEveryone`)
that makes that consequence explicit rather than leaving it implicit.
133/133 tests pass.

## Round 7 -- 2026-09-28

### Review findings #1 and #4

- **#4 fixed: logins now survive a server restart.** Account session tokens
  are persisted with the account (`PersistedAccount.SessionToken`, optional so
  older save files still load). On launch the app checks its saved token via
  new `POST /api/accounts/{id}/session`. `join-real` now returns
  `accountLinked`. Either path signs the player out with a visible "your
  sign-in expired" message instead of silently seating them at Level 1.
  Live-verified: signed in, restarted the server, same token still accepted;
  a stale token got 401 on `/session` and `accountLinked: false` on
  `join-real`. 135/135 tests (+2: tokens survive snapshot/restore; older
  snapshots without tokens still load).
- **#1 partially verified.** No Unity install exists in this environment, so
  a real compile is still Codex's first job. Ran Roslyn's C# parser (C# 9,
  Unity's language version) over all 28 scripts under the default,
  UNITY_ANDROID, UNITY_IOS and UNITY_EDITOR configurations: 0 syntax errors.
  This catches structural mistakes, not wrong Unity API names or types.
- #2 (stakes can't reach the cap) and #3 (host leaving) wait on owner
  answers -- see docs/decisions/2026-09-27-rank-host-and-rewards.md.

## Round 8 -- 2026-09-28

### Review findings #2 and #3, plus the XP formula

- **#3 fixed: the host leaving.** `StreetDiceTableStore.HandleDeparture` runs on
  every departure path (leave button and disconnect expiry). The party keeps
  the departing host's level for the rest of the party (`LockedTableLevel`),
  and the host role passes to the highest-ranked player still seated. Also
  fixed a related gap: `HostId` was never saved, so a server restart cleared
  every party's host, rank and music controls. `HostId` and
  `LockedTableLevel` now persist.
- **#2 fixed: bets can reach the cap.** Tapping a bill now adds it to the
  stake like dropping cash, up to the party's cap and what both sides can
  cover, with a Clear button and a "Stake $X (max $Y)" label. Run Same and
  Double Up respect the cap too. The app reads `betCap` and
  `prestigeBillsUnlocked` from the state poll. $50/$100 join the picker and
  ground piles only when unlocked *and* their art exists, so a missing
  texture can't crash a pile. `SelectMainWager` still sets an exact single
  bill, so the existing Editor readiness checks stay valid; stacking needs its
  own readiness check. The pile's look should match the owner's stacked-bills
  screenshot once uploaded.
- **XP is now a combination of playing, winning and money won**, with
  per-opponent and daily caps against farming (see the rank doc). Both the
  shooter and the catcher earn XP on every finished shot. Game Stats shows XP,
  shots, wins and XP to next level.
- Live-verified with `dotnet run` and real server-rolled dice: a come-out loss
  gave the shooter 10 XP and the $20-winning catcher 27 XP; the host leaving
  moved the host role to p2 with the level locked. 146/146 tests (+11). All 28
  Unity scripts still parse with 0 syntax errors.
- New `docs/decisions/2026-09-28-ads-bankroll-and-seat-hold.md` records the
  approved ad layout, one-bankroll-per-account and seat hold (not built).
