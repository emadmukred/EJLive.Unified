# Command 030 — Screenshot Capture Delivery

## Priority
متوسطة

## Target layer
Client/Server/Core

## Files / context
ScreenshotCaptureService, GhostRemoteEngine

## Goal
التقاط لقطات شاشة حسب سياسة خصوصية وإرسالها للسرفر فقط.

## Paste this command to Codex
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

## Forbidden actions
- No local client analytics UI.
- No screenshots without policy/command context.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Screenshots delivered to server and retained according to policy.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
