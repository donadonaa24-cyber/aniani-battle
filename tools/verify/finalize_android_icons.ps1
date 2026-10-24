param([Parameter(Mandatory=$true)][string]$Project,[string]$Tag='android')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskProject=[IO.Path]::GetFullPath($Project).TrimEnd('\','/')
$taskGradle=Join-Path $taskProject 'Library/Bee/Android/Prj/IL2CPP/Gradle'
$taskTarget=Join-Path $taskGradle 'launcher/src/main/res/mipmap-mdpi/app_icon.png'
if(-not $taskTarget.StartsWith($taskProject+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Generated launcher must stay in this project.'}
if((Get-FileHash (Join-Path $taskProject 'ProjectSettings/ProjectVersion.txt')).Hash -ne (Get-FileHash (Join-Path $taskRoot 'AniBato/ProjectSettings/ProjectVersion.txt')).Hash){throw 'Project marker mismatch.'}
if(-not (Test-Path $taskTarget) -or -not (Select-String -Path (Join-Path $taskGradle 'launcher/build.gradle') -SimpleMatch 'com.aniani.anibato' -Quiet)){throw 'Expected generated AniBato launcher is missing.'}
$taskSource=Join-Path $taskRoot 'art_source/app_icon/legacy-mdpi.png'
$taskApk=Join-Path $taskRoot 'Build/Android/AniBato-arm64.apk'
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Get-ProtectedPayload([string]$file){
 $taskSnapshot=@{};$taskZip=[IO.Compression.ZipFile]::OpenRead($file)
 try{foreach($taskEntry in $taskZip.Entries){if($taskEntry.FullName -match '^(assets/|lib/|classes.*\.dex$|AndroidManifest\.xml$)'){
  $taskStream=$taskEntry.Open();$taskHasher=[Security.Cryptography.SHA256]::Create()
  try{$taskSnapshot[$taskEntry.FullName]=[BitConverter]::ToString($taskHasher.ComputeHash($taskStream)).Replace('-','')}finally{$taskStream.Dispose();$taskHasher.Dispose()}
 }}}finally{$taskZip.Dispose()}
 return $taskSnapshot
}
$taskBefore=Get-ProtectedPayload $taskApk
if($taskBefore.Count -lt 10){throw 'Unexpected game payload.'}
# Unity 6000.6 exposes Adaptive slots but ships a fixed mdpi compatibility image.
# Override only that generated Gradle resource, then rebuild/sign with the same toolchain.
Copy-Item -LiteralPath $taskSource -Destination $taskTarget -Force
$taskRuntime='C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/PlaybackEngines/AndroidPlayer'
$taskLauncher=@(Get-ChildItem (Join-Path $taskRuntime 'Tools/gradle/lib/gradle-launcher-*.jar'))
if($taskLauncher.Count -ne 1){throw 'Gradle launcher is ambiguous.'}
$taskOutput=Join-Path $taskRoot ('TestResults/'+$Tag+'_Launcher.log')
$taskErrors=Join-Path $taskRoot ('TestResults/'+$Tag+'_Launcher.err.log')
$taskWatch=[Diagnostics.Stopwatch]::StartNew()
$taskArgs=@('-classpath',('"'+$taskLauncher[0].FullName+'"'),'org.gradle.launcher.GradleMain','--no-daemon','--offline',':launcher:assembleRelease')
$taskProcess=Start-Process -FilePath (Join-Path $taskRuntime 'OpenJDK/bin/java.exe') -ArgumentList $taskArgs -WorkingDirectory $taskGradle -WindowStyle Hidden -RedirectStandardOutput $taskOutput -RedirectStandardError $taskErrors -PassThru
$null=$taskProcess.Handle;$taskProcess.WaitForExit();if($taskProcess.ExitCode -ne 0){throw ('Gradle launcher rebuild failed: '+$taskProcess.ExitCode)}
$taskRebuilt=Join-Path $taskGradle 'launcher/build/outputs/apk/release/launcher-release.apk'
$taskAfter=Get-ProtectedPayload $taskRebuilt
if($taskAfter.Count -ne $taskBefore.Count){throw 'Game payload entry count changed.'}
foreach($taskName in $taskBefore.Keys){if($taskAfter[$taskName] -ne $taskBefore[$taskName]){throw ('Game payload changed: '+$taskName)}}
Copy-Item -LiteralPath $taskRebuilt -Destination $taskApk -Force
$taskWatch.Stop()
$taskReportPath=Join-Path $taskRoot 'TestResults/android-build.json'
$taskReport=Get-Content $taskReportPath -Raw | ConvertFrom-Json
$taskReport.bytes=(Get-Item $taskApk).Length
$taskReport | Add-Member -NotePropertyName launcherFinalized -NotePropertyValue $true -Force
$taskReport | Add-Member -NotePropertyName launcherSeconds -NotePropertyValue $taskWatch.Elapsed.TotalSeconds -Force
[IO.File]::WriteAllText($taskReportPath,($taskReport | ConvertTo-Json))
$taskProof=@{source='art_source/app_icon/legacy-mdpi.png';adaptiveUnchanged=$true;protectedGameEntries=$taskBefore.Count;gamePayloadUnchanged=$true;bytes=(Get-Item $taskApk).Length;gradleOffline=$true}
[IO.File]::WriteAllText((Join-Path $taskRoot 'TestResults/android-launcher-finalization.json'),($taskProof | ConvertTo-Json))
Write-Host ('Launcher finalized; '+$taskBefore.Count+' game payload entries unchanged.')
