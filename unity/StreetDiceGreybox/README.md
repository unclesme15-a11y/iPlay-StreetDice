# Street Dice Greybox

This Unity project is the first test client for the iPlay Cee-lo & Craps prototype.

It uses a realistic Kling-generated bodega garage/service-door environment plate at runtime, with Unity reserved for dice, UI, audio placeholders, and server/local game state. The primitive street scene remains only as a fallback if the Kling plate asset is missing.

It creates the playable scene at runtime:

- first-person shooter camera over a physical street-ground rolling lane
- no human player bodies for the first playable demo
- non-shooter seats represented by mic/speaker markers
- pulsing mic indicators for table talk / active player feedback
- two Unity dice with pip geometry for Craps
- third pip die enabled in Cee-lo mode
- throw animation that tumbles into the lane and lands on the server/local result
- roll-lock moment when the dice settle
- compact top-right point / shot status
- optional tutorial mode for dice math, phase, active point group, side-bet count, and rule explanations
- player-adjacent Fade/Catch and point-group Side Bet buttons
- mode switch between Craps and Cee-lo
- Cee-lo banker/player local table flow plus server evaluator call at `/api/cee-lo/evaluate`
- tutorial-only deterministic test rolls for seven, point hit, grouped number, Cee-lo `4-5-6`, and Cee-lo `1-2-3`
- explicit roll states for fade window, rolling, locked, resolving, and shooter decision
- placeholder audio for dice roll, lock, fade, win, and loss events
- Kling bodega garage/service-door environment plate with no characters, wet asphalt, curb/sidewalk edge, and no tables
- streak meter with red/orange hot dice override at full streak
- standalone local Demo Table mode for APK testing without a running backend
- optional server mode for Create, Open Shot, Fade, Roll, Run Same, Double Up, and Voice Gate

For standalone demo testing, open the project in Unity, press Play, then tap **Demo Table**.

For server-authoritative testing, run the backend first:

```powershell
.\tools\verify-street-dice-local.ps1 -StartServer
```

For manual Unity server testing, start the backend separately on `http://localhost:5108`, open this project in Unity, press Play, then tap **Server**.

For non-interactive visual smoke testing:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe' -batchmode -quit -projectPath .\unity\StreetDiceGreybox -executeMethod StreetDiceDemoBuild.CaptureSmokeScreenshot -logFile .\unity\smoke-screenshot.log
```

The screenshot is written to `artifacts/unity-smoke/street-dice-demo-smoke.png`.

The smoke screenshot verifies the current art pipeline: a Kling-generated low ground-level wet asphalt bodega garage/service-door plate with no characters, with Unity dice overlaid. Human players stay as mic/profile overlays; computer-generated opponent bodies can be added later after the environment framing is right.

For the full no-APK validation gate:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\verify-street-dice-full.ps1
```

That command runs backend tests, starts/stops the local backend, probes `/health`, probes Cee-lo evaluation, compiles Unity, and refreshes the smoke screenshot.
