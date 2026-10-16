param([ValidateRange(1024,65535)][int]$Port=8799)
$ErrorActionPreference='Stop'
$Root=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if(-not (Test-Path -LiteralPath (Join-Path $Root 'Build/WebGL/index.html'))){throw 'Build/WebGL/index.html is missing. Run build_webgl.ps1 first.'}
$url='http://127.0.0.1:'+$Port+'/'
try{
    $response=Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2
    if($response.Content -match 'unity-canvas'){Write-Output $url;return}
    throw 'The selected port is occupied by another server.'
}catch{
    if($_.Exception.Message -eq 'The selected port is occupied by another server.'){throw}
}
$runtimeNode=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/node/bin/node.exe'
if(-not (Test-Path -LiteralPath $runtimeNode)){$runtimeNode=(Get-Command node -ErrorAction Stop).Source}
$results=Join-Path $Root 'TestResults'
New-Item -ItemType Directory -Force $results | Out-Null
$process=Start-Process -FilePath $runtimeNode -ArgumentList @(('"'+(Join-Path $PSScriptRoot 'serve_webgl.cjs')+'"'),$Port) -WorkingDirectory $Root -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $results 'webgl_server.log') -RedirectStandardError (Join-Path $results 'webgl_server_error.log')
for($attempt=0;$attempt -lt 20;$attempt++){
    if($process.HasExited){throw 'The local WebGL server could not start. See TestResults/webgl_server_error.log.'}
    try{$null=Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 1;$process.Id | Set-Content (Join-Path $results 'webgl_server.pid');Write-Output $url;return}catch{Start-Sleep -Milliseconds 200}
}
throw 'The local WebGL server did not respond.'
