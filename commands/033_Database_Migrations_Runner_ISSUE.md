ISSUE TITLE:
[Track 033] Database Migrations Runner

ISSUE BODY:

# [Track 033] Database Migrations Runner

## Objective
إدارة الجداول بالإصدارات لا بتعديلات عشوائية.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not modify SQLite schema ad-hoc.
- Do not drop production tables in migration.

## Target Files / Layers
**Target layer:** Core/Data

**Files / context:**
```text
Data/Migrations, DatabaseMigrationRunner
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build versioned database migration runner.

Required work:
1. Create `schema_migrations` table.
2. Create migrations folder under `src/EJLive.Core/Data/Migrations/`.
3. Required tables: source_truth_records, active_compile_map, transfer_sessions, journal_archive, parser_transactions, vendor_events, correlation_events, command_queue, command_audit, telemetry_events, client_health_snapshots.
4. Each migration has MigrationId, Description, Up SQL, optional Down SQL, Verification Query.
5. Add tests for fresh DB and existing DB upgrade.
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
- Fresh and upgrade migrations pass.
- Verification detects missing tables.

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
