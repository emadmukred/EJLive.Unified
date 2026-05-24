# Command 015 — Wincor/ProView EJ Parser Skeleton

## Priority
عالية

## Target layer
Core/Parsing/Wincor

## Files / context
WincorEjTransactionParser

## Goal
بناء Skeleton منظم لـ Wincor/Nixdorf بدون افتراضات خاطئة.

## Paste this command to Codex
```text
Task: Implement Wincor parser skeleton.

Required work:
1. Define supported file patterns and timestamp formats as configurable strategy.
2. Extract only fields confidently parsed.
3. Unknown lines must be preserved as evidence.
4. Add fixtures even if minimal.
5. Mark low-confidence fields with ConfidenceScore.
```

## Forbidden actions
- Do not use one parser for all vendors.
- Do not classify success from APPROVED only.
- Do not discard unknown lines.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Parser registered.
- Fixtures pass.
- Raw evidence preserved.
- Confidence is explicit when uncertain.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
