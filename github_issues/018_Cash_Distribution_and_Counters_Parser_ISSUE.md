ISSUE TITLE:
[Track 018] Cash Distribution and Counters Parser

ISSUE BODY:

# [Track 018] Cash Distribution and Counters Parser

## Objective
استخراج توزيع الكاش والكاسيت من الجورنال والحسابات.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not infer cash loss without evidence.
- Do not mix counters snapshots with per-transaction dispense without timestamps.

## Target Files / Layers
**Target layer:** Core/Parsing/Cash

**Files / context:**
```text
CashDistributionParser, CassetteCounterParser
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build cash distribution/counters parser.

Required work:
1. Extract DIST CASH CASS 1..4 values.
2. Extract cassette counters: CASSETTE, REJECTED, REMAINING, DISPENSED, TOTAL.
3. Normalize denominations and currency.
4. Link cash evidence to EjTransaction.
5. Add tests for missing cassette line, partial dispense, reject/retract evidence.
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
- Cash fields appear in parser_transactions.
- Partial dispense/cash risk reports use evidence not guesswork.

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
