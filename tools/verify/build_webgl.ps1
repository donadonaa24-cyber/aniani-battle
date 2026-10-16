param([string]$Tag='webgl', [switch]$RestoreWindows)
$ErrorActionPreference='Stop'
$Root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$Project=Join-Path $Root 'AniBato'
$Unity='C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe'
$Results=Join-Path $Root 'TestResults'
New-Item -ItemType Directory -Force $Results | Out-Null
if(Test-Path (Join-Path $Project 'Temp/UnityLockfile')){
 try{[IO.File]::Open((Join-Path $Project 'Temp/UnityLockfile'),'Open','ReadWrite','None').Close()}catch{throw 'AniBato editor is open.'}
}
$log=Join-Path $Results ($Tag+'_WebGL.log')
$p=Start-Process -FilePath $Unity -ArgumentList @('-batchmode','-quit','-buildTarget','WebGL','-projectPath',('"'+$Project+'"'),'-executeMethod','AniBato.EditorTools.WebGLBuilder.Build','-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
$null=$p.Handle;$p.WaitForExit();$code=$p.ExitCode
Write-Host "WebGL=$code"
Select-String -Path $log -Pattern 'error CS|Exception:|WEBGL_REPORT|Build completed' | Select-Object -Last 15 | ForEach-Object {$_.Line}
if($code -ne 0){exit $code}
if($RestoreWindows){
 $r=Start-Process -FilePath $Unity -ArgumentList @('-batchmode','-quit','-buildTarget','Win64','-projectPath',('"'+$Project+'"'),'-logFile',('"'+(Join-Path $Results ($Tag+'_RestoreWindows.log'))+'"')) -WindowStyle Hidden -PassThru
 $null=$r.Handle;$r.WaitForExit();exit $r.ExitCode
}
