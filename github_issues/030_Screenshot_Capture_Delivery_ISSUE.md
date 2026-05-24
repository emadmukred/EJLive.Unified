ISSUE TITLE:
[Track 030] Screenshot Capture Delivery

ISSUE BODY:

# [Track 030] Screenshot Capture Delivery

## Objective
التقاط لقطات شاشة حسب سياسة خصوصية وإرسالها للسرفر فقط.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No local client analytics UI.
- No screenshots without policy/command context.

## Target Files / Layers
**Target layer:** Client/Server/Core

**Files / context:**
```text
ScreenshotCaptureService, GhostRemoteEngine
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement screenshot capture and delivery.

Required work:
1. Separate screenshot telemetry from remote control session.
2. Configurable cadence and max size.
3. JPEG compression quality configurable.
4. Metadata: width, height, quality, hash, timestamp.
5. Server retention cleanup.
6. Add tests for capture failure, no desktop/session0, retention cleanup.
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
- Screenshots delivered to server and retained according to policy.

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
