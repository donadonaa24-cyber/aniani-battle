# Offline checks only. Native Unity validation remains the external reviewer's task.
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
$env:DOTNET_CLI_HOME = Join-Path $taskRootPath 'TestResults/dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
function Invoke-Dotnet([string[]]$Arguments, [string]$Log) {
    & dotnet @Arguments | Tee-Object -FilePath (Join-Path $taskRootPath "TestResults/$Log")
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed: $($Arguments -join ' ')" }
}
foreach ($project in 'BattleValidation','UnityCompileValidation','GamePlayerCompileValidation') {
    Invoke-Dotnet @('restore',"tools/Validation/$project.csproj",'--configfile','tools/Validation/NuGet.Config') "phase5b-restore-$project.log"
}
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','TestResults/phase5b-dotnet-battle.xml') 'phase5b-dotnet-battle.log'
Invoke-Dotnet @('build','tools/Validation/UnityCompileValidation.csproj','--no-restore') 'phase5b-dotnet-unity-compile.log'
Invoke-Dotnet @('build','tools/Validation/GamePlayerCompileValidation.csproj','--no-restore') 'phase5b-dotnet-player-compile.log'
& (Join-Path $PSScriptRoot 'ValidateAssemblyReferences.ps1') -Report 'TestResults/phase5b-assembly-references.json'
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','--cpu-report','TestResults/phase5b-cpu-final') 'phase5b-cpu-final.log'
Copy-Item -LiteralPath TestResults/phase5b-cpu-final.md -Destination docs/CPU_BALANCE_REPORT.md
& powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase5B.ps1 | Tee-Object -FilePath TestResults/phase5b-animation-verification.log
if ($LASTEXITCODE -ne 0) { throw 'Animation verification failed' }
Write-Output 'Phase 5B offline checks passed. Unity native validation is still required.'
