# Command 029 — File Image Distribution Service

## Priority
عالية

## Target layer
Server/Client/Core

## Files / context
FileDistributionService, ImageSyncEngine

## Goal
توزيع الصور والملفات للسرفر/الكلاينت عبر staging وchecksum.

## Paste this command to Codex
```text
Task: Build server-to-ATM file/image distribution.

Required work:
1. Define AllowedDestinationFolders per vendor/type.
2. Receive to staging first.
3. Verify SHA256.
4. Promote atomically to destination.
5. Return receipt: destination, checksum, bytes, status.
6. Add rollback when promote fails.
```

## Forbidden actions
- No path traversal.
- No deployment outside allowlist.
- No executable replacement without signing policy.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- File arrives in correct allowed folder.
- Receipt stored and visible on server.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
