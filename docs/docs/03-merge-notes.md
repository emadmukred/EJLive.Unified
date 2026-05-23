# Merge Notes

## Strategy

The uploaded files contain invalid XML, incomplete class/namespace closures, and repeated type definitions that cannot compile together. The rebuild therefore uses a two-track strategy:

1. Preserve the uploaded files verbatim in `legacy/original`.
2. Build a clean SDK-style solution under `src` that exposes one canonical runtime surface for each duplicated or missing type.

This keeps every original line available for audit while preventing duplicate/incomplete definitions from entering the compiler.

## Project File Repair

The original `.csproj` files were not edited in place. New SDK-style projects were created:

- `src/EJLive.Shared/EJLive.Shared.csproj`
- `src/EJLive.Core/EJLive.Core.csproj`
- `src/EJLive.Client.WinForms/EJLive.Client.WinForms.csproj`
- `src/EJLive.Server.WinForms/EJLive.Server.WinForms.csproj`
- `src/EJLive.Monitoring.WinForms/EJLive.Monitoring.WinForms.csproj`
- `src/EJLive.Installer.WinForms/EJLive.Installer.WinForms.csproj`
- `src/EJLive.LegacyReference/EJLive.LegacyReference.csproj`

`EJLive.Unified.sln` references only valid projects.

## UI Consolidation

Client UI tabs are consolidated into:

- Connection
- Sync
- Journal
- Remote Control
- Services
- Settings
- Agent Config

Server UI tabs are consolidated into:

- Fleet
- Network Map
- Journal Viewer
- Sync Dashboard
- Remote Commands
- Alerts
- Archive
- Reports
- Settings

Monitoring and installer interfaces are retained as separate executable projects. UI text is English in the buildable forms.

The Coder01 package contributed additional UI direction after the initial merge: ATM state cards, remote screenshot preview, pending remote command grids, journal quick filters, command routing, and file watcher fallback behavior. These were integrated into the existing buildable forms without replacing the unified project structure.

## Service Consolidation

The old `NetworkManager`, `AdvancedNetworkManager`, and `NetworkEngine` split is represented as:

- `NetworkEngine`: transport, session, connection, and send operations.
- Client form: user actions and UI orchestration.
- `CommunicationProtocol`: serializable protocol messages and command envelopes.
- `ServerEngine`: TCP listener, handshake acceptance, heartbeat acknowledgements, journal completion acknowledgements, and remote screen frame logging.
- `FileWatcherEngine`: file-system events plus polling fallback and duplicate unchanged event suppression.

The old journal processors are represented as:

- `JournalOutbox`: durable queue-like runtime surface.
- `JournalSyncService`: sync orchestration.
- `JournalSyncTrackingService`: sync records and status tracking.
- `TransactionAnalysisEngine`: journal analysis.

The old remote command handlers are represented as:

- `RemoteCommand`
- `RemoteCommandEnvelope`
- `CommunicationProtocol`
- client/server UI command buttons.
- `GhostRemoteEngine` plus `CommunicationProtocol.BuildGhostStart`, `BuildGhostFrame`, and `BuildGhostStop` for screen preview capture and frame transfer.
- `CommunicationProtocol.BuildCommandResult`, `RemoteCommandEnvelope.TryParse`, `ServerEngine.SendCommand`, and `ServerEngine.BroadcastCommand` for server-to-client command routing.

## Legacy Traceability

Original code remains available in `legacy/original` and is linked into Visual Studio through `EJLive.LegacyReference` as non-compiling content. This avoids deleting or hiding legacy code while keeping the unified solution buildable.
