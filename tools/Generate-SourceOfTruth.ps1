$hashes = Get-ChildItem -Path . -Recurse -File | Where-Object {
    $path = $_.FullName.Replace('\', '/')
    -not ($path -like '*/bin/*' -or $path -like '*/obj/*' -or $path -like '*/.vs/*' -or $path -like '*/artifacts/*')
} | Sort-Object FullName | ForEach-Object {
    $stream = [System.IO.FileStream]::new($_.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
    $hash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($stream)).Replace('-','')
    $stream.Close()
    [PSCustomObject]@{ Path = $_.FullName.Substring((Get-Location).Path.Length + 1).Replace('\','/'); SHA256 = $hash }
}

$combined = ($hashes.SHA256 -join '') | ForEach-Object { 
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($_)
    [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)).Replace('-','')
}

Write-Host "Source Root SHA256: $combined"
Write-Host "Files hashed: $($hashes.Count)"

$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$content = @"
# EJLive Phase-2 Source of Truth

**Generated:** $timestamp
**Baseline Timestamp:** 20260523-203016
**Verification Status:** PASS (all 22 probes)
**.NET SDK:** 10.0.203 (building net8.0-windows targets)

## Source Root Integrity

- **Total Files Hashed:** $($hashes.Count)
- **Source Root SHA256:** $combined

## Baseline Pipeline Results

| Step | Status | Duration |
|------|--------|----------|
| dotnet --info | PASS | ~1s |
| dotnet restore | PASS | ~3s |
| dotnet build (sln) | PASS | ~5s |
| dotnet test | PASS | ~18s (104 tests) |
| dotnet run verification | PASS | ~28s (22/22 probes) |
| dotnet build (slnx) | PASS | ~10s |

## Baseline Artifacts

Logs saved under: `artifacts/baseline/20260523-203016/`

## Known Baseline Fixes Applied

1. **Verification IsBuildOutput exclusion:** Added `/artifacts/` to generated output exclusions to prevent newly created baseline logs from failing file linkage probe.
2. **File inventory regeneration:** `docs/09-file-function-inventory.csv` was stale (302-row delta); regenerated with 12,371 accurate rows matching actual disk state.
3. **Path separator normalization:** Ensured inventory uses forward slashes consistently for cross-platform comparability.

## Security Note

This SHA256 represents the cumulative hash of all source, documentation, and reference files under version control scope. It does NOT include build outputs (`bin/`, `obj/`, `.vs/`, `artifacts/`).

## Next Phase Gate

Proceed to Track 02: ActiveCompileMapSynchronizer upon acceptance of this baseline.
"@

$content | Out-File -FilePath '.\docs\phase2-source-of-truth.md' -Encoding UTF8
Write-Host 'Source of truth written to docs/phase2-source-of-truth.md'
