ISSUE TITLE:
[Track 026] Safe Remote Command Queue

ISSUE BODY:

# [Track 026] Safe Remote Command Queue

## Objective
تنفيذ أوامر التحكم عبر طابور مؤمن وموقّع.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No arbitrary shell execution.
- No unsigned command execution.
- No password in logs.

## Target Files / Layers
**Target layer:** Server/Core/Security

**Files / context:**
```text
CommandQueue, CommandSigningEngine, SafeRemoteCommandExecutor
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build safe remote command queue.

Required work:
1. Create command states: Draft, Approved, Sent, Ack, Completed, Failed, Expired, Rejected.
2. Every command must have CommandId, CorrelationId, OperatorId, Role, TargetATM, TimestampUtc, ExpiryUtc, Signature.
3. Verify signature before sending and before executing.
4. Critical commands require admin role, maintenance window, operator confirmation, audit before/after.
5. Add tests for tampered signature, stale timestamp, unauthorized restart.
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
- Unsafe commands fail closed.
- Audit exists for every command attempt.

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
