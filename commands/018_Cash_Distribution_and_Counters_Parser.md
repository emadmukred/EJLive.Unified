# Command 018 — Cash Distribution and Counters Parser

## Priority
عالية

## Target layer
Core/Parsing/Cash

## Files / context
CashDistributionParser, CassetteCounterParser

## Goal
استخراج توزيع الكاش والكاسيت من الجورنال والحسابات.

## Paste this command to Codex
```text
Task: Build cash distribution/counters parser.

Required work:
1. Extract DIST CASH CASS 1..4 values.
2. Extract cassette counters: CASSETTE, REJECTED, REMAINING, DISPENSED, TOTAL.
3. Normalize denominations and currency.
4. Link cash evidence to EjTransaction.
5. Add tests for missing cassette line, partial dispense, reject/retract evidence.
```

## Forbidden actions
- Do not infer cash loss without evidence.
- Do not mix counters snapshots with per-transaction dispense without timestamps.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Cash fields appear in parser_transactions.
- Partial dispense/cash risk reports use evidence not guesswork.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
