param([string]$Report = 'TestResults/phase4-assembly-references.json')
$ErrorActionPreference = 'Stop'
$rootPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$projectRoot = Join-Path $rootPath 'AniBato/Assets/_Project'
$assemblyFiles = Get-ChildItem -LiteralPath $projectRoot -Recurse -File -Filter '*.asmdef'
$assemblies = @{}
foreach ($file in $assemblyFiles) {
    $definition = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
    $assemblies[$definition.name] = @{ Definition = $definition; Path = $file.FullName; Directory = $file.DirectoryName }
}
$results = @()
foreach ($entry in $assemblies.GetEnumerator()) {
    $owner = $entry.Value
    $sources = Get-ChildItem -LiteralPath $owner.Directory -Recurse -File -Filter '*.cs'
    foreach ($source in $sources) {
        $contents = Get-Content -LiteralPath $source.FullName -Raw -Encoding UTF8
        foreach ($target in $assemblies.Keys) {
            # Friend-assembly names in AssemblyInfo are access grants, not dependencies.
            if ($target -eq $entry.Key -or -not $target.StartsWith('AniBato.') -or $target.EndsWith('Tests')) { continue }
            $pattern = '\b' + [regex]::Escape($target) + '\b'
            if ($contents -match $pattern -and $owner.Definition.references -notcontains $target) {
                throw "Missing asmdef reference: $($entry.Key) -> $target ($($source.Name))"
            }
        }
    }
    $results += [pscustomobject]@{ Assembly = $entry.Key; SourceCount = $sources.Count; References = $owner.Definition.references }
}
$results | Sort-Object Assembly | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $rootPath $Report) -Encoding UTF8
Write-Output "asmdef references checked: $($assemblies.Count) assemblies."
