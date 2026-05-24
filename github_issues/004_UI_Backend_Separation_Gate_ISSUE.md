ISSUE TITLE:
[Track 004] UI Backend Separation Gate

ISSUE BODY:

# [Track 004] UI Backend Separation Gate

## Objective
فصل الواجهة عن التنفيذ حتى لا يؤثر تعليق الواجهة على العمليات.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not remove UI tabs.
- Do not move business logic into forms.
- Do not run socket/file/parsing work on UI thread.

## Target Files / Layers
**Target layer:** Client.WinForms/Server.WinForms/Core

**Files / context:**
```text
src/EJLive.Client.WinForms/**/*, src/EJLive.Server.WinForms/**/*, src/EJLive.Core/**/*
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Separate UI from execution.

Required work:
1. Scan WinForms event handlers for blocking operations: File.ReadAllBytes, Socket/TcpClient/TcpListener, ParseJournal, ArchiveFile, SendCommand, Thread.Sleep, Task.Wait, Result.
2. Move long-running operations to Core services, background workers, or command queues.
3. UI buttons must enqueue commands or call async service methods only.
4. Server UI must read `DashboardSnapshotViewModel`, not execute ingestion/parsing directly.
5. Client UI must become config/admin companion only.
6. Add verification probe: fail if blocking operations remain in UI handlers.
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
- UI can freeze/close without stopping Client.Service.
- Server ingestion can continue if Server UI is unavailable.
- Verification blocks new UI-bound execution logic.

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
