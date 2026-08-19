# Street Dice Greybox

This Unity project is the first test client for the Street Dice prototype.

It creates the greybox scene at runtime:

- top-down angled camera over a street dice rolling area
- five player seat markers
- two Unity dice cubes
- magnifier-style result HUD
- streak meter with red/orange hot dice override at full streak
- buttons for Create, Join, Fill Bots, Open Shot, Fade, Roll, Run Same, Double Up, and Voice Gate

Run the backend first:

```powershell
.\tools\verify-street-dice-local.ps1 -StartServer
```

For manual Unity play testing, start the backend separately on `http://localhost:5108`, open this project in Unity, and press Play.
