param([Parameter(Mandatory=$true)][ValidateSet('toma','ren','chizuru')][string]$Character,
      [Parameter(Mandatory=$true)][string]$Group,
      [Parameter(Mandatory=$true)][int]$Version)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -Path @('Manifest.cs','ImageProcessing.cs','AtlasPoseSplitter.cs' | ForEach-Object { Join-Path $PSScriptRoot $_ }) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$manifest=Get-Content "art_source/$Character.manifest.json" -Raw | ConvertFrom-Json
$source=[AniBato.SpriteForge.Raster]::Load((Join-Path $taskRootPath "art_source/art3_walk_attacks/generated/$($Character)_$($Group)_keys_v$Version.png"))
[AniBato.SpriteForge.Pixels]::Key($source,$manifest.keyTolerance)
$parts=[AniBato.SpriteForge.AtlasPoseSplitter]::Split($source,70)
$scale=$manifest.targetHeight/[double]$parts[0].SourceBounds.Height
$depth=@{toma=25;ren=31;chizuru=34}[$Character]
$rows=for($i=0;$i -lt $parts.Count;$i++) {
    $p=$parts[$i]
    $f=[AniBato.SpriteForge.Pixels]::ModalShrink($p.Image,[Drawing.Rectangle]::new(0,0,$p.Image.Width,$p.Image.Height),$scale)
    $bounds=[AniBato.SpriteForge.Pixels]::Bounds($f,0,$f.Width)
    $left=$f.Width;$right=-1
    $inverted=$Character -eq 'ren' -and (($Group -eq 'attack1' -and $i -eq 3) -or ($Group -eq 'attack2' -and $i -gt 0) -or ($Group -eq 'attack3' -and $i -eq 1))
    $headTop=if($inverted) { $bounds.Bottom-$depth } else { $bounds.Top }
    for($y=$headTop;$y -lt [Math]::Min($f.Height,$headTop+$depth);$y++) { for($x=0;$x -lt $f.Width;$x++) {
        if($f.Pixels[$y*$f.Width+$x] -ne 0) { $left=[Math]::Min($left,$x);$right=[Math]::Max($right,$x) }
    } }
    [pscustomobject]@{index=$i;headWidth=($right-$left+1);pixelHeight=$bounds.Height;rawBounds=$p.SourceBounds.ToString();commonScale=$scale}
}
$rows | ConvertTo-Json -Depth 3
# Diagnostic only. Adoption still requires NormalizeWalkAttackAtlas and the complete build verification.
