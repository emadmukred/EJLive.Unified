ISSUE TITLE:
[Track 042] Reference Promotion Protocol

ISSUE BODY:

# [Track 042] Reference Promotion Protocol

## Objective
ترقية الملفات المرجعية بأمان بدون كسر المشروع.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not compile reference files directly.
- Do not delete legacy/reference files without disposition plan.

## Target Files / Layers
**Target layer:** Docs/Verification

**Files / context:**
```text
docs/original-audit, reference files
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement reference promotion protocol.

Required work:
1. Create `docs/promotions/` ticket template.
2. Each reference file promotion must state source file, target active service, adapter approach, tests, rollback.
3. Verification fails if reference file enters compile without promotion ticket.
4. Add promotion map CSV.
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
- Every promoted file has ticket and tests.

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
