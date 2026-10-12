param([switch]$VerifyOnly)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AnimationSmoothing.cs'),(Join-Path $PSScriptRoot 'SpriteForge.cs'),(Join-Path $PSScriptRoot 'PhaseSixArt.cs'),(Join-Path $PSScriptRoot 'PhaseSixBArt.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
if (-not $VerifyOnly) { [AniBato.SpriteForge.PhaseSixBArt]::Prepare($taskRootPath) }
foreach ($character in 'toma','ren','chizuru') {
    [AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath "art_source/$character.manifest.json"), $VerifyOnly.IsPresent)
}
