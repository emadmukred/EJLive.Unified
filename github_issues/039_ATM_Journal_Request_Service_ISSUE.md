ISSUE TITLE:
[Track 039] ATM Journal Request Service

ISSUE BODY:

# [Track 039] ATM Journal Request Service

## Objective
طلب ملفات جورنال محددة من أي ATM.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No arbitrary file read from client.
- No path traversal.

## Target Files / Layers
**Target layer:** Server/Client/Core

**Files / context:**
```text
JournalRequestService
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build server initiated journal request service.

Required work:
1. Define JournalRequest command with file pattern, date range, vendor strategy, max bytes.
2. Client validates request against allowed journal roots.
3. Client packages requested files through chunked transfer.
4. Server archives under vendor/date/ATM.
5. Add tests for invalid path, too large request, missing file, successful request.
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
- Server can request allowed logs and receive audited result.

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
