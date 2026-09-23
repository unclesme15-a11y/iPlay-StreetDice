# Visual And Camera Plan

## The Idea In One Line

The people and the place are real video. The bags, the board hit areas, and the score are Unity. The server decides what counts.

This is the same split as Street Dice. There, Kling makes the bodega and Unity owns the dice. Here, Kling makes the park cookout and the opponents, and Unity owns the bags.

## Camera Setups (Plates)

Every setup is a **locked Kling plate**: one approved still frame that every clip for that angle starts from. That way opponent clips cut together cleanly and the Unity boards stay lined up.

| # | Name | What you see | When it's used |
|---|------|--------------|----------------|
| 1 | **Thrower View** | First person from your pitcher's box, looking down the lane at the far board, about 27 ft away. Your hand holding a bag comes in from the bottom edge. | Aiming and releasing. Also shows far-end opponents throwing toward you. |
| 2 | **Far Board Cam** | Close-up of the far board, low and slightly to the side. | Right after you release: the camera cuts here so you can see the bag land. |
| 3 | **Near Board Cam** | Close-up of the board at your feet, facing down-lane toward the far players. | When the far end throws at your board (2v2 only). |
| 4 | **Neighbor Cut** | Short side angle of the player sharing your end, on the other side of the board. | When the player beside you throws. In singles this is **always** how you see your opponent, because official singles has both players at the same end. |

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

- **Far-end players (2v2 only, throwing at you):** stays on Thrower View. They step up, throw, and the bag comes toward the camera. Cut to **Near Board Cam** for the landing, then play their reaction clip.
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

## Environment Look (Locked Direction)

**Scene 1: big open park, summertime hood BBQ cookout.**

- Wide open green park in full summer afternoon sun. The cornhole lane is on flat, trimmed grass with plenty of open space around it.
- Cookout life in the **background only**: grills smoking, pop-up tents, folding tables, coolers, a speaker, string lights or balloons on the tents.
- A **big, female-heavy crowd** off in the distance: mostly Black women, then Latina, then white. Curvy, thick-figured adults in summer cookout outfits (sundresses, shorts, tank tops, sandals), hanging out, dancing, and watching the game. Some men mixed in, but mostly women.
- The crowd stays **behind and to the sides** of the boards, never between them. The lane has to stay clear and readable.
- Background people are slightly soft-focus so the boards and the throwing players stand out.
- No logos, no brand names, no readable text.

Later scenes (after this one is locked): backyard, rooftop, beach.
