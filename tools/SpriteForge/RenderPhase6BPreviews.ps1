$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path (Join-Path $PSScriptRoot 'PhaseSixBPreviews.cs') -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
[AniBato.SpriteForge.PhaseSixBPreviews]::Render($taskRootPath)
