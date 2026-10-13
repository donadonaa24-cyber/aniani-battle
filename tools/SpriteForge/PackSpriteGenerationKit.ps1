$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$kitPath=Join-Path $taskRootPath 'art_source/sprite_generation_kit'
$selection=Get-Content -LiteralPath (Join-Path $taskRootPath 'art_source/art3_walk_attacks/source-selection.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$approved=@($selection.PSObject.Properties | Where-Object Value | ForEach-Object { [IO.Path]::GetFileName($_.Value) })
$files=@(Get-ChildItem -LiteralPath $kitPath -Recurse -File | Where-Object {
    $relative=$_.FullName.Substring($kitPath.Length+1).Replace('\','/')
    $_.Name -ne 'checksums.json' -and (-not $relative.StartsWith('walk_attacks/generated/') -or $_.Extension -ne '.png' -or $approved -contains $_.Name)
} | Sort-Object FullName)
$hashes=[ordered]@{}
foreach($file in $files){$relative=$file.FullName.Substring($kitPath.Length+1).Replace('\','/');$hashes[$relative]=(Get-FileHash -LiteralPath $file.FullName).Hash}
$manifestPath=Join-Path $kitPath 'checksums.json'
[IO.File]::WriteAllText($manifestPath,($hashes|ConvertTo-Json -Depth 4),[Text.UTF8Encoding]::new($false))
Add-Type -AssemblyName System.IO.Compression,System.IO.Compression.FileSystem
$temporary=Join-Path $taskRootPath ('art_source/sprite_generation_kit-'+[Guid]::NewGuid().ToString('N')+'.zip')
$writer=[IO.Compression.ZipFile]::Open($temporary,[IO.Compression.ZipArchiveMode]::Create)
try{
    foreach($file in @($files)+@(Get-Item -LiteralPath $manifestPath)){
        $relative=$file.FullName.Substring($kitPath.Length+1).Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($writer,$file.FullName,('sprite_generation_kit/'+$relative),[IO.Compression.CompressionLevel]::Optimal)|Out-Null
    }
}finally{$writer.Dispose()}
$size=(Get-Item -LiteralPath $temporary).Length
if($size -ge 95MB){throw 'Portable ZIP exceeds Git size limit; original ZIP retained.'}
$archive=[IO.Compression.ZipFile]::OpenRead($temporary)
try{
    foreach($relative in $hashes.Keys){
        $entry=$archive.GetEntry('sprite_generation_kit/'+$relative)
        if(-not $entry){throw ('Missing entry: '+$relative)}
        $stream=$entry.Open();$sha=[Security.Cryptography.SHA256]::Create()
        try{$actual=([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','')}finally{$sha.Dispose();$stream.Dispose()}
        if($actual -ne $hashes[$relative]){throw ('Checksum mismatch: '+$relative)}
    }
}finally{$archive.Dispose()}
$destination=Join-Path $taskRootPath 'art_source/sprite_generation_kit.zip'
if(Test-Path -LiteralPath $destination){
    $backup=Join-Path $taskRootPath ('TestResults/sprite_generation_kit-before-recovery-'+[Guid]::NewGuid().ToString('N')+'.zip')
    [IO.File]::Replace($temporary,$destination,$backup)
}else{[IO.File]::Move($temporary,$destination)}
Write-Output ('Portable kit: '+$hashes.Count+' verified files; '+[Math]::Round($size/1MB,2)+' MiB. Approved sources and every prompt retained; rejected original PNGs remain in workspace.')
