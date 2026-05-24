# Command 040 — Time Sync Service

## Priority
متوسطة

## Target layer
Client/Server/Core

## Files / context
TimeSyncService

## Goal
مزامنة توقيت ATM مع السيرفر بطريقة مدققة.

## Paste this command to Codex
```text
Task: Build controlled time sync.

Required work:
1. Server exposes signed server time response.
2. Client calculates clock skew.
3. Audit time adjustment intent and result.
4. Enforce max allowed skew adjustment unless admin override.
5. Add tests for small skew, large skew, invalid signature.
```

## Forbidden actions
- Do not adjust system time without signed command/policy.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Time sync is auditable and bounded.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
