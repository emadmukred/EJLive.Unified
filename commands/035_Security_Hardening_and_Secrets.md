# Command 035 — Security Hardening and Secrets

## Priority
حرجة

## Target layer
Core/Security

## Files / context
SecurityHelper, SecretProtector, CommandSigningEngine

## Goal
تقوية التشفير والتوقيع وحماية الأسرار.

## Paste this command to Codex
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

## Forbidden actions
- No plaintext passwords/secrets in config/logs.
- No weak hash for security decision.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Secret roundtrip works.
- Invalid signatures fail.
- Logs are redacted.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
