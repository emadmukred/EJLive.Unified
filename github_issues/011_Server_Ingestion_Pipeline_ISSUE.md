ISSUE TITLE:
[Track 011] Server Ingestion Pipeline

ISSUE BODY:

# [Track 011] Server Ingestion Pipeline

## Objective
السيرفر يستقبل ويؤرشف ويحلل خارج الواجهة.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- ServerMainForm must not parse files.
- ServerMainForm must not block on TCP receive.

## Target Files / Layers
**Target layer:** Server/Core/Data

**Files / context:**
```text
ServerEngine, UnifiedJournalStorageService, IngestionPipeline, DatabaseManager
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build Server Ingestion Pipeline.

Required pipeline:
Receive -> Staging -> Verify SHA256 -> Archive -> Parse -> Index -> Snapshot.

Required work:
1. Create `IngestionPipeline` service outside WinForms.
2. Write journal files to staging first.
3. Move to archive only after SHA256 match.
4. Insert journal_archive and transfer_sessions records.
5. Trigger server-side parsing and dashboard snapshot refresh.
6. Add tests for duplicate file, checksum mismatch, missing ATM_ID, archive unavailable.
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
- Every accepted file has archive, audit, and parse summary.
- Pipeline continues without UI.

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
