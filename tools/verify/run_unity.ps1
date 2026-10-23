# Unity verification: BuildAll -> EditMode -> PlayMode -> Windows build, then a test summary.
# Usage (from anywhere):  powershell -ExecutionPolicy Bypass -File tools\verify\run_unity.ps1 -Tag phase4
# Optional:               -Only buildall|editmode|playmode|build|summary   -Filter <test name>
# Notes:
#  - Never pass -nographics (Personal license has no headless entitlement -> exit 198).
#  - Fails if the Unity editor has AniBato open (Temp\UnityLockfile exists). Close the editor first.
#  - Must run OUTSIDE the Codex sandbox (the sandbox blocks Unity's license IPC).
#  - ASCII only on purpose: Windows PowerShell 5.1 misreads non-BOM UTF-8 literals.
param(
    [Parameter(Mandatory = $true)][string]$Tag,
    [ValidateSet('all', 'buildall', 'editmode', 'playmode', 'build', 'summary')][string]$Only = 'all',
    [string]$Filter = '',
    [string]$CaptureRoot = '',
    [ValidateSet('Lossless','BC7')][string]$WindowsTextures = 'BC7'
)
$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$Project = Join-Path $Root 'AniBato'
$Results = Join-Path $Root 'TestResults'
$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
New-Item -ItemType Directory -Force $Results | Out-Null
if($CaptureRoot){
    $capturePath=[IO.Path]::GetFullPath((Join-Path $Root $CaptureRoot))
    if(-not $capturePath.StartsWith($Root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'CaptureRoot must stay within this project.'}
    New-Item -ItemType Directory -Force $capturePath | Out-Null
    $env:ANIBATO_CAPTURE_ROOT=$capturePath
}

if (Test-Path (Join-Path $Project 'Temp\UnityLockfile')) {
    $locked = $true
    try { [IO.File]::Open((Join-Path $Project 'Temp\UnityLockfile'), 'Open', 'ReadWrite', 'None').Close(); $locked = $false } catch { }
    if ($locked) { Write-Host 'ABORT: the Unity editor has AniBato open. Close it and retry.'; exit 3 }
}

function Invoke-Unity([string[]]$UnityArgs) {
    # Start-Process -Wait waits for the whole descendant tree on Windows.
    # Unity can leave a shared compiler prewarmer alive after the batch exits.
    # Wait only for the actual Unity batch process and retain its real exit code.
    $p = Start-Process -FilePath $Unity -ArgumentList $UnityArgs -PassThru -NoNewWindow
    $null = $p.Handle
    $p.WaitForExit()
    return $p.ExitCode
}
function Q([string]$s) { return '"' + $s + '"' }

if ($Only -in 'all', 'buildall') {
    $log = Join-Path $Results "${Tag}_buildall.log"
    $code = Invoke-Unity @('-batchmode', '-quit', '-projectPath', (Q $Project), '-executeMethod', 'AniBato.EditorTools.ProjectBuilder.BuildAll', '-logFile', (Q $log))
    Write-Host "buildall=$code"
    Select-String -Path $log -Pattern 'error CS|Exception:' -Encoding UTF8 | Select-Object -First 15 | ForEach-Object { $_.Line }
    if ($code -ne 0 -and $Only -eq 'all') { Write-Host 'Stopping: BuildAll failed (see log).'; exit $code }
}
foreach ($plat in 'EditMode', 'PlayMode') {
    if ($Only -notin 'all', $plat.ToLower()) { continue }
    $xml = Join-Path $Results "${Tag}_$plat.xml"; $log = Join-Path $Results "${Tag}_$plat.log"
    $a = @('-batchmode', '-projectPath', (Q $Project), '-runTests', '-testPlatform', $plat, '-testResults', (Q $xml), '-logFile', (Q $log))
    if ($Filter) { $a += @('-testFilter', $Filter) }
    $code = Invoke-Unity $a
    Write-Host "$plat=$code"
}
if ($Only -in 'all', 'build') {
    $log = Join-Path $Results "${Tag}_build.log"
    $exe = Join-Path $Root "Build\$Tag\AniBato.exe"
    $windowsMethod = if ($WindowsTextures -eq 'BC7') { 'AniBato.EditorTools.WindowsBuilder.BuildHighQuality' } else { 'AniBato.EditorTools.WindowsBuilder.Build' }
    $code = Invoke-Unity @('-batchmode', '-quit', '-projectPath', (Q $Project), '-buildTarget', 'Win64', '-executeMethod', $windowsMethod, '-anibatoWindowsOutput', (Q $exe), '-logFile', (Q $log))
    Write-Host "winbuild=$code"
    if($code -eq 0 -and (Test-Path (Join-Path $Root 'art_source/fonts/OFL-NotoCJK.txt'))){
        Copy-Item -LiteralPath (Join-Path $Root 'art_source/fonts/OFL-NotoCJK.txt') -Destination (Join-Path (Split-Path $exe) 'ThirdPartyNotices.txt') -Force
    }
    Select-String -Path $log -Pattern 'Build Finished' -Encoding UTF8 | ForEach-Object { $_.Line }
    # Retention: Windows builds are ~100-160 MB each and every version is recoverable from git,
    # so keep only the newest $KeepBuilds build folders (this one included).
    $KeepBuilds = 2
    $builds = Get-ChildItem (Join-Path $Root 'Build') -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'AniBato.exe') } |
        Sort-Object LastWriteTime -Descending
    $buildRootPath = [IO.Path]::GetFullPath((Join-Path $Root 'Build'))
    $builds | Select-Object -Skip $KeepBuilds | ForEach-Object {
        $oldBuildPath = [IO.Path]::GetFullPath($_.FullName)
        if (-not $oldBuildPath.StartsWith($buildRootPath + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
            ($_.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Unsafe build retention path; refusing to remove it.' }
        Write-Host "removing old build: $($_.Name)"
        $inUse = Get-Process -Name AniBato -ErrorAction SilentlyContinue | Where-Object {
            try { $_.Path -and $_.Path.StartsWith($oldBuildPath + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) } catch { $false }
        }
        if ($inUse) { Write-Warning "Keeping running game build: $($_.Name)"; return }
        try { Remove-Item -LiteralPath $oldBuildPath -Recurse -Force }
        catch { Write-Warning "Old build is in use; kept for a later cleanup: $($_.Name)" }
    }
}
# Summary of test XML files for this tag (failed cases with message + first stack line).
foreach ($plat in 'EditMode', 'PlayMode') {
    $xml = Join-Path $Results "${Tag}_$plat.xml"
    if (-not (Test-Path $xml)) { continue }
    $x = [xml](Get-Content $xml -Encoding UTF8); $r = $x.'test-run'
    Write-Host "$plat total=$($r.total) passed=$($r.passed) failed=$($r.failed)"
    $x.SelectNodes('//test-case') | Where-Object { $_.result -ne 'Passed' } | ForEach-Object {
        Write-Host "  FAIL $($_.fullname)"
        $msg = $_.failure.message.'#cdata-section'; if ($msg) { Write-Host ('    ' + ($msg -replace '\s+', ' ').Trim()) }
        $st = $_.failure.'stack-trace'.'#cdata-section'; if ($st) { Write-Host ('    ' + (($st -split "`n") | Where-Object { $_ -match '_Project' } | Select-Object -First 1).Trim()) }
    }
}
