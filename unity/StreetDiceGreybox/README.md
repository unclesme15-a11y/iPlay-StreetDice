# Street Dice Greybox

This Unity project is the first test client for the iPlay Cee-lo & Craps prototype.

It creates the greybox scene at runtime:

- first-person shooter camera over a physical street-ground rolling lane
- no avatar bodies for the first playable demo
- non-shooter seats represented by mic/speaker markers
- pulsing mic indicators for table talk / active player feedback
- two Unity dice with pip geometry
- throw animation that tumbles into the lane and lands on the server/local result
- roll-lock moment when the dice settle
- compact top-right point / shot status
- optional tutorial mode for dice math and phase details
- player-adjacent Fade/Catch and Side Bet overlays
- streak meter with red/orange hot dice override at full streak
- standalone local Demo Table mode for APK testing without a running backend
- optional server mode for Create, Open Shot, Fade, Roll, Run Same, Double Up, and Voice Gate

For standalone demo testing, open the project in Unity, press Play, then tap **Demo Table**.

For server-authoritative testing, run the backend first:

```powershell
.\tools\verify-street-dice-local.ps1 -StartServer
```

For manual Unity server testing, start the backend separately on `http://localhost:5108`, open this project in Unity, press Play, then tap **Server**.
