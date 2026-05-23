#Requires -Version 5.1
param(
    [string]$SolutionRoot = ".",
    [string]$ActiveCompileMapPath = ".\artifacts\ActiveCompileMap.csv",
    [string]$OutputPath = ".\artifacts\UnusedFileDispositionPlan.md"
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path $SolutionRoot).Path

$map = Import-Csv $ActiveCompileMapPath
$mapLookup = @{}
foreach ($r in $map) { $mapLookup[$r.Path] = $r }

# Find all .cs files on disk under src/
$allCsFiles = Get-ChildItem -Path (Join-Path $root 'src') -Filter '*.cs' -Recurse -File
$orphans = @()
$deprecated = @()
$referenceOnly = @()

foreach ($f in $allCsFiles) {
    $relPath = $f.FullName.Substring($root.Length + 1).Replace('\', '/')
    if (-not $mapLookup.ContainsKey($relPath)) {
        $orphans += $relPath
    } else {
        $state = $mapLookup[$relPath].CompileState
        if ($state -eq 'Deprecated') { $deprecated += $relPath }
        elseif ($state -eq 'ReferenceOnly') { $referenceOnly += $relPath }
    }
}

$lines = @()
$lines += "# Unused File Disposition Plan"
$lines += ""
$lines += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$lines += ""
$lines += "## Summary"
$lines += ""
$lines += "| Category | Count |"
$lines += "|----------|-------|"
$lines += "| Orphan files (not in any .csproj) | $($orphans.Count) |"
$lines += "| Deprecated files (Compile Remove) | $($deprecated.Count) |"
$lines += "| ReferenceOnly files (None + Link) | $($referenceOnly.Count) |"
$lines += ""

if ($orphans.Count -gt 0) {
    $lines += "## Orphan Files (Not in Any .csproj)"
    $lines += ""
    $lines += "These files exist on disk but are not referenced in any .csproj file."
    $lines += ""
    foreach ($o in ($orphans | Sort-Object)) {
        $lines += "- ``$o``"
    }
    $lines += ""
}

if ($deprecated.Count -gt 0) {
    $lines += "## Deprecated Files (Compile Remove)"
    $lines += ""
    $lines += "These files are explicitly excluded from compilation via ``<Compile Remove>``."
    $lines += ""
    foreach ($d in ($deprecated | Sort-Object)) {
        $lines += "- ``$d``"
    }
    $lines += ""
}

if ($referenceOnly.Count -gt 0) {
    $lines += "## Reference-Only Files (None + Link)"
    $lines += ""
    $lines += "These files are preserved as reference-only via ``<None Include Link=>``."
    $lines += ""
    $lines += "_Full list omitted; see ActiveCompileMap.csv for complete entries._"
    $lines += ""
}

$lines += "## Recommendations"
$lines += ""
$lines += "1. **Orphan files**: Review for deletion or inclusion in a project."
$lines += "2. **Deprecated files**: Safe to delete if replacements are stable; retain for audit if needed."
$lines += "3. **ReferenceOnly files**: Retain until Phase-3 replacement verification is complete."
$lines += ""

$lines | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "UnusedFileDispositionPlan generated at $OutputPath"
Write-Host "Orphans: $($orphans.Count), Deprecated: $($deprecated.Count), ReferenceOnly: $($referenceOnly.Count)"
