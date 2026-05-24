# Command 005 — Agent Controller Runtime

## Priority
عالية

## Target layer
Client.Service/Core

## Files / context
IAgentController.cs, AgentHeadlessController.cs, Core/Agent services

## Goal
جعل AgentController قلبًا منظّمًا لمكونات الكلاينت.

## Paste this command to Codex
```text
Task: Build Agent Controller Runtime.

Required work:
1. Define or confirm `IAgentController` with StartAll, StopAll, GetStatus, ForceJournalSync, ForceLogBackup.
2. Implement state machine: Stopped, Starting, Running, Paused, Failed.
3. Coordinate NetworkSessionManager, HeartbeatService, AdvancedFileWatcher, JournalOutboxAdapter, HealthReporter.
4. Ensure exceptions in observers never crash the agent.
5. Add tests for state transitions and idempotent StopAll.
```

## Forbidden actions
- Do not execute dangerous remote commands in AgentHeadlessController.
- Do not parse financial analytics in client UI.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Agent lifecycle is deterministic.
- Status contains state, connection, session, outbox, last heartbeat, last sync, last error.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
