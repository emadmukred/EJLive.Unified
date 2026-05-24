ISSUE TITLE:
[Track 032] ATM Vendor Strategy Registry

ISSUE BODY:

# [Track 032] ATM Vendor Strategy Registry

## Objective
تحديد مسارات الجورنال والتحليل لكل نوع صراف.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not scatter vendor paths in UI.
- Do not hardcode one path globally.

## Target Files / Layers
**Target layer:** Core/Vendors

**Files / context:**
```text
IAtmVendorStrategy, strategies
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build ATM Vendor Strategy Registry.

Required work:
1. Create IAtmVendorStrategy with journal paths, file patterns, rollover logic, encoding, parser, XFS adapter, trace adapter, image destination policy.
2. Implement NcrVendorStrategy, GrgVendorStrategy, WincorVendorStrategy, DieboldVendorStrategy, HyosungVendorStrategy.
3. Select strategy from ATM type with normalization.
4. Add tests for vendor selection and defaults.
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
- Parser/FileWatcher/XFS selection comes from vendor strategy.

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
