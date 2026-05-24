# Command 027 — Windows Policy Enforcer Audit Mode

## Priority
حرجة

## Target layer
Client/Core/Security

## Files / context
SafeWindowsPolicyEnforcer, WindowsRemoteAccessService

## Goal
تحويل Registry/RDP/Firewall/WinRM إلى عمليات Audit/Enforce قابلة للرجوع.

## Paste this command to Codex
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

## Forbidden actions
- Do not disable Defender.
- Do not disable Firewall.
- Do not bypass GPO.
- Do not open RDP/WinRM globally.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Audit mode changes nothing.
- Enforce mode writes audit and rollback plan.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
