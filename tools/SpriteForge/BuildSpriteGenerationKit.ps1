$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
& (Join-Path $PSScriptRoot 'CompareArt3Proportions.ps1')
$kitPath = Join-Path $taskRootPath 'art_source/sprite_generation_kit'
New-Item -ItemType Directory -Force -Path $kitPath,(Join-Path $kitPath 'references'),(Join-Path $kitPath 'generated'),(Join-Path $kitPath 'ready/crouch'),(Join-Path $kitPath 'ready/idle'),(Join-Path $kitPath 'ready/actions'),(Join-Path $kitPath 'actions/tools') | Out-Null
Copy-Item -LiteralPath 'docs/SPRITE_GENERATION_GUIDE.md' -Destination (Join-Path $kitPath 'README.md')
Copy-Item -LiteralPath 'art_source/art3_crouch/proportion-check.json' -Destination (Join-Path $kitPath 'proportion-check.json')
$palettes = [ordered]@{}
$characters = @()
$prompts = @()
$normalizations = @()
$log = Get-Content -LiteralPath 'art_source/art3_crouch/generation-log.json' -Encoding UTF8 | ConvertFrom-Json
$normalization = Get-Content -LiteralPath 'art_source/art3_crouch/normalization.json' -Encoding UTF8 | ConvertFrom-Json
foreach ($key in 'toma','ren','chizuru') {
    Copy-Item -LiteralPath "art_source/art3_idle/$($key)_idle_v1.png" -Destination (Join-Path $kitPath "references/$($key)_approved_idle.png")
    Copy-Item -LiteralPath "art_source/art3_idle/breathing/ready/$key/idle_00.png" -Destination (Join-Path $kitPath "references/$($key)_standing_224x160.png")
    Copy-Item -LiteralPath "art_source/art3_crouch/ready/$key/crouch_00.png" -Destination (Join-Path $kitPath "references/$($key)_crouch_224x160.png")
    Copy-Item -LiteralPath "art_source/art3_crouch/generated/$($key)_crouch_pair_v1.png" -Destination (Join-Path $kitPath "generated/$($key)_crouch_pair_v1.png")
    Copy-Item -LiteralPath "art_source/art3_crouch/ready/$key" -Destination (Join-Path $kitPath 'ready/crouch') -Recurse -Force
    Copy-Item -LiteralPath "art_source/art3_idle/breathing/ready/$key" -Destination (Join-Path $kitPath 'ready/idle') -Recurse -Force
    $report = Get-Content -LiteralPath "AniBato/Assets/_Project/Art/Sprites/$key/forge-report.json" -Encoding UTF8 | ConvertFrom-Json
    $palettes[$key] = @($report.palette)
    $metadata = Get-Content -LiteralPath "art_source/art3_crouch/ready/$key/crouch.json" -Encoding UTF8 | ConvertFrom-Json
    $characters += [ordered]@{ key=$key; standingHeight=$metadata.targetHeight; crouchNeutralHeight=$metadata.headHeights[0]; crouchHeadHeights=@($metadata.headHeights)
        authoritativeReference="references/$($key)_standing_224x160.png"; referenceSha256=(Get-FileHash -LiteralPath "art_source/art3_idle/breathing/ready/$key/idle_00.png").Hash }
    $generated = $log.images | Where-Object character -eq $key
    $prompts += [ordered]@{ character=$key; prompt=$generated.prompt; transparent_background=$true
        images=@([ordered]@{ file="references/$($key)_approved_idle.png"; role='Approved identity and costume; Image 1' },
                 [ordered]@{ file="references/$($key)_standing_224x160.png"; role='Authoritative pixel scale and palette; Image 2' }) }
    $record = $normalization | Where-Object character -eq $key
    $record.source = "generated/$($key)_crouch_pair_v1.png"
    $normalizations += $record
}
$spec = [ordered]@{ schemaVersion=2; game='AniBato'; cellWidth=224; cellHeight=160; baseline=159; torsoAnchorX=112
    coordinateOrigin='top-left'; pixelsPerUnit=50; pivot=@(0.5,0); alphaValues=@(0,255); maxColorsPerCharacter=32
    frames=8; fps=6; sheetWidth=1792; sheetHeight=160; fixedFeetRows=@(148,159); fullTranslationEndRow=132
    breathingOffsets=@(0,-1,-2,-1,0,1,2,1); rounding='midpoint-to-even'; previewScale=3; previewGifDelayCentiseconds=17
    interpolation='Point'; mipmaps=$false; compression='Uncompressed'; independentPoseResize=$false
    partScaleCheck='proportion-check.json'; compareHeadAndShoesWithApprovedIdle=$true; characters=$characters
    groundJumpStartupTicks=2; landingTicks=8; shallowHeight=@{toma=104;ren=89;chizuru=98}; groundJumpState=15; jumpStartupAnimationState=34
    actionNormalization='actions/normalized/*-scale.json'; actionSelection='actions/source-selection.json'; actionPrompts='actions/generation-log.json'
    motions=@(@{name='jump';frames=6;fps=15;loop=$false;keys=@(1,1,1,2,2,2);state=3},
      @{name='fall';frames=6;fps=15;loop=$true;keys=@(3,3,3,3,3,3);state=4},
      @{name='land';frames=2;fps=15;loop=$false;keys=@(1,-1);state=5;aliasState=34;negativeKey='approved-idle-00'},
      @{name='guard';frames=6;fps=15;loop=$true;keys=@(1,1,1,1,1,1);state=15},
      @{name='dodge';frames=3;fps=12;loop=$false;keys=@(1,2,3);state=16},
      @{name='dash';frames=8;fps=15;loop=$true;keys=@(1,1,2,2,3,3,4,4);state=2}) }
foreach ($folder in 'generated','normalized') {
    Copy-Item -LiteralPath "art_source/art3_actions/$folder" -Destination (Join-Path $kitPath 'actions') -Recurse -Force
}
foreach ($name in 'generation-log.json','source-selection.json','source-inventory.json','verification.json','README.md') {
    Copy-Item -LiteralPath "art_source/art3_actions/$name" -Destination (Join-Path $kitPath "actions/$name") -Force
}
foreach ($key in 'toma','ren','chizuru') {
    Copy-Item -LiteralPath "art_source/art3_actions/ready/$key" -Destination (Join-Path $kitPath 'ready/actions') -Recurse -Force
}
foreach ($name in 'Manifest.cs','ImageProcessing.cs','AnimationSmoothing.cs','SpriteForge.cs','AtlasPoseSplitter.cs','ActionAtlasBuilder.cs','ArtThreeActions.cs','ArtThreeIdle.cs','ArtThreeCrouch.cs','BuildArt3Actions.ps1','NormalizeActionAtlas.ps1','RecordActionSources.ps1') {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination (Join-Path $kitPath "actions/tools/$name") -Force
}
$encoding = New-Object Text.UTF8Encoding($false)
foreach ($item in @(@{name='spec.json';value=$spec},@{name='palettes.json';value=$palettes},@{name='prompts.json';value=$prompts},@{name='normalization.json';value=$normalizations})) {
    [IO.File]::WriteAllText((Join-Path $kitPath $item.name), (ConvertTo-Json -InputObject $item.value -Depth 14), $encoding)
}
$hashes = [ordered]@{}
Get-ChildItem -LiteralPath $kitPath -File -Recurse | Where-Object Name -ne 'checksums.json' | Sort-Object FullName | ForEach-Object {
    $relative = $_.FullName.Substring($kitPath.Length+1).Replace('\','/')
    $hashes[$relative] = (Get-FileHash -LiteralPath $_.FullName).Hash
}
[IO.File]::WriteAllText((Join-Path $kitPath 'checksums.json'), (ConvertTo-Json -InputObject $hashes -Depth 4), $encoding)
$zipPath = Join-Path $taskRootPath 'art_source/sprite_generation_kit.zip'
Compress-Archive -LiteralPath $kitPath -DestinationPath $zipPath -Force
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    foreach ($relative in $hashes.Keys) {
        $entryName = 'sprite_generation_kit/' + $relative
        $entry = $archive.Entries | Where-Object { $_.FullName.Replace('\','/') -eq $entryName }
        if (-not $entry) { throw "Missing portable-kit entry: $relative" }
        $stream = $entry.Open(); $sha = [Security.Cryptography.SHA256]::Create()
        try { $actual = ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','') }
        finally { $sha.Dispose(); $stream.Dispose() }
        if ($actual -ne $hashes[$relative]) { throw "Portable-kit checksum mismatch: $relative" }
    }
    Write-Output "Sprite generation kit: $($hashes.Count) files verified inside ZIP; references, palettes, prompts, guide and ready frames included."
}
finally { $archive.Dispose() }
