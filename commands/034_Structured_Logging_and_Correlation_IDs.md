# Command 034 — Structured Logging and Correlation IDs

## Priority
عالية

## Target layer
Shared/Core/Server/Client

## Files / context
OperationalEvent, AppLogger

## Goal
تتبع رحلة كل ملف وأمر وعملية عبر CorrelationId.

## Paste this command to Codex
```text
Task: Implement structured logging and observability.

Required work:
1. Create OperationalEvent with EventId, CorrelationId, ATM_ID, Component, Severity, Message, TimestampUtc, DataJson.
2. Every transfer, heartbeat, parser, xfs, command, policy change must use CorrelationId.
3. Local rolling logs on client; telemetry_events on server.
4. Redact card/account/password/secrets.
5. Add tests for redaction and log rotation.
```

## Forbidden actions
- No passwords/secrets/card full PAN in logs.
- No silent catch at boundaries without logging.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- A file journey is traceable from client watcher to server archive.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
