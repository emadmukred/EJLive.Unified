# Command 045 — Final Packaging and Release Readiness

## Priority
عالية

## Target layer
Build/Release

## Files / context
publish scripts, artifacts/release

## Goal
إخراج حزمة تشغيل احترافية بعد نجاح كل البوابات.

## Paste this command to Codex
```text
Task: Prepare release package.

Required work:
1. Publish Client.Service, Server, Monitoring, Installer with consistent version.
2. Include config templates, migrations, install manifest, rollback instructions.
3. Include baseline logs and test summary.
4. Include known limitations.
5. Do not mark production-ready if any critical track is incomplete.
```

## Forbidden actions
- Do not ship debug secrets.
- Do not ship unverified binaries.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Release folder is reproducible and documented.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
