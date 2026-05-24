# Command 003 — Client Service Headless Foundation

## Priority
حرجة

## Target layer
Client.Service

## Files / context
src/EJLive.Client.Service/**/*, src/Compatibility/**/*

## Goal
تثبيت الكلاينت كخدمة Windows صامتة لا تعتمد على WinForms.

## Paste this command to Codex
```text
Task: Harden EJLive.Client.Service as headless production service.

Required work:
1. Ensure `ClientAgentWindowsService` derives from `BackgroundService`.
2. Ensure `Program.cs` uses `Host.CreateApplicationBuilder`, `AddWindowsService`, and `AddHostedService<ClientAgentWindowsService>`.
3. Ensure no direct reference to `System.Windows.Forms` inside `EJLive.Client.Service`.
4. Keep compatibility with legacy `AgentBootstrapper` only through reflection or adapter.
5. Ensure service starts `IAgentController`, `AgentHealthReporter`, and supervision loop.
6. Add tests for Start/Stop/GetStatus without UI.
```

## Forbidden actions
- No MessageBox/Toast/Form in Client.Service.
- No analytics dashboard in client.
- No direct risky Windows policy changes in this task.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Client.Service builds.
- No WinForms references in Client.Service.
- Service lifecycle is testable.
- Health JSON path is documented.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
