# Visual And Camera Plan

## The Idea In One Line

The people and the place are real video. The bags, the board hit areas, and the score are Unity. The server decides what counts.

This is the same split as Street Dice. There, Kling makes the bodega and Unity owns the dice. Here, Kling makes the backyard/block party and the opponents, and Unity owns the bags.

## Camera Setups (Plates)

Every setup is a **locked Kling plate**: one approved still frame that every clip for that angle starts from. That way opponent clips cut together cleanly and the Unity boards stay lined up.

| # | Name | What you see | When it's used |
|---|------|--------------|----------------|
| 1 | **Thrower View** | First person from your pitcher's box, looking down the lane at the far board, about 27 ft away. Your hand holding a bag comes in from the bottom edge. | Aiming and releasing. Also shows far-end opponents throwing toward you. |
| 2 | **Far Board Cam** | Close-up of the far board, low and slightly to the side. | Right after you release: the camera cuts here so you can see the bag land. |
| 3 | **Near Board Cam** | Close-up of the board at your feet, facing down-lane toward the far players. | When the far end throws at your board (doubles / Face-Off). |
| 4 | **Neighbor Cut** | Short side angle of the player sharing your end, on the other side of the board. | When the player beside you throws (singles, and doubles at your end). |

Every plate has a matching **invisible Unity board**, meaning a collider plus a hole trigger, placed so it sits exactly on the video board. Bags hit the invisible board, but it looks like they hit the real one in the video.

## Hand-Off Moment (Most Important Timing)

Every opponent throw clip has a **release frame**. That's the moment the bag leaves their hand.

1. The clip plays: they step up, swing, release.
2. At the release frame, the video bag has to be out of shot or hidden by the hand. Unity spawns its own bag at that exact spot on screen.
3. From there the Unity bag flies along the path the server returned.
4. The clip keeps running (follow-through), then switches to a **reaction clip** chosen by the result.

Each clip gets a small data file, for example:

```json
{ "clip": "marcus_throw_02", "plate": "thrower_view", "releaseTime": 1.42, "releaseScreen": [0.61, 0.38], "throwHand": "right" }
```

This is why one throw clip can be reused for every outcome. The video only shows the throw. Unity and the server decide whether it's a cornhole, a woody, or a miss.

## Local Player Throw Flow

1. **Thrower View**: your hand holds the bag at the bottom of the screen (same idea as the Street Dice hand rig).
2. Drag down to wind up. The pull length sets power, and the angle sets aim. A small arc meter shows a flat slide versus a high lob.
3. Let go: the hand swings forward and the bag leaves the screen heading down-lane.
4. About 0.3 s into the flight, **cut to Far Board Cam**. The bag drops into frame and lands, slides, or goes in the hole.
5. Hold for about 1.2 s after it stops, show the result tag (`+3 CORNHOLE`, `WOODY`, `OFF`), then cut back to Thrower View.

Setting: **"Stay on me"** turns off the board-cam cut for players who'd rather watch the full flight.

## Opponent Throw Flow

- **Far-end opponent (throwing at you):** stays on Thrower View. They step up, throw, and the bag comes toward the camera. Cut to **Near Board Cam** for the landing, then play their reaction clip.
- **Neighbor opponent (same end as you):** play the **Neighbor Cut** clip, then at release cut to **Far Board Cam** for the landing, same as your own throws.

## Bag Visuals

- Two team colors per match, for example iPlay black and iPlay green. Avoid red/orange, which stays reserved as a "hot" state the way Street Dice uses it.
- Soft cloth-like mesh with a small wobble while flying. Bags squash and settle on landing.
- Contact shadow on the board so bags don't look like they're floating on the video.
- **Hole masking:** the hole on each invisible board has a depth mask, so a bag going in really disappears into the video's hole.
- Bags stay on the board for the whole inning (they block and stack), and they're cleared with a quick sweep at the end of the inning.

## HUD

```text
[ Team A 14  |  inning 7  |  Team B 9 ]
[ Bags left:  ■ ■ □ □      ■ ■ ■ □  ]
[ This inning: +4 vs +1 -> Team A +3 ]
```

- Tutorial mode explains cancellation scoring live, using the actual inning's numbers.
- Mic/profile rings on each seat for voice chat, the same pattern as Street Dice.

## Environment Look

Start with one setting and lock it before making more:

- Recommended first plate: **city block party / rec-center court at golden hour**. Paved or turf lane, string lights, people blurred in the background, no logos.
- Later: backyard grass, rooftop, tailgate, and beach as extra "courts".
- The lane between the boards must stay clear and readable. Don't put people or objects between the boards.
