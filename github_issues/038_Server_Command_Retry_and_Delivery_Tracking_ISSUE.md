ISSUE TITLE:
[Track 038] Server Command Retry and Delivery Tracking

ISSUE BODY:

# [Track 038] Server Command Retry and Delivery Tracking

## Objective
تتبع أوامر السيرفر وإعادة المحاولة بحذر.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not replay critical command after expiration.
- Do not execute duplicate command without idempotency.

## Target Files / Layers
**Target layer:** Server/Core

**Files / context:**
```text
CommandQueue, CommandDeliveryTracker
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement command delivery tracker.

Required work:
1. Store command queue state transitions.
2. Retry transient failures with exponential backoff.
3. Do not retry critical commands blindly.
4. Show command status in server dashboard snapshot.
5. Add tests for disconnected ATM and expired command.
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
- Operators can see Draft/Sent/Ack/Completed/Failed/Expired.

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
