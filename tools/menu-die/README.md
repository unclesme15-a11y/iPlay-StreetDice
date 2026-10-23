# Menu die face art

The 3D menu die (`Assets/StreetDiceMenuDie.cs`) uses six face textures in
`Assets/Resources/Branding/MenuDie/`. They were made from the approved die render
(`Assets/Resources/Branding/menu-die-static.png`):

1. `unwarp_faces.py` -- straightens the CRAPS, CEE-LO and top (logo) faces out of the render.
   Writes raw faces; then the first half of the old build normalised them into `out/face-*.png`.
2. `build_faces.py <raw dir> <BarlowCondensed-SemiBold.ttf> <out dir>` -- builds every face on
   a generated worn-ivory surface: CRAPS / CEE-LO lettering lifted from the render, PROFILE /
   SETTINGS / TUTORIAL ON|OFF in rough brush lettering, the blue i-play logo for the bottom.
   Side lettering stays on the flat part of the face (the rounded edge is the outer ~13%);
   the top word stays in the middle 58% because it turns to stay readable as the die spins.
3. `preview_render.py <MenuDie dir> <screenshot> <menu-die-static.png> <out.png> <yaw> <pitch>`
   -- software render with the same shape, camera, lights and label rule as the Unity code,
   for checking the look without Unity. `docs/reference/menu-die-preview.png` came from it.

Needs Python with numpy, opencv-python-headless and pillow. Save the final faces as JPG
(quality 92) named `menu-die-<face>.jpg`.
