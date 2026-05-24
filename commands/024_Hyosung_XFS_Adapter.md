# Command 024 — Hyosung XFS Adapter

## Priority
عالية

## Target layer
Core/XFS

## Files / context
HyosungXfsAdapter

## Goal
تطبيع أحداث XFS/Trace الخاصة بـ Hyosung.

## Paste this command to Codex
```text
Task: Implement Hyosung XFS adapter.

Required work:
1. Create `HyosungXfsAdapter` implementing IXfsVendorAdapter.
2. Parse device class when possible: CDM, IDC, PTR, SIU, PIN, CIM, VDM.
3. Extract severity, code, message, timestamp, raw line number.
4. Preserve source file and raw line.
5. Add fixtures for at least error, warning, info, unknown code.
```

## Forbidden actions
- Do not hardcode one ATM model only.
- Do not drop unknown vendor codes.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Adapter registered.
- Normalized events produced.
- Unknown events preserved.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
