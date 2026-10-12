# Offline checks. Unity BuildAll/tests/build are performed by Claude outside the sandbox.
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
    Invoke-Dotnet @('restore',"tools/Validation/$project.csproj",'--configfile','tools/Validation/NuGet.Config') "phase6a-restore-$project.log"
}
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','TestResults/phase6a-dotnet-battle.xml') 'phase6a-dotnet-battle.log'
Invoke-Dotnet @('build','tools/Validation/UnityCompileValidation.csproj','--no-restore') 'phase6a-dotnet-unity-compile.log'
Invoke-Dotnet @('build','tools/Validation/GamePlayerCompileValidation.csproj','--no-restore') 'phase6a-dotnet-player-compile.log'
& (Join-Path $PSScriptRoot 'ValidateAssemblyReferences.ps1') -Report 'TestResults/phase6a-assembly-references.json'
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','--cpu-report','TestResults/phase6a-cpu-final') 'phase6a-cpu-final.log'
Copy-Item -LiteralPath TestResults/phase6a-cpu-final.md -Destination docs/CPU_BALANCE_REPORT.md
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','--phase6-tables') 'phase6a-balance-tables.log'
& powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase6A.ps1 -Rebuild | Tee-Object -FilePath TestResults/phase6a-animation-verification.log
if ($LASTEXITCODE -ne 0) { throw 'Animation verification failed' }
Write-Output 'Phase 6A offline checks passed. Unity native validation remains required. No commit or push.'
