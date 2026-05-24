# Command 009 — Journal Outbox Compatibility Adapter

## Priority
عالية

## Target layer
Client.Service/Core/Sync

## Files / context
JournalOutbox, JournalOutboxAdapter

## Goal
الحفاظ على القديم والجديد عبر Adapter يمنع كسر API.

## Paste this command to Codex
```text
Task: Implement JournalOutboxAdapter.

Required work:
1. Create interface `IJournalOutboxAdapter` with PendingCount, EnqueueFile, EnqueueDelta, EnqueueForceSyncMarker, MarkAcked, MarkFailed, DeadLetter.
2. Wrap existing JournalOutbox APIs without changing them directly.
3. Support both old and new method names where needed.
4. Add tests for enqueue, pending count, force marker, missing payload, retry.
```

## Forbidden actions
- Do not rename JournalOutbox public API in this task.
- Do not delete old queue behavior.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- AgentHeadlessController uses adapter, not direct fragile calls.
- Old/new JournalOutbox variations are handled.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
