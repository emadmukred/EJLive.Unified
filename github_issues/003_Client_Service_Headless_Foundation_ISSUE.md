ISSUE TITLE:
[Track 003] Client Service Headless Foundation

ISSUE BODY:

# [Track 003] Client Service Headless Foundation

## Objective
تثبيت الكلاينت كخدمة Windows صامتة لا تعتمد على WinForms.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No MessageBox/Toast/Form in Client.Service.
- No analytics dashboard in client.
- No direct risky Windows policy changes in this task.

## Target Files / Layers
**Target layer:** Client.Service

**Files / context:**
```text
src/EJLive.Client.Service/**/*, src/Compatibility/**/*
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

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
- Client.Service builds.
- No WinForms references in Client.Service.
- Service lifecycle is testable.
- Health JSON path is documented.

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
