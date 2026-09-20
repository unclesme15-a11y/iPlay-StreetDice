$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$dotnet = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe'
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$url = "http://127.0.0.1:$port"
$serverDll = Join-Path $repo 'server\src\IPlayStreetDice.Server\bin\Debug\net8.0\IPlayStreetDice.Server.dll'
$server = Start-Process -FilePath $dotnet -ArgumentList @($serverDll, '--urls', $url) -WindowStyle Hidden -PassThru
function PostJson($Path, $Body) {
    Invoke-RestMethod -Method Post -Uri "$url$Path" -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Compress)
}
try {
    $ready = $false
    for ($i = 0; $i -lt 40; $i++) {
        try { $null = Invoke-RestMethod "$url/health"; $ready = $true; break } catch { Start-Sleep -Milliseconds 250 }
    }
    if (!$ready) { throw 'Server did not start.' }
    $game = PostJson '/api/street-dice/create' @{}
    $base = "/api/street-dice/$($game.gameId)"
    $sessions = @{}
    foreach ($id in @('p1', 'p2', 'p3')) {
        $joined = PostJson "$base/join" @{playerId=$id; playerName=$id}
        $sessions[$id] = $joined.playerSessionToken
    }
    $passed = PostJson "$base/pass" @{playerId='p1'; playerSessionToken=$sessions.p1}
    if ($passed.state.shooterId -ne 'p2') { throw 'Pass failed.' }
    $null = PostJson "$base/shot" @{shooterId='p2'; shooterSessionToken=$sessions.p2; catcherId='p1'; amount=20}
    $null = PostJson "$base/roll" @{shooterId='p2'; playerSessionToken=$sessions.p2; die1=5; die2=5}
    $null = PostJson "$base/side-bet" @{playerId='p3'; playerSessionToken=$sessions.p3; type='MissPointGroup'; amount=10; targetPointNumber=4}
    try {
        $null = PostJson "$base/pass" @{playerId='p2'; playerSessionToken=$sessions.p2}
        throw 'An active point was passed.'
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -ne 400) { throw }
    }
    $left = PostJson "$base/leave" @{playerId='p2'; playerSessionToken=$sessions.p2}
    if ($left.state.lastResolution.result -ne 'ShooterForfeitLoss') { throw 'Forfeit result missing.' }
    $shooter = $left.state.players | Where-Object id -eq 'p2'
    if ($shooter.balance -ne 970 -or !$shooter.hasLeft) { throw 'Forfeit balance incorrect.' }
    $again = PostJson "$base/leave" @{playerId='p2'; playerSessionToken=$sessions.p2}
    if (($again.state.players | Where-Object id -eq 'p2').balance -ne 970) { throw 'Repeated leave charged twice.' }
    Write-Output 'HTTP verification passed: authenticated pass, active-point rejection, grouped-bet forfeit, and repeated-leave protection.'
} finally {
    if (!$server.HasExited) { Stop-Process -Id $server.Id -Force; $server.WaitForExit() }
}
