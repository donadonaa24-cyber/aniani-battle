param(
    [Parameter(Mandatory=$true)][string]$Group,
    [string]$Clip,
    [string]$Source,
    [int]$PropWidth=0,
    [switch]$NewClip
)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @('Manifest.cs','ImageProcessing.cs','AnimationSmoothing.cs','SpriteForge.cs','AtlasPoseSplitter.cs','TomaActionAssets.cs' | ForEach-Object {Join-Path $PSScriptRoot $_}) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
if($PropWidth -gt 0){
    if(-not $Source){throw 'Prop requires a source path.'}
    $Clip=$Group
    [AniBato.SpriteForge.TomaActionAssets]::PrepareProp($taskRootPath,$Group,$Source,$PropWidth,15)
}else{
    if(-not $Clip){throw 'Body requires a clip name.'}
    [AniBato.SpriteForge.TomaActionAssets]::PrepareBody($taskRootPath,$Group,$Clip,$NewClip.IsPresent,3)
}
[AniBato.SpriteForge.Forge]::Run((Join-Path $taskRootPath 'art_source/toma.manifest.json'),$false)
[AniBato.SpriteForge.TomaActionAssets]::PublishVerify($taskRootPath,$Clip)
