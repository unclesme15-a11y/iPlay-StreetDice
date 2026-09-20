# Money Settlement Animation

- Each denomination is shown as one complete physical bill. Repeated denominations remain separate, so a $40 Double Up visibly places two $20 bills and never substitutes an `xN` marker.
- Settlement uses the same route in both directions: the losing player's ground position to the winning player's ground position. This makes local wins and losses spatially consistent.
- Bills first lift from the loser, present their full face while crossing the pavement, then settle at the winner. Multiple denominations travel together.
- A short paper peel sound plays on pickup and a softer paper landing sound plays on arrival. These are generated runtime clips and obey the Dice & Impact volume option.
- Balance and game-rule settlement remains immediate and authoritative. The animation represents the already-decided transfer and cannot affect outcomes.
- The metal roll-up door remains the approved impact target at width 2.9 world units (`x = -1.45` through `+1.45`). Metal impact volume follows normalized collision strength.
- Brick and side-boundary collisions no longer play an impact sound. On Android they trigger the existing coarse device vibration path with a short debounce; editor and desktop builds remain silent for those collisions.
- Pavement dice sounds remain active.
- Bill textures use stronger anisotropic filtering, a sharper mip bias, slightly brighter paper material, and a larger ground mesh so the portrait and denomination read more clearly at the first-person camera angle.
- No APK built.
