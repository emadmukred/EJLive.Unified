ISSUE TITLE:
[Track 015] Wincor/ProView EJ Parser Skeleton

ISSUE BODY:

# [Track 015] Wincor/ProView EJ Parser Skeleton

## Objective
بناء Skeleton منظم لـ Wincor/Nixdorf بدون افتراضات خاطئة.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not use one parser for all vendors.
- Do not classify success from APPROVED only.
- Do not discard unknown lines.

## Target Files / Layers
**Target layer:** Core/Parsing/Wincor

**Files / context:**
```text
WincorEjTransactionParser
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement Wincor parser skeleton.

Required work:
1. Define supported file patterns and timestamp formats as configurable strategy.
2. Extract only fields confidently parsed.
3. Unknown lines must be preserved as evidence.
4. Add fixtures even if minimal.
5. Mark low-confidence fields with ConfidenceScore.
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
- Parser registered.
- Fixtures pass.
- Raw evidence preserved.
- Confidence is explicit when uncertain.

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
