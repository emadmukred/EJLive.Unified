# Command 043 — NOC Dashboard UI Upgrade After Snapshot

## Priority
متوسطة

## Target layer
Monitoring/Server UI

## Files / context
MainDashboardForm, ServerMainForm

## Goal
تحسين UI بعد اكتمال snapshots فقط.

## Paste this command to Codex
```text
Task: Upgrade NOC dashboard without moving execution into UI.

Required work:
1. Add filters by vendor/region/status/severity.
2. Add cards, grids, and lightweight charts bound to snapshot view models.
3. Add tabs only if data source exists in Core snapshot.
4. Add UI smoke/performance tests where possible.
```

## Forbidden actions
- No parsing in UI.
- No socket/file transfer in UI.
- No blocking DB queries on UI thread.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- UI stays responsive with 500 ATM simulated.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
