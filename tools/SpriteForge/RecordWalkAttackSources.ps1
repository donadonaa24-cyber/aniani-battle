$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
Add-Type -AssemblyName System.Drawing
$selection = Get-Content 'art_source/art3_walk_attacks/source-selection.json' -Raw -Encoding UTF8 | ConvertFrom-Json
$records = @()
Get-ChildItem 'art_source/art3_walk_attacks/generated' -Filter '*.png' -File | Sort-Object Name | ForEach-Object {
    if ($_.Name -notmatch '^(toma|ren|chizuru)_(walk_inbetweens|walk|dash|attack1|attack2|attack3|bag_throw)_keys_v(\d+)\.png$') { throw 'Unknown source file.' }
    $key=$Matches[1];$group=$Matches[2];$version=[int]$Matches[3]
    $relative='art_source/art3_walk_attacks/generated/'+$_.Name
    $selected=$selection.PSObject.Properties[$key+'/'+$group]
    $adopted=$selected -and $selected.Value -and $relative -eq $selected.Value
    $image=[Drawing.Image]::FromFile($_.FullName)
    try { $records += [ordered]@{character=$key;group=$group;version=$version;file=$relative;width=$image.Width;height=$image.Height;adopted=$adopted;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash} }
    finally {$image.Dispose()}
}
[IO.File]::WriteAllText((Join-Path $taskRootPath 'art_source/art3_walk_attacks/source-inventory.json'),(ConvertTo-Json -InputObject $records -Depth 6),(New-Object Text.UTF8Encoding($false)))
Write-Output "Walk/attack source inventory: $($records.Count) raw PNGs, $(($records | Where-Object adopted).Count) adopted; dimensions and SHA-256 saved."
