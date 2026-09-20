param(
    [string]$BaseUrl = "http://localhost:5108",
    [switch]$StartServer,
    [string]$ProjectPath = "$PSScriptRoot\..\server\src\IPlayStreetDice.Server\IPlayStreetDice.Server.csproj"
)

$ErrorActionPreference = "Stop"

$serverProcess = $null
if ($StartServer) {
    $dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
    if (-not (Test-Path $dotnet)) {
        $dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
        if (-not (Test-Path $dotnet)) { $dotnet = "dotnet" }
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
    $bots = Invoke-JsonPost "/api/street-dice/$gameId/bots/fill" @{ targetPlayers = 5 }

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

    $offered = Invoke-JsonPost "/api/street-dice/$gameId/wager/offer" @{
        fromId = "p3"
        playerSessionToken = $p3.playerSessionToken
        toId = "p1"
        outcome = "Crap"
        number = 0
        amount = 5
    }
    $accepted = Invoke-JsonPost "/api/street-dice/$gameId/wager/accept" @{
        recipientId = "p1"
        playerSessionToken = $p1.playerSessionToken
        offerId = $offered.offer.id
    }
    if ($accepted.offer.status -ne "Accepted") {
        throw "Shooter did not accept the paired CRAP offer."
    }
    $window = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/street-dice/$gameId"
    Start-Sleep -Milliseconds ([int][Math]::Ceiling($window.bettingWindow.shooterRemainingMilliseconds) + 100)

    $first = Invoke-JsonPost "/api/street-dice/$gameId/roll/prepare" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        power = 0.5
        aim = 0
        leftHanded = $false
    }
    $pending = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/street-dice/$gameId"
    if ($pending.pendingRoll.rollId -ne $first.rollId -or $pending.pendingRoll.remainingFadeMilliseconds -le 0) {
        throw "Catcher could not see the pending physical roll and fade time."
    }
    if ($pending.pendingRoll.PSObject.Properties.Name -contains "faces") {
        throw "Pending roll leaked die faces before the catcher could fade."
    }

    $fade = Invoke-JsonPost "/api/street-dice/$gameId/roll/fade" @{
        catcherId = "p2"
        playerSessionToken = $p2.playerSessionToken
        rollId = $first.rollId
    }
    if ($fade.result.result -ne "Faded") {
        throw "Fade/Catch did not nullify the roll."
    }
    $afterFade = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/street-dice/$gameId"
    if ($afterFade.pendingRoll -or $afterFade.state.shooterId -ne "p1" -or
        $afterFade.wagers[0].status -ne "Accepted") {
        throw "Fade/Catch did not clear the roll and keep the shooter."
    }

    $prepared = Invoke-JsonPost "/api/street-dice/$gameId/roll/prepare" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        power = 0.6
        aim = -0.2
        leftHanded = $false
    }
    Start-Sleep -Milliseconds ([int][Math]::Ceiling($prepared.remainingFadeMilliseconds) + 100)
    $committed = Invoke-JsonPost "/api/street-dice/$gameId/roll/commit" @{
        shooterId = "p1"
        playerSessionToken = $p1.playerSessionToken
        rollId = $prepared.rollId
    }
    if ($committed.faces.Count -ne 2 -or $committed.frames.Count -lt 2) {
        throw "Committed physical roll has no two-face result and pose replay."
    }
    if ($committed.result.roll -and ($committed.result.roll.die1 -ne $committed.faces[0] -or
        $committed.result.roll.die2 -ne $committed.faces[1])) {
        throw "Settled wager and visible die faces disagreed."
    }
    $afterCommit = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/street-dice/$gameId"
    if ($afterCommit.pendingRoll) {
        throw "Committed roll remained pending."
    }
    if (($afterCommit.state.players | Where-Object { $_.PSObject.Properties.Name -contains "balance" }).Count -ne 0) {
        throw "Table state exposed private player balances."
    }
    $walletSessions = @(
        @{ playerId = "p1"; playerSessionToken = $p1.playerSessionToken },
        @{ playerId = "p2"; playerSessionToken = $p2.playerSessionToken },
        @{ playerId = "p3"; playerSessionToken = $p3.playerSessionToken }
    ) + @($bots.bots | ForEach-Object {
        @{ playerId = $_.playerId; playerSessionToken = $_.playerSessionToken }
    })
    $total = 0
    foreach ($session in $walletSessions) {
        $wallet = Invoke-JsonPost "/api/street-dice/$gameId/wallet" $session
        if ($wallet.playerId -ne $session.playerId) { throw "Wallet identity mismatch." }
        $total += $wallet.balance
    }
    $foreignWallet = Invoke-JsonPostStatus "/api/street-dice/$gameId/wallet" @{
        playerId = "p2"; playerSessionToken = $p1.playerSessionToken
    }
    if ($foreignWallet.StatusCode -ne 401) { throw "Another player's wallet was readable." }
    if ($total -ne 5000) {
        throw "Play-money balances were not conserved."
    }
    $oldRoll = Invoke-JsonPostStatus "/api/street-dice/$gameId/roll" @{ shooterId = "p1"; die1 = 6; die2 = 6 }
    $oldFade = Invoke-JsonPostStatus "/api/street-dice/$gameId/fade" @{ catcherId = "p2" }
    $oldBot = Invoke-JsonPostStatus "/api/street-dice/$gameId/bots/advance" @{}
    $oldSideBet = Invoke-JsonPostStatus "/api/street-dice/$gameId/side-bet" @{
        playerId = "p3"; playerSessionToken = $p3.playerSessionToken
        type = "MissPointGroup"; amount = 20; targetPointNumber = 4
    }
    if ($oldRoll.StatusCode -ne 410 -or $oldFade.StatusCode -ne 410 -or
        $oldBot.StatusCode -ne 410 -or $oldSideBet.StatusCode -ne 410) {
        throw "A retired roll, fade, bot or side-bet bypass is still callable."
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
        finalShooter = $afterCommit.state.shooterId
        finalCatcher = $afterCommit.state.catcherId
        streak = $afterCommit.state.streak
        committedFaces = ($committed.faces -join ",")
        replayFrames = $committed.frames.Count
        voiceGateStatus = $voice.StatusCode
    }
}
finally {
    if ($serverProcess -and -not $serverProcess.HasExited) {
        Stop-Process -Id $serverProcess.Id -Force
    }
}
