param([switch]$Install)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$assets = Join-Path $root 'unity/StreetDiceGreybox/Assets/Resources/Fades'
$output = Join-Path $root 'artifacts/video-compatibility'
New-Item -ItemType Directory -Force -Path $output | Out-Null

function Read-Video($path) {
    $json = & ffprobe -v error -select_streams v:0 -show_streams -of json $path
    if ($LASTEXITCODE -ne 0) { throw "Cannot inspect $path" }
    return ($json | ConvertFrom-Json).streams[0]
}

$results = @()
foreach ($style in @('tap', 'plant', 'wave')) {
    $name = "catch-$style-kling.mp4"
    $source = Join-Path $assets $name
    $backup = Join-Path $output "original-$name"
    if (!(Test-Path -LiteralPath $backup)) { Copy-Item -LiteralPath $source -Destination $backup }
    $target = Join-Path $output $name
    $before = Read-Video $backup
    # The source omits color metadata. Tag conventional HD limited-range BT.709 without grading pixels.
    & ffmpeg -hide_banner -loglevel error -y -i $backup -map 0:v:0 -an -vf 'setparams=range=limited:color_primaries=bt709:color_trc=bt709:colorspace=bt709' -c:v libx264 -preset slow -crf 16 -profile:v baseline -pix_fmt yuv420p -bf 0 -fps_mode passthrough -color_range tv -colorspace bt709 -color_primaries bt709 -color_trc bt709 -x264-params 'colorprim=bt709:transfer=bt709:colormatrix=bt709' -movflags +faststart+write_colr $target
    if ($LASTEXITCODE -ne 0) { throw "Encoding failed: $style" }
    $after = Read-Video $target
    if ($before.width -ne $after.width -or $before.height -ne $after.height -or
        $before.nb_frames -ne $after.nb_frames -or
        [Math]::Abs([double]$before.duration - [double]$after.duration) -gt 0.001 -or
        $after.has_b_frames -ne 0 -or $after.color_primaries -ne 'bt709') {
        throw "Compatibility validation failed: $style"
    }
    $packetJson = & ffprobe -v error -select_streams v:0 -show_packets -show_entries packet=pts_time,dts_time -of json $target
    if ($LASTEXITCODE -ne 0) { throw "Cannot inspect packet timing: $style" }
    $previous = -1.0
    foreach ($packet in ($packetJson | ConvertFrom-Json).packets) {
        $pts = [double]$packet.pts_time
        if ($pts -le $previous -or [Math]::Abs($pts - [double]$packet.dts_time) -gt 0.000001) {
            throw "Non-monotonic or reordered packet: $style"
        }
        $previous = $pts
    }
    $results += [pscustomobject]@{ Style=$style; Frames=$after.nb_frames; Seconds=$after.duration; Profile=$after.profile; Color=$after.color_primaries; Output=$target }
}
# Only replace runtime copies once every generated clip has passed validation.
if ($Install) {
    foreach ($result in $results) { Copy-Item -LiteralPath $result.Output -Destination (Join-Path $assets "catch-$($result.Style)-kling.mp4") -Force }
}
$results | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output 'verification.json')
$results | Format-Table Style,Frames,Seconds,Profile,Color
