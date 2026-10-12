param([switch]$VerifyOnly, [switch]$RebuildCheck)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AnimationSmoothing.cs'),(Join-Path $PSScriptRoot 'SpriteForge.cs'),(Join-Path $PSScriptRoot 'ArtThreeIdle.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$before = @{}
if ($RebuildCheck) {
    Get-ChildItem -LiteralPath 'AniBato/Assets/_Project/Art/Sprites','docs/art_wip','art_source/art3_idle/breathing/ready' -Recurse -File |
        Where-Object { $_.Extension -in '.png','.json','.gif' } |
        ForEach-Object { $before[$_.FullName] = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
}
if (-not $VerifyOnly) { [AniBato.SpriteForge.ArtThreeIdle]::Prepare($taskRootPath) }
foreach ($character in 'toma','ren','chizuru') {
    [AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath "art_source/$character.manifest.json"), $VerifyOnly.IsPresent)
}
if (-not $VerifyOnly) { [AniBato.SpriteForge.ArtThreeIdle]::Publish($taskRootPath) }
[AniBato.SpriteForge.ArtThreeIdle]::Verify($taskRootPath)
if ($RebuildCheck) {
    foreach ($path in $before.Keys) {
        if ($before[$path] -ne (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash) { throw "Non-reproducible artifact: $([IO.Path]::GetFileName($path))" }
    }
    Write-Output "ART3B reproducibility: $($before.Count) files matched SHA-256."
}
