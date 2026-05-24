# Command 038 — Server Command Retry and Delivery Tracking

## Priority
عالية

## Target layer
Server/Core

## Files / context
CommandQueue, CommandDeliveryTracker

## Goal
تتبع أوامر السيرفر وإعادة المحاولة بحذر.

## Paste this command to Codex
```text
Task: Implement command delivery tracker.

Required work:
1. Store command queue state transitions.
2. Retry transient failures with exponential backoff.
3. Do not retry critical commands blindly.
4. Show command status in server dashboard snapshot.
5. Add tests for disconnected ATM and expired command.
```

## Forbidden actions
- Do not replay critical command after expiration.
- Do not execute duplicate command without idempotency.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Operators can see Draft/Sent/Ack/Completed/Failed/Expired.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
