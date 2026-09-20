# Sky Display Lifecycle

- The sky display is hidden while dice are in the hand.
- It appears at the release frame and follows the same live Unity dice through their roll and lock.
- The display surface uses `Assets/Resources/Environments/bodega-pavement-exact-crop.png`, a literal square crop from the approved bodega environment. No generated or substitute asphalt is active in the display.
- A dark flatscreen-style casing with a narrow metallic edge frames the display.
- After lock, the rolled number starts at the dice on the main ground view, moves toward the player, scales slightly, and fades over 0.8 seconds. It never appears inside the overhead display.
- At the exact end of that transition, the display hides and `ResetDiceToShooter` removes the settled dice from the ground and returns them to the active player's start position.
- Cee-lo currently shows the three individual values in the same transition rather than a summed craps total.
- A fade/catch visual should use a live Unity hand, not a generated video, so the catching player's configured hand shade can enter from their seat and stop the actual dice. That hand interception is the next animation asset/rig pass; it is not represented by a prerecorded clip.
- No APK built.

The earlier generated pavement experiment is archived outside Unity resources and is not shipped.
