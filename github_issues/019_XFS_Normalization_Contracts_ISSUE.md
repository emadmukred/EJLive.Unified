ISSUE TITLE:
[Track 019] XFS Normalization Contracts

ISSUE BODY:

# [Track 019] XFS Normalization Contracts

## Objective
تأسيس طبقة XFS موحدة حسب معيار متعدد البائعين.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not discard raw XFS lines.
- Do not vendor-lock Core contracts.

## Target Files / Layers
**Target layer:** Core/XFS

**Files / context:**
```text
IXfsVendorAdapter, NormalizedXfsEvent, XfsAdapterRegistry
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Build XFS normalization foundation.

Required work:
1. Define `IXfsVendorAdapter` and `XfsAdapterRegistry`.
2. Define `NormalizedXfsEvent`: EventId, ATM_ID, Vendor, DeviceClass, Code, Message, Severity, TimestampUtc, SourceFile, RawLineNumber, RawLine.
3. DeviceClass enum must include CDM, IDC, PTR, SIU, PIN, CIM, VDM, Camera, Alarm, Unknown.
4. Unknown codes must be preserved.
5. Add unit tests for adapter registration and unknown event preservation.
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
- All vendor events normalize to common model.
- Unknown codes are searchable.

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
