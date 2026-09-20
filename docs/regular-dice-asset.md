# Regular Dice Asset

The owner's `dice (1).glb` by macriciox is the main regular dice model. The unchanged source is `Assets/Art/RegularDice/dice.glb`; attribution is in `CREDITS.md`.

Run `RegularDiceAssetBuild.PrepareAndInspect` to generate the centered, unit-size `MacricioxRegularDie.prefab`, its material, and white/black six-face inspection renders. The game keeps the approved 0.09-unit world scale and instances the die twice for craps or three times for Cee-lo.

White uses the original albedo. Black, green, and blue use a material tint with contrasting light pips while retaining the source normal, roughness, metallic, and occlusion maps. Recoloring uses the source albedo's light body/dark pip regions, so it does not replace the authored pip shapes with procedural dots.

Verified imported face layout: +Y=4, -Y=3, +Z=6, -Z=5, +X=1, -X=2. This differs from standard opposite faces summing to seven. A separate rotation mapping preserves the author's layout and shows the correct authoritative roll.

At full streak, regular visuals turn off and the Geug hot model turns on. Breaking the streak restores macriciox's model and the selected regular color. Procedural dice remain a fallback only if the imported prefab is unavailable. `HotDiceAssetBuild.VerifyAndCapture` checks both models' face mappings, appearance transitions, and regular color choices.
