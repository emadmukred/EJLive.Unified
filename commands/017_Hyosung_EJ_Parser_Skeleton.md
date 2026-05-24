# Command 017 — Hyosung EJ Parser Skeleton

## Priority
متوسطة

## Target layer
Core/Parsing/Hyosung

## Files / context
HyosungEjTransactionParser

## Goal
تهيئة Hyosung Parser قابل للتوسع لاحقاً.

## Paste this command to Codex
```text
Task: Implement Hyosung parser skeleton.

Required work:
1. Register vendor parser in EjParserRegistry.
2. Extract safe common fields only.
3. Preserve raw evidence and confidence.
4. Add minimal fixtures and tests.
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
