param(
    [string]$Url = "http://localhost:5108",
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe"
)

$ErrorActionPreference = "Stop"

$repo = Split-Path -Parent $PSScriptRoot
$dotnet = Join-Path $env:LOCALAPPDATA "Microsoft\dotnet\dotnet.exe"
if (!(Test-Path $dotnet)) {
    $dotnet = "dotnet"
}

Push-Location $repo
try {
    & $dotnet test .\IPlayStreetDice.sln --nologo

    $serverProject = Join-Path $repo "server\src\IPlayStreetDice.Server"
    $server = Start-Process -FilePath $dotnet -ArgumentList @(
        "run",
        "--project",
        $serverProject,
        "--urls",
        $Url
    ) -PassThru -WindowStyle Hidden

    try {
        $healthy = $false
        for ($i = 0; $i -lt 30; $i++) {
            Start-Sleep -Milliseconds 500
            try {
                $health = Invoke-RestMethod -Uri "$Url/health" -TimeoutSec 2
                if ($health.status -eq "ok") {
                    $healthy = $true
                    break
                }
            } catch {
                if ($server.HasExited) { throw }
            }
        }

        if (!$healthy) {
            throw "Street Dice backend did not become healthy at $Url."
        }

        $ceeLoWin = Invoke-RestMethod -Method Post -Uri "$Url/api/cee-lo/evaluate" -ContentType "application/json" -Body '{"die1":4,"die2":5,"die3":6}'
        if ($ceeLoWin.result.outcome -ne "AutomaticWin") {
            throw "Cee-lo 4-5-6 probe did not return AutomaticWin."
        }

        $ceeLoPoint = Invoke-RestMethod -Method Post -Uri "$Url/api/cee-lo/evaluate" -ContentType "application/json" -Body '{"die1":2,"die2":2,"die3":4}'
        if ($ceeLoPoint.result.outcome -ne "Point" -or $ceeLoPoint.result.point -ne 4) {
            throw "Cee-lo pair-point probe did not return point 4."
        }
    } finally {
        if ($server -and !$server.HasExited) {
            Stop-Process -Id $server.Id -Force
            $server.WaitForExit()
        }
    }

    if (!(Test-Path $UnityPath)) {
        throw "Unity executable not found at $UnityPath."
    }

    & $UnityPath -batchmode -quit -projectPath .\unity\StreetDiceGreybox -logFile .\unity\compile.log
    $compileProblems = Select-String -Path .\unity\compile.log -Pattern "warning CS|error CS|Exception|Build failed|Compilation failed|Scripts have compiler errors"
    if ($compileProblems) {
        $compileProblems | ForEach-Object { Write-Host $_.Line }
        throw "Unity compile log contains C# warnings/errors or build failures."
    }

    & $UnityPath -batchmode -quit -projectPath .\unity\StreetDiceGreybox -executeMethod StreetDiceDemoBuild.CaptureSmokeScreenshot -logFile .\unity\smoke-screenshot.log
    $smokeProblems = Select-String -Path .\unity\smoke-screenshot.log -Pattern "error CS|Exception|Build failed|Compilation failed|Scripts have compiler errors"
    if ($smokeProblems) {
        $smokeProblems | ForEach-Object { Write-Host $_.Line }
        throw "Unity smoke screenshot log contains failures."
    }

    $screenshot = Join-Path $repo "artifacts\unity-smoke\street-dice-demo-smoke.png"
    if (!(Test-Path $screenshot)) {
        throw "Unity smoke screenshot was not created."
    }

    Write-Host "Street Dice full validation passed."
    Write-Host "Smoke screenshot: $screenshot"
} finally {
    Pop-Location
}
