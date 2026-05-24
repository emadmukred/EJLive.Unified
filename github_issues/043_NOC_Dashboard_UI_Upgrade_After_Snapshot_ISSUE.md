ISSUE TITLE:
[Track 043] NOC Dashboard UI Upgrade After Snapshot

ISSUE BODY:

# [Track 043] NOC Dashboard UI Upgrade After Snapshot

## Objective
تحسين UI بعد اكتمال snapshots فقط.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No parsing in UI.
- No socket/file transfer in UI.
- No blocking DB queries on UI thread.

## Target Files / Layers
**Target layer:** Monitoring/Server UI

**Files / context:**
```text
MainDashboardForm, ServerMainForm
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Upgrade NOC dashboard without moving execution into UI.

Required work:
1. Add filters by vendor/region/status/severity.
2. Add cards, grids, and lightweight charts bound to snapshot view models.
3. Add tabs only if data source exists in Core snapshot.
4. Add UI smoke/performance tests where possible.
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
- UI stays responsive with 500 ATM simulated.

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
