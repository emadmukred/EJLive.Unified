ISSUE TITLE:
[Track 041] Installer Deployment Rollback

ISSUE BODY:

# [Track 041] Installer Deployment Rollback

## Objective
تثبيت وترقية وإزالة نظيفة مع rollback.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not hide service name.
- Do not install without audit log.

## Target Files / Layers
**Target layer:** Installer/Client.Service/Server

**Files / context:**
```text
Installer.WinForms, WindowsServiceRegistrationService
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build installer deployment and rollback foundation.

Required work:
1. Create install manifest listing files, services, paths, ports, prerequisites.
2. Install service with clear name: EJLive Client Agent Service.
3. Configure Windows Service recovery.
4. Add rollback from last-known-good.
5. Write install audit log.
6. Add tests/smoke instructions for fresh install, upgrade, failed service start, uninstall.
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
- Install/uninstall/rollback are documented and repeatable.

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
