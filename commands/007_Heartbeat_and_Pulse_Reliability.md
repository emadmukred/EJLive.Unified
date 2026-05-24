# Command 007 — Heartbeat and Pulse Reliability

## Priority
عالية

## Target layer
Client.Service/Server/Core

## Files / context
NetworkEngine, ServerEngine, AgentHeadlessController

## Goal
إرسال نبضات دورية موثوقة مع Backoff وإظهار الحالة في السيرفر.

## Paste this command to Codex
```text
Task: Implement reliable heartbeat/pulse.

Required work:
1. Heartbeat payload must be JSON with ATM_ID, SessionId, AgentState, OutboxCount, LastJournalOffset, CPU, Memory, Disk, WatcherState.
2. Server replies with HeartbeatAck containing ServerTimeUtc and PendingCommandCount.
3. Add heartbeat timeout thresholds: Online, Warning, Offline, CriticalOffline.
4. Add exponential backoff + jitter for reconnect.
5. Prevent overlapping heartbeat sends.
```

## Forbidden actions
- Do not send per-second heartbeat by default unless configured.
- Do not block file transfer on heartbeat failure.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Online/offline state is accurate.
- Reconnect storms are prevented.
- Heartbeat test covers server down/up.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
