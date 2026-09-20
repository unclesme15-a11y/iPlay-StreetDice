# Hot-Streak Dice Asset

The owner-supplied `dice.glb` by Geug is preserved in `Assets/Art/HotDice`.
Attribution is in the repository's `CREDITS.md` and must ship in the final game credits.

Unity glTFast 6.20.0 imports the source. Run `HotDiceAssetBuild.PrepareAndInspect` to rebuild the normalized prefab, gameplay material, and six-face inspection image. The material retains the source texture maps but uses opaque depth rendering so the back-face pips do not confuse the visible roll.

The prefab replaces macriciox's regular dice only at the existing full-streak threshold. Regular dice and the player's selected color return when the streak ends. All three Cee-lo dice use the same switch. The size remains the approved 0.09-unit world size. Source faces are +Y=6, -Y=1, +Z=2, -Z=4, +X=5, -X=3, so hot dice have their own result-to-orientation mapping.

The source layout differs from standard opposite faces summing to seven. We preserve its authored geometry and explicitly map all six results rather than assuming the procedural die's layout.

The current HUD magnifier is still a symbolic readout; it does not yet show a live camera rendering of this model.
