# Command 037 — Outbox Maintenance and Quota

## Priority
متوسطة

## Target layer
Client.Service/Core/Sync

## Files / context
OutboxMaintenanceService

## Goal
منع تضخم Outbox وحماية الموارد.

## Paste this command to Codex
```text
Task: Implement outbox maintenance.

Required work:
1. Add MaxOutboxBytes and MaxOutboxItems settings.
2. Dead-letter items after max retries.
3. Clean orphan payloads after grace period.
4. Report Pending/Failed/AwaitingAck/DeadLetter in telemetry.
5. Add tests for quota exceeded and orphan payload cleanup.
```

## Forbidden actions
- Do not delete unacknowledged payloads before grace period.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Outbox remains within quota.
- Dead-letter report visible.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
