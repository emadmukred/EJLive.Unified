# Command 033 — Database Migrations Runner

## Priority
حرجة

## Target layer
Core/Data

## Files / context
Data/Migrations, DatabaseMigrationRunner

## Goal
إدارة الجداول بالإصدارات لا بتعديلات عشوائية.

## Paste this command to Codex
```text
Task: Build versioned database migration runner.

Required work:
1. Create `schema_migrations` table.
2. Create migrations folder under `src/EJLive.Core/Data/Migrations/`.
3. Required tables: source_truth_records, active_compile_map, transfer_sessions, journal_archive, parser_transactions, vendor_events, correlation_events, command_queue, command_audit, telemetry_events, client_health_snapshots.
4. Each migration has MigrationId, Description, Up SQL, optional Down SQL, Verification Query.
5. Add tests for fresh DB and existing DB upgrade.
```

## Forbidden actions
- Do not modify SQLite schema ad-hoc.
- Do not drop production tables in migration.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Fresh and upgrade migrations pass.
- Verification detects missing tables.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
