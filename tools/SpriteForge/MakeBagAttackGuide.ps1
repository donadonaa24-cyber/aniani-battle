$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Add-Type -AssemblyName System.Drawing
$bmp=[Drawing.Bitmap]::new(1280,480)
$g=[Drawing.Graphics]::FromImage($bmp)
$g.Clear([Drawing.Color]::White)
$red=[Drawing.Pen]::new([Drawing.Color]::Crimson,14)
$blue=[Drawing.Pen]::new([Drawing.Color]::RoyalBlue,12)
$body=[Drawing.Pen]::new([Drawing.Color]::Gray,8)
$font=[Drawing.Font]::new('Arial',13)
for($i=0;$i -lt 4;$i++){
    $x=$i*320+128
    $g.DrawString(@('RULER','TWIST WINDUP','FRONT CHEST / CROSS-BODY HIT','RETURN / BAG SAME HAND')[$i],$font,[Drawing.Brushes]::Black,[single]($i*320+8),[single]15)
    $g.DrawEllipse($body,[single]($x-34),[single]78,68,75)
    $g.FillEllipse([Drawing.Brushes]::Orange,[single]($x-35),[single]155,70,130)
    $g.DrawString('FRONT',$font,[Drawing.Brushes]::Black,[single]($x-31),[single]180)
    $g.DrawLine($body,$x,275,$x-28,331)
    $g.DrawLine($body,$x-28,331,$x-56,397)
    $g.DrawLine($body,$x,275,$x+39,336)
    $g.DrawLine($body,$x+39,336,$x+65,397)
    $g.DrawLine($blue,$x+24,185,$x+30,240)
    $g.DrawLine($blue,$x+30,240,$x+9,277)
    $handX=$x-40;$handY=272
    if($i -eq 1){$handX=$x-12;$handY=269}
    if($i -eq 2){
        $handX=$x+100;$handY=227
        $g.DrawLine($red,$x-24,185,$x+30,254)
        $g.DrawLine($red,$x+30,254,$handX,$handY)
    }else{
        $g.DrawLine($red,$x-24,185,$x-39,236)
        $g.DrawLine($red,$x-39,236,$handX,$handY)
    }
    if($i -gt 0){
        $g.DrawArc([Drawing.Pens]::SaddleBrown,[single]($handX-9),[single]$handY,28,30,180,180)
        $g.FillRectangle([Drawing.Brushes]::SaddleBrown,[single]($handX-20),[single]($handY+15),56,48)
        $g.FillRectangle([Drawing.Brushes]::Gold,[single]($handX+1),[single]($handY+35),11,9)
    }
}
$g.DrawString('RED = anatomical RIGHT arm from SCREEN-LEFT shoulder on FRONT torso; crosses OVER chest. BLUE = EMPTY LEFT arm. Guide only, keep original character/art colors.',$font,[Drawing.Brushes]::Black,8,441)
$path=Join-Path $taskRootPath 'art_source/art3_walk_attacks/pose-guides/chizuru_attack2.png'
$bmp.Save($path)
$red.Dispose();$blue.Dispose();$body.Dispose();$font.Dispose();$g.Dispose();$bmp.Dispose()
Write-Output 'Anatomical right-arm bag guide saved; never game art.'
