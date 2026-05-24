ISSUE TITLE:
[Track 010] Reliable Chunked Transfer Session

ISSUE BODY:

# [Track 010] Reliable Chunked Transfer Session

## Objective
نقل ملفات قابل للاستكمال مع SHA256 وACK/NAK.

## Scope
Allowed:
- Work only inside the target layer and files listed below.
- Add or update tests directly related to this Track.
- Add adapters/facades instead of deleting existing working code.

Forbidden:
- Do not archive before full SHA256 verification.
- Do not rely on TCP success alone.

## Target Files / Layers
**Target layer:** Core/Network/Server

**Files / context:**
```text
CommunicationProtocol, NetworkEngine, ServerEngine, TransferSession
```

## Required Actions
Run this as one isolated Codex task. Do not continue to any other Track.

```text
Task: Implement reliable chunked transfer.

Required work:
1. Add `TransferSession` model: TransferId, ATM_ID, FileName, Length, ChunkSize, TotalChunks, ReceivedChunksBitmap, FileSHA256, NextExpectedOffset.
2. Protocol flow: StartFile -> Chunk -> ChunkAck/ChunkNak -> Complete -> VerifySHA256 -> JournalAck.
3. Server writes to staging until hash verifies.
4. Resume transfer after disconnect from NextExpectedOffset.
5. Add tests for disconnect at 90%, checksum mismatch, duplicate Complete, out-of-order chunk.
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
- Large files resume without full resend.
- Duplicate archive prevented.
- Corrupted transfer rejected.

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
