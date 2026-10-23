param(
    [string]$WindowsBuild = 'Build/mobile_final_2',
    [string]$AndroidApk = 'Build/Android/AniBato-arm64.apk',
    [string]$Version = 'v0.1.0-trial.20261023'
)
$ErrorActionPreference = 'Stop'
$taskRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$windowsRoot = (Resolve-Path (Join-Path $taskRoot $WindowsBuild)).Path
$apkPath = (Resolve-Path (Join-Path $taskRoot $AndroidApk)).Path
$releaseRoot = Join-Path $taskRoot "Build/Distribution/$Version"
if ($Version -notmatch '^v[0-9A-Za-z.\-]+$') { throw 'Invalid version' }
if (!$windowsRoot.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'Build must be inside workspace' }
if (!$apkPath.StartsWith($taskRoot + [IO.Path]::DirectorySeparatorChar)) { throw 'APK must be inside workspace' }
foreach ($required in @('AniBato.exe','UnityPlayer.dll','AniBato_Data','MonoBleedingEdge','ThirdPartyNotices.txt')) {
    if (!(Test-Path -LiteralPath (Join-Path $windowsRoot $required))) { throw "Missing $required" }
}
if (Test-Path -LiteralPath $releaseRoot) { throw 'Release directory exists; never overwrite published packages' }
New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
$zipPath = Join-Path $releaseRoot "AniBato-Windows-x64-$Version.zip"
$publicApk = Join-Path $releaseRoot "AniBato-Android-arm64-$Version.apk"
$readme = @"
あにあにバトル！！ / $Version（無料試作版）

Windows 64bit版：ZIPを右クリックして「すべて展開」し、展開先のAniBato.exeを起動します。
AniBato_DataやDLLを含む同じフォルダのファイル一式が必要です。
キーボード・ゲームパッドに対応。設定の操作説明・キー設定から確認できます。

Android版：Android 8.0以降・64bit ARM端末向け。APKを端末で開いてインストールします。
Google Play外の試作用署名です。端末に応じてダウンロード元アプリのインストール許可が必要です。
横持ちの半透明タッチパッドに対応。Android実機の動作・発熱・メモリ使用量は未確認です。

ダウンロード版はCPU対戦・練習・ローカル操作向けです。
招待コードによるオンライン対戦はブラウザ版をご利用ください。
https://donadonaa24-cyber.github.io/aniani-asobiba/games/anibato/
導入案内：https://donadonaa24-cyber.github.io/aniani-asobiba/anibato/#downloads

検証済みWindowsビルドとAndroid APKを梱包。ゲーム内容・原画は変更していません。
キャラクター等のイラストは生成AIを使用しています。
フォントライセンスはThirdPartyNotices.txt（APKではResources内）に含まれます。
"@
[IO.File]::WriteAllText((Join-Path $releaseRoot 'README.txt'), $readme, [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $apkPath -Destination $publicApk
Add-Type -AssemblyName System.IO.Compression
$sourceFiles = @(Get-ChildItem -LiteralPath $windowsRoot -Recurse -File)
if ($sourceFiles | Where-Object { $_.Extension -in @('.pdb','.mdb','.log','.apk','.zip') -or $_.Name -match '^(\.env|supabase\.txt)$' }) { throw 'Unexpected private/debug/archive file in build' }
$stream = [IO.File]::Open($zipPath, [IO.FileMode]::CreateNew)
$archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in $sourceFiles) {
        $relative = $file.FullName.Substring($windowsRoot.Length + 1).Replace('\','/')
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, "AniBato/$relative", [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $releaseRoot 'README.txt'), 'AniBato/README.txt', [IO.Compression.CompressionLevel]::Optimal) | Out-Null
} finally { $archive.Dispose(); $stream.Dispose() }
$zip = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    if ($zip.Entries.Count -ne $sourceFiles.Count + 1) { throw 'ZIP file count mismatch' }
    foreach ($file in $sourceFiles) {
        $relative = $file.FullName.Substring($windowsRoot.Length + 1).Replace('\','/')
        $entry = $zip.GetEntry("AniBato/$relative")
        if (!$entry -or $entry.Length -ne $file.Length) { throw "ZIP entry mismatch: $relative" }
        $inputStream = $entry.Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $digest = ([BitConverter]::ToString($sha.ComputeHash($inputStream))).Replace('-','').ToLowerInvariant() }
        finally { $sha.Dispose(); $inputStream.Dispose() }
        if ($digest -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()) { throw "ZIP data mismatch: $relative" }
    }
    $reader = [IO.StreamReader]::new($zip.GetEntry('AniBato/README.txt').Open(), [Text.Encoding]::UTF8)
    try { if ($reader.ReadToEnd() -ne $readme) { throw 'ZIP README mismatch' } }
    finally { $reader.Dispose() }
} finally { $zip.Dispose() }
$files = @($zipPath,$publicApk) | ForEach-Object {
    $file = Get-Item -LiteralPath $_
    if ($file.Length -ge 2GB) { throw 'GitHub Release asset exceeds 2 GiB' }
    [ordered]@{ name=$file.Name; bytes=$file.Length; sha256=(Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$checksums = ($files | ForEach-Object { "$($_.sha256)  $($_.name)" }) -join "`n"
[IO.File]::WriteAllText((Join-Path $releaseRoot 'SHA256SUMS.txt'), $checksums + "`n", [Text.UTF8Encoding]::new($false))
$manifest = [ordered]@{ version=$Version; sourceCommit=(git -C $taskRoot rev-parse HEAD); windowsBuild=$WindowsBuild; zipEntries=$sourceFiles.Count+1; zipAllEntriesVerified=$true; androidDeviceVerified=$false; files=$files }
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $releaseRoot 'manifest.json') -Encoding utf8
$manifest | ConvertTo-Json -Depth 6
