param([Parameter(Mandatory=$true)][ValidateSet('jump','jump_start','guard','dodge','dash')][string]$Group)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AtlasPoseSplitter.cs'),(Join-Path $PSScriptRoot 'ActionAtlasBuilder.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
[AniBato.SpriteForge.ActionAtlasBuilder]::NormalizeGroup($taskRootPath,$Group)
