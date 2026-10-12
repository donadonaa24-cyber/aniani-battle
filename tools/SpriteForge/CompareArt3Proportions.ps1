param()
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
public static class Art3Parts {
  public static int[] Head(Bitmap b, int depth) {
    int top=160, left=224, right=-1;
    for(int y=0;y<160 && top==160;y++) for(int x=0;x<224;x++) if(b.GetPixel(x,y).A!=0) { top=y; break; }
    for(int y=top;y<top+depth;y++) for(int x=0;x<224;x++) if(b.GetPixel(x,y).A!=0) { left=Math.Min(left,x); right=Math.Max(right,x); }
    return new[]{left,top,right-left+1,depth};
  }
  public static int[][] Shoes(Bitmap b) {
    var seen=new HashSet<int>(); var parts=new List<int[]>();
    for(int y=152;y<160;y++) for(int x=0;x<224;x++) {
      int i=y*224+x; if(seen.Contains(i) || b.GetPixel(x,y).A==0) continue;
      var queue=new Queue<int>(); queue.Enqueue(i); seen.Add(i);
      int left=x,right=x,top=y,bottom=y;
      while(queue.Count!=0) {
        int p=queue.Dequeue(), px=p%224, py=p/224;
        left=Math.Min(left,px); right=Math.Max(right,px); top=Math.Min(top,py); bottom=Math.Max(bottom,py);
        int[,] shifts={{-1,0},{1,0},{0,-1},{0,1}};
        for(int d=0;d<4;d++) {
          int nx=px+shifts[d,0], ny=py+shifts[d,1], n=ny*224+nx;
          if(nx<0 || nx>=224 || ny<152 || ny>=160 || seen.Contains(n) || b.GetPixel(nx,ny).A==0) continue;
          seen.Add(n); queue.Enqueue(n);
        }
      }
      if(bottom-top+1>=4 && bottom>=157) parts.Add(new[]{left,top,right-left+1,bottom-top+1});
    }
    if(parts.Count!=2) throw new InvalidOperationException("Expected two separated shoe silhouettes in the bottom eight rows.");
    return parts.OrderBy(p=>p[0]).ToArray();
  }
}
'@
$results = @()
foreach ($entry in @(@{key='toma';depth=25},@{key='ren';depth=31},@{key='chizuru';depth=34})) {
    $key = $entry.key
    $idle = [Drawing.Bitmap]::FromFile("art_source/art3_idle/breathing/ready/$key/idle_00.png")
    $crouch = [Drawing.Bitmap]::FromFile("art_source/art3_crouch/ready/$key/crouch_00.png")
    try {
        $idleHead = [Art3Parts]::Head($idle,$entry.depth); $crouchHead = [Art3Parts]::Head($crouch,$entry.depth)
        $idleShoes = [Art3Parts]::Shoes($idle); $crouchShoes = [Art3Parts]::Shoes($crouch)
        $headDelta = $crouchHead[2]-$idleHead[2]; $shoeDelta = @(($crouchShoes[0][2]-$idleShoes[0][2]),($crouchShoes[1][2]-$idleShoes[1][2]))
        Write-Output "$key head width $($idleHead[2]) -> $($crouchHead[2]); shoes $($idleShoes[0][2]),$($idleShoes[1][2]) -> $($crouchShoes[0][2]),$($crouchShoes[1][2])"
        if ([Math]::Abs($headDelta) -gt 2 -or [Math]::Abs($shoeDelta[0]) -gt 3 -or [Math]::Abs($shoeDelta[1]) -gt 3) {
            throw "Part scale differs from approved idle: $key"
        }
        $results += [ordered]@{ character=$key; headRoiDepth=$entry.depth; approvedIdleHeadBounds=@($idleHead); crouchHeadBounds=@($crouchHead)
            headWidthDelta=$headDelta; approvedIdleShoeBounds=@($idleShoes); crouchShoeBounds=@($crouchShoes); shoeWidthDeltas=$shoeDelta
            shoeRoiTop=152; visualReviewRequired=$true; maxHeadWidthDelta=2; maxShoeWidthDelta=3 }
    }
    finally { $idle.Dispose(); $crouch.Dispose() }
}
$encoding = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $taskRootPath 'art_source/art3_crouch/proportion-check.json'), (ConvertTo-Json -InputObject $results -Depth 8), $encoding)
Write-Output 'Approved idle versus crouch part-scale comparison PASS. Silhouette widths support, but do not replace, visual review.'
