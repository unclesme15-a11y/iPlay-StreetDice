# Hand Pack Integration

The Street Dice demo can use the purchased RRFreelance first-person hand pack for the local bottom shooter throw.

Local package path:

```text
C:\Users\uncle\AppData\Roaming\Unity\Asset Store-5.x\RRFreelance\3D ModelsCharactersHumanoidsHumans\First Person Hand.unitypackage
```

Imported runtime prefabs:

```text
Assets/RRFreelance/FirstPersonHand/Resources/FirstPersonHands/FirstPersonHand_L.prefab
Assets/RRFreelance/FirstPersonHand/Resources/FirstPersonHands/FirstPersonHand_R.prefab
```

The entire `Assets/RRFreelance` folder is ignored because it contains paid Asset Store content. The committed controller loads those prefabs with `Resources.Load`; if they are missing, it falls back to simple generated hands so the project still compiles and the demo remains runnable.

Current behavior:

- Only the local first-person shooter gets visible hands.
- Hands must enter from the first-person bottom edge; wrists should stay off-screen and the palm side should be the primary visible surface.
- `FirstPersonDiceHand` poses the imported finger bones on a sampled timeline. Imported Animator components are disabled so they cannot override the hand pose or wrist framing.
- Human opponents stay represented by mic/profile overlays.
- AI opponent bodies and Kling throw clips are later visual layers.
- Dice results remain server/local-authoritative; the hand animation never decides the roll.

## Realism Requirements

- Palm faces up toward the player, with the wrist at or below the bottom screen edge.
- Current prototype timing: enter/cup, release at 0.48 seconds, snap at 0.82 seconds, lower after 0.95 seconds, fully hidden by 1.28 seconds. Dice are attached to the palm surface until the exact release pose, then follow a physics replay.
- The finger snap MUST be visible after dice release and BEFORE the shooting hand lowers out of frame. The prototype uses constrained finger posing and a thumb/middle-finger contact/flick. This is not a motion-captured or artist-finished snap clip. The installed pack does not supply one. It must not delay or change the dice result.
- Initial release uses the three existing hand skin materials. The later expansion target remains six skin shades for men's hands and the same six for women's hands, twelve combinations. Use swatches spanning very light, light, medium, tan/brown, deep brown, and very deep brown, with natural undertone variation.
- The three initial material swatches are selectable beside the dice colors; the selection is saved locally. `StreetDiceDemoBuild.EnsureDemoScene` prepares their Resources copies when the paid pack is present. Paid materials remain inside the ignored pack directory.
- Each shade needs credible palm/back contrast, nail beds, creases, and surface detail under the actual scene lighting. A uniform color multiplication is not a finished skin variant.
- The installed pack provides three base skin materials over two albedo textures, plus palm/nail masks. No separate women's mesh or snap clip was found in the installed asset inventory. The twelve production variants still require asset authoring and visual review; do not represent them as delivered.
- Current dice use the owner-supplied macriciox model for regular rolls and the Geug model for full hot streaks. Procedural 3D meshes remain a missing-asset fallback. Retain the approved small on-screen size and verify detail in the magnified camera.

## Physics Presentation

- `DicePhysicsReplay` simulates beveled convex dice in an isolated Unity physics scene at 120 Hz, with gravity, angular velocity, friction, restitution, continuous collision detection, and sleep/settle checks.
- Simulation poses are prepared locally before display. A constant cube-symmetry rotation maps each model's numbered faces to the already-selected result, avoiding a last-frame face change. This does not replace backend result validation or supply multiplayer synchronization.
- Cocked or off-screen landings are rejected before display. Failed presentation attempts never choose a new game result; the controller falls back to showing the existing result rather than leaving input locked.
- The final pose remains fixed after settling. Skin/hot-state changes preserve the resting pose.
- The wrist anchor has a 10% viewport-height bottom margin; the visible hand crop still requires visual review for any new hand asset or camera.
- Bottom controls are hidden while rolling so they cannot cover the hand or interrupt the animation.
- The current snap and skin lighting remain a prototype, not the final lifelike animation. A filmed real underhand roll with a visible snap is the reference needed for an artist-authored or motion-captured production clip. No new Kling footage was generated for this motion pass.

## Verification

Run Unity in batch mode with `-executeMethod ThrowMotionCapture.Start` and without `-quit`; the capture enters play mode and exits when finished. It writes frames to `artifacts/unity-smoke/throw-motion` and runs `ThrowMotionVerification` across 180 combinations of faces, dice counts, seats, and aspect ratios. Checks cover release continuity, wrist-anchor margin, correct regular/hot landing faces, visible pavement placement, and stationary roll lock. The paid hand pack and imported dice prefabs must be present. Physical handset performance and production animation quality are separate approval gates; no APK is built by this command.

## Refinement Pass

- Preserves the approved camera, 0.09-unit dice size, wrist margin, release time, snap time, and exit time.
- Reduces excessive skin sheen and shader glow, and lowers the overlay lights without changing the photo environment. The three source skin materials and texture detail are retained.
- Enables soft key-light shadows and replaces hard rectangular pavement shadow stamps with feathered, height-fading contact shadows.
- Gives the fingers slightly different grip/release curves and adds relaxation after the snap. This remains a procedural motion study, not motion capture.
- Verification also checks that the middle finger moves during the snap and the hand is inactive after exit. `capture.json` records the exact frame count and frame rate so video encoding does not include stale frames from longer previous captures.
- The roll trigger remains the existing button. Gesture triggering is a separate design decision to discuss and test; this pass does not implement it or change game outcomes.
