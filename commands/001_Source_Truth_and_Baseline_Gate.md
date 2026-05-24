# Command 001 — Source Truth and Baseline Gate

## Priority
حرجة

## Target layer
Solution/Verification

## Files / context
EJLive.Unified.sln, EJLive.Unified.slnx, Directory.Build.props, src/**/*.csproj, docs/**/*.csv

## Goal
تثبيت مصدر الحقيقة ومنع أي تطوير قبل نجاح خط البناء والاختبار.

## Paste this command to Codex
```text
Task: Establish source-of-truth and baseline gate.

Scope:
- Do not add features.
- Do not refactor business logic.
- Only inspect solution/project/build/test/verification setup.

Required work:
1. Generate `docs/phase2-source-of-truth.md` containing repository branch, commit hash, solution files, SDK version, and build commands.
2. Generate `artifacts/baseline/<yyyyMMdd-HHmmss>/` with restore/build/test/verification logs.
3. If build fails, fix only project references, compile includes, namespaces, or missing links.
4. Create `artifacts/ActiveCompileMap.csv` with Project, FilePath, CompileState, Reason.
5. Add verification probe that fails if source-of-truth file is missing.
```

## Forbidden actions
- No feature work.
- No UI changes.
- No parser changes.
- No remote command changes.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Baseline logs exist.
- ActiveCompileMap.csv exists.
- restore/build/test/verification pass.
- All fixes are limited to build/project integrity.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
