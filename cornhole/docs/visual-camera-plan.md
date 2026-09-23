# Visual And Camera Plan

## The Idea In One Line

The people and the place are real video. The bags, the board hit areas, and the score are Unity. The server decides what counts.

This is the same split as Street Dice. There, Kling makes the bodega and Unity owns the dice. Here, Kling makes the park cookout and the opponents, and Unity owns the bags.

## Camera Setups (Plates)

Every setup is a **locked Kling plate**: one approved still frame that every clip for that angle starts from. That way opponent clips cut together cleanly and the Unity boards stay lined up.

| # | Name | What you see | When it's used |
|---|------|--------------|----------------|
| 1 | **Thrower View** | First person from your pitcher's box, looking down the lane at the far board, about 27 ft away. **Your own board is visible in the lower part of the frame, right in front of you.** Your hand holding a bag comes in from the bottom-right edge (bottom-left for lefties), so it doesn't cover your board. | Aiming and releasing. Also where you watch far-end players throw at the board right in front of you. No cut needed for that. |
| 2 | **Far Board Cam** | Close-up of the far board, low and slightly to the side. **On by default.** | Only for bags thrown at the **far** board: yours, and your neighbor's. |
| 3 | **Neighbor Cut** | Short side angle of the player sharing your end, on the other side of the board. | When the player beside you throws. In singles this is **always** how you see your opponent, because official singles has both players at the same end. |

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

Board cam is **on by default**. A **"Stay on me"** setting turns it off for players who'd rather watch the full flight.

## Opponent Throw Flow

- **Far-end players (2v2 only, throwing at you):** stays on Thrower View the whole time. They step up and throw, and the bag flies toward you and lands on the board right in front of you. No camera cut. Then their reaction clip plays.
- **Neighbor opponent (same end as you):** play the **Neighbor Cut** clip, then at release cut to **Far Board Cam** for the landing, same as your own throws.

## Bag Visuals

- Each team picks its own bag color from the palette below (see **Bag Color Pick**).
- Soft cloth-like mesh with a small wobble while flying. Bags squash and settle on landing.
- Contact shadow on the board so bags don't look like they're floating on the video.
- **Hole masking:** the hole on each invisible board has a depth mask, so a bag going in really disappears into the video's hole.
- Bags stay on the board for the whole inning (they block and stack), and they're cleared with a quick sweep at the end of the inning.

## Bag Color Pick

Right after Match Setup, every team picks a bag color. **First come, first served.**

- When a team picks a color, it's locked for that match. It goes grey on everyone else's screen with the team's name on it ("Taken by Team A").
- Two teams can never have the same color.
- In 2v2, whichever partner taps first picks for the team, and the other partner sees it right away.
- Video bot teams pick **last**, from whatever colors are left.
- 15-second timer. If a team doesn't pick in time, it gets a random free color.

**Palette (12 colors, chosen so no two are easy to mix up on a wood board from 27 ft):**

| Color | Hex |
|-------|-----|
| Black | `#1B1B1B` |
| White | `#F4F4F2` |
| Red | `#C8102E` |
| Orange | `#FF6A13` |
| Yellow | `#FFD100` |
| Kelly Green | `#009A44` |
| Royal Blue | `#1D4ED8` |
| Sky Blue | `#6CC5F0` |
| Purple | `#6B2C91` |
| Hot Pink | `#E0218A` |
| Teal | `#00A5A8` |
| Maroon | `#6D1A2A` |

- On top of the color, each team's bags get a small iPlay stitch pattern (team A plain, team B with a stitched X). That way color-blind players can still tell whose bag is whose.
- The HUD bag counters and the score bar use each team's chosen color.

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

## Crowd Behavior (Modern Crowd)

The crowd should feel like a real cookout in 2026, where everybody has a phone out. Every idle loop and reaction clip mixes in:

- People **glancing down at their phone** now and then, like they're checking messages or firing off a quick text, then looking back up.
- **Women taking selfies**: posing, fixing their hair, a couple of friends squeezing into one shot.
- People **recording for a reel**: phone held up sideways or vertical, some talking to their camera.
- Everyday cookout stuff: holding a plate or a cup, laughing, dancing a little, pointing at the game.

**Following the bag with their phones.** Kling can't see the Unity bag, so we fake it with timing:

| Moment | Crowd clip that plays | What the crowd does |
|--------|-----------------------|---------------------|
| Nobody throwing | `crowd_idle` loop | Texting, selfies, talking, eating. Nobody's locked in. |
| Someone steps up | `crowd_lock_in` | A few people raise their phones and start recording toward the lane. |
| Bag in the air | `crowd_track` | Phones up. Heads and phones swing from the thrower toward the board, following the bag's path. |
| Cornhole (in the hole) | `crowd_hype` | Cheers, phones still recording, a couple of people turn the phone around to film themselves reacting. |
| Woody / miss | `crowd_settle` | "Ooh," then people drop their phones back down and go back to texting. |

Because each clip is timed to the throw, it looks like the crowd is following the bag, even though the video never actually sees it.

Later scenes (after this one is locked): backyard, rooftop, beach.
