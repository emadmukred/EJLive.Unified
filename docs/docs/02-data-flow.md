# EJLive Data Flow

## Client Journal Sync

1. `ClientMainForm` receives operator action or file watcher signal.
2. `EJLive.Application` can validate workflow readiness and route to business runtime.
3. `UnifiedBusinessRuntime` exposes journal sync, state tracking, alerts, and engines.
4. `JournalOutbox`, `NetworkEngine`, and `CommunicationProtocol` package files into handshake/start/chunk/complete frames.
5. `SecurityHelper` supplies checksums, compression, and encryption helpers.
6. `ServerEngine` receives frames and updates connected ATM state.
7. `DatabaseManager` persists audit/sync records to SQLite.
8. Server/monitoring UI reads snapshots and refreshes grids, cards, maps, logs, and reports.

## Remote Command Flow

1. Server operator selects target in `ServerMainForm`.
2. Server builds `RemoteCommandEnvelope`.
3. `ServerEngine.SendCommand` or `BroadcastCommand` sends the command frame.
4. Client `NetworkEngine` receives and passes the message to `ClientMainForm`.
5. Client executes supported local action or returns a guarded refusal for sensitive operations.
6. Client sends command result through `CommunicationProtocol.BuildCommandResult`.
7. Server logs the result and updates command grid.

## Database Flow

| Source | Service | Store | Consumer |
|---|---|---|---|
| UI settings and runtime events | `DatabaseManager`, `AuditLogger`, `JournalSyncTrackingService` | SQLite tables `audit_log`, `sync_records` | Server dashboard, verification, reports |
| ATM connection events | `OperationalStateStore` | In-memory state, optional audit/sync records | Fleet cards, network map, monitoring dashboard |
| Alerts and failures | `AlertManager`, `JournalSyncAlertService` | In-memory alert list plus audit hooks | Alerts tab, reports, verification |

## Reference Source Flow

Imported source archives do not flow into the runtime assembly directly. They flow into `EJLive.LegacyReference` as `None` items so Visual Studio can browse them without compiling duplicate/conflicting classes. Future migrations should promote one behavior at a time into `EJLive.Core`, `EJLive.Business`, or `EJLive.Application`, then add focused tests before replacing compiled callers.
