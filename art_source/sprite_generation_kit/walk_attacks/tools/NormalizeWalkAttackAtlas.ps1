param([Parameter(Mandatory=$true)][ValidateSet('walk','walk_inbetweens','dash','attack1','attack2','attack3')][string]$Group,
      [ValidateSet('toma','ren','chizuru')][string]$Character)
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @('Manifest.cs','ImageProcessing.cs','AtlasPoseSplitter.cs','ActionAtlasBuilder.cs' | ForEach-Object { Join-Path $PSScriptRoot $_ }) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
if ($Character) { [AniBato.SpriteForge.ActionAtlasBuilder]::NormalizeWalkAttack($taskRootPath,$Group,$Character) }
else { [AniBato.SpriteForge.ActionAtlasBuilder]::NormalizeWalkAttack($taskRootPath,$Group) }
