# Command 025 — EJ XFS TRACE Correlation Engine

## Priority
حرجة

## Target layer
Core/Correlation

## Files / context
CorrelationEngine, CorrelationEvent

## Goal
ربط العمليات المالية بأحداث الجهاز بثقة محسوبة.

## Paste this command to Codex
```text
Task: Build EJ + XFS + TRACE Correlation Engine.

Required matching levels:
1. Strong: TransactionNumber, STAN, RRN.
2. Medium: ATM_ID + timestamp window + device class + host direction.
3. Weak: nearby timestamp + same file session + error burst.

Required output:
- CorrelationId
- TransactionId
- VendorEventId
- ConfidenceScore
- Impact
- Explanation

Add tests for exact match, time-window match, no match, multiple candidates.
```

## Forbidden actions
- Do not claim cash loss from weak match.
- Do not hide uncorrelated events.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Correlation confidence is explainable.
- Cash risk depends on evidence.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
