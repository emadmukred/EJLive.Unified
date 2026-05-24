# Command 026 — Safe Remote Command Queue

## Priority
حرجة

## Target layer
Server/Core/Security

## Files / context
CommandQueue, CommandSigningEngine, SafeRemoteCommandExecutor

## Goal
تنفيذ أوامر التحكم عبر طابور مؤمن وموقّع.

## Paste this command to Codex
```text
Task: Build safe remote command queue.

Required work:
1. Create command states: Draft, Approved, Sent, Ack, Completed, Failed, Expired, Rejected.
2. Every command must have CommandId, CorrelationId, OperatorId, Role, TargetATM, TimestampUtc, ExpiryUtc, Signature.
3. Verify signature before sending and before executing.
4. Critical commands require admin role, maintenance window, operator confirmation, audit before/after.
5. Add tests for tampered signature, stale timestamp, unauthorized restart.
```

## Forbidden actions
- No arbitrary shell execution.
- No unsigned command execution.
- No password in logs.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Unsafe commands fail closed.
- Audit exists for every command attempt.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
