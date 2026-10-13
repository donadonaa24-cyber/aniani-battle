$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Add-Type -AssemblyName System.Drawing
$folder=Join-Path $taskRootPath 'art_source/art3_walk_attacks/pose-guides'
New-Item -ItemType Directory -Force -Path $folder | Out-Null
foreach($group in 'walk','walk_inbetweens','dash') {
    $bmp=New-Object Drawing.Bitmap(1600,420)
    $g=[Drawing.Graphics]::FromImage($bmp)
    $g.Clear([Drawing.Color]::White)
    $font=New-Object Drawing.Font('Arial',13)
    $near=New-Object Drawing.Pen([Drawing.Color]::FromArgb(215,55,40),13)
    $far=New-Object Drawing.Pen([Drawing.Color]::FromArgb(30,120,190),13)
    $body=New-Object Drawing.Pen([Drawing.Color]::FromArgb(115,115,115),14)
    $points=if($group -eq 'walk') {
        @(@(135,175,120,225,100,285,160,225,190,285),@(135,175,128,230,134,285,115,220,97,247),@(135,175,160,225,190,285,120,225,100,285),@(135,175,115,220,97,247,128,230,134,285))
    } elseif($group -eq 'walk_inbetweens') {
        @(@(141,182,123,231,108,281,157,230,178,285),@(133,167,147,218,163,268,123,226,119,285),@(141,182,157,230,178,285,123,231,108,281),@(133,167,123,226,119,285,147,218,163,268))
    } else {
        @(@(135,170,120,220,85,245,165,220,195,285),@(140,160,178,200,195,240,110,210,85,250),@(135,170,165,220,195,285,120,220,85,245),@(140,160,110,210,85,250,178,200,195,240))
    }
    $labels=@('STANDING RULER','LEFT SUPPORT / RIGHT SWING','LEFT SUPPORT / RIGHT SWING','RIGHT SUPPORT / LEFT SWING','RIGHT SUPPORT / LEFT SWING')
    if($group -eq 'dash'){$labels=@('STANDING RULER','LEFT CONTACT / RIGHT BACK','FLIGHT / RIGHT KNEE FORWARD','RIGHT CONTACT / LEFT BACK','FLIGHT / LEFT KNEE FORWARD')}
    $armSwing=if($group -eq 'walk'){@(-55,-4,55,4)}elseif($group -eq 'walk_inbetweens'){@(-36,32,36,-32)}else{@(-60,30,60,-30)}
    for($i=0;$i -lt 5;$i++) {
        $o=$i*320+20
        $g.DrawString($labels[$i],$font,[Drawing.Brushes]::Black,[single]($o-10),[single]15)
        $p=if($i -eq 0){@(135,170,115,225,105,285,160,225,180,285)}else{$points[$i-1]}
        $hx=$o+$p[0];$hy=$p[1]+50
        $swing=if($i -eq 0){0}else{$armSwing[$i-1]}
        $sy=170+($p[1]-170)
        $fy=$sy+78-[int]([Math]::Abs($swing)*0.18)
        $g.DrawLine($far,[single]($hx-5),[single]$sy,[single]($hx-5-$swing*0.50),[single]($sy+39))
        if($group -eq 'dash' -and $i -gt 0){$fy=$sy+20;$fx=$hx-5-$swing*0.5+30}else{$fx=$hx-5-$swing}
        $g.DrawLine($far,[single]($hx-5-$swing*0.50),[single]($sy+39),[single]$fx,[single]$fy)
        $g.DrawEllipse($body,[single]($hx-28),[single]74,[single]60,[single]65)
        $g.DrawLine($body,[single]$hx,[single]145,[single]$hx,[single]$hy)
        $g.DrawLine($far,[single]$hx,[single]$hy,[single]($o+$p[2]),[single]($p[3]+50))
        $g.DrawLine($far,[single]($o+$p[2]),[single]($p[3]+50),[single]($o+$p[4]),[single]($p[5]+50))
        $g.DrawLine($far,[single]($o+$p[4]-7),[single]($p[5]+50),[single]($o+$p[4]+19),[single]($p[5]+50))
        $g.DrawLine($near,[single]$hx,[single]$hy,[single]($o+$p[6]),[single]($p[7]+50))
        $g.DrawLine($near,[single]($o+$p[6]),[single]($p[7]+50),[single]($o+$p[8]),[single]($p[9]+50))
        $g.DrawLine($near,[single]($o+$p[8]-7),[single]($p[9]+50),[single]($o+$p[8]+19),[single]($p[9]+50))
        $g.DrawLine($near,[single]($hx+3),[single]$sy,[single]($hx+3+$swing*0.50),[single]($sy+39))
        $nx=if($group -eq 'dash' -and $i -gt 0){$hx+3+$swing*0.5+30}else{$hx+3+$swing}
        $g.DrawLine($near,[single]($hx+3+$swing*0.50),[single]($sy+39),[single]$nx,[single]$fy)
    }
    $g.DrawString('RED = LEFT near arm/leg, in FRONT. BLUE = RIGHT far arm/leg, BEHIND. Arms pendulum opposite legs; use original costume colors.',$font,[Drawing.Brushes]::Black,[single]20,[single]385)
    $bmp.Save((Join-Path $folder ($group+'.png')))
    $near.Dispose();$far.Dispose();$body.Dispose();$font.Dispose();$g.Dispose();$bmp.Dispose()
}
Write-Output 'Three gait pose guides saved; input references only, never game art.'
