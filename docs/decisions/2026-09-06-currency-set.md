# Currency Set

- Main wager presets and side-bet presets: $1, $5, $10, $20.
- Main stakes are selected before Shoot and cannot change while committed or in a point.
- Side-bet selection only affects new bets. Existing bets retain their original amounts.
- Run Same and Double Up remain as previously agreed. A $40 stake uses two $20 bills, not a new denomination.
- Ground piles show each player's committed main and side-bet stakes, including the shooter's matching exposure. Before commitment, the main two stakes are staged. The main-options backdrop displays one of each bill as a prop preview, not a balance.
- $20: preserve the approved note design/material, replace the portrait with the supplied Tubman likeness, remove JACKSON and leave the nameplate blank.
- Original $20 archived under `artifacts/currency/original-approved`; the existing runtime $20 resource now contains the revised image.
- Matching $1 Washington, $5 Lincoln and $10 Hamilton notes retain the worn-paper style and fictional prop-money markings.
- No APK built. All amounts are offline demo play money. Public distribution still needs reference-rights review as recorded in CREDITS.md.

## Generation Record

Verification: Unity readiness run passed with four loaded bill textures, all four main/side wager amounts, live-wager edit guard, correct forfeit transfers and two $20 notes for a $40 Double Up. The 180 motion presentations and sky-camera pixel check passed. Backend tests: 37/37. Updated captures are under `artifacts/unity-smoke/readiness`. The editor still logs an unrelated UnityEditor.Search startup exception; the readiness process exits successfully. Physical Android-device testing remains pending; no APK was built.

Tool: built-in image generation, not Kling. All four assets are edits/variants of the approved iPlay prop-note material, not environment replacements.

$20 output: `exec-5ab63d13-26d5-48a5-93a5-b69139eee95b.png`

Exact prompt:

> EDIT image 1, the approved worn iPlay twenty-dollar PROP banknote. Image 2 is ONLY the portrait reference. Preserve image 1's exact full-note composition, landscape aspect ratio, paper texture, worn edges, engraved style, subdued colors, all denomination 20 numerals, twenty dollars wording, serials, seals, signatures, and especially the small 'iPlay PROP MONEY' and 'NOT LEGAL TENDER' markings. Replace the central Andrew Jackson portrait with the woman's portrait from image 2, faithfully retaining her facial likeness, headwrap and clothing, rendered as fine engraved banknote linework in the same size and position as the old portrait. Remove the word 'JACKSON' from the small lower name plaque, leaving that plaque unlettered. Do not transfer any other design, large USA text, captions or watermark from image 2. No other changes. Flat straight-down texture edge to edge, no background padding or perspective.

References: approved original $20 and user-supplied `Tubman20.jpg`.

The other three prompts use the following exact template, replacing the bracketed fields with the values in the table. Each references only the approved original $20.

> Create a matching [TITLE] fictional iPlay prop banknote texture using this approved twenty-dollar prop note as the design/material reference. Same exact wide landscape 2.35:1 proportions and straight orthographic edge-to-edge full-note framing, worn cotton paper, tiny creases, dense muted green/charcoal engraved linework and realistic paper detail. Change ALL denomination numerals to [NUMBER], change denomination words to [WORDS], use an engraved [PERSON] portrait in the central portrait position instead of Jackson, and a small [NAME] name plaque. The note must prominently retain the small text 'iPlay PROP MONEY' and 'NOT LEGAL TENDER' in the reference's left-lower area, with fictional serials. This is game prop currency, not a usable real note. No Jackson portrait/name, no remaining 20 numerals or TWENTY denomination words, no additional backdrop/padding, no tilt or cast shadows. Maintain the same aged realistic visual family as the reference.

| TITLE | NUMBER | WORDS | PERSON | NAME | Output |
| --- | --- | --- | --- | --- | --- |
| ONE DOLLAR | 1 | ONE DOLLAR | George Washington | WASHINGTON | exec-b6843e57-ac75-4cd1-9230-f17f1ff0b7c6.png |
| FIVE DOLLAR | 5 | FIVE DOLLARS | Abraham Lincoln | LINCOLN | exec-fb5bedcb-72ca-442f-a524-a150b902a490.png |
| TEN DOLLAR | 10 | TEN DOLLARS | Alexander Hamilton | HAMILTON | exec-dbc9035c-7ee9-492f-80c6-1fb057f1549b.png |
