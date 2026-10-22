param([string]$Tag='android', [switch]$RestoreWindows)
$ErrorActionPreference='Stop'
$taskRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskProject=Join-Path $taskRoot 'AniBato'
$taskUnity='C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe'
$taskResults=Join-Path $taskRoot 'TestResults'
if(Test-Path (Join-Path $taskProject 'Temp/UnityLockfile')){
 try{[IO.File]::Open((Join-Path $taskProject 'Temp/UnityLockfile'),'Open','ReadWrite','None').Close()}catch{throw 'AniBato editor is open.'}
}
New-Item -ItemType Directory -Force -Path $taskResults | Out-Null
function Invoke-Target([string]$target,[string]$method,[string]$log){
 $taskArgs=@('-batchmode','-quit','-buildTarget',$target,'-projectPath',('"'+$taskProject+'"'),'-logFile',('"'+$log+'"'))
 if($method){$taskArgs+=@('-executeMethod',$method)}
 $taskProcess=Start-Process -FilePath $taskUnity -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
 $null=$taskProcess.Handle;$taskProcess.WaitForExit();return $taskProcess.ExitCode
}
$taskCode=Invoke-Target 'Android' 'AniBato.EditorTools.AndroidBuilder.Build' (Join-Path $taskResults ($Tag+'_Android.log'))
Write-Host "Android=$taskCode"
if($RestoreWindows){$taskRestore=Invoke-Target 'Win64' '' (Join-Path $taskResults ($Tag+'_RestoreWindows.log'));Write-Host "RestoreWindows=$taskRestore";if($taskRestore -ne 0){exit $taskRestore}}
exit $taskCode
