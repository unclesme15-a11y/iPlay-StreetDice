# Visual And Camera Plan

## Current Environment Reference

The current base target is the low ground-level bodega roll-up/service-door frame:

- Camera is almost on the wet asphalt.
- Foreground is dark, wet alley pavement.
- Ground should be one continuous dark pavement material from the first-person foreground to the closed door.
- Use the garage-up composition as the framing reference, but the roll-up garage/service door is fully down. Do not show a visible open bay or anything behind an open door.
- Brick walls/pillars sit on both sides of the garage frame.
- The scene should feel like a real Brooklyn back-of-bodega/service-door spot, not a game arena.
- No table. No chairs. No casino styling.
- Start with no characters in the environment plate.

The side-player reference is useful only for later body placement:

- Players crouch on left and right edges.
- They should frame the lane without blocking the rolling surface.
- This comes after the environment/camera target is correct.

The older Power Slap-style frame is only a broad layout reminder:

- People around edges.
- Clear center floor area.
- First-person-friendly bottom edge.

It is not the current production look.

## Table Layout

Target layout for up to 5 players:

```text
[ Top HUD / Dice Magnifier / Result Readout ]

        Player 4        Rolling Zone        Player 3

        Player 5        Dice / Street       Player 2

[ First-person Shooter / local controls / voice / bet buttons ]
```

The bottom player is the first-person local seat when that player is shooting.

Current art pass sequence:

1. Lock the low ground bodega closed roll-up/service-door environment.
2. Add dice readability on the real ground.
3. Add UI/mic markers only as prototype overlays.
4. Map each seat to a throw lane while keeping the phone/table camera fixed.
5. Add player-adjacent Fade/Catch and side-bet overlays.
6. Add computer-generated opponent bodies later, using crouched edge placement.
7. Human players stay represented by mic/profile overlays instead of bodies.

## Seat And Shooter View

The phone view should remain the same table/ground view when another player shoots.

- Local shooter / `p1`: dice enter from the bottom of the screen.
- Left human / `p3`: dice enter from the left edge.
- Right human / `p4`: dice enter from the right edge.
- Catcher AI / `p2`: dice enter from the back of the lane.
- Back AI / `bot-5`: dice enter from the back-right lane.

Human seats are represented by profile/mic overlays only. They should not receive generated fake bodies.

Computer opponents can later receive generated crouched body plates or short Kling throw clips. When an AI opponent shoots, Kling can provide body language/pose and Unity still owns the dice, final roll, fade state, side bets, and HUD.

## Rolling Area

- Dark pavement, alley floor, rooftop floor, or branded iPlay dice lane.
- Center zone must remain visually readable.
- Avoid curved curbs, sidewalk transitions, or mixed street/sidewalk ground surfaces in the bodega alley scene.
- Avoid casino table styling.
- Use urban iPlay visual language.

## Magnifier / Result Display

The top display should show final counted dice values only.

Counted roll example:

```text
4 + 2 = 6
POINT: 6
```

Faded roll example:

```text
FADED
SHOOT AGAIN
```

Do not show a final dice result when a roll is faded, because the roll does not count.

## Fade/Catch Visual

When Catcher fades:

- Dice freeze, blur, get interrupted, or reset before final lock.
- Result display says **FADED - SHOOT AGAIN**.
- No payout animation.
- No side-bet animation.
- Shooter receives dice again.

Avoid "dissolve" as a required visual. The important meaning is: roll did not count.

## Dice Visual System

Normal dice skins:

- Black
- White
- Green
- Blue

Prototype dice target:

- Rounded high-resolution mesh, not cube primitives.
- Recessed pip wells with high-contrast inset pips.
- Procedural surface grain and subtle bump detail.
- Small scuffs/wear marks so the dice do not read as flat plastic blocks.
- Contact shadows under every die to keep them grounded on the Kling plate.
- World dice should be physically small, close to real dice scale, and only barely readable from the main first-person street camera.
- The top-right magnified dice readout is responsible for showing what the dice landed on.

Streak build:

- Chosen color remains visible.
- Add subtle glow, trail, pulse, or heat rim.

Full streak:

- Dice become red/orange hot dice.
- Top magnifier and streak meter shift to red/orange accents.
- Red/orange is temporary and reserved for full streak only.

Streak broken:

- Dice return to the player's chosen color.

## Real Clip + Unity Overlay Plan

Real/Kling clip should provide:

- Street energy.
- Computer opponent presence only when an AI/NPC is physically at the table.
- Body language and tension.
- First-person roll anticipation.

Unity should provide:

- Actual dice.
- Dice physics/animation.
- Roll result.
- Fade/Catch timing.
- Side bet UI.
- Streak meter.
- Voice indicators.
- Server-confirmed state.

The roll outcome should be decided by the backend, not by interpreting the clip.

## Realism Upgrade Status

The current playable prototype covers the first pass of the 10 realism jumps:

1. Real dice direction: procedural rounded dice now have bevels, recessed pip wells, surface grain, normal detail, scuffs, contact shadows, and player-selectable colors.
2. Shadow-catcher ground: dice and Cee-lo dice use contact shadows over the Kling plate so they sit on the street surface instead of floating.
3. Matched lighting: overhead street light, cool door spill, and warm dice practical light are tuned for the wet bodega closed-door frame.
4. Camera/lens matching: fixed phone-friendly first-person ground view remains locked to the bodega closed roll-up door plate.
5. Realistic roll behavior: throws use directional lanes, bounce, skid, random landing offsets, and a lock-to-result finish.
6. Motion blur: dice now get subtle colored motion streaks during active rolls, with red/orange reserved for full streak.
7. Hands only first: the bottom shooter gets a first-person hand throw rig, but only fingers and part of the palm-side hand should show from the bottom edge; wrists should never enter frame, and human opponents remain mic/profile overlays.
8. Street audio: placeholder beeps were replaced with generated dice-on-pavement grit and short impact taps.
9. Occlusion: a subtle threshold mask helps blend Unity dice into the garage entrance area.
10. UI restraint: top-right dice math stays quiet unless tutorial mode is enabled; normal mode keeps only the needed point/shot status.

The next asset-quality jump is replacing procedural dice with scanned or professionally authored dice models/materials, but the current code path already supports authoritative landing results, selected dice color, and hot-streak red/orange override.
