# Command 004 — UI Backend Separation Gate

## Priority
حرجة

## Target layer
Client.WinForms/Server.WinForms/Core

## Files / context
src/EJLive.Client.WinForms/**/*, src/EJLive.Server.WinForms/**/*, src/EJLive.Core/**/*

## Goal
فصل الواجهة عن التنفيذ حتى لا يؤثر تعليق الواجهة على العمليات.

## Paste this command to Codex
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

## Forbidden actions
- Do not remove UI tabs.
- Do not move business logic into forms.
- Do not run socket/file/parsing work on UI thread.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- UI can freeze/close without stopping Client.Service.
- Server ingestion can continue if Server UI is unavailable.
- Verification blocks new UI-bound execution logic.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
