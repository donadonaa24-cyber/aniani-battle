param([switch]$VerifyOnly, [switch]$RebuildCheck, [switch]$WalkOnly, [switch]$GaitsOnly)
$ErrorActionPreference = 'Stop'
if ($WalkOnly -and $GaitsOnly) { throw 'Choose only one partial-build mode.' }
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @('Manifest.cs','ImageProcessing.cs','AnimationSmoothing.cs','SpriteForge.cs','ArtThreeIdle.cs','ArtThreeCrouch.cs','AtlasPoseSplitter.cs','ActionAtlasBuilder.cs','ArtThreeActions.cs','ArtThreeWalkAttacks.cs','BagAtlasBuilder.cs' | ForEach-Object { Join-Path $PSScriptRoot $_ }) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$before = @{}
if ($RebuildCheck) {
    Get-ChildItem -LiteralPath 'AniBato/Assets/_Project/Art/Sprites','docs/art_wip','art_source/art3_walk_attacks/ready','art_source/art3_walk_attacks/normalized','art_source/art3_actions/ready','art_source/art3_idle/breathing/ready','art_source/art3_crouch/ready' -Recurse -File |
        Where-Object { $_.Extension -in '.png','.json','.gif' } |
        ForEach-Object { $before[$_.FullName] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
}
if (-not $VerifyOnly) {
    [AniBato.SpriteForge.ArtThreeWalkAttacks]::Prepare($taskRootPath,$WalkOnly.IsPresent,$GaitsOnly.IsPresent)
    if (-not $WalkOnly -and -not $GaitsOnly) { [AniBato.SpriteForge.BagAtlasBuilder]::Prepare($taskRootPath) }
    & (Join-Path $PSScriptRoot 'RecordWalkAttackSources.ps1')
}
foreach ($character in 'toma','ren','chizuru') { [AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath "art_source/$character.manifest.json"), $VerifyOnly.IsPresent) }
if (-not $VerifyOnly) { [AniBato.SpriteForge.ArtThreeWalkAttacks]::Publish($taskRootPath,$WalkOnly.IsPresent,$GaitsOnly.IsPresent) }
[AniBato.SpriteForge.ArtThreeWalkAttacks]::Verify($taskRootPath,$WalkOnly.IsPresent,$GaitsOnly.IsPresent)
if (-not $WalkOnly -and -not $GaitsOnly) { [AniBato.SpriteForge.BagAtlasBuilder]::PublishVerify($taskRootPath) }
[AniBato.SpriteForge.ArtThreeActions]::Verify($taskRootPath)
[AniBato.SpriteForge.ArtThreeCrouch]::Verify($taskRootPath)
[AniBato.SpriteForge.ArtThreeIdle]::Verify($taskRootPath)
$unchanged = Get-Content 'art_source/art3_walk_attacks/unchanged-sprites-sha256.json' -Raw | ConvertFrom-Json
foreach ($entry in $unchanged.PSObject.Properties) { if ((Get-FileHash -LiteralPath $entry.Name).Hash -ne $entry.Value) { throw "Unrelated sprite changed: $($entry.Name)" } }
Write-Output "Unrelated sprites: $(@($unchanged.PSObject.Properties).Count) original PNG hashes matched."
if ($RebuildCheck) {
    foreach ($path in $before.Keys) { if ($before[$path] -ne (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash) { throw "Non-reproducible artifact: $([IO.Path]::GetFileName($path))" } }
    Write-Output "ART3E reproducibility: $($before.Count) files matched SHA-256."
}
