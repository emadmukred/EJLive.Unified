ISSUE TITLE:
[Track 035] Security Hardening and Secrets

ISSUE BODY:

# [Track 035] Security Hardening and Secrets

## Objective
تقوية التشفير والتوقيع وحماية الأسرار.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- No plaintext passwords/secrets in config/logs.
- No weak hash for security decision.

## Target Files / Layers
**Target layer:** Core/Security

**Files / context:**
```text
SecurityHelper, SecretProtector, CommandSigningEngine
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Harden security and secrets.

Required work:
1. Use DPAPI or Windows Credential Manager for local secrets.
2. Use SHA256/HMAC for integrity/signatures; MD5 only as legacy identifier.
3. Add KeyRotationVersion.
4. Add log redaction.
5. Production transport must support TLS or a clearly documented secure channel strategy.
6. Add tests for DPAPI roundtrip, signature reject, redaction.
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
- Secret roundtrip works.
- Invalid signatures fail.
- Logs are redacted.

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
