# Street Dice Kling Test Prompts

Use these prompts for first visual tests. The Unity dice/result overlay remains authoritative; Kling clips are background action and table energy.

## Concept Frame

Recommended first low-risk test:

```text
Top-down first-person street dice game angle, urban indoor floor mat, five adult players positioned around a clear central rolling area, one first-person shooter position at the bottom edge of frame, two players along the left side, two players along the right side, realistic live-action sports broadcast lighting, clean empty rolling lane in the center for Unity dice overlay, no casino table, no money visible, no brand logos, intense table energy, cinematic realism, 16:9
```

Suggested model:

```powershell
kling text_to_image --model kling-image-v3_0_omni --aspect_ratio 16:9 --img_resolution 2k --imageCount 1 --prompt "<prompt>" --skill-name kling-cli --skill-version 0.1.3
```

## Shooter Ready Clip

```text
First-person street dice table moment from a high angled camera, the shooter at the bottom prepares to roll into the empty central area, surrounding players lean in and react, realistic urban sports-game atmosphere, no casino styling, no visible cash, leave clear center space for Unity dice overlay and result magnifier, handheld broadcast feel, 5 second clip
```

## Fade / Catch Clip

```text
Top-down street dice angle, the opposing player across the rolling area reaches in to stop the roll before it counts, tense but controlled motion, other players react around the edges, central rolling area remains readable for Unity overlay, no casino table, no visible cash, realistic live-action sports reference
```

## Hot Dice Activation Clip

```text
Street dice players around a central rolling lane react to a hot streak moment, energetic but grounded, red-orange light accents can flash briefly near the dice area, no casino styling, no money visible, leave central area clear for Unity red-orange hot dice overlay, realistic live-action camera angle
```

## Notes

- Start with text-to-image to lock the layout before spending video credits.
- Use image-to-video after selecting a concept frame.
- Keep generated clips free of real brand marks and cash imagery.
- The Unity layer owns the dice color, dice total, point, fade state, and streak meter.
