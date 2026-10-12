param([switch]$Rebuild)
$ErrorActionPreference = 'Stop'
Add-Type -Path @((Join-Path $PSScriptRoot 'Manifest.cs'),(Join-Path $PSScriptRoot 'ImageProcessing.cs'),(Join-Path $PSScriptRoot 'AnimationSmoothing.cs'),(Join-Path $PSScriptRoot 'SpriteForge.cs')) -ReferencedAssemblies System.Drawing,System.Web.Extensions,System.Core
$serializer = New-Object System.Web.Script.Serialization.JavaScriptSerializer
$serializer.MaxJsonLength = 16777216
$inventory = New-Object System.Collections.Generic.List[object]
$repro = New-Object System.Collections.Generic.List[object]
$totalFrames = 0
foreach ($character in 'toma','ren','chizuru') {
    $manifestPath = Join-Path (Get-Location) "art_source/$character.manifest.json"
    $manifest = $serializer.Deserialize([System.IO.File]::ReadAllText($manifestPath),[AniBato.SpriteForge.Manifest])
    [AniBato.SpriteForge.Forge]::Run($manifestPath,$true)
    $states = New-Object System.Collections.Generic.List[int]
    foreach ($animation in $manifest.animations) {
        $sourcePath = Join-Path (Get-Location) "$($manifest.sourceDirectory)/$($animation.name).png"
        $source = [AniBato.SpriteForge.Raster]::Load($sourcePath)
        [AniBato.SpriteForge.Pixels]::Key($source,$manifest.keyTolerance)
        [AniBato.SpriteForge.Pixels]::RemoveSpecks($source,$(if($animation.anchor -eq 'effect'){12}else{70}))
        $sourceBounds = [AniBato.SpriteForge.Pixels]::Split($source,12)
        if ($sourceBounds.Count -ne $animation.frames) { throw "$character/$($animation.name): raw split mismatch" }
        $stem = "$($manifest.outputDirectory)/$($animation.name)"
        $metadata = $serializer.Deserialize([System.IO.File]::ReadAllText((Join-Path (Get-Location) "$stem.json")),[AniBato.SpriteForge.Metadata])
        $sheet = [AniBato.SpriteForge.Raster]::Load((Join-Path (Get-Location) "$stem.png"))
        for ($index=0; $index -lt $metadata.frameCount; $index++) {
            $bounds = [AniBato.SpriteForge.Pixels]::Bounds($sheet,($index*$manifest.cellWidth),(($index+1)*$manifest.cellWidth))
            if ($metadata.headHeights[$index] -ne $manifest.baseline-$bounds.Top+1) { throw "$character/$($animation.name): head height does not match opaque top" }
            $totalFrames++
        }
        foreach ($binding in $metadata.bindings) {
            if ($binding.Clip -ne $animation.name -or $binding.ActiveFrame -lt 0 -or $binding.ActiveFrame -ge $metadata.frameCount) { throw 'Invalid binding clip/active frame' }
            $states.Add($binding.State)
        }
        foreach ($relativePath in @("$($manifest.sourceDirectory)/$($animation.name).png","$stem.png","$stem.json")) {
            $file = Get-Item -LiteralPath $relativePath
            $inventory.Add([ordered]@{path=$relativePath;bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $relativePath -Algorithm SHA256).Hash})
        }
    }
    if ((($states | Sort-Object) -join ',') -ne ((0..23) -join ',')) { throw "${character}: missing or duplicate animation state binding" }
    Write-Output "$character contracts: raw frame counts,24 unique bindings, active pose indices, opaque head heights PASS"
    if ($Rebuild -and $character -ne 'chizuru') {
        $paths = @($manifest.animations | ForEach-Object { "$($manifest.outputDirectory)/$($_.name).png"; "$($manifest.outputDirectory)/$($_.name).json" }) + @("$($manifest.outputDirectory)/forge-report.json")
        $before = @{}
        foreach ($path in $paths) { $before[$path] = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }
        [AniBato.SpriteForge.Forge]::Run($manifestPath,$false)
        foreach ($path in $paths) {
            $after = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
            if ($before[$path] -ne $after) { throw "Non-reproducible output: $path" }
            $repro.Add([ordered]@{path=$path;sha256=$after;matches=$true})
        }
    }
}
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path (Get-Location) 'docs/art_wip/art2_inventory.json'),($inventory | ConvertTo-Json -Depth 6),$utf8)
if ($Rebuild) {
    [System.IO.File]::WriteAllText((Join-Path (Get-Location) 'TestResults/art2-spriteforge-reproducibility.json'),($repro | ConvertTo-Json -Depth 6),$utf8)
    Write-Output "Reproducibility: $($repro.Count) PNG/JSON/report files have identical SHA-256 after rebuilding."
}
Write-Output "ART2 contracts PASS: $totalFrames frames across all three characters."
