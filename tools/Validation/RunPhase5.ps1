# Offline checks only. Unity native tests/builds must be run outside the sandbox.
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
foreach ($project in 'BattleValidation', 'UnityCompileValidation', 'GamePlayerCompileValidation') {
    Invoke-Dotnet @('restore', "tools/Validation/$project.csproj", '--configfile', 'tools/Validation/NuGet.Config') "phase5-restore-$project.log"
}
Invoke-Dotnet @('run', '--project', 'tools/Validation/BattleValidation.csproj', '--no-restore', '--', 'TestResults/phase5-dotnet-battle.xml') 'phase5-dotnet-battle.log'
Invoke-Dotnet @('build', 'tools/Validation/UnityCompileValidation.csproj', '--no-restore') 'phase5-dotnet-unity-compile.log'
Invoke-Dotnet @('build', 'tools/Validation/GamePlayerCompileValidation.csproj', '--no-restore') 'phase5-dotnet-player-compile.log'
& (Join-Path $PSScriptRoot 'ValidateAssemblyReferences.ps1') -Report 'TestResults/phase5-assembly-references.json'
Invoke-Dotnet @('run', '--project', 'tools/Validation/BattleValidation.csproj', '--no-restore', '--', '--cpu-report-before', 'TestResults/phase5-cpu-gauge-before') 'phase5-cpu-before.log'
Invoke-Dotnet @('run', '--project', 'tools/Validation/BattleValidation.csproj', '--no-restore', '--', '--cpu-report', 'TestResults/phase5-cpu-final') 'phase5-cpu-final.log'
Copy-Item -LiteralPath (Join-Path $taskRootPath 'TestResults/phase5-cpu-final.md') -Destination (Join-Path $taskRootPath 'docs/CPU_BALANCE_REPORT.md')
Write-Output 'Phase 5 offline checks passed. Unity native validation is still required.'
