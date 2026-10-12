param([switch]$VerifyOnly, [switch]$RebuildCheck)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @('Manifest.cs','ImageProcessing.cs','AnimationSmoothing.cs','SpriteForge.cs','ArtThreeIdle.cs','ArtThreeCrouch.cs','AtlasPoseSplitter.cs','ActionAtlasBuilder.cs','ArtThreeActions.cs' | ForEach-Object { Join-Path $PSScriptRoot $_ }) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$before = @{}
if ($RebuildCheck) {
    Get-ChildItem -LiteralPath 'AniBato/Assets/_Project/Art/Sprites','docs/art_wip','art_source/art3_actions/ready','art_source/art3_actions/normalized','art_source/art3_idle/breathing/ready','art_source/art3_crouch/ready' -Recurse -File |
        Where-Object { $_.Extension -in '.png','.json','.gif' } |
        ForEach-Object { $before[$_.FullName] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
}
if (-not $VerifyOnly) {
    [AniBato.SpriteForge.ArtThreeActions]::Prepare($taskRootPath)
    & (Join-Path $PSScriptRoot 'RecordActionSources.ps1')
}
foreach ($character in 'toma','ren','chizuru') { [AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath "art_source/$character.manifest.json"), $VerifyOnly.IsPresent) }
if (-not $VerifyOnly) { [AniBato.SpriteForge.ArtThreeActions]::Publish($taskRootPath) }
[AniBato.SpriteForge.ArtThreeActions]::Verify($taskRootPath)
[AniBato.SpriteForge.ArtThreeCrouch]::Verify($taskRootPath)
[AniBato.SpriteForge.ArtThreeIdle]::Verify($taskRootPath)
if ($RebuildCheck) {
    foreach ($path in $before.Keys) { if ($before[$path] -ne (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash) { throw "Non-reproducible artifact: $([IO.Path]::GetFileName($path))" } }
    Write-Output "ART3D reproducibility: $($before.Count) files matched SHA-256."
}
