# Crowd Footage Plan -- Green-Screen Characters in Crowd Sockets

Owner decision 2026-09-30: the reactive crowd uses the owner's green-screen character
footage (made with Kling through fal.ai, stored in the owner's Google Drive), keyed and
placed into fixed **crowd sockets** around the bodega door scene. This replaces the earlier
plan of Claude sourcing whole-scene crowd clips.

## How it works (plain version)

- The scene is the closed bodega roll-up door with brick walls on both sides, filmed from
  almost on the pavement (`docs/visual-camera-plan.md`).
- A **crowd socket** is a fixed spot in that scene where one person can stand: in front of
  the door, against a brick pillar, leaning on the wall. Each socket plays one character's
  green-screen clip with the green removed, so the person looks like they're standing there.
- Each character has a small **reaction set** (idle, watching, cheer, groan, hype). The game
  swaps clips when something happens -- dice in the air, hot dice, a big win, a seven-out.
- **Crowd size follows the host's rank** (already approved): more sockets fill as the host
  ranks up.

| Host rank | People in the crowd |
| --- | --- |
| Unranked (L1) | 3 |
| Shooter (L2) | 5 |
| Skilled Shooter (L3) | 7 |
| Pro Shooter (L4) | 9 |
| DICE G🎲D (L5) | 12 (full) |

Numbers are tunable; the rule "more rank, bigger crowd" is the owner's.

## The reaction set (per character)

| Clip | Length | Plays when | Loop? |
| --- | --- | --- | --- |
| `idle` | 10 s | Default, between everything | Yes -- must start and end in the same pose |
| `watch` | 5 s | Dice are in the air | Hold last pose, back to idle |
| `cheer` | 5 s | Point hit, big win, come-out 7/11 | Once, back to idle |
| `groan` | 5 s | Seven-out, come-out crap, Cee-lo 1-2-3 | Once, back to idle |
| `hype` | 5 s | Hot dice (full streak), level-up moment | Once, back to idle |

Minimum to ship a character: `idle` + `cheer` + `groan`. `watch` and `hype` make it feel alive.

## Shooting rules (so the green keys out clean)

1. **Solid, flat, evenly lit green** background, no shadows or folds on it.
2. **No green on the person** -- no green clothes, hats, shoes or jewelry.
3. **Locked camera.** No pan, zoom, dolly or cuts. The person moves, the camera doesn't.
4. **Full body in frame**, head to feet, with a little space above the head and below
   the feet. Nothing leaving the frame (arms, hair, hands).
5. **Low camera angle** -- around knee-to-waist height, looking very slightly up -- to
   match the game camera sitting almost on the pavement. People shot at eye level or from
   above will look pasted in.
6. **Light from above and slightly in front**, like the overhead street light in the scene.
   No colored or rim lights.
7. **Portrait 9:16, 1080p**, 24 or 30 fps. Portrait puts more pixels on a standing person.
8. **Same person across all their clips.** Use image-to-video with a still frame from that
   character's existing clip as the start image, so the face and outfit stay the same.
9. No text, logos, watermarks, cash, or other people.

## Kling prompts (image-to-video, through fal.ai)

Use a still frame of the character from their existing green-screen clip as the **start
image**, 9:16, 1080p. Paste the shared part first, then the reaction line.

**Shared (every clip):**

```text
The same person from the reference image, full body head to feet, standing on a solid flat
evenly lit chroma green background. Camera locked off, completely still, low angle at knee
height looking slightly up. Soft overhead light from slightly in front. No other people, no
props, no text. The person is a spectator at a street dice game happening on the ground in
front of them.
```

**idle (10 s):**

```text
Relaxed idle: shifts weight from foot to foot, small head turns, glances down at the ground
in front of them, arms loose or crossed, subtle natural breathing. Ends in exactly the same
pose it started in.
```

**watch (5 s):**

```text
Leans forward a little, eyes following dice rolling on the ground in front of them, tense
anticipation, rubs hands together, then holds still watching.
```

**cheer (5 s):**

```text
Big win reaction: jumps back, points at the ground, claps and shouts with excitement, then
settles back to relaxed standing.
```

**groan (5 s):**

```text
Painful loss reaction: grabs head with both hands, turns half away, groans and shakes head,
then settles back to relaxed standing.
```

**hype (5 s):**

```text
Hyping up the shooter: waves arms, bounces on their feet, calls out and pumps a fist, then
settles back to relaxed standing.
```

**Negative prompt (if fal.ai's Kling form has one):**

```text
camera movement, zoom, pan, cut, shaky camera, extra people, text, watermark, logo, green
clothing, shadows on background, cropped feet, cropped head, blurry
```

## Google Drive layout (for new clips)

Codex pulls the footage straight from the owner's Google Drive. Existing clips can stay
where they are. For new ones, this layout makes them easy to find -- one folder per
character, files named by reaction:

```text
iPlay Crowd/
  big-mike/
    idle.mp4
    watch.mp4
    cheer.mp4
    groan.mp4
    hype.mp4
  auntie-dee/
    idle.mp4
    ...
```

Existing fal.ai clips don't need to be renamed first -- Codex inventories whatever is
there and reports which character is missing which reaction.

## What Codex builds (see the Codex prompt, "Crowd")

1. **Get the footage.** Codex logs into the owner's Google Drive, finds the green-screen
   character clips, and downloads copies -- never moving, renaming or deleting anything in
   the Drive -- into `artifacts/reference-private/crowd-raw/` (git-ignored).
2. **Inventory.** Write `docs/crowd-inventory.md`: one row per character, which reactions
   exist, length, resolution, and a flag for anything breaking the shooting rules (camera
   moves, cropped feet, green clothing, eye-level angle). The owner fills the gaps with the
   prompts above.
3. **Bake.** A repeatable script (`tools/bake-crowd.ps1`, ffmpeg) keys out the green,
   trims, and bakes each clip into a small flipbook (about 256-320 px tall, 12-15 fps,
   texture atlas with alpha). Phones can't play 12 HD videos at once; flipbooks are cheap.
   Baked output goes in `unity/StreetDiceGreybox/Assets/Crowd/Baked/` (git-ignored, like the
   paid hand pack -- Drive is the source; re-bake any time). Commit the script and a small
   `crowd-manifest.json` (characters, reactions, frame counts).
4. **Sockets.** 12 fixed crowd sockets in the scene: back row in front of the roll-up door,
   side spots against the brick pillars, never over the roll lane, the seat HUDs or the
   opponents' spots. Each socket has a position, scale (far = smaller), facing toward the
   dice, and a soft blob shadow under the feet. Fill sockets in a fixed order so a small
   crowd still looks placed on purpose.
5. **Variety.** Random characters with no repeats, random start frame, random left/right
   mirror, slight per-socket tint to match the scene light.
6. **Reactions.** Hook the events already specced (dice in the air, hot dice, big win,
   point hit, seven-out / come-out crap, Cee-lo 1-2-3, level-up). Each person reacts after
   a small random delay (0-0.4 s), and about a third keep watching instead of reacting, so
   the crowd never moves in unison. Every reaction returns to idle.
7. **Crowd size by host rank** (table above). Same everywhere: private parties, The Jungle,
   offline.
8. **Approval gate.** First bake **one** character and screenshot them in two sockets
   (near and far) before doing the rest. The owner approves the look first.
