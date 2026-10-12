$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName PresentationCore
# Source-faithful cut-in: Windows' installed WIC WebP decoder, no downloads.
$cutinUri = New-Object System.Uri((Join-Path (Get-Location) 'docs/art_reference/characters/toma_keyvisual.webp'))
$decoder = [System.Windows.Media.Imaging.BitmapDecoder]::Create($cutinUri,[System.Windows.Media.Imaging.BitmapCreateOptions]::PreservePixelFormat,[System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad)
$resized = New-Object System.Windows.Media.Imaging.TransformedBitmap($decoder.Frames[0],(New-Object System.Windows.Media.ScaleTransform(0.5,0.5)))
$encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
$encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($resized))
$stream = [System.IO.File]::Create((Join-Path (Get-Location) 'AniBato/Assets/_Project/Art/Characters/toma-keyvisual-cutin.png'))
try { $encoder.Save($stream) } finally { $stream.Dispose() }
# All idle cells use PPU50 and baseline159. Draw the first frame at the SAME 3x
# nearest-neighbor scale, without fitting individual silhouettes to a common box.
$canvas = New-Object System.Drawing.Bitmap(1500,600)
$graphics = [System.Drawing.Graphics]::FromImage($canvas)
$font = New-Object System.Drawing.Font('Consolas',18)
try {
    $graphics.Clear([System.Drawing.Color]::FromArgb(34,39,49))
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half
    $graphics.DrawLine([System.Drawing.Pens]::Gray,0,510,1500,510)
    $characters = @(@{key='toma';label='Toma / 124px'},@{key='chizuru';label='Chizuru / 112px'},@{key='ren';label='Ren / 100px'})
    for ($index=0; $index -lt $characters.Length; $index++) {
        $character = $characters[$index]
        $metadata = Get-Content -Raw -Encoding utf8 "AniBato/Assets/_Project/Art/Sprites/$($character.key)/idle.json" | ConvertFrom-Json
        if ($metadata.pixelsPerUnit -ne 50 -or $metadata.baseline -ne 159) { throw 'Lineup requires matching PPU/baseline.' }
        $sheet = [System.Drawing.Bitmap]::FromFile((Join-Path (Get-Location) "AniBato/Assets/_Project/Art/Sprites/$($character.key)/idle.png"))
        try {
            $centerX = 250 + $index*500
            $destination = New-Object System.Drawing.Rectangle(($centerX-$metadata.cellWidth*3/2),30,($metadata.cellWidth*3),($metadata.cellHeight*3))
            $source = New-Object System.Drawing.Rectangle(0,0,$metadata.cellWidth,$metadata.cellHeight)
            $graphics.DrawImage($sheet,$destination,$source,[System.Drawing.GraphicsUnit]::Pixel)
            $graphics.DrawString($character.label,$font,[System.Drawing.Brushes]::White,($centerX-130),535)
        } finally { $sheet.Dispose() }
    }
    $canvas.Save((Join-Path (Get-Location) 'docs/art_wip/all_characters_lineup.png'),[System.Drawing.Imaging.ImageFormat]::Png)
} finally { $font.Dispose(); $graphics.Dispose(); $canvas.Dispose() }
Write-Output 'ART2 previews: source cut-in 512x768; lineup at common 3x scale / PPU50.'
