# Dice pregame visual direction

User direction: use the graffiti lettering and visual treatment of the iPlay pregame screens in Google Drive, not their playing-card theme. Hand color means natural white/light, medium brown and black/dark skin. Green, blue and other stylized skin colors are not acceptable.

## Reference discovery

Drive folder: https://drive.google.com/drive/folders/12o_veIGYvrM-B0JnI2YkQWoB1HaNc3As

- APPROVED-STYLE-shared-settings-v1.png: 1WGkgE-4k2yWO3sxuJhl4DOpKxU-65Tto, image/png, 2208006 bytes.
- APPROVED-STYLE-spades-title-menu-v1.png: 1AYNs6q3Ykv54Gj03tMEz8jgb1JTtstP_, image/png, 2677521 bytes.
- Inspected the matching local pregame source images in 02_iPlay/artifacts/pre-game-screen-redesign. Local sizes match Drive metadata. The connector did not expose a checksum for byte-for-byte verification.

## Concept

Saved image: artifacts/ui/dice-pregame-settings-graffiti-concept-v1.png.

Generated with the built-in image generation tool, not Kling and not a runtime capture. This concept is not yet wired into Unity and is not user-approved. Existing gameplay ground and fade motions were not changed.

Prompt direction: landscape pregame settings; reference the existing graffiti/brush lettering, distressed black materials, restrained gold edges and cyan iPlay accents; use pip-dice details instead of playing cards or suits. Show three realistically textured hand display models under neutral lighting, with natural light, medium-brown and deep-brown skin, never colored rim lighting. Include Hand Color, Tap/Plant/Wave, four regular dice colors, SFX, Tutorial and Save. Preserve minimal text.

Correction prompt: remove the generated explanatory text under Hand Color, Fade Style and Dice Color; preserve all other artwork and controls. In particular, fading must never be described as throwing the dice.

Next implementation must use functional controls and separately rendered assets, not flatten this whole concept into a clickable screenshot. The artwork here does not replace the actual runtime hand asset or certify its skin appearance.

## Runtime preparation

### Current verified refinement

- The slider-contrast and default-toggle limitations below are now superseded: pregame has a cyan filled volume track, larger gold-edged thumb, and a large outlined tutorial checkbox with a clear checkmark and matching lettering. Interaction remains Unity slider/toggle logic, not a flattened image.
- `unity/readiness-pregame-styled-controls.log` passes the full suite and control pointer/persistence checks at all three phone sizes. Inspected actual desktop checked-state and 1280x720 captures: checkmark renders and controls fit without overlap. Broader visual treatment and hand realism remain unfinished.

- Pregame now includes working sound volume and tutorial settings, sharing saved values with the in-game drawer. Tutorial loads from preferences; volume is clamped. Centered the title and relocated Credits to a separate footer.
- `unity/readiness-pregame-controls.log` passes full readiness plus pointer/persistence/bounds tests for these controls at all three phone sizes. The 2400x1080 capture fits without overlap. Slider contrast and default tutorial-toggle styling still need refinement; this is not finished visual polish.

- Replaced the four flat dice swatches with cached 256x256 previews of `Dice/MacricioxRegularDie`, using gameplay's `ApplyRegularDieColor` and an isolated neutral preview light. White/black/green/blue remain selectable, with a cyan selection underline and unchanged saved preferences. The same one-shot studio now captures hands and dice; render targets are released on controller destruction.
- `unity/readiness-dice-picker-pixels.log` passes all readiness checks and new per-preview pixel checks for nonblank framing, distinct white/black brightness and green/blue channel dominance, plus cache reuse. The checks and actual pointer selections run at each of the three phone resolutions. Inspected the refreshed 1280x720 menu capture: four pip-dice previews are visible and remain clear of adjacent rows.
- Actual dice thumbnails are now implemented; the older limitation below about color rectangles is superseded. This does not certify final dice lighting or menu realism. The small heading alignment, broad visual treatment and hand realism remain refinement work. No APK built.

- Expanded the live menu into wider rows and enlarged hand selection targets, with White/Brown/Black labels. Moved the divider below those labels.
- Preview camera now shows the extended fingers from above instead of foreshortening them. Cached renders are 512x512. Reduced smoothness and disabled environment reflections in the preview material; the dark textured albedo receives a moderate warm correction instead of the much darker UI swatch tint. These changes affect selection previews, not gameplay hands or Kling footage.
- `unity/readiness-pregame-phones.log` passes the full existing suite plus real OnGUI pointer selection/persistence for every hand shade, dice color and fade style at 1280x720, 2340x1080 and 2400x1080. Captures are `artifacts/unity-smoke/readiness/phone-<width>x<height>-pregame.png`. Inspected 1280x720 and 2400x1080: labels and controls fit without overlap.
- Backend test rerun: 54 passed. No APK built; these are Editor layout checks, not physical touch/device validation.
- Still unfinished: reference-quality lettering/materials, actual dice thumbnails instead of color rectangles, and hand realism matching the concept. Current hand previews are visibly synthetic. The readiness log also reports pre-existing Editor indexing and fade-video timestamp/color-primary warnings; it is not a clean-warning certification.

- Generated a separate clean background asset with the built-in image tool, using the settings concept as reference: dark charcoal street texture, understated corner pip-dice motifs, no words, hands, panels or controls. Saved in `Assets/Resources/UI/pregame-dice-background.png`.
- Pregame screens now draw that full-screen background with separate live controls, and no longer use the floating black panel over the gameplay alley. Subtle cyan/gold separator lines establish section boundaries. Gameplay environment is unchanged.
- `unity/readiness-pregame-background.log` passes the existing readiness suite. Inspected the actual menu capture: the background renders and controls remain legible. The narrow control arrangement, standard button materials and hand-model realism are still unfinished relative to the concept.

- Added Permanent Marker by Font Diner from Google Fonts' official repository, with its Apache 2.0 license retained. The font is applied through an isolated pregame GUISkin; gameplay labels and credits retain the original skin. This is a marker-style approximation, not the exact brush lettering embedded in the reference artwork.
- `unity/readiness-pregame-type.log` passes existing selection and full readiness checks. Inspected `main-options.png`: text renders and fits its existing controls. Background, row layout, dice accents and hand presentation still need the substantive visual redesign.

- Renamed the existing selector to Hand Color. Preview capture now isolates scene lights and uses a neutral white display light and ambient fill, restoring scene lighting after capture. Preview tints use the existing flesh-tone palette rather than the imported material tint.
- `unity/readiness-natural-hand-color.log` passes the existing readiness suite. Inspected the runtime capture: no cyan placeholder; three shaded models are visible. They still do not match the brighter, more realistic concept hands, so this is not final visual completion.
- The full graffiti/dice pregame redesign remains unimplemented. Do not present the concept as an implemented screen.
