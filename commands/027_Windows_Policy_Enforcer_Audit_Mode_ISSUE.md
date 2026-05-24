ISSUE TITLE:
[Track 027] Windows Policy Enforcer Audit Mode

ISSUE BODY:

# [Track 027] Windows Policy Enforcer Audit Mode

## Objective
تحويل Registry/RDP/Firewall/WinRM إلى عمليات Audit/Enforce قابلة للرجوع.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not disable Defender.
- Do not disable Firewall.
- Do not bypass GPO.
- Do not open RDP/WinRM globally.

## Target Files / Layers
**Target layer:** Client/Core/Security

**Files / context:**
```text
SafeWindowsPolicyEnforcer, WindowsRemoteAccessService
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement Windows Policy Enforcer with Audit-first behavior.

Required work:
1. Default mode is Audit only.
2. Enforce mode requires explicit config and signed command.
3. Capture BeforeSnapshot and AfterSnapshot.
4. Generate RollbackPlan for any change.
5. Respect Domain GPO; do not override domain policy.
6. Firewall changes must be scoped to approved ServerIP only.
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
- Audit mode changes nothing.
- Enforce mode writes audit and rollback plan.

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
