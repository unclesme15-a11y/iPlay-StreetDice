# Visual And Camera Plan

## Current Environment Reference

The closest current target is the low ground-level bodega/service-door frame:

- Camera is almost on the wet asphalt.
- Foreground is dark, wet street texture.
- A raised curb/sidewalk cuts horizontally across the middle of frame.
- Closed metal roll-up service door is centered in the background.
- Brick walls/pillars sit on both sides of the door.
- The scene should feel like a real Brooklyn back-of-bodega/service-door spot, not a game arena.
- No table. No chairs. No casino styling.
- Do not prioritize side players yet.

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

1. Lock the low ground bodega/service-door environment.
2. Add dice readability on the real ground.
3. Add UI/mic markers only as prototype overlays.
4. Add side players later, using crouched edge placement.

## Rolling Area

- Street mat, pavement, alley floor, rooftop floor, or branded iPlay dice lane.
- Center zone must remain visually readable.
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
- Hands entering frame.
- Players leaning/crouching around the lane.
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
