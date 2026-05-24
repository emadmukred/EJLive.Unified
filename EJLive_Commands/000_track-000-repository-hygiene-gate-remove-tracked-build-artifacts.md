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

## CODEX COMMAND — [Track 000] Repository Hygiene Gate — Remove Tracked Build Artifacts

**Suggested branch:** `track-000-repository-hygiene-gate-remove-tracked-build-artifacts`  
**GitHub Issue:** `#9`

انسخ النص التالي كاملًا إلى Codex:

```text
You are Codex working inside the EJLive.Unified repository.

Execute exactly this issue only:
[Track 000] Repository Hygiene Gate — Remove Tracked Build Artifacts
GitHub Issue: #9

Mandatory pre-checks:
1. Read AGENTS.md if present.
2. Read MASTER_COMMAND_INDEX.md if present.
3. Read .codex/instructions/EJLive_Strict_Project_Rules.md if present.
4. Run: git branch --show-current
5. Run: git status --short
6. If the working tree is not clean, STOP and report the dirty files. Do not continue.
7. Use one isolated branch only. Suggested branch name: track-000-repository-hygiene-gate-remove-tracked-build-artifacts

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
[Track 000] Repository Hygiene Gate — Remove Tracked Build Artifacts
Objective
Fix repository hygiene before running any Phase-2 development tracks. The repository currently tracks build outputs such as bin/, obj/, .vs/, *.dll, *.pdb, caches, and generated MSBuild files. These files cause huge diffs after every build and make Track 001 and later PRs unsafe to review.

This issue must run before Track 001, Track 002, and all later issues.

Scope
Allowed:

.gitignore
tracked build artifacts removal from Git index
repository cleanup documentation
verification that no bin/obj/.vs files remain tracked
Forbidden:

Do not change application logic.
Do not change UI.
Do not change parser logic.
Do not change remote command logic.
Do not change service behavior.
Do not run broad refactors.
Required Actions
Create or update .gitignore to exclude at minimum:
[Bb]in/
[Oo]bj/
.vs/
TestResults/
*.dll
*.exe
*.pdb
*.deps.json
*.runtimeconfig.json
*.cache
*.user
*.suo
*.rsuser
Remove tracked build artifacts from Git index only. Do not delete source files.
Required cleanup examples:

git rm -r --cached --ignore-unmatch .vs
git rm -r --cached --ignore-unmatch "src/**/bin"
git rm -r --cached --ignore-unmatch "src/**/obj"
If shell glob support differs on Windows, enumerate project folders explicitly.

Verify tracked artifacts are gone:
git ls-files | grep -E "(^|/)(bin|obj)/|^\.vs/|\.dll$|\.pdb$|\.cache$|\.deps\.json$|\.runtimeconfig\.json$"
On Windows CMD/PowerShell, use equivalent commands.

Add documentation:
docs/repository-hygiene.md
The document must explain:

why build outputs are ignored
what should never be committed
how to clean local build outputs safely
how to verify the repository is clean
Required Verification Commands
Run after cleanup:

git status
git diff --stat
git ls-files | findstr /I "\\bin\\ \\obj\\ .vs .dll .pdb .cache .deps.json .runtimeconfig.json"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
If build fails because of real source errors, document them but do not fix them in this issue unless the failure is caused by accidentally removing a required source file. Track 001 is responsible for baseline build fixes.

Acceptance Criteria
.gitignore exists and covers build outputs.
bin/, obj/, .vs/, compiled binaries, PDBs, generated caches, and runtime files are no longer tracked by Git.
Running dotnet build does not create tracked modified files in git status.
PR file count is reasonable and mostly consists of deletions of tracked generated files plus .gitignore and documentation.
No source behavior changed.
Required Output
Modified files list
Removed tracked artifact count
git ls-files verification result
Build result
Remaining real source errors, if any
Rollback plan
Execution Order
This issue must be completed before:

#3 Track 001 Source Truth and Baseline Gate
#4 Track 002 Active Compile Map Synchronizer
#5 Track 003 Client Service Headless Foundation
#6 Track 004 UI Backend Separation Gate
#7 Track 005A Client Companion UI Preservation
#8 Track 005B Unified Client UI
Important
Do not mix this cleanup with Track 001. This is a separate PR whose only purpose is repository hygiene.
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

