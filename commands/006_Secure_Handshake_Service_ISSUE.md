ISSUE TITLE:
[Track 006] Secure Handshake Service

ISSUE BODY:

# [Track 006] Secure Handshake Service

## Objective
بناء جلسة اتصال موثقة بين الكلاينت والسيرفر.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not use handshake as security replacement for TLS/signatures.
- Do not accept anonymous ATM_ID.

## Target Files / Layers
**Target layer:** Core/Network

**Files / context:**
```text
CommunicationProtocol, NetworkEngine, ServerEngine, Client.Service
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement SecureHandshakeService.

Required work:
1. Define `HandshakeRequest`, `HandshakeResponse`, `HandshakeState`.
2. Include SessionId, ProtocolVersion, ClientVersion, ATM_ID, MachineId, Nonce, TimestampUtc, Capabilities.
3. Server must reject stale timestamps and duplicate sessions for same ATM unless explicitly replacing previous session.
4. Client must persist session state and expire it on disconnect.
5. Add tests for valid, stale, malformed, duplicate handshakes.
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
- Handshake required before file transfer or commands.
- Duplicate sessions are visible and resolved deterministically.

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
