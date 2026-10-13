param(
    [Parameter(Mandatory=$true)][string]$Group,
    [Parameter(Mandatory=$true)][string]$Source,
    [switch]$Airborne
)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @('Manifest.cs','ImageProcessing.cs','AtlasPoseSplitter.cs','ActionAtlasBuilder.cs' | ForEach-Object {Join-Path $PSScriptRoot $_}) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
[AniBato.SpriteForge.ActionAtlasBuilder]::NormalizeTomaMotion($taskRootPath,$Group,$Source,$Airborne.IsPresent)
