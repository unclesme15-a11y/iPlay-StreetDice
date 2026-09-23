# Cornhole Kling Shot List And Prompts

Rule of thumb from Street Dice: **lock stills with text-to-image first, then make every video clip with image-to-video from that locked still.** That keeps the boards in the same place in every clip, so the Unity invisible boards stay lined up.

## 1. Plates (Stills, No Opponents)

Scene: **big open park, summertime hood BBQ cookout, female-heavy crowd in the distance.** The crowd is background only and never stands in the lane.

If Kling softens or refuses a prompt, keep the descriptions of the women's bodies to "curvy" and "thick-figured". Words about specific body parts get filtered more often.

### Thrower View (Plate 1)

Your own board has to be visible right in front of you in the lower part of the frame. Far-end players throw at it, and there's no camera cut for that board.

```text
First-person point of view standing in a cornhole pitcher's box on flat trimmed grass, eye level about 5 feet, a regulation wooden cornhole board with a round hole on the ground directly in front of the viewer filling the lower left part of the frame, looking straight down a clear 27 foot lane at a matching far cornhole board, wide open green city park at a summertime hood BBQ cookout, bright afternoon sun, grills smoking, pop-up tents, folding tables and coolers in the background, a big lively crowd in the distance behind and to the sides of the lane, mostly curvy thick-figured adult Black women, with Latina and white women too, in summer cookout outfits like sundresses, shorts and tank tops, a few men mixed in, many people holding phones, some texting, some taking selfies, some recording, background crowd slightly soft focus, no people between the boards, no logos, no text, realistic photo, natural lighting, 9:16 vertical
```

### Far Board Cam (Plate 2)

Used only for bags thrown at the far board.

```text
Low close-up camera angle beside a regulation wooden cornhole board with a round hole, same summertime park BBQ cookout on trimmed grass, board fills the lower half of the frame, clean empty board surface, cookout tents and a crowd of mostly curvy adult Black and Latina women soft and blurred in the background, several of them holding phones up recording toward the board, no bags, no people on the board, no logos, no text, realistic photo, 9:16 vertical
```

### Neighbor Cut (Plate 3)

```text
Side angle from a cornhole pitcher's box looking across the near cornhole board at the opposite pitcher's box on the other side of it, empty box, flat trimmed grass, same summertime park BBQ cookout behind, crowd of mostly curvy adult women in the soft background with phones out, no logos, no text, realistic photo, 9:16 vertical
```

## 1b. Crowd Behavior Clips (Image-To-Video From Plate 1)

These play behind the game and are timed to the throws, so it looks like the crowd is following the bag. Leave the lane and both boards completely empty in all of them.

Always include: **"camera completely locked off, boards and lane do not change, nobody walks into the lane"**.

| Clip | Length | Prompt add-on (goes after the Plate 1 description) |
|------|--------|-----------------------------------------------------|
| `crowd_idle` | 6-8 s loop | the crowd relaxes and socializes, several people glance down at their phones texting then look back up, two women take a selfie together fixing their hair, one woman records herself talking to her phone for a reel, people holding plates and cups, laughing, light dancing |
| `crowd_lock_in` | 2 s | several people in the crowd stop what they are doing and raise their phones toward the cornhole lane to start recording |
| `crowd_track` | 2-3 s | most of the crowd holds phones up recording, their heads and phones slowly pan from the near side of the lane toward the far cornhole board as if following a bag through the air |
| `crowd_hype` | 3 s | the crowd erupts cheering, phones still up recording, a couple of women flip their phones around to film their own excited reactions, people jumping and pointing at the far board |
| `crowd_settle` | 3 s | the crowd reacts with a small groan, then people lower their phones and go back to texting, talking and eating |

Make 2-3 takes of `crowd_idle` and pick the most natural one. Kling does better with busy crowds when each action is spelled out, like above.

## 2. Character Clip Set (Per Video Opponent)

Make one character's full set first (Keisha). Use the same character reference image for every clip. The full roster is in `docs/characters.md`.

### Character Reference Portrait (Text-To-Image, One Per Character)

```text
Full-body photo of <character description from characters.md>, standing on grass at a summertime park BBQ cookout, relaxed natural pose, holding a cornhole bag, plain clothing with no logos or text, bright afternoon sunlight, realistic photo, 9:16 vertical
```

Example (Keisha):

```text
Full-body photo of a 34 year old curvy Black woman with long braids pulled up, wearing a bright yellow sundress, white sneakers and big hoop earrings, standing on grass at a summertime park BBQ cookout, relaxed confident pose, holding a cornhole bag, plain clothing with no logos or text, bright afternoon sunlight, realistic photo, 9:16 vertical
```

| Clip | Length | Plate it starts from | Notes |
|------|--------|----------------------|-------|
| `idle` | 4-5 s loop | Plate 1 (far end) | Standing in the box, bouncing a bag, looking toward camera. Some takes: a quick glance down at their phone, then pocket it. |
| `stepup` | 2 s | Plate 1 | Squares up, lifts the bag. |
| `throw` | 3 s | Plate 1 | Underhand throw toward camera. The bag must leave frame or be hidden right at release. |
| `react_good` | 2-3 s | Plate 1 | Fist pump, points, nods. |
| `react_bad` | 2-3 s | Plate 1 | Head shake, hands on hips. |
| `react_opponent_scored` | 2-3 s | Plate 1 | "Nice shot" nod or playful frustration. |
| `neighbor_throw` | 3 s | Plate 3 side angle | Same character throwing from beside the camera player. |

### Example Throw Clip Prompt (Image-To-Video From Plate 1)

```text
The character from the reference image stands in the far pitcher's box beside the far cornhole board, facing the camera, steps forward and makes a smooth underhand cornhole toss toward the camera, the bag leaves their hand and immediately exits the top of the frame, camera completely locked off and still, boards and background do not move, realistic, natural motion, 3 seconds
```

Key words to keep in every clip prompt: **"camera completely locked off"**, **"boards and background do not move"**. If the camera drifts, the Unity boards won't line up.

## Notes

- Kling may change the board's position slightly between clips. Reject any clip where the board moves. Check by laying it over the locked still.
- Delete the bag from the video right at release (or pick takes where the hand hides it). Unity always draws the flying bag.
- Keep clothing plain, with no logos or team names.
- Age range for every character is 30-50. Put the exact age in each prompt, or Kling tends to make people look younger.
