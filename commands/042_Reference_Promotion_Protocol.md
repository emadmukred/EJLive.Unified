# Command 042 — Reference Promotion Protocol

## Priority
حرجة

## Target layer
Docs/Verification

## Files / context
docs/original-audit, reference files

## Goal
ترقية الملفات المرجعية بأمان بدون كسر المشروع.

## Paste this command to Codex
```text
Task: Implement reference promotion protocol.

Required work:
1. Create `docs/promotions/` ticket template.
2. Each reference file promotion must state source file, target active service, adapter approach, tests, rollback.
3. Verification fails if reference file enters compile without promotion ticket.
4. Add promotion map CSV.
```

## Forbidden actions
- Do not compile reference files directly.
- Do not delete legacy/reference files without disposition plan.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Every promoted file has ticket and tests.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
