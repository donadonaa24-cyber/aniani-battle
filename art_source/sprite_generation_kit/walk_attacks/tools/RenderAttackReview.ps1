$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Add-Type -AssemblyName System.Drawing
foreach($character in 'toma','ren','chizuru'){
    $bmp=[Drawing.Bitmap]::new(1792,1020)
    $g=[Drawing.Graphics]::FromImage($bmp)
    $g.Clear([Drawing.Color]::FromArgb(34,39,49))
    $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
    $font=[Drawing.Font]::new('Arial',13)
    for($row=0;$row -lt 3;$row++){
        for($frame=0;$frame -lt 4;$frame++){
            $p=Join-Path $taskRootPath ('art_source/art3_walk_attacks/normalized/'+$character+'/attack'+($row+1)+'_key_'+$frame.ToString('D2')+'.png')
            $image=[Drawing.Bitmap]::new($p)
            try{
                $g.DrawString(('attack'+($row+1)+' / '+@('RULER','WINDUP','IMPACT','RECOVERY')[$frame]),$font,[Drawing.Brushes]::White,[single]($frame*448+8),[single]($row*340+4))
                $g.DrawImage($image,[Drawing.Rectangle]::new($frame*448,$row*340+20,448,320),[Drawing.Rectangle]::new(0,0,224,160),[Drawing.GraphicsUnit]::Pixel)
            }finally{$image.Dispose()}
        }
    }
    $bmp.Save((Join-Path $taskRootPath ('docs/art_wip/art3e_'+$character+'_attack_review.png')))
    $font.Dispose();$g.Dispose();$bmp.Dispose()
}
Write-Output 'Three normal-attack review boards saved at integer2x.'
