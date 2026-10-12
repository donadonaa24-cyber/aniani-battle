param([string]$Manifest = 'art_source/chizuru.manifest.json', [switch]$VerifyOnly, [switch]$SelfTest)
$ErrorActionPreference = 'Stop'
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'), (Join-Path $PSScriptRoot 'ImageProcessing.cs'), (Join-Path $PSScriptRoot 'AnimationSmoothing.cs'), (Join-Path $PSScriptRoot 'SpriteForge.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
if ($SelfTest) { [AniBato.SpriteForge.Forge]::SelfTest() }
else { [AniBato.SpriteForge.Forge]::Run((Join-Path (Get-Location) $Manifest), $VerifyOnly.IsPresent) }
