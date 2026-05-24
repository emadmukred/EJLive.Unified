# Command 012 — EJ Parser Registry and Contracts

## Priority
حرجة

## Target layer
Core/Parsing

## Files / context
IEjTransactionParser, EjParserRegistry, EjParseContext, EjTransaction

## Goal
تأسيس عقود تحليل الجورنال حسب نوع الصراف.

## Paste this command to Codex
```text
Task: Build vendor-specific EJ parser foundation.

Required work:
1. Create `IEjTransactionParser` with Vendor, CanParse, Parse.
2. Create `EjParserRegistry` selecting parser by ATM vendor/type and file context.
3. Create `EjParseContext` with ATM_ID, Vendor, SourceFile, Encoding, Lines, FileTimestamp, TimeZone.
4. Create `EjTransaction` and `EjTransactionEvidence` preserving raw line ranges.
5. Create GenericFallbackSignalParser as last resort only.
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
