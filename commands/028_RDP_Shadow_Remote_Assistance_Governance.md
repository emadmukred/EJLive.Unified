# Command 028 — RDP Shadow Remote Assistance Governance

## Priority
عالية

## Target layer
Client/Server/Security

## Files / context
RemoteAssistanceSessionService

## Goal
تنظيم RDP/Shadow/Ghost كجلسات مصرح بها ومدققة.

## Paste this command to Codex
```text
Task: Build governed remote assistance sessions.

Required work:
1. Rename operator-facing concept to RemoteAssistanceSession, not hidden ghost.
2. Create RequestId, OperatorId, ATM_ID, Reason, Approval, StartTime, Timeout, StopCommand.
3. Precheck: TermService, NLA, firewall scope, active sessions via quser, local rights, domain policy, LAPS state.
4. No credentials stored or logged.
5. Add tests for denied policy, timeout, no active session, stop session.
```

## Forbidden actions
- Do not use no-consent shadowing unless explicit enterprise policy permits and audit records it.
- Do not store passwords.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Every remote session is auditable and time-bound.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
