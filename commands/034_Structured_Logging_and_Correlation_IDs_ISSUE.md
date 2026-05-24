ISSUE TITLE:
[Track 034] Structured Logging and Correlation IDs

ISSUE BODY:

# [Track 034] Structured Logging and Correlation IDs

## Objective
تتبع رحلة كل ملف وأمر وعملية عبر CorrelationId.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No passwords/secrets/card full PAN in logs.
- No silent catch at boundaries without logging.

## Target Files / Layers
**Target layer:** Shared/Core/Server/Client

**Files / context:**
```text
OperationalEvent, AppLogger
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement structured logging and observability.

Required work:
1. Create OperationalEvent with EventId, CorrelationId, ATM_ID, Component, Severity, Message, TimestampUtc, DataJson.
2. Every transfer, heartbeat, parser, xfs, command, policy change must use CorrelationId.
3. Local rolling logs on client; telemetry_events on server.
4. Redact card/account/password/secrets.
5. Add tests for redaction and log rotation.
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
- A file journey is traceable from client watcher to server archive.

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
