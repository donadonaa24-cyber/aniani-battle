param([string]$Build = 'Build/size_lossless_final', [int]$Seconds = 15)
$ErrorActionPreference = 'Stop'
$taskRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$taskExe = (Resolve-Path (Join-Path $taskRoot "$Build/AniBato.exe")).Path
if (!$taskExe.StartsWith((Join-Path $taskRoot 'Build') + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Smoke target must stay in Build' }
if ($Seconds -lt 5 -or $Seconds -gt 30) { throw 'Smoke duration must be 5-30 seconds' }
$taskLog = Join-Path $taskRoot 'TestResults/size_windows_player.log'
# CLI batch mode: no OS key presses, foreground input or window capture.
$taskProcess = Start-Process -FilePath $taskExe -ArgumentList @('-batchmode','-screen-width','960','-screen-height','540','-logFile',('"'+$taskLog+'"')) -WindowStyle Hidden -PassThru
$taskPeak = 0
try {
    for ($taskSecond=0;$taskSecond -lt $Seconds;$taskSecond++) {
        Start-Sleep -Seconds 1
        $taskProcess.Refresh()
        if ($taskProcess.HasExited) { throw "Player exited during startup: $($taskProcess.ExitCode)" }
        $taskPeak = [Math]::Max($taskPeak,$taskProcess.WorkingSet64)
    }
    $taskLogStream = [IO.File]::Open($taskLog,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
    $taskLogReader = [IO.StreamReader]::new($taskLogStream)
    try { $taskText = $taskLogReader.ReadToEnd() } finally { $taskLogReader.Dispose(); $taskLogStream.Dispose() }
    if ($taskText -notmatch 'Initialize engine version') { throw 'No Unity runtime initialization in log' }
    $taskFailures = [regex]::Matches($taskText,'(?im)^.*(?:NullReferenceException|MissingReferenceException|TypeLoadException|MissingMethodException|Failed to read|Failed to load|corrupt|couldn.t load scene).*$')
    if ($taskFailures.Count) { throw ($taskFailures.Value -join "`n") }
    $taskResult = [ordered]@{build=$Build;seconds=$Seconds;runtimeStarted=$true;stayedAlive=$true;startupErrors=0;peakWorkingSetBytes=$taskPeak;mode='CLI batch; startup only; gameplay covered by Unity PlayMode suite'}
    $taskResult | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskRoot 'TestResults/size_windows_smoke.json') -Encoding utf8
    $taskResult | ConvertTo-Json
} finally {
    if (!$taskProcess.HasExited) {
        $taskLive = Get-Process -Id $taskProcess.Id -ErrorAction SilentlyContinue
        if ($taskLive -and $taskLive.Path -eq $taskExe) { Stop-Process -Id $taskProcess.Id; $taskProcess.WaitForExit() }
    }
    $taskProcess.Dispose()
}
