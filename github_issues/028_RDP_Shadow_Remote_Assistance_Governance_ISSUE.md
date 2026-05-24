ISSUE TITLE:
[Track 028] RDP Shadow Remote Assistance Governance

ISSUE BODY:

# [Track 028] RDP Shadow Remote Assistance Governance

## Objective
تنظيم RDP/Shadow/Ghost كجلسات مصرح بها ومدققة.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not use no-consent shadowing unless explicit enterprise policy permits and audit records it.
- Do not store passwords.

## Target Files / Layers
**Target layer:** Client/Server/Security

**Files / context:**
```text
RemoteAssistanceSessionService
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build governed remote assistance sessions.

Required work:
1. Rename operator-facing concept to RemoteAssistanceSession, not hidden ghost.
2. Create RequestId, OperatorId, ATM_ID, Reason, Approval, StartTime, Timeout, StopCommand.
3. Precheck: TermService, NLA, firewall scope, active sessions via quser, local rights, domain policy, LAPS state.
4. No credentials stored or logged.
5. Add tests for denied policy, timeout, no active session, stop session.
```

## Required Verification Commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
```

## Acceptance Criteria
- Every remote session is auditable and time-bound.

## Required Tests
- Add or update unit tests when code is changed.
- Add or update verification probes when architecture, compile map, service boundary, parser, XFS, security, database, or transport behavior is changed.
- For parser/XFS work, add fixtures and expected output snapshots.
- For UI/backend separation work, prove backend execution continues without blocking UI.

## Required Codex Output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan

## Pull Request Requirements
- Link this Issue in the PR body.
- Use `.github/pull_request_template.md`.
- Include exact files modified/added.
- Include exact commands executed and their results.
- Include rollback plan.
- Do not merge until restore/build/test/verification pass.
