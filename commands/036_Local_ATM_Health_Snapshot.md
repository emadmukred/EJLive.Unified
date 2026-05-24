# Command 036 — Local ATM Health Snapshot

## Priority
عالية

## Target layer
Client.Service/Core

## Files / context
AgentHealthReporter, client_health_snapshots

## Goal
تجميع صحة الجهاز وإرسالها للسيرفر.

## Paste this command to Codex
```text
Task: Build local ATM health snapshot.

Required work:
1. Collect service state, connection, heartbeat time, outbox count, disk usage, memory, CPU if available, watcher roots, last journal sync.
2. Write health.json atomically.
3. Send health to server telemetry.
4. Add tests for health report generation and JSON validity.
```

## Forbidden actions
- Do not block service if metrics fail.
- Do not include secrets.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- health.json valid and server receives snapshot.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
