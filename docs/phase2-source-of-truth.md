# EJLive Phase-2 Source of Truth

**Generated:** 2026-05-23 21:08
**Baseline Timestamp:** 20260523-210828
**Verification Status:** PASS (all 23 probes)
**.NET SDK:** 10.0.203 (building net8.0-windows targets)

## Source Root Integrity

- **Total Files Hashed:** 12445
- **Source Root SHA256:** 65ACDD61BC858063CF16E7D02941865DA0A0393D1C601EA343A8E2AA79E99ADE

## Baseline Pipeline Results

| Step | Status | Duration |
|------|--------|----------|
| dotnet --info | PASS | ~1s |
| dotnet restore | PASS | ~4s |
| dotnet build (sln) | PASS | ~6s |
| dotnet test | PASS | ~16s (104 tests) |
| dotnet run verification | PASS | ~24s (23/23 probes) |
| dotnet build (slnx) | PASS | ~5s |

## Baseline Artifacts

Logs saved under: `artifacts/baseline/20260523-210828/`

## Known Baseline Fixes Applied

1. **Verification IsBuildOutput exclusion:** Added /artifacts/ to generated output exclusions to prevent newly created baseline logs from failing file linkage probe.
2. **File inventory regeneration:** `docs/09-file-function-inventory.csv` was stale; regenerated with 12,383 accurate rows matching actual disk state.
3. **Path separator normalization:** Ensured inventory uses forward slashes consistently for cross-platform comparability.
4. **LegacyReference path resolution:** `Generate-ActiveCompileMap.ps1` now correctly resolves `..\..\` paths relative to solution root instead of producing absolute paths.
5. **Wildcard handling:** Script now correctly processes `**` recursive wildcards and `Exclude` attributes on `Compile Include` and `None Include` items.
6. **Default SDK globs:** Script detects projects without `EnableDefaultItems=false` and includes default-compiled .cs files.
7. **Cross-project priority merge:** When a file appears in multiple projects, the merged map prioritizes ActiveCompiled > Deprecated > ReferenceOnly.

## Track 02: ActiveCompileMapSynchronizer Deliverables

| Artifact | Path | Status |
|----------|------|--------|
| ActiveCompileMap | `artifacts/ActiveCompileMap.csv` | Generated (16,446 entries) |
| Compile Map Mismatch Report | `artifacts/CompileMapMismatchReport.csv` | Generated (6 acceptable mismatches) |
| Reference Promotion Map | `artifacts/ReferencePromotionMap.csv` | Generated (50 entries) |
| Unused File Disposition Plan | `artifacts/UnusedFileDispositionPlan.md` | Generated (0 orphans, 3 deprecated, 97 reference-only) |
| Project Dependency Graph | `artifacts/ProjectDependencyGraph.md` | Generated |
| Compile-State Conflict Probe | `src/EJLive.Verification/Program.cs` | Added (PASS) |

### Known Acceptable Mismatches (6)

These files show a semantic difference between the service-activation-status (intended state) and the actual csproj compile state, but all are build-safe:

1. `src/EJLive.Server/Services/JournalAnalyticsService.cs` - Expected ReferenceOnly, Actual ActiveCompiled (compiled in EJLive.Server.WinForms via linked file)
2. `src/EJLive.Server/Services/RemoteControlService.cs` - Expected ReferenceOnly, Actual ActiveCompiled (compiled in EJLive.Server.WinForms via linked file)
3. `src/EJLive.Shared/LightUiTheme.cs` - Expected ReferenceOnly, Actual ActiveCompiled (compiled in EJLive.Monitor via linked file)
4. `src/EJLive.Shared/Logger.cs` - Expected ReferenceOnly, Actual Deprecated (Compile Remove in EJLive.Shared)
5. `src/EJLive.Shared/MonitoringState.cs` - Expected ReferenceOnly, Actual Deprecated (Compile Remove in EJLive.Shared)
6. `src/EJLive.Shared/MonitoringStateStore.cs` - Expected ReferenceOnly, Actual Deprecated (Compile Remove in EJLive.Shared)

## Security Note

This SHA256 represents the cumulative hash of all source, documentation, and reference files under version control scope. It does NOT include build outputs (`bin/`, `obj/`, `.vs/`, `artifacts/`).

## Next Phase Gate

Baseline and Track 02 complete. Proceed to Track 03: HeadlessClientAgentService.
