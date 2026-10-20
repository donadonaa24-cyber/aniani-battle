# Uses the current saved SO assets. Run BuildAll first after changing migrations.
param([ValidatePattern('^[a-z0-9_]+$')][string]$Label='final')
$ErrorActionPreference='Stop'
$auditRoot=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$auditProject=Join-Path $auditRoot 'AniBato'
$auditResults=Join-Path $auditRoot 'TestResults'
New-Item -ItemType Directory -Force $auditResults | Out-Null
if(Test-Path (Join-Path $auditProject 'Temp/UnityLockfile')){
 try{[IO.File]::Open((Join-Path $auditProject 'Temp/UnityLockfile'),'Open','ReadWrite','None').Close()}catch{throw 'AniBato editor is open.'}
}
$env:ANIBATO_BALANCE_LABEL=$Label
$auditLog=Join-Path $auditResults ('stamina_'+$Label+'_audit.log')
$auditArgs=@('-batchmode','-quit','-projectPath',('"'+$auditProject+'"'),'-executeMethod','AniBato.EditorTools.StaminaBalanceAudit.Run','-logFile',('"'+$auditLog+'"'))
$auditProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe' -ArgumentList $auditArgs -PassThru -WindowStyle Hidden
$null=$auditProcess.Handle
$auditProcess.WaitForExit()
Write-Output ('audit='+$auditProcess.ExitCode)
if($auditProcess.ExitCode -ne 0){exit $auditProcess.ExitCode}
Get-Content (Join-Path $auditRoot ('docs/codex_logs/stamina_'+$Label+'.md')) -Encoding UTF8
