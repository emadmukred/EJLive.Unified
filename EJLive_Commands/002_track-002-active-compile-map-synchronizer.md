# EJLive Codex Execution Prompts — Commands Matching Each GitHub Issue

هذا الملف يحوّل كل GitHub Issue إلى أمر جاهز للنسخ داخل Codex Desktop/CLI.

## طريقة الاستخدام الصارمة

1. نفّذ أمرًا واحدًا فقط في كل جلسة Codex.
2. افتح Branch منفصل لكل Track.
3. لا تنفذ Track جديد قبل إنهاء السابق أو حفظه في Branch منفصل.
4. لا تسمح لـ Codex بخلط أكثر من Issue في نفس التعديل.
5. بعد كل Build يجب أن يكون `git status` خاليًا من `bin/`, `obj/`, `.vs/`.
6. لا تعمل Merge إلى `main` إلا بعد مراجعة Pull Request ونجاح restore/build/test/verification.

## أوامر التحقق القياسية

```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
dotnet build .\EJLive.Unified.slnx --no-restore -m:1 /p:BuildInParallel=false -v:m
```

## CODEX COMMAND — [Track 002] Active Compile Map Synchronizer

**Suggested branch:** `track-002-active-compile-map-synchronizer`  
**GitHub Issue:** `#4`

انسخ النص التالي كاملًا إلى Codex:

```text
You are Codex working inside the EJLive.Unified repository.

Execute exactly this issue only:
[Track 002] Active Compile Map Synchronizer
GitHub Issue: #4

Mandatory pre-checks:
1. Read AGENTS.md if present.
2. Read MASTER_COMMAND_INDEX.md if present.
3. Read .codex/instructions/EJLive_Strict_Project_Rules.md if present.
4. Run: git branch --show-current
5. Run: git status --short
6. If the working tree is not clean, STOP and report the dirty files. Do not continue.
7. Use one isolated branch only. Suggested branch name: track-002-active-compile-map-synchronizer

Strict execution rules:
- Do not move to any other Track.
- Do not implement unrelated features.
- Do not rewrite the whole project.
- Do not delete working legacy/reference code unless the issue explicitly requires it and a replacement/rollback is documented.
- Do not commit build outputs: bin/, obj/, .vs/, *.dll, *.exe, *.pdb, *.cache, *.deps.json, *.runtimeconfig.json.
- Do not add stealth behavior.
- Do not disable Defender, firewall, EDR, logging, audit, or security tools.
- Do not add arbitrary shell execution.
- Do not log secrets, passwords, service keys, card numbers, or account numbers.
- If a requested change is unsafe, implement the safe governed equivalent and document the reason.
- If build/test fails, fix only inside the issue scope. If failure is unrelated, document it and stop.

Issue specification:
---
# [Track 002] Active Compile Map Synchronizer

## Objective
كشف الملفات النشطة والمرجعية ومنع إدخال ملفات Reference بالخطأ.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not compile reference-only files automatically.
- Do not delete unknown files.

## Target Files / Layers
**Target layer:** Verification/Core

**Files / context:**
```text
*.csproj, docs/09-file-function-inventory.csv, docs/12-service-activation-status.csv
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build Active Compile Map Synchronizer.

Required work:
1. Create tool `tools/Generate-ActiveCompileMap.ps1` or C# equivalent.
2. Parse all `.csproj` files, including Compile Include/Remove and EnableDefaultCompileItems.
3. Compare against docs inventory CSV when available.
4. Output `artifacts/ActiveCompileMap.csv`.
5. Add verification rule that fails on undocumented sensitive compile files.
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
- Active and reference files are classified.
- Mismatches are reported.
- Verification fails on unsafe mismatch.

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

After implementation:
1. Run the verification commands required by the issue.
2. Run: git status --short
3. Confirm that git status does not include bin/, obj/, .vs/, dll, exe, pdb, or generated cache files.
4. Produce a final report with:
   - Modified files
   - Added files
   - Removed/deprecated files, if any
   - Tests added/updated
   - Commands executed
   - Verification result
   - Known limitations
   - Rollback plan

Do not create or merge a pull request unless explicitly asked in this session.
```

---

