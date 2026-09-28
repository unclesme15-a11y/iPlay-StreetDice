# Fifty and Hundred Dollar Prop Notes

- New denominations for the Level 3 prestige-bill unlock (see RankLadder.PrestigeBillUnlockLevel):
  $50 and $100, joining the existing $1, $5, $10 and $20 set.
- Follow the exact established template from `2026-09-06-currency-set.md`: same worn
  iPlay PROP MONEY / NOT LEGAL TENDER prop-note material, same "regular" real-president
  portrait treatment already used on $5 (Lincoln) and $10 (Hamilton) -- not the custom
  portraits reserved for $1 and $20 (Tubman), per instruction.
- $50: Ulysses S. Grant portrait, GRANT name plaque.
- $100: Benjamin Franklin portrait, FRANKLIN name plaque. Franklin was never a president --
  matching the *real* $100 bill here, same as the reference $1/$5/$10/$20 set matches the
  real bill each denomination is based on.
- No image-generation tool is available in this session, so the two PNGs below are
  **prompts only** -- not yet generated. Whoever has image-gen access (this project has
  been using Codex/built-in image generation for the rest of this set) runs these and
  drops the outputs into `Assets/Resources/Money/iplay-note-50.png` and `iplay-note-100.png`,
  same as the existing files in that folder.

## Prompts

Both use the currency-set.md template verbatim, referencing the approved original $20 as
the design/material source (same as the $1/$5/$10 generation did):

> Create a matching FIFTY DOLLAR fictional iPlay prop banknote texture using this approved
> twenty-dollar prop note as the design/material reference. Same exact wide landscape 2.35:1
> proportions and straight orthographic edge-to-edge full-note framing, worn cotton paper,
> tiny creases, dense muted green/charcoal engraved linework and realistic paper detail.
> Change ALL denomination numerals to 50, change denomination words to FIFTY DOLLARS, use an
> engraved Ulysses S. Grant portrait in the central portrait position instead of Jackson, and
> a small GRANT name plaque. The note must prominently retain the small text 'iPlay PROP
> MONEY' and 'NOT LEGAL TENDER' in the reference's left-lower area, with fictional serials.
> This is game prop currency, not a usable real note. No Jackson portrait/name, no remaining
> 20 numerals or TWENTY denomination words, no additional backdrop/padding, no tilt or cast
> shadows. Maintain the same aged realistic visual family as the reference.

> Create a matching HUNDRED DOLLAR fictional iPlay prop banknote texture using this approved
> twenty-dollar prop note as the design/material reference. Same exact wide landscape 2.35:1
> proportions and straight orthographic edge-to-edge full-note framing, worn cotton paper,
> tiny creases, dense muted green/charcoal engraved linework and realistic paper detail.
> Change ALL denomination numerals to 100, change denomination words to ONE HUNDRED DOLLARS,
> use an engraved Benjamin Franklin portrait in the central portrait position instead of
> Jackson, and a small FRANKLIN name plaque. The note must prominently retain the small text
> 'iPlay PROP MONEY' and 'NOT LEGAL TENDER' in the reference's left-lower area, with
> fictional serials. This is game prop currency, not a usable real note. No Jackson
> portrait/name, no remaining 20 numerals or TWENTY denomination words, no additional
> backdrop/padding, no tilt or cast shadows. Maintain the same aged realistic visual family
> as the reference.

## Still open

- (Resolved 2026-09-28) A player below Level 3 wins a trophy note by beating a Level 3+
  player in a shot of $50 or more -- see `PlayerAccount.TrophyNotes` and the rank doc.
- No APK built. All amounts are offline demo play money, same standing note as the rest of
  this currency set.
