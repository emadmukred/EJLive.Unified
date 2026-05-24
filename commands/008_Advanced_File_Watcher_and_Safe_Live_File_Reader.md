# Command 008 — Advanced File Watcher and Safe Live File Reader

## Priority
حرجة

## Target layer
Client.Service/Core/File

## Files / context
FileWatcherEngine, SafeLiveFileReader, JournalOffsetStore

## Goal
قراءة ملفات EJ الحية بدون قفل وبدون تكرار.

## Paste this command to Codex
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

## Forbidden actions
- Do not lock ATM journal files exclusively.
- Do not resend whole file when offset delta is enough.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- No duplicate bytes after restart.
- No lock on EJDATA.LOG.
- Rollover/truncate detected.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
