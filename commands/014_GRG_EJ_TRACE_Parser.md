# Command 014 — GRG EJ/TRACE Parser

## Priority
عالية

## Target layer
Core/Parsing/GRG

## Files / context
GrgEjTransactionParser, GrgTraceParser

## Goal
تحليل GRG بسياق الملفات اليومية وربط TRACE.

## Paste this command to Codex
```text
Task: Implement GRG parser skeleton and first functional pass.

Required work:
1. Support daily EJ files and trace file pairing.
2. Extract timestamp, operation id, card/account where available, amount/currency, host response, dispense result, device state.
3. Keep trace events separate but linkable through time window and transaction/session identifiers.
4. Add fixtures for success, decline, dispense failure, device fault, daily rollover.
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
