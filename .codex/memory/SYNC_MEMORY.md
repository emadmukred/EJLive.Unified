# Sync and Journal Memory

## Critical sync requirements

- Persistent sync state is required.
- Client outbox queue is required.
- Delivery confirmation is required.
- Idempotency is required.
- Checksum verification is required.
- Offset tracking is required for NCR fixed journal files.
- Daily file identity tracking is required for GRG/Wincor style logs.

## Recommended state fields

- SyncId
- ATM_ID
- FileName
- FilePath
- Offset
- Length
- Checksum
- State
- RetryCount
- LastAttemptUtc
- LastAckUtc
- ErrorCode
- ErrorMessage

## Idempotency key

ATM_ID + FileName + Offset + Checksum + SyncId

## Protocol direction

Prefer chunked/length-prefixed transfer:

START_FILE → CHUNK → CHUNK_ACK → COMPLETE → VERIFY

## Safety note

Do not lock live ATM journal files. Use safe file sharing and incremental reads.
