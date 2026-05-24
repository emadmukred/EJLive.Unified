ISSUE TITLE:
[Track 013] NCR EJ Transaction Parser

ISSUE BODY:

# [Track 013] NCR EJ Transaction Parser

## Objective
تحليل NCR بالاعتماد على حدود العملية وإشارات الصرف الفعلية.

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
**Target layer:** Core/Parsing/NCR

**Files / context:**
```text
NcrEjTransactionParser
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

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
