# Command 041 — Installer Deployment Rollback

## Priority
عالية

## Target layer
Installer/Client.Service/Server

## Files / context
Installer.WinForms, WindowsServiceRegistrationService

## Goal
تثبيت وترقية وإزالة نظيفة مع rollback.

## Paste this command to Codex
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

## Forbidden actions
- Do not hide service name.
- Do not install without audit log.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Install/uninstall/rollback are documented and repeatable.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
