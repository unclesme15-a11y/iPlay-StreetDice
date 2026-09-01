# Street Dice Greybox

This Unity project is the first test client for the iPlay Cee-lo & Craps prototype.

It creates the greybox scene at runtime:

- first-person shooter camera over a physical street-ground rolling lane
- no avatar bodies for the first playable demo
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
- Cee-lo local evaluator plus server evaluator call at `/api/cee-lo/evaluate`
- tighter bodega/street greybox with closed service door, curb/asphalt edge, ground markings, and no tables
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
