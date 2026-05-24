# Command 031 — Server Dashboard Snapshot Model

## Priority
عالية

## Target layer
Server/Monitoring/Core

## Files / context
DashboardSnapshotService, ViewModels

## Goal
جعل الواجهات تقرأ Snapshots فقط.

## Paste this command to Codex
```text
Task: Build dashboard snapshot model.

Required work:
1. Create FleetStatusViewModel, SyncStatusViewModel, CommandAuditViewModel, JournalAnalysisViewModel, XfsHealthViewModel, CashRiskViewModel.
2. UI refresh reads snapshot from service/cache.
3. No DB-heavy query or parsing in UI thread.
4. Add tests for snapshot generation with 500 ATM simulated and 100k events.
```

## Forbidden actions
- Do not put parser/network work inside forms.
- Do not block UI thread.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Dashboard remains responsive.
- Data source is snapshot service.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
