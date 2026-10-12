param([switch]$Rebuild, [string]$Tag = 'phase6a')
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AnimationSmoothing.cs'),(Join-Path $PSScriptRoot 'SpriteForge.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$json = New-Object System.Web.Script.Serialization.JavaScriptSerializer
$json.MaxJsonLength = 16777216
$report = New-Object System.Collections.Generic.List[object]
$paths = Get-ChildItem AniBato/Assets/_Project/Art/Sprites,docs/art_wip -Recurse -File | Where-Object { $_.Extension -in '.png','.json','.gif' -and $_.Name -ne 'README.md' }
$before = @{}
foreach ($path in $paths) { $before[$path.FullName] = (Get-FileHash -LiteralPath $path.FullName -Algorithm SHA256).Hash }
foreach ($character in 'toma','ren','chizuru') {
    $path = Join-Path $taskRootPath "art_source/$character.manifest.json"
    $manifest = $json.Deserialize([IO.File]::ReadAllText($path), [AniBato.SpriteForge.Manifest])
    [AniBato.SpriteForge.Forge]::Run($path, (-not $Rebuild.IsPresent))
    foreach ($clip in $manifest.animations) {
        $sheet = [AniBato.SpriteForge.Raster]::Load((Join-Path $taskRootPath "$($manifest.outputDirectory)/$($clip.name).png"))
        $count = [AniBato.SpriteForge.AnimationSmoothing]::Count($clip)
        $gif = Join-Path $taskRootPath "docs/art_wip/$($character)_$($clip.name)_preview.gif"
        $image = [Drawing.Image]::FromFile($gif)
        try {
            $frames = $image.GetFrameCount([Drawing.Imaging.FrameDimension]::Time)
            if ($frames -ne $count) { throw 'GIF frame count mismatch' }
            for ($i=0; $i -lt $frames; $i++) { $image.SelectActiveFrame([Drawing.Imaging.FrameDimension]::Time,$i) | Out-Null; $image.GetPixel(112,120) | Out-Null }
        } finally { $image.Dispose() }
        if ($clip.stableIdle) {
            $bounds = [AniBato.SpriteForge.Pixels]::Bounds($sheet,0,$manifest.cellWidth)
            $chest = $bounds.Top + [int][math]::Floor($bounds.Height * 2 / 5)
            for ($i=1; $i -lt $count; $i++) {
                $frameBounds = [AniBato.SpriteForge.Pixels]::Bounds($sheet,($i*$manifest.cellWidth),(($i+1)*$manifest.cellWidth))
                if ([math]::Abs($frameBounds.Top-$bounds.Top) -gt 1) { throw 'Breathing head drift exceeds one pixel' }
                for ($y=$chest+2; $y -lt $manifest.cellHeight; $y++) {
                    for ($x=0; $x -lt $manifest.cellWidth; $x++) {
                        if ($sheet.Pixels[$y*$sheet.Width+$x] -ne $sheet.Pixels[$y*$sheet.Width+$i*$manifest.cellWidth+$x]) { throw "$character $($clip.name): torso/feet drift" }
                    }
                }
            }
        }
        if ($clip.name -in 'win','lose') {
            $idle = [AniBato.SpriteForge.Raster]::Load((Join-Path $taskRootPath "$($manifest.outputDirectory)/idle.png"))
            $baseBounds = [AniBato.SpriteForge.Pixels]::Bounds($idle,0,$manifest.cellWidth)
            $resultBounds = [AniBato.SpriteForge.Pixels]::Bounds($sheet,0,$manifest.cellWidth)
            if ([math]::Abs($resultBounds.Top-$baseBounds.Top) -gt 2 -or $resultBounds.Bottom -ne $baseBounds.Bottom) { throw 'Result pose differs too much from idle' }
        }
        if ($Tag -eq 'phase6b' -and $clip.name -in 'walk','dash') {
            if ($clip.sourceClip -ne "$($clip.name)_phase6b" -or $count -ne 8) { throw 'Drawn motion keys are not connected' }
            $uniqueFrames = New-Object System.Collections.Generic.HashSet[string]
            for ($i=0; $i -lt $count; $i++) {
                $frameBytes = New-Object byte[] ($manifest.cellWidth * $manifest.cellHeight * 4)
                for ($y=0; $y -lt $manifest.cellHeight; $y++) {
                    [Buffer]::BlockCopy($sheet.Pixels,($y*$sheet.Width+$i*$manifest.cellWidth)*4,$frameBytes,$y*$manifest.cellWidth*4,$manifest.cellWidth*4)
                }
                $hasher = [Security.Cryptography.SHA256]::Create()
                try { $uniqueFrames.Add([Convert]::ToBase64String($hasher.ComputeHash($frameBytes))) | Out-Null } finally { $hasher.Dispose() }
            }
            $minimum = if ($clip.name -eq 'dash') { 8 } else { 6 }
            if ($uniqueFrames.Count -lt $minimum) { throw 'Motion clip does not contain enough distinct clean drawings' }
        }
        $report.Add([ordered]@{ character=$character; clip=$clip.name; frames=$count; gif=$gif.Substring($taskRootPath.Length+1); synthesis= $(if ($clip.stableIdle) {'single-reference-breathing'} elseif ($clip.sourceClip -like '*_phase6b') {'drawn-key-inbetweens'} else {'clean-key-hold'}); sha256=(Get-FileHash -LiteralPath $gif).Hash })
    }
}
if ($Rebuild) { foreach ($path in $paths) { if ($before[$path.FullName] -ne (Get-FileHash -LiteralPath $path.FullName -Algorithm SHA256).Hash) { throw "Non-reproducible: $($path.Name)" } } }
$utf8 = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $taskRootPath "TestResults/$Tag-animation-verification.json"),($report | ConvertTo-Json -Depth 5),$utf8)
Write-Output "$Tag`: $($report.Count) complete GIFs; calm result poses; binary alpha/shared palette verified; reproducibility=$($Rebuild.IsPresent), $($paths.Count) files."
