# Command 011 — Server Ingestion Pipeline

## Priority
حرجة

## Target layer
Server/Core/Data

## Files / context
ServerEngine, UnifiedJournalStorageService, IngestionPipeline, DatabaseManager

## Goal
السيرفر يستقبل ويؤرشف ويحلل خارج الواجهة.

## Paste this command to Codex
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

## Forbidden actions
- ServerMainForm must not parse files.
- ServerMainForm must not block on TCP receive.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Every accepted file has archive, audit, and parse summary.
- Pipeline continues without UI.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
