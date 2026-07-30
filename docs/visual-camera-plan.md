# Visual And Camera Plan

## Reference Angle

The attached Power Slap-style frame is useful as a camera/layout reference, not as production content.

Useful traits:

- Top-down angled view.
- People around the edges.
- Clear center floor area.
- First-person-friendly bottom edge.
- Strong spectacle framing.

For Street Dice, the center floor area becomes the rolling zone.

## Table Layout

Target layout for up to 5 players:

```text
[ Top HUD / Dice Magnifier / Result Readout ]

        Player 4        Rolling Zone        Player 3

        Player 5        Dice / Street       Player 2

[ First-person Shooter / local controls / voice / bet buttons ]
```

The bottom player is the first-person local seat when that player is shooting.

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
