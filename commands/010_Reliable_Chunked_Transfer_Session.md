# Command 010 — Reliable Chunked Transfer Session

## Priority
حرجة

## Target layer
Core/Network/Server

## Files / context
CommunicationProtocol, NetworkEngine, ServerEngine, TransferSession

## Goal
نقل ملفات قابل للاستكمال مع SHA256 وACK/NAK.

## Paste this command to Codex
```text
Task: Implement reliable chunked transfer.

Required work:
1. Add `TransferSession` model: TransferId, ATM_ID, FileName, Length, ChunkSize, TotalChunks, ReceivedChunksBitmap, FileSHA256, NextExpectedOffset.
2. Protocol flow: StartFile -> Chunk -> ChunkAck/ChunkNak -> Complete -> VerifySHA256 -> JournalAck.
3. Server writes to staging until hash verifies.
4. Resume transfer after disconnect from NextExpectedOffset.
5. Add tests for disconnect at 90%, checksum mismatch, duplicate Complete, out-of-order chunk.
```

## Forbidden actions
- Do not archive before full SHA256 verification.
- Do not rely on TCP success alone.

## Verification commands
```powershell
$env:EJLIVE_DATABASE_PATH = "$env:TEMP\ejlive_codex_verify.db"
dotnet restore .\EJLive.Unified.sln
dotnet build .\EJLive.Unified.sln --no-restore -m:1 /p:BuildInParallel=false -v:m
dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false --logger "console;verbosity=minimal"
dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj --no-restore -m:1 /p:BuildInParallel=false
```

## Acceptance criteria
- Large files resume without full resend.
- Duplicate archive prevented.
- Corrupted transfer rejected.

## Required Codex output
- Modified files
- Added files
- Deprecated/removed files, if any
- Tests added or updated
- Commands executed
- Verification result
- Known limitations
- Rollback plan
