# AI Video Characters -- CPU Opponents and Crowd

Claude's recommendation, 2026-10-29. Owner asked how to bring the AI video (Kling via fal.ai)
into the game for the crowd and the CPU opponents. Builds on `docs/crowd-footage-plan.md`
(crowd sockets) and the Kling fade clips already in the game
(`StreetDiceFadePresentation.cs`, `Resources/Fades/`).

## The one rule that makes it work

**Generate the video ahead of time, play it back live.** Nothing is generated while you
play -- AI video takes minutes per clip and costs credits, so live generation on a phone
isn't possible. Instead every character gets a library of short clips (idle, throw, cheer,
groan...) made once with Kling, and the game picks which clip to play the instant something
happens. Like a video game cut-scene library, but for every move.

**The dice are never in the video.** The server rolls the dice and Unity draws them (that's
what keeps the game fair). The video only shows people: a hand that shakes and throws, a
face that reacts. The real dice appear out of the video hand at the exact release moment.
This is already how the Kling fade hands work: the video hand taps, and the game's dice are
added on top.

## Three tiers of video people

| Tier | Who | How many on screen | Size | How it plays |
| --- | --- | --- | --- | --- |
| 1. The shooter | Whoever has the dice (CPU) | 1 | Big, close | Real video (VideoPlayer), one at a time |
| 2. The table | The 4 CPU opponents crouched at the left/right edges | up to 4 | Medium | Light flipbooks; one action at a time switches to video |
| 3. The crowd | Standing people by the door and walls | 3-12 by host rank | Small, far | Flipbooks only (see crowd plan) |

Why tiers: a phone can only decode a few videos at once. The person doing something
important gets real video; everyone else uses cheap flipbooks (a picture strip played fast,
like a flip book).

## CPU opponents: give them a personality

Today the 4 CPU seats are four betting styles in `DemoOpponentPolicy` (seat 0-3: how big
they bet, how often they accept, how fast they answer). Each style becomes a character the
player recognizes, with a look, a name and a set of clips that match how they play.
Example roster (owner names them):

| Seat style today | Character idea | Plays like | Moves like |
| --- | --- | --- | --- |
| Cautious, small bets | **Old Head** | Bets $5-$10, often passes | Slow, arms folded, shakes head |
| Big bets, takes a lot | **Big Spender** | Stacks $50s, accepts most bets | Loud, flashy, points at you |
| Picky, declines a lot | **The Hustler** | Waits, then strikes | Sly, rubs hands, quick throws |
| Middle | **Young Buck** | Average | Hyper, bounces, talks trash |

Build 8-10 characters so the table isn't the same four every game; offline picks 4 at random.

### Clip list per opponent

All from the same start picture of that character (Kling image-to-video), so the face and
outfit never change. Green screen, locked camera, crouched like they're at the dice.

| Clip | Length | Plays when |
| --- | --- | --- |
| `idle` | 8-10 s loop | Waiting (default) |
| `watch` | 4 s | Dice are in the air |
| `shake-throw` | 3 s | They shoot -- hands cupped (dice hidden), shake, throw. The release frame time is written down so the game's dice appear right then |
| `pick-up` | 2 s | They get the dice back |
| `offer` | 2 s | They propose a bet (point at the shooter / lay money down) |
| `accept` | 1.5 s | They take your bet (nod, slap the ground) |
| `decline` | 1.5 s | They say no (wave it off) |
| `win` | 3 s | They win a bet or hit the point |
| `lose` | 3 s | They seven-out or lose a bet |
| `pass` | 2 s | They pass the dice |
| `fade` | already made | The Kling catch hands (Tap / Plant / Wave) -- each character gets one of the three |

Minimum to ship one character: `idle`, `shake-throw`, `win`, `lose`. The rest can come later.

### Camera angles (important)

The game camera sits almost on the pavement looking at the closed roll-up door. Opponents
crouch at the left and right edges, so each needs to be filmed **from the side, low, at
crouch height**:
- Left-side seats: character faces right (toward the dice lane).
- Right-side seats: mirror the same clips -- no second generation needed.
- Close seats (bottom) bigger, far seats (top) smaller -- same clips, scaled.

Getting the angle wrong is the #1 reason video people look pasted on. Test one character in
one seat before generating the rest.

## How it plays in the game (what Codex builds)

1. **Character data.** `Resources/Characters/<name>/character.json`: name, betting style
   (maps to `DemoOpponentPolicy` seat profile), fade style, voice, and each clip's file, length,
   loop or one-shot, and key moments (`releaseAt` for the throw, `contactAt` for the fade).
2. **Video actor.** One component per seat: plays `idle`, switches to an action clip when the
   game says so, then crossfades back to `idle`. Only one actor at a time uses a real
   VideoPlayer (the one doing the big action); the others use flipbooks.
3. **Game hooks.** Every CPU decision already exists in code -- they just trigger clips:
   bot shoots -> `shake-throw` and spawn the dice at `releaseAt`; bot offers -> `offer`;
   accepts/declines -> `accept`/`decline`; bet settles -> `win`/`lose`; bot passes -> `pass`;
   bot catcher fades -> its fade clip; dice in the air -> everyone `watch`.
4. **Timing rule.** Game rules never wait on video. If a clip is late or missing, the game
   carries on and the actor jumps back to idle. Clips can't change any result.
5. **Transparent video that works on iPhone and Android.** Pack each clip as an ordinary MP4
   with the colour on top and the cut-out mask underneath ("packed alpha"), and a small shader
   joins them. Works with every phone's video chip (see-through WebM doesn't work on iPhone).
   Flipbooks for idle loops and the crowd.
6. **Online.** CPU characters are for offline play. Online, each human player can pick one of
   the same characters as their **avatar**, and their real moves trigger it for everyone (they
   shoot -> everyone sees their character throw). Same clips, no extra footage.
7. **Crowd.** As in `docs/crowd-footage-plan.md` -- same pipeline, smaller, flipbooks only.

## Making the footage (owner, with Kling via fal.ai)

1. One **start picture** per character: full body, crouched, side-on, plain green background,
   low camera, overhead street light. Approve the look first -- everything else comes from it.
2. Image-to-video for each clip with the same start picture. Prompt pattern (shared part +
   action line, like the crowd prompts):
   - Shared: "The same person from the reference image, crouched low on a solid flat evenly
     lit chroma green background, seen from the side at crouch height, camera locked off,
     soft overhead light. No dice, no money, no props, no text, no other people."
   - `shake-throw`: "Cups both hands together, shakes them hard twice by his ear, then
     throws forward low across the ground and holds the follow-through. Hands stay cupped
     until the throw so nothing in them is visible."
   - `win`: "Jumps up from the crouch, claps, points at the ground and shouts, then crouches
     back down."
   - `lose`: "Throws head back, slaps the ground once, shakes head, settles back into the
     crouch."
   - and so on for each clip in the table above.
3. Drop them in Google Drive, one folder per character, files named by clip. Codex pulls,
   keys, packs and wires them.
4. **Budget guide:** the existing 3-second 1080p fade clip cost 24 Kling credits. A full
   character is ~10 clips, so plan on a few hundred credits each including retries.

## Order of work

1. One opponent character, one seat, four clips (`idle`, `shake-throw`, `win`, `lose`).
   Codex builds the video actor and the throw-to-dice timing. Owner approves on a phone.
2. Fill out that character's clips; then the rest of the roster.
3. Crowd sockets with the crowd footage.
4. Online avatars.

## Rules

- Characters are made up -- never a real person's likeness.
- No real money in the footage (the game's own bills are drawn by Unity).
- Raw clips never go in GitHub (`*.mp4` is git-ignored); Drive is the master copy.
  Commit the character JSON files and the bake scripts.
