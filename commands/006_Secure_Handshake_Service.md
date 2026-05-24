# Command 006 — Secure Handshake Service

## Priority
حرجة

## Target layer
Core/Network

## Files / context
CommunicationProtocol, NetworkEngine, ServerEngine, Client.Service

## Goal
بناء جلسة اتصال موثقة بين الكلاينت والسيرفر.

## Paste this command to Codex
```text
Task: Implement SecureHandshakeService.

Required work:
1. Define `HandshakeRequest`, `HandshakeResponse`, `HandshakeState`.
2. Include SessionId, ProtocolVersion, ClientVersion, ATM_ID, MachineId, Nonce, TimestampUtc, Capabilities.
3. Server must reject stale timestamps and duplicate sessions for same ATM unless explicitly replacing previous session.
4. Client must persist session state and expire it on disconnect.
5. Add tests for valid, stale, malformed, duplicate handshakes.
```

## Forbidden actions
- Do not use handshake as security replacement for TLS/signatures.
- Do not accept anonymous ATM_ID.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Handshake required before file transfer or commands.
- Duplicate sessions are visible and resolved deterministically.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
