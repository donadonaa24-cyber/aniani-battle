param([string]$Apk='Build/Android/AniBato-arm64.apk')
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskApk=[IO.Path]::GetFullPath((Join-Path $taskRoot $Apk))
if(-not $taskApk.StartsWith($taskRoot+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'APK must stay inside workspace.'}
$taskAapt='C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/build-tools/36.0.0/aapt.exe'
$taskRelative=$taskApk.Substring($taskRoot.Length+1)
Push-Location $taskRoot
try {
 $taskBadging=& $taskAapt dump badging $taskRelative
 if($LASTEXITCODE -ne 0){throw 'Badging failed.'}
 $taskValues=@(& $taskAapt dump --values resources $taskRelative)
 if($LASTEXITCODE -ne 0){throw 'Resource dump failed.'}
 $taskIconLine=@($taskBadging | Where-Object {$_ -match '^application:.* icon='})[0]
 if($taskIconLine -notmatch "icon='([^']+)'" ){throw 'No launcher icon.'}
 $taskLauncher=$matches[1]
 $taskXml=@(& $taskAapt dump xmltree $taskRelative $taskLauncher)
 if($LASTEXITCODE -ne 0 -or -not ($taskXml -match 'E: adaptive-icon')){throw 'Launcher must resolve to adaptive icon.'}
} finally {Pop-Location}
$taskOut=Join-Path $taskRoot 'TestResults/android_app_icons'
New-Item -ItemType Directory -Force $taskOut | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$taskZip=[IO.Compression.ZipFile]::OpenRead($taskApk)
$taskEntries=@()
try {
 for($taskIndex=0;$taskIndex -lt $taskValues.Count-1;$taskIndex++){
  if($taskValues[$taskIndex] -match 'resource (0x[0-9a-f]+) com\.aniani\.anibato:mipmap/(app_icon|app_icon_round|ic_launcher_background|ic_launcher_foreground):'){
   $taskId=$matches[1];$taskKind=$matches[2]
   if($taskValues[$taskIndex+1] -match '\(string8\) "([^"]+\.png)"'){
    $taskEntry=$taskZip.GetEntry($matches[1]);if(-not $taskEntry){throw 'Missing icon bitmap.'}
    $taskName=$taskKind+'-'+$taskEntries.Count+'.png'
    [IO.Compression.ZipFileExtensions]::ExtractToFile($taskEntry,(Join-Path $taskOut $taskName),$true)
    $taskEntries+=@{kind=$taskKind;resource=$taskEntry.FullName;id=$taskId;file=('TestResults/android_app_icons/'+$taskName)}
   }
  }
 }
}finally{$taskZip.Dispose()}
foreach($taskKind in 'app_icon','ic_launcher_background','ic_launcher_foreground'){
 if(-not ($taskEntries | Where-Object {$_.kind -eq $taskKind})){throw ('Missing '+$taskKind)}
}
foreach($taskKind in 'ic_launcher_background','ic_launcher_foreground'){
 $taskIds=@($taskEntries | Where-Object {$_.kind -eq $taskKind} | ForEach-Object {$_.id} | Select-Object -Unique)
 if($taskIds.Count -ne 1 -or -not ($taskXml -match ('@'+$taskIds[0]+'\b'))){throw ('Adaptive launcher does not reference '+$taskKind)}
}
$taskReport=@{apk=$Apk;launcher=$taskLauncher;adaptiveXml=$taskXml;entries=$taskEntries}
[IO.File]::WriteAllText((Join-Path $taskOut 'resources.json'),($taskReport | ConvertTo-Json -Depth 6))
Write-Host ('Extracted '+$taskEntries.Count+' launcher bitmaps; adaptive XML verified.')
