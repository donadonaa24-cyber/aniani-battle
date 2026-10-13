param(
    [Parameter(Mandatory=$true)][ValidateSet('toma','ren','chizuru')][string]$Character,
    [Parameter(Mandatory=$true)][ValidateSet('walk','walk_inbetweens','dash','attack1','attack2','attack3')][string]$Group,
    [Parameter(Mandatory=$true)][ValidateRange(1,999)][int]$Version
)
$ErrorActionPreference='Stop'
$taskRootPath=(Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$selectionPath=Join-Path $taskRootPath 'art_source/art3_walk_attacks/source-selection.json'
$original=[IO.File]::ReadAllText($selectionPath)
$selection=$original | ConvertFrom-Json
$source='art_source/art3_walk_attacks/generated/'+$Character+'_'+$Group+'_keys_v'+$Version+'.png'
if (-not (Test-Path -LiteralPath (Join-Path $taskRootPath $source))) { throw 'Source missing.' }
$selection | Add-Member -NotePropertyName ($Character+'/'+$Group) -NotePropertyValue $source -Force
$accepted=$false
try {
    [IO.File]::WriteAllText($selectionPath,($selection | ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'NormalizeWalkAttackAtlas.ps1') -Character $Character -Group $Group
    if ($LASTEXITCODE -ne 0) { throw 'Normalization rejected the candidate; original selection restored.' }
    $accepted=$true
    Write-Output ('Adopted '+$Character+'/'+$Group+' v'+$Version+' after all keys passed.')
} finally {
    if (-not $accepted) { [IO.File]::WriteAllText($selectionPath,$original,[Text.UTF8Encoding]::new($false)) }
}
