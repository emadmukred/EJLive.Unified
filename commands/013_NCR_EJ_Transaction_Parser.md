# Command 013 — NCR EJ Transaction Parser

## Priority
حرجة

## Target layer
Core/Parsing/NCR

## Files / context
NcrEjTransactionParser

## Goal
تحليل NCR بالاعتماد على حدود العملية وإشارات الصرف الفعلية.

## Paste this command to Codex
```text
Task: Implement NCR EJ parser.

Required markers:
*TRANSACTION START*, TRANSACTION END, CARD INSERTED, PIN ENTERED, ATR RECEIVED, GENAC 1 : ARQC, GENAC 2 : TC, NOTES STACKED, NOTES PRESENTED, NOTES TAKEN, DIST CASH, M-codes, R-codes, STAN, RRN, Account, Masked Card, Amount, Currency.

Required classifications:
Success, Failed, Suspicious, Reversal, PartialDispense, ApprovedNoDispense, CashJam, Retract, CardCaptured, HostDeclined, HardwareFault, MissingSequence, DuplicateSequence.

Rules:
- Success must not depend on APPROVED alone.
- Success requires NOTES PRESENTED + NOTES TAKEN or a documented alternative evidence.
- Preserve raw start/end line.
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
