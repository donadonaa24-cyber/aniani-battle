param([switch]$Rebuild)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AnimationSmoothing.cs'),(Join-Path $PSScriptRoot 'SpriteForge.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$json = New-Object System.Web.Script.Serialization.JavaScriptSerializer
$json.MaxJsonLength = 16777216
$report = New-Object System.Collections.Generic.List[object]
$repro = New-Object System.Collections.Generic.List[object]
if ($Rebuild) {
    $paths = New-Object System.Collections.Generic.List[string]
    foreach ($character in 'toma','ren','chizuru') {
        Get-ChildItem -LiteralPath (Join-Path $taskRootPath "AniBato/Assets/_Project/Art/Sprites/$character") -File |
            Where-Object { $_.Extension -in '.png','.json' } | ForEach-Object { $paths.Add($_.FullName) }
        Get-ChildItem -LiteralPath (Join-Path $taskRootPath 'docs/art_wip') -File |
            Where-Object { $_.Name -like "$($character)_contact*.png" -or $_.Name -like "$($character)_*_preview.gif" } |
            ForEach-Object { $paths.Add($_.FullName) }
    }
    $before = @{}
    foreach ($path in $paths) { $before[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
    foreach ($character in 'toma','ren','chizuru') {
        [AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath "art_source/$character.manifest.json"), $false)
    }
    foreach ($path in $paths) {
        $after = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        if ($before[$path] -ne $after) { throw "Non-reproducible output: $path" }
        $repro.Add([ordered]@{path=$path.Substring($taskRootPath.Length+1);sha256=$after;matches=$true})
    }
}
foreach ($character in 'toma','ren','chizuru') {
    $path = Join-Path $taskRootPath "art_source/$character.manifest.json"
    $manifest = $json.Deserialize([System.IO.File]::ReadAllText($path), [AniBato.SpriteForge.Manifest])
    [AniBato.SpriteForge.Forge]::Run($path, $true)
    $idle = [AniBato.SpriteForge.Raster]::Load((Join-Path $taskRootPath "$($manifest.outputDirectory)/idle.png"))
    $bounds = [AniBato.SpriteForge.Pixels]::Bounds($idle,0,$manifest.cellWidth)
    $chest = $bounds.Top + [int][math]::Floor($bounds.Height * 2 / 5)
    for ($frame=1; $frame -lt 8; $frame++) {
        for ($y=$chest; $y -lt $manifest.cellHeight; $y++) {
            for ($x=0; $x -lt $manifest.cellWidth; $x++) {
                if ($idle.Pixels[$y*$idle.Width+$x] -ne $idle.Pixels[$y*$idle.Width+$frame*$manifest.cellWidth+$x]) { throw "$character idle torso/feet drift" }
            }
        }
        $cell = [AniBato.SpriteForge.Pixels]::Bounds($idle,($frame*$manifest.cellWidth),(($frame+1)*$manifest.cellWidth))
        if ([math]::Abs($cell.Top-$bounds.Top) -gt 1) { throw "$character idle head drift" }
    }
    foreach ($clip in $manifest.animations | Where-Object { $_.inbetweenFrames -gt 0 }) {
        $gif = Join-Path $taskRootPath "docs/art_wip/$($character)_$($clip.name)_preview.gif"
        $image = [System.Drawing.Image]::FromFile($gif)
        try {
            $frames = $image.GetFrameCount([System.Drawing.Imaging.FrameDimension]::Time)
            if ($frames -ne $clip.inbetweenFrames) { throw 'GIF frame count mismatch' }
            for ($i=0; $i -lt $frames; $i++) { $image.SelectActiveFrame([System.Drawing.Imaging.FrameDimension]::Time,$i) | Out-Null; $image.GetPixel(112,120) | Out-Null }
            $report.Add([ordered]@{character=$character;clip=$clip.name;frames=$frames;fps=$clip.fps;gif=$gif.Substring($taskRootPath.Length+1);sha256=(Get-FileHash -LiteralPath $gif).Hash;idleAnchorsFixed=$true})
        } finally { $image.Dispose() }
    }
}
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $taskRootPath 'TestResults/phase5b-animation-verification.json'),($report | ConvertTo-Json -Depth 5),$utf8)
if ($Rebuild) {
    [System.IO.File]::WriteAllText((Join-Path $taskRootPath 'TestResults/phase5b-spriteforge-reproducibility.json'),($repro | ConvertTo-Json -Depth 5),$utf8)
    Write-Output "Reproducibility passed: $($repro.Count) PNG/JSON/contact/GIF files have identical SHA-256 after rebuilding."
}
Write-Output "Phase 5B animation checks passed: $($report.Count) GIFs; idle torso/feet fixed; head drift <= 1 pixel."
