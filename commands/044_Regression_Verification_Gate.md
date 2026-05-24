# Command 044 — Regression Verification Gate

## Priority
حرجة

## Target layer
Tests/Verification

## Files / context
EJLive.Tests, EJLive.Verification

## Goal
منع التراجع وكسر الطبقات.

## Paste this command to Codex
```text
Task: Build regression verification gate.

Required probes:
1. Client.Service has no WinForms references.
2. UI handlers have no blocking file/socket/parsing logic.
3. Reference files are not compiled without promotion ticket.
4. Dangerous commands require policy/signature/audit.
5. Migrations exist for required tables.
6. Vendor parser registry includes expected vendors.
7. XFS adapters normalize unknown lines.
```

## Forbidden actions
- Do not allow warnings to hide hard architectural violations.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Verification fails on intentional violation.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
