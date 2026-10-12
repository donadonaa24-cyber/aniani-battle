# Offline validation. Unity native tests/build require Claude's external runtime.
$ErrorActionPreference = 'Stop'
$taskRootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location -LiteralPath $taskRootPath
$env:DOTNET_CLI_HOME = Join-Path $taskRootPath 'TestResults/dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
function Invoke-Dotnet([string[]]$Arguments,[string]$Log) {
    & dotnet @Arguments | Tee-Object -FilePath "TestResults/$Log"
    if ($LASTEXITCODE -ne 0) { throw 'dotnet validation failed' }
}
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','TestResults/phase6b-dotnet-battle.xml') 'phase6b-dotnet-battle.log'
Invoke-Dotnet @('build','tools/Validation/UnityCompileValidation.csproj','--no-restore') 'phase6b-dotnet-unity-compile.log'
Invoke-Dotnet @('build','tools/Validation/GamePlayerCompileValidation.csproj','--no-restore') 'phase6b-dotnet-player-compile.log'
& (Join-Path $PSScriptRoot 'ValidateAssemblyReferences.ps1') -Report 'TestResults/phase6b-assembly-references.json'
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','--cpu-report','TestResults/phase6b-cpu-final') 'phase6b-cpu-final.log'
Invoke-Dotnet @('run','--project','tools/Validation/BattleValidation.csproj','--no-restore','--','--phase6b-camera') 'phase6b-camera.log'
& powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildPhase6B.ps1 | Tee-Object -FilePath TestResults/phase6b-art-build.log
if ($LASTEXITCODE -ne 0) { throw 'Art build failed' }
& powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/VerifyPhase6A.ps1 -Tag phase6b -Rebuild | Tee-Object -FilePath TestResults/phase6b-animation-verification.log
if ($LASTEXITCODE -ne 0) { throw 'Art verification failed' }
& powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/RenderPhase6BPreviews.ps1
if ($LASTEXITCODE -ne 0) { throw 'Preview rendering failed' }
Write-Output 'Phase 6B offline checks passed. Unity validation is pending. No commit or push.'
