# Verification Checklist

## Build

- Command executed: `dotnet build EJLive.Unified\EJLive.Unified.sln --configuration Debug`
- Result: success.
- Warnings: 0.
- Errors: 0.
- Command executed: `dotnet build EJLive.Unified\EJLive.Unified.sln --configuration Release`
- Result: success.
- Warnings: 0.
- Errors: 0.
- Command executed: `dotnet build EJLive.Unified\EJLive.Unified.slnx --configuration Debug`
- Result: success.
- Warnings: 0.
- Errors: 0.
- Command executed: `dotnet build EJLive.Unified\EJLive.Unified.slnx --configuration Release`
- Result: success.
- Warnings: 0.
- Errors: 0.

## Latest Improvement Pass

- Client, Server, and Monitoring views now include metric summary cards.
- WinForms grids use double buffering and consistent row/header styling.
- Server includes a Coder01-inspired `Network Map` tab with color-coded ATM operational cards.
- Server auto-starts its listener on form show and keeps manual Start/Stop controls.
- Client auto-connects on form show when `AutoConnect` is enabled.
- Client `Remote Control` now includes screenshot preview, screenshot persistence, pending command grid, and remote frame send support.
- Client journal view includes quick filters for approved, declined, capture, cash, and error terms.
- Client outbox grid refresh avoids unnecessary repainting when queue state is unchanged.
- Ping, journal load, journal analysis, vendor log analysis, and connected journal send run asynchronously.
- Network writes are serialized and async send APIs are available.
- SQLite initialization now creates performance indexes, uses WAL-oriented pragmas, and migrates existing old schemas before index creation.
- Operational state and sync tracking use concurrent dictionaries.

## Runtime Probes

- SQLite legacy-schema probe: success. Old `audit_log` and `sync_records` tables are upgraded with `created_at_utc` and `updated_at_utc` before indexes are created.
- Client/server network probe: success. `NetworkEngine` connected to `ServerEngine`, and the server accepted `ATM-SMOKE` with 0 errors.
- Remote command routing probe: success. `ServerEngine.SendCommand` delivered `CMD_PING`, client returned `CommandResult`, and server logged the result.
- File watcher fallback probe: success. A new `EJDATA.LOG` file was detected by `FileWatcherEngine`.
- WinForms UI composition probe: success. Client, server, and monitoring forms constructed with expected tabs and button counts.
- Project file linkage probe: success. 402 non-build-output files were classified under solution projects, `reference-source` entries, `EJLive.LegacyReference`, or explicit setup/runtime links. The generated inventory also has 402 rows.
- Verification project executed successfully in both Debug and Release configurations.

## Project Coverage

- `EJLive.Shared`: builds.
- `EJLive.Core`: builds.
- `EJLive.Client.WinForms`: builds.
- `EJLive.Server.WinForms`: builds.
- `EJLive.Monitoring.WinForms`: builds.
- `EJLive.Installer.WinForms`: builds.
- `EJLive.Monitor`: builds as the legacy monitoring dashboard library.
- `EJLive.LegacyReference`: builds and links preserved original files as content.
- `EJLive.Verification`: builds and runs runtime probes.

## Required Types

- `JournalOutbox`: present.
- `RetryPolicy`: present.
- `DatabaseManager`: present.
- `SecurityHelper`: present.
- `ClientConfig`: present.
- `RemoteCommand`: present.
- `GhostRemoteEngine`: present.
- `TransactionAnalysisEngine`: present.
- `ATMType`: present.
- `ATMStatus`: present.
- `SyncStatus`: present.
- `AlertSeverity`: present.
- `LiveSyncProgress`: present.

## UI Coverage

- Client connection controls: present.
- Client sync controls: present.
- Client journal load/analyze/export controls: present.
- Client remote command controls: present.
- Client screenshot preview and screenshot folder workflow: present.
- Client service controls: present.
- Client settings controls: present.
- Client agent config controls: present.
- Server fleet controls: present.
- Server network map cards: present.
- Server journal viewer controls: present.
- Server sync dashboard controls: present.
- Server remote command target selector and command history: present.
- Server alert controls: present.
- Server archive controls: present.
- Server report controls: present.
- Server settings controls: present.
- Monitoring dashboard executable: present.
- Monitoring operational map cards: present.
- Installer executable: present.
- Installer `Setup Wizard` action: present and opens linked `EJLive.Setup.SetupWizardForm`.
- Legacy monitor construction: present.

## Preservation

- Original C# file count preserved in `legacy/original`: 134.
- Original C# line count preserved in `legacy/original`: 36,996.
- Original malformed project files preserved in `legacy/original`.
- Original supporting scripts/config/spreadsheet XML/OCR helper files copied into `legacy/original`.
- Root artifacts, tools, docs, `src/EJLive.Server`, and `src/EJLive.Setup` are linked through `EJLive.LegacyReference`.
- Duplicate/incomplete source splits are linked as `reference-source` items inside their owning projects.
- `docs/09-file-function-inventory.csv` records each file's owner, item mode, runtime function, and future integration status.

## Remaining Risk

- The buildable implementation unifies and stabilizes the runtime surface, but many legacy files were too structurally broken to compile directly.
- Some legacy designer layouts are represented in consolidated code-built WinForms screens rather than compiled as original designer partial classes.
- Live ATM/XFS/device integration still requires environment-specific validation against NCR, GRG, Diebold, and Windows service deployments.
