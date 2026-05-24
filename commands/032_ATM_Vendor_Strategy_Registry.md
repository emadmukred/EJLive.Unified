# Command 032 — ATM Vendor Strategy Registry

## Priority
عالية

## Target layer
Core/Vendors

## Files / context
IAtmVendorStrategy, strategies

## Goal
تحديد مسارات الجورنال والتحليل لكل نوع صراف.

## Paste this command to Codex
```text
Task: Build ATM Vendor Strategy Registry.

Required work:
1. Create IAtmVendorStrategy with journal paths, file patterns, rollover logic, encoding, parser, XFS adapter, trace adapter, image destination policy.
2. Implement NcrVendorStrategy, GrgVendorStrategy, WincorVendorStrategy, DieboldVendorStrategy, HyosungVendorStrategy.
3. Select strategy from ATM type with normalization.
4. Add tests for vendor selection and defaults.
```

## Forbidden actions
- Do not scatter vendor paths in UI.
- Do not hardcode one path globally.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Parser/FileWatcher/XFS selection comes from vendor strategy.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
