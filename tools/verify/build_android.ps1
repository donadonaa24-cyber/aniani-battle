param([string]$Tag='android', [switch]$RestoreWindows, [switch]$AliasCheckOnly)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskProject=Join-Path $taskRoot 'AniBato'
$taskOriginalProject=$taskProject
$taskUnity='C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe'
$taskResults=Join-Path $taskRoot 'TestResults'
if(Test-Path (Join-Path $taskProject 'Temp/UnityLockfile')){
 try{[IO.File]::Open((Join-Path $taskProject 'Temp/UnityLockfile'),'Open','ReadWrite','None').Close()}catch{throw 'AniBato editor is open.'}
}
New-Item -ItemType Directory -Force -Path $taskResults | Out-Null
function Invoke-Target([string]$target,[string]$method,[string]$log){
 $taskArgs=@('-batchmode','-quit','-buildTarget',$target,'-projectPath',('"'+$taskProject+'"'),'-logFile',('"'+$log+'"'))
 if($method){$taskArgs+=@('-executeMethod',$method)}
 $taskProcess=Start-Process -FilePath $taskUnity -ArgumentList $taskArgs -WorkingDirectory $taskProject -WindowStyle Hidden -PassThru
 $null=$taskProcess.Handle;$taskProcess.WaitForExit();return $taskProcess.ExitCode
}
$taskAliasDrive=$null
$taskAliasProbe=$null
$taskSubst=Join-Path $env:WINDIR 'System32/subst.exe'
$taskCode=1
try{
 # Android rejects a non-ASCII project path. Alias this same workspace without moving any files.
 if($taskProject -match '[^\x00-\x7F]'){
  $taskBusy=@([Environment]::GetLogicalDrives() | ForEach-Object {$_.Substring(0,2).ToUpperInvariant()})
  $taskBusy+=@(& $taskSubst | ForEach-Object {if($_ -match '^([A-Z]:)'){$matches[1]}})
  foreach($taskLetter in 'Q','R','S','T','U','V','W','X','Y','Z'){
   if($taskBusy -notcontains ($taskLetter+':')){$taskAliasDrive=$taskLetter+':';break}
  }
  if(-not $taskAliasDrive){throw 'No unused drive letter for the temporary ASCII alias.'}
  $taskProbeName='android-alias-'+[guid]::NewGuid().ToString('N')+'.txt'
  $taskProbeToken=[guid]::NewGuid().ToString('N')
  $taskAliasProbe=Join-Path $taskResults $taskProbeName
  [IO.File]::WriteAllText($taskAliasProbe,$taskProbeToken)
  & $taskSubst $taskAliasDrive $taskRoot
  if($LASTEXITCODE -ne 0){$taskAliasDrive=$null;throw 'Could not create ASCII project alias.'}
  $taskAliasedRoot=$taskAliasDrive+'\'
  $taskAliasedProbe=Join-Path (Join-Path $taskAliasedRoot 'TestResults') $taskProbeName
  $taskOriginalMarker=Join-Path $taskRoot 'AniBato/ProjectSettings/ProjectVersion.txt'
  $taskAliasMarker=Join-Path $taskAliasedRoot 'AniBato/ProjectSettings/ProjectVersion.txt'
  if((Get-FileHash -LiteralPath $taskOriginalMarker).Hash -ne (Get-FileHash -LiteralPath $taskAliasMarker).Hash -or [IO.File]::ReadAllText($taskAliasedProbe) -ne $taskProbeToken){throw 'Alias does not reference the intended project.'}
  $taskProject=Join-Path $taskAliasedRoot 'AniBato'
  Write-Host "Temporary ASCII project path: $taskProject"
 }
 if($AliasCheckOnly){$taskCode=0;Write-Host 'ASCII alias marker check passed.'}
 else{
  $taskCode=Invoke-Target 'Android' 'AniBato.EditorTools.AndroidBuilder.Build' (Join-Path $taskResults ($Tag+'_Android.log'))
  Write-Host "Android=$taskCode"
  if($RestoreWindows){$taskProject=$taskOriginalProject;$taskRestore=Invoke-Target 'Win64' '' (Join-Path $taskResults ($Tag+'_RestoreWindows.log'));Write-Host "RestoreWindows=$taskRestore";if($taskRestore -ne 0){$taskCode=$taskRestore}}
 }
}finally{
 if($taskAliasDrive){
  # A unique workspace marker avoids parsing localized/native command output.
  if($taskAliasedProbe -and [IO.File]::Exists($taskAliasedProbe) -and [IO.File]::ReadAllText($taskAliasedProbe) -eq $taskProbeToken){
   & $taskSubst $taskAliasDrive /d
   if($LASTEXITCODE -ne 0){Write-Warning 'Temporary drive could not be detached.'}
  }else{Write-Warning 'Drive mapping changed; left untouched.'}
 }
 if($taskAliasProbe -and [IO.File]::Exists($taskAliasProbe) -and [IO.File]::ReadAllText($taskAliasProbe) -eq $taskProbeToken){Remove-Item -LiteralPath $taskAliasProbe}
}
exit $taskCode
