param([string]$Apk='Build/Android/AniBato-arm64.apk')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskApk=[IO.Path]::GetFullPath((Join-Path $taskRoot $Apk))
if(-not $taskApk.StartsWith($taskRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'APK must stay inside this project.'}
$taskRuntime='C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$taskTools=Join-Path $taskRuntime 'SDK/build-tools/36.0.0'
$env:JAVA_HOME=Join-Path $taskRuntime 'OpenJDK'
$taskBadging=& (Join-Path $taskTools 'aapt.exe') dump badging $taskApk
if($LASTEXITCODE -ne 0){throw 'aapt failed.'}
$taskSigning=& (Join-Path $taskTools 'apksigner.bat') verify --verbose $taskApk
if($LASTEXITCODE -ne 0){throw 'APK signature failed.'}
if(-not ($taskBadging -match "package: name='com.aniani.anibato'")){throw 'Wrong package.'}
if(-not ($taskBadging -match "sdkVersion:'26'")){throw 'Wrong minimum API.'}
if(-not ($taskBadging -match "native-code: 'arm64-v8a'")){throw 'Wrong ABI.'}
if(-not ($taskBadging -match 'launchable-activity:')){throw 'No launchable activity.'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$taskZip=[IO.Compression.ZipFile]::OpenRead($taskApk)
try{
 $taskLibraries=@($taskZip.Entries | Where-Object {$_.FullName -match '^lib/'} | ForEach-Object {$_.FullName})
 foreach($taskLib in 'lib/arm64-v8a/libunity.so','lib/arm64-v8a/libil2cpp.so'){
  if($taskLibraries -notcontains $taskLib){throw ('Missing '+$taskLib)}
 }
 if($taskLibraries | Where-Object {$_ -notmatch '^lib/arm64-v8a/'}){throw 'Unexpected ABI.'}
 $taskResult=[ordered]@{
  apk=$Apk;bytes=(Get-Item -LiteralPath $taskApk).Length;sha256=(Get-FileHash -LiteralPath $taskApk -Algorithm SHA256).Hash.ToLowerInvariant()
  signatureVerified=$true;minimumApi=26;package='com.aniani.anibato';abi='arm64-v8a';il2cpp=$true;unityRuntime=$true
  manifest=@($taskBadging | Where-Object {$_ -match '^(package:|sdkVersion:|targetSdkVersion:|launchable-activity:|native-code:|application:)'})
  signatureSchemes=@($taskSigning | Where-Object {$_ -match '^Verified using '})
  deviceVerified=$false
 }
 $taskJson=$taskResult | ConvertTo-Json -Depth 5
 [IO.File]::WriteAllText((Join-Path $taskRoot 'TestResults/android-inspection.json'),$taskJson)
 Write-Output $taskJson
}finally{$taskZip.Dispose()}
