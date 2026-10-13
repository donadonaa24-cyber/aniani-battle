param([switch]$WalkOnly)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -AssemblyName System.Drawing
$clips=if($WalkOnly){@('walk')}else{@('walk','dash')}
foreach($key in 'toma','ren','chizuru') { foreach($clip in $clips) {
    $sheet=New-Object Drawing.Bitmap((Join-Path $taskRootPath "art_source/art3_walk_attacks/ready/$key/$($clip)_sheet.png"))
    $board=New-Object Drawing.Bitmap(1792,1440)
    $g=[Drawing.Graphics]::FromImage($board)
    $g.Clear([Drawing.Color]::FromArgb(34,39,49));$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor;$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
    $font=New-Object Drawing.Font('Arial',15)
    for($i=0;$i -lt 8;$i++) {
        $x=($i%4)*448;$y=[int][Math]::Floor($i/4)*720
        $label=if($clip -eq 'walk'){@('L contact','L down','L passing','L up','R contact','R down','R passing','R up')[$i]}else{@('L contact','L contact','R knee flight','R knee flight','R contact','R contact','L knee flight','L knee flight')[$i]}
        $g.DrawString("$key $clip frame $i : $label",$font,[Drawing.Brushes]::White,[single]($x+8),[single]($y+4))
        $dst=[Drawing.Rectangle]::new($x,($y+30),448,320);$src=[Drawing.Rectangle]::new(($i*224),0,224,160)
        $g.DrawImage($sheet,$dst,$src,[Drawing.GraphicsUnit]::Pixel)
        $g.DrawString('Arms 3x: shoulder -> elbow -> wrist',$font,[Drawing.Brushes]::White,[single]($x+8),[single]($y+352))
        $dstArm=[Drawing.Rectangle]::new(($x+32),($y+378),384,192);$srcArm=[Drawing.Rectangle]::new(($i*224+48),73,128,64)
        $g.DrawImage($sheet,$dstArm,$srcArm,[Drawing.GraphicsUnit]::Pixel)
        $g.DrawString('Feet 4x',$font,[Drawing.Brushes]::White,[single]($x+8),[single]($y+574))
        $dstLeg=[Drawing.Rectangle]::new(($x+4),($y+600),440,120);$srcLeg=[Drawing.Rectangle]::new(($i*224+55),130,110,30)
        $g.DrawImage($sheet,$dstLeg,$srcLeg,[Drawing.GraphicsUnit]::Pixel)
    }
    $board.Save((Join-Path $taskRootPath "docs/art_wip/art3e_$($key)_$($clip)_frames.png"))
    $font.Dispose();$g.Dispose();$board.Dispose();$sheet.Dispose()
} }
Write-Output 'Full-body, 3x shoulder-to-wrist and 4x feet gait review boards saved.'
