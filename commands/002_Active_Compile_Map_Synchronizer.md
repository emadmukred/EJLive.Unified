# Command 002 — Active Compile Map Synchronizer

## Priority
حرجة

## Target layer
Verification/Core

## Files / context
*.csproj, docs/09-file-function-inventory.csv, docs/12-service-activation-status.csv

## Goal
كشف الملفات النشطة والمرجعية ومنع إدخال ملفات Reference بالخطأ.

## Paste this command to Codex
```text
Task: Build Active Compile Map Synchronizer.

Required work:
1. Create tool `tools/Generate-ActiveCompileMap.ps1` or C# equivalent.
2. Parse all `.csproj` files, including Compile Include/Remove and EnableDefaultCompileItems.
3. Compare against docs inventory CSV when available.
4. Output `artifacts/ActiveCompileMap.csv`.
5. Add verification rule that fails on undocumented sensitive compile files.
```

## Forbidden actions
- Do not compile reference-only files automatically.
- Do not delete unknown files.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Active and reference files are classified.
- Mismatches are reported.
- Verification fails on unsafe mismatch.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
