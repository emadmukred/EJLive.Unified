ISSUE TITLE:
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
