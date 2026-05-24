ISSUE TITLE:
[Track 045] Final Packaging and Release Readiness

ISSUE BODY:

# [Track 045] Final Packaging and Release Readiness

## Objective
إخراج حزمة تشغيل احترافية بعد نجاح كل البوابات.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not ship debug secrets.
- Do not ship unverified binaries.

## Target Files / Layers
**Target layer:** Build/Release

**Files / context:**
```text
publish scripts, artifacts/release
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Prepare release package.

Required work:
1. Publish Client.Service, Server, Monitoring, Installer with consistent version.
2. Include config templates, migrations, install manifest, rollback instructions.
3. Include baseline logs and test summary.
4. Include known limitations.
5. Do not mark production-ready if any critical track is incomplete.
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
- Release folder is reproducible and documented.

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
