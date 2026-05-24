# Command 039 — ATM Journal Request Service

## Priority
عالية

## Target layer
Server/Client/Core

## Files / context
JournalRequestService

## Goal
طلب ملفات جورنال محددة من أي ATM.

## Paste this command to Codex
```text
Task: Build server initiated journal request service.

Required work:
1. Define JournalRequest command with file pattern, date range, vendor strategy, max bytes.
2. Client validates request against allowed journal roots.
3. Client packages requested files through chunked transfer.
4. Server archives under vendor/date/ATM.
5. Add tests for invalid path, too large request, missing file, successful request.
```

## Forbidden actions
- No arbitrary file read from client.
- No path traversal.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Server can request allowed logs and receive audited result.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
