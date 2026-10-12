param([switch]$VerifyOnly)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AnimationSmoothing.cs'),(Join-Path $PSScriptRoot 'SpriteForge.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
foreach ($character in 'toma','ren','chizuru') {
    $source = Join-Path $taskRootPath "AniBato/Assets/_Project/Art/Sprites/$character"
    $backup = Join-Path $taskRootPath "art_source/phase5b_keys/$character"
    if (-not $VerifyOnly -and -not (Test-Path -LiteralPath $backup)) {
        New-Item -ItemType Directory -Path $backup -Force | Out-Null
        Get-ChildItem -LiteralPath $source -File | Where-Object { $_.Extension -in '.png','.json' } | Copy-Item -Destination $backup
    }
    [AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath "art_source/$character.manifest.json"), $VerifyOnly.IsPresent)
}
