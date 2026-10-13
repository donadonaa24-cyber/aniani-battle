param(
    [Parameter(Mandatory=$true)][string]$PromptFile,
    [Parameter(Mandatory=$true)][string]$GeneratedPath
)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$entry=Get-Content -LiteralPath $PromptFile -Raw -Encoding UTF8 | ConvertFrom-Json
$destination=[IO.Path]::GetFullPath((Join-Path $taskRootPath $entry.file))
$allowed=[IO.Path]::GetFullPath((Join-Path $taskRootPath 'art_source/art3_walk_attacks/generated'))+[IO.Path]::DirectorySeparatorChar
if (-not $destination.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase)) { throw 'Destination must remain in the generated source folder.' }
if (Test-Path -LiteralPath $destination) { throw 'Version already exists; never overwrite source art.' }
Copy-Item -LiteralPath $GeneratedPath -Destination $destination
$logPath=Join-Path $taskRootPath 'art_source/art3_walk_attacks/generation-log.json'
$log=Get-Content -LiteralPath $logPath -Raw -Encoding UTF8 | ConvertFrom-Json
$log.entries=@($log.entries)+@($entry)
[IO.File]::WriteAllText($logPath,($log | ConvertTo-Json -Depth 16),[Text.UTF8Encoding]::new($false))
Write-Output ('Saved source and full prompt: '+$entry.file)
