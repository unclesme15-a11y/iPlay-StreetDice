param(
    [string]$BaseUrl = "http://localhost:5108",
    [switch]$StartServer,
    [string]$ProjectPath = "$PSScriptRoot\..\server\src\IPlayStreetDice.Server\IPlayStreetDice.Server.csproj"
)

$ErrorActionPreference = "Stop"

$serverProcess = $null
if ($StartServer) {
    $dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
    if (-not (Test-Path $dotnet)) {
        $dotnet = "dotnet"
    }

    $serverProcess = Start-Process `
        -FilePath $dotnet `
        -ArgumentList @("run", "--project", (Resolve-Path $ProjectPath), "--urls", $BaseUrl) `
        -PassThru `
        -WindowStyle Hidden

    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        try {
            Invoke-RestMethod -Method Get -Uri "$BaseUrl/health" | Out-Null
            $ready = $true
            break
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    if (-not $ready) {
        throw "Street Dice server did not become ready at $BaseUrl."
    }
}

function Invoke-JsonPost {
    param(
        [string]$Path,
        [object]$Body
    )

    Invoke-RestMethod `
        -Method Post `
        -Uri "$BaseUrl$Path" `
        -ContentType "application/json" `
        -Body ($Body | ConvertTo-Json -Depth 10)
}

function Invoke-JsonPostStatus {
    param(
        [string]$Path,
        [object]$Body
    )

    try {
        $response = Invoke-WebRequest `
            -Method Post `
            -Uri "$BaseUrl$Path" `
            -ContentType "application/json" `
            -Body ($Body | ConvertTo-Json -Depth 10)
        return @{ StatusCode = [int]$response.StatusCode; Content = $response.Content }
    }
    catch {
        if ($_.Exception.Response) {
            return @{ StatusCode = [int]$_.Exception.Response.StatusCode; Content = "" }
        }
        throw
    }
}

try {
    $health = Invoke-RestMethod -Method Get -Uri "$BaseUrl/health"
    if ($health.status -ne "ok") {
        throw "Health check did not return ok."
    }

    $created = Invoke-JsonPost "/api/street-dice/create" @{}
    $gameId = $created.gameId

    $p1 = Invoke-JsonPost "/api/street-dice/$gameId/join" @{ playerName = "Shooter"; playerId = "p1" }
    $p2 = Invoke-JsonPost "/api/street-dice/$gameId/join" @{ playerName = "Catcher"; playerId = "p2" }
    $p3 = Invoke-JsonPost "/api/street-dice/$gameId/join" @{ playerName = "Side Bettor"; playerId = "p3" }
    Invoke-JsonPost "/api/street-dice/$gameId/bots/fill" @{ targetPlayers = 5 } | Out-Null

    Invoke-JsonPost "/api/street-dice/$gameId/dice-color" @{
        playerId = "p1"
        playerSessionToken = $p1.playerSessionToken
        color = "Green"
    } | Out-Null

    Invoke-JsonPost "/api/street-dice/$gameId/shot" @{
        shooterId = "p1"
        shooterSessionToken = $p1.playerSessionToken
        catcherId = "p2"
        amount = 20
    } | Out-Null

    Invoke-JsonPost "/api/street-dice/$gameId/side-bet" @{
        playerId = "p3"
        playerSessionToken = $p3.playerSessionToken
        type = "ComeOutWin"
        amount = 5
    } | Out-Null

    $fade = Invoke-JsonPost "/api/street-dice/$gameId/fade" @{
        catcherId = "p2"
        playerSessionToken = $p2.playerSessionToken
    }
    if ($fade.result.result -ne "Faded") {
        throw "Fade/Catch did not nullify the roll."
    }

    $point = Invoke-JsonPost "/api/street-dice/$gameId/roll" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        die1 = 3
        die2 = 2
    }
    if ($point.result.result -ne "PointEstablished" -or $point.state.point -ne 5) {
        throw "Point was not established at 5."
    }

    Invoke-JsonPost "/api/street-dice/$gameId/side-bet" @{
        playerId = "p3"
        playerSessionToken = $p3.playerSessionToken
        type = "HitPoint"
        amount = 10
    } | Out-Null

    $hit = Invoke-JsonPost "/api/street-dice/$gameId/roll" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        die1 = 4
        die2 = 1
    }
    if ($hit.result.result -ne "ShooterPointWin") {
        throw "Point hit did not resolve as a shooter win."
    }

    Invoke-JsonPost "/api/street-dice/$gameId/decision/run-same" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
    } | Out-Null

    Invoke-JsonPost "/api/street-dice/$gameId/roll" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        die1 = 2
        die2 = 2
    } | Out-Null

    $sevenOut = Invoke-JsonPost "/api/street-dice/$gameId/roll" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        die1 = 3
        die2 = 4
    }
    if ($sevenOut.result.result -ne "ShooterSevenOutLoss" -or $sevenOut.state.shooterId -ne "p2") {
        throw "Seven-out did not hand dice to the Catcher."
    }

    $voice = Invoke-JsonPostStatus "/api/street-dice/$gameId/voice/access-token" @{
        playerId = "p1"
        playerSessionToken = $p1.playerSessionToken
    }
    if ($voice.StatusCode -ne 200 -and $voice.StatusCode -ne 501) {
        throw "Voice gate returned unexpected status $($voice.StatusCode)."
    }

    [pscustomobject]@{
        ok = $true
        gameId = $gameId
        finalShooter = $sevenOut.state.shooterId
        finalCatcher = $sevenOut.state.catcherId
        streak = $sevenOut.state.streak
        voiceGateStatus = $voice.StatusCode
    }
}
finally {
    if ($serverProcess -and -not $serverProcess.HasExited) {
        Stop-Process -Id $serverProcess.Id -Force
    }
}
