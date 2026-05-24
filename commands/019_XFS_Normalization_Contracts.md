# Command 019 — XFS Normalization Contracts

## Priority
حرجة

## Target layer
Core/XFS

## Files / context
IXfsVendorAdapter, NormalizedXfsEvent, XfsAdapterRegistry

## Goal
تأسيس طبقة XFS موحدة حسب معيار متعدد البائعين.

## Paste this command to Codex
```text
Task: Build XFS normalization foundation.

Required work:
1. Define `IXfsVendorAdapter` and `XfsAdapterRegistry`.
2. Define `NormalizedXfsEvent`: EventId, ATM_ID, Vendor, DeviceClass, Code, Message, Severity, TimestampUtc, SourceFile, RawLineNumber, RawLine.
3. DeviceClass enum must include CDM, IDC, PTR, SIU, PIN, CIM, VDM, Camera, Alarm, Unknown.
4. Unknown codes must be preserved.
5. Add unit tests for adapter registration and unknown event preservation.
```

## Forbidden actions
- Do not discard raw XFS lines.
- Do not vendor-lock Core contracts.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- All vendor events normalize to common model.
- Unknown codes are searchable.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
