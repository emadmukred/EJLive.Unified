# Command 016 — Diebold/Agilis EJ Parser Skeleton

## Priority
عالية

## Target layer
Core/Parsing/Diebold

## Files / context
DieboldEjTransactionParser

## Goal
بناء Skeleton لـ Diebold مع فصل host/device events.

## Paste this command to Codex
```text
Task: Implement Diebold parser skeleton.

Required work:
1. Define Diebold vendor strategy and expected log categories.
2. Parse timestamp, device class, host response, transaction hints.
3. Preserve unknown raw lines.
4. Add fixture-based tests.
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
