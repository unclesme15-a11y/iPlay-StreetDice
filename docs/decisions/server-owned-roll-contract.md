# Server-owned roll contract

Normal online Craps now uses the server's prepare/fade/commit routes. The former face-posting `/roll` and non-roll-ID `/fade` routes return HTTP 410. Unity's local demo still chooses faces for controlled rule practice; online Cee-lo still uses a client-chosen three-die evaluator. Full five-seat online and device validation remain incomplete, so `/health` still reports `serverAuthoritativeRolls:false` for the overall demo.

## Throw lifecycle

1. The seated shooter submits normalized gesture power and aim, with no die values. The server validates the active shot, betting deadline, shooter token, gesture bounds and absence of another pending throw. It samples a private physical trajectory for two Craps dice or three Cee-lo dice.
2. The server returns a roll ID, fade deadline, remaining fade milliseconds and public starting poses. `GET` exposes the ID and remaining time to the catcher, not frames or faces. The starting orientation is sampled independently of the private spin seed and shown in the held hand. No exact motion frames are streamed before fade closes. It must not reveal final poses, die values, a seed from which they can be predicted, or a payout.
3. The catcher alone can fade before the deadline. Fade cancels the pending throw, discards its private result, settles nothing, increases fade count/momentum according to the rules and lets the same shooter throw again. A late, duplicated or non-catcher fade is rejected.
4. After the fade deadline, the shooter or a server timer commits the pending throw exactly once. The server determines the top face of each physically settled die, applies the existing rule engine and wager book, and publishes the remaining trajectory, face values and settlement. Repeated commits return the same committed event, not a fresh simulation or payout.
5. Every seated client must use the committed trajectory and outcome. The current Unity shooter path replays committed frames and faces; the catcher path discovers and fades pending rolls. Replay delivery to other seated clients and true independent player sessions remain open. A phone must never select or post winning die faces. Leaving with live wagers retains the established crap/forfeit behavior, including a pending throw.

The current pre-fade visual timeline shows the hand, then hides dice until the 1.25-second fade window closes. It avoids streaming a predictable physical trajectory but creates a visible delay before replay; final catcher timing and visual continuity still need user approval. Exact early poses plus velocities may let a modified client extrapolate a deterministic rigid-body landing even without face values. Do not introduce that stream as a cosmetic fix without resolving the security tradeoff.

## Physics requirements

- Use a maintained .NET 8 rigid-body library for the backend rather than a custom collision engine. `BepuPhysics` 2.4.0 is the current stable NuGet package; its official docs show fixed timesteps, static geometry, dynamic bodies and explicit buffer-pool cleanup. Evaluate it against the existing Unity pavement/door geometry before integration.
- Launch velocity and direction come from the bounded gesture. Streak, wager amount, player identity and bot personality must not change outcome odds. A secure server seed supplies natural variation; clients must not learn it during the fade window.
- Dice must settle uncocked on pavement inside the approved door-width throw lane. Metal-door collisions are allowed, brick contact produces haptics only, and no tables or vents are collision surfaces. Invalid/no-count throws need explicit retry rules before they can affect wagers.
- Record enough pose/contact data to replay the actual server throw smoothly on phone layouts. The client may render richer art and sound, but may not rotate a losing physical result into a winning face.

## Verification gates

- Two- and three-die fixed-seed physics cases settle with valid top faces and no penetration; weak/strong and left/right gestures diverge without role-based odds changes.
- Catcher fade before deadline cancels every outcome and payout; late and unauthorized fades fail. Duplicate prepare/commit and concurrent leave/commit conserve the virtual ledger.
- A modified client cannot choose faces, predict a withheld result during the fade interval, or replay an old roll ID to receive a second payout.
- Complete local and online rounds agree across server state, Unity sky-cam visuals and phone aspect ratios. A standalone/mobile profile and explicit APK approval are still separate gates.
