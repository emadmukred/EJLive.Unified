ISSUE TITLE:
[Track 008] Advanced File Watcher and Safe Live File Reader

ISSUE BODY:

# [Track 008] Advanced File Watcher and Safe Live File Reader

## Objective
قراءة ملفات EJ الحية بدون قفل وبدون تكرار.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not lock ATM journal files exclusively.
- Do not resend whole file when offset delta is enough.

## Target Files / Layers
**Target layer:** Client.Service/Core/File

**Files / context:**
```text
FileWatcherEngine, SafeLiveFileReader, JournalOffsetStore
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement AdvancedFileWatcher and SafeLiveFileReader.

Required work:
1. Combine FileSystemWatcher with polling fallback.
2. Use FileShare.ReadWrite | FileShare.Delete for live file reads.
3. Add StableReadWindow to avoid partial line reads.
4. Add JournalOffsetStore persisted in SQLite or client_state.json.
5. Support vendor file strategies: NCR overwrite, GRG daily files, Wincor/Diebold/Hyosung patterns.
6. Add tests for growing file, truncate, rollover, locked file, and partial last line.
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
- No duplicate bytes after restart.
- No lock on EJDATA.LOG.
- Rollover/truncate detected.

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
