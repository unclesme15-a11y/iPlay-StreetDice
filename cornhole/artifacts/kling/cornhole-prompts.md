# Cornhole Kling Shot List And Prompts

Rule of thumb from Street Dice: **lock stills with text-to-image first, then make every video clip with image-to-video from that locked still.** That keeps the boards in the same place in every clip, so the Unity invisible boards stay lined up.

## 1. Plates (Stills, No Opponents)

### Thrower View (Plate 1)

```text
First-person point of view standing in a cornhole pitcher's box at a city block party court at golden hour, eye level about 5 feet, looking straight down a clear 27 foot lane at a regulation wooden cornhole board with a round hole, a matching board just out of frame at the bottom, smooth paved court surface, string lights overhead, blurred crowd far in the background, no people between the boards, no logos, no text, realistic photo, natural lighting, 9:16 vertical
```

### Far Board Cam (Plate 2)

```text
Low close-up camera angle beside a regulation wooden cornhole board with a round hole, same city block party court at golden hour, board fills the lower half of the frame, clean empty board surface, string lights and soft blurred background, no bags, no people, no logos, no text, realistic photo, 9:16 vertical
```

### Near Board Cam (Plate 3)

```text
Low close-up behind and above a regulation wooden cornhole board with a round hole, looking down the lane toward the far board 27 feet away, same city block party court at golden hour, board in the lower third of the frame, no bags, no logos, no text, realistic photo, 9:16 vertical
```

## 2. Character Clip Set (Per Video Opponent)

Make one character's full set first. Use the same character reference image for every clip.

| Clip | Length | Plate it starts from | Notes |
|------|--------|----------------------|-------|
| `idle` | 4-5 s loop | Plate 1 (far end) | Standing in the box, bouncing a bag, looking toward camera. |
| `stepup` | 2 s | Plate 1 | Squares up, lifts the bag. |
| `throw` | 3 s | Plate 1 | Underhand throw toward camera. The bag must leave frame or be hidden right at release. |
| `react_good` | 2-3 s | Plate 1 | Fist pump, points, nods. |
| `react_bad` | 2-3 s | Plate 1 | Head shake, hands on hips. |
| `react_opponent_scored` | 2-3 s | Plate 1 | "Nice shot" nod or playful frustration. |
| `neighbor_throw` | 3 s | Plate 4 side angle | Same character throwing from beside the camera player. |

### Example Throw Clip Prompt (Image-To-Video From Plate 1)

```text
An adult man in a plain dark t-shirt stands in the far pitcher's box beside the far cornhole board, facing the camera, steps forward and makes a smooth underhand cornhole toss toward the camera, the bag leaves his hand and immediately exits the top of the frame, camera completely locked off and still, boards and background do not move, realistic, natural motion, 3 seconds
```

Key words to keep in every clip prompt: **"camera completely locked off"**, **"boards and background do not move"**. If the camera drifts, the Unity boards won't line up.

## Notes

- Kling may change the board's position slightly between clips. Reject any clip where the board moves. Check by laying it over the locked still.
- Delete the bag from the video right at release (or pick takes where the hand hides it). Unity always draws the flying bag.
- Keep clothing plain, with no logos or team names.
