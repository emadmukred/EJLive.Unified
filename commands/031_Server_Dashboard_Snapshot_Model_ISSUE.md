ISSUE TITLE:
[Track 031] Server Dashboard Snapshot Model

ISSUE BODY:

# [Track 031] Server Dashboard Snapshot Model

## Objective
جعل الواجهات تقرأ Snapshots فقط.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not put parser/network work inside forms.
- Do not block UI thread.

## Target Files / Layers
**Target layer:** Server/Monitoring/Core

**Files / context:**
```text
DashboardSnapshotService, ViewModels
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build dashboard snapshot model.

Required work:
1. Create FleetStatusViewModel, SyncStatusViewModel, CommandAuditViewModel, JournalAnalysisViewModel, XfsHealthViewModel, CashRiskViewModel.
2. UI refresh reads snapshot from service/cache.
3. No DB-heavy query or parsing in UI thread.
4. Add tests for snapshot generation with 500 ATM simulated and 100k events.
```

## Required Verification Commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
```

## Acceptance Criteria
- Dashboard remains responsive.
- Data source is snapshot service.

## Required Tests
- Add or update unit tests when code is changed.
- Add or update verification probes when architecture, compile map, service boundary, parser, XFS, security, database, or transport behavior is changed.
- For parser/XFS work, add fixtures and expected output snapshots.
- For UI/backend separation work, prove backend execution continues without blocking UI.

## Required Codex Output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan

## Pull Request Requirements
- Link this Issue in the PR body.
- Use `.github/pull_request_template.md`.
- Include exact files modified/added.
- Include exact commands executed and their results.
- Include rollback plan.
- Do not merge until restore/build/test/verification pass.
