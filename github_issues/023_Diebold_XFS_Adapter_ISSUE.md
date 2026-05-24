ISSUE TITLE:
[Track 023] Diebold XFS Adapter

ISSUE BODY:

# [Track 023] Diebold XFS Adapter

## Objective
تطبيع أحداث XFS/Trace الخاصة بـ Diebold.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not hardcode one ATM model only.
- Do not drop unknown vendor codes.

## Target Files / Layers
**Target layer:** Core/XFS

**Files / context:**
```text
DieboldXfsAdapter
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement Diebold XFS adapter.

Required work:
1. Create `DieboldXfsAdapter` implementing IXfsVendorAdapter.
2. Parse device class when possible: CDM, IDC, PTR, SIU, PIN, CIM, VDM.
3. Extract severity, code, message, timestamp, raw line number.
4. Preserve source file and raw line.
5. Add fixtures for at least error, warning, info, unknown code.
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
- Adapter registered.
- Normalized events produced.
- Unknown events preserved.

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
