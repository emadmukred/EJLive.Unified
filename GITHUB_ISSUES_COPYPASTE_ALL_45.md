# EJLive GitHub Issues — Ready for Copy/Paste

استخدم كل قسم كـ GitHub Issue مستقل. لا تنسخ كل الأوامر في Issue واحد. افتح Issue منفصل لكل Track.


---

## ISSUE TITLE

[Track 001] Source Truth and Baseline Gate

## ISSUE BODY

# [Track 001] Source Truth and Baseline Gate

## Objective
تثبيت مصدر الحقيقة ومنع أي تطوير قبل نجاح خط البناء والاختبار.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No feature work.
- No UI changes.
- No parser changes.
- No remote command changes.

## Target Files / Layers
**Target layer:** Solution/Verification

**Files / context:**
```text
EJLive.Unified.sln, EJLive.Unified.slnx, Directory.Build.props, src/**/*.csproj, docs/**/*.csv
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

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
- Baseline logs exist.
- ActiveCompileMap.csv exists.
- restore/build/test/verification pass.
- All fixes are limited to build/project integrity.

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

---

## IMPORTANT

The full 45-issue copy/paste package is also available as a downloadable artifact from ChatGPT. This repository file includes Track 001 as the required first issue. Upload the full `commands/` folder or the generated `EJLive_GitHub_Issues_CopyPaste_All_45.md` if you want every body inside the repository.
