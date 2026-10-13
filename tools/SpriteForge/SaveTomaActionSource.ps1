param(
    [Parameter(Mandatory=$true)][string]$PromptFile,
    [Parameter(Mandatory=$true)][string]$GeneratedPath
)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$entry=Get-Content -LiteralPath $PromptFile -Raw -Encoding UTF8 | ConvertFrom-Json
$destination=[IO.Path]::GetFullPath((Join-Path $taskRootPath $entry.file))
$allowed=[IO.Path]::GetFullPath((Join-Path $taskRootPath 'art_source/toma_actions/generated'))+[IO.Path]::DirectorySeparatorChar
if(-not $destination.StartsWith($allowed,[StringComparison]::OrdinalIgnoreCase) -or $entry.character -ne 'toma'){throw 'Source must belong to Toma and remain in its generated directory.'}
if(Test-Path -LiteralPath $destination){throw 'Version exists; never overwrite source art.'}
Copy-Item -LiteralPath $GeneratedPath -Destination $destination
$logPath=Join-Path $taskRootPath 'art_source/toma_actions/generation-log.json'
$log=Get-Content -LiteralPath $logPath -Raw -Encoding UTF8 | ConvertFrom-Json
$log.entries=@($log.entries)+@($entry)
[IO.File]::WriteAllText($logPath,($log|ConvertTo-Json -Depth 16),[Text.UTF8Encoding]::new($false))
Write-Output ('Saved Toma source and full prompt: '+$entry.file)
