# Source Analysis

Analysis date: 2026-05-15.

## Inventory

- Original C# files preserved: 134.
- Original C# lines preserved: 36,996.
- Original non-build files copied into the legacy archive: solution files, project files, config files, scripts, OCR helper files, spreadsheet XML fragments, and upgrade/build helpers.
- Unified build projects: 7.
- Unified SDK-style project files validated as XML/MSBuild: 7 OK, 0 invalid.

## Original Project File State

The uploaded project files contain multiple merged project definitions and malformed XML. The unified solution does not mutate those originals; it preserves them in `legacy/original` and replaces them with clean SDK-style projects under `src`.

| Original project | XML status |
| --- | --- |
| `EJLive.Client.WinForms/EJLive.Client.WinForms.csproj` | Invalid |
| `EJLive.Core/EJLive.Core.csproj` | OK |
| `EJLive.Installer.WinForms/EJLive.Installer.WinForms.csproj` | Invalid |
| `EJLive.Monitor/EJLive.Monitor.csproj` | OK |
| `EJLive.Monitoring.WinForms/EJLive.Monitoring.WinForms.csproj` | Invalid |
| `EJLive.Server.WinForms/EJLive.Server.WinForms.csproj` | Invalid |
| `EJLive.Shared/EJLive.Shared.csproj` | Invalid |

## Structural C# Issues Found In Originals

Files with unbalanced brace counts in the original source:

| File | Brace delta |
| --- | ---: |
| `EJLive.Client.WinForms/ClientMainForm.cs` | +5 |
| `EJLive.Core/Constants.cs` | +3 |
| `EJLive.Core/Engine/CommunicationProtocol.cs` | +1 |
| `EJLive.Core/Engine/FileWatcherEngine.cs` | +1 |
| `EJLive.Core/Engine/GhostRemoteEngine.cs` | +1 |
| `EJLive.Core/Engine/ImageSyncEngine.cs` | +1 |
| `EJLive.Core/Engine/NetworkEngine.cs` | -1 |
| `EJLive.Core/Engine/ReportExportEngine.cs` | +1 |
| `EJLive.Core/Engine/ServerEngine.cs` | +1 |
| `EJLive.Core/Engine/TransactionAnalysisEngine.cs` | +1 |
| `EJLive.Core/Models/ATMInfo.cs` | +1 |
| `EJLive.Core/Models/JournalSyncModels.cs` | +1 |
| `EJLive.Core/Models/TransactionModels.cs` | +4 |
| `EJLive.Core/Xfs/Adapters/CardReaderTraceAdapter.cs` | +1 |
| `EJLive.Core/Xfs/Adapters/DebugTraceAdapter.cs` | +1 |
| `EJLive.Core/Xfs/Adapters/HostMessageInAdapter.cs` | +1 |
| `EJLive.Core/Xfs/Adapters/HostMessageOutAdapter.cs` | +1 |
| `EJLive.Core/Xfs/Adapters/NcrJournalAdapter.cs` | +1 |
| `EJLive.Core/Xfs/Adapters/OoxfsRuntimeAdapter.cs` | +1 |
| `EJLive.Installer.WinForms/InstallerForm.cs` | +3 |
| `EJLive.Installer.WinForms/InstallerForm.Designer.cs` | +1 |
| `EJLive.Monitoring.WinForms/MainDashboardForm.cs` | +5 |
| `EJLive.Monitoring.WinForms/MainDashboardForm.Designer.cs` | +2 |
| `EJLive.Server.WinForms/ServerMainForm.cs` | +3 |
| `EJLive.Server.WinForms/ServerMainForm.Designer.cs` | +2 |

## Duplicate And Overlapping Types

The original tree has duplicate definitions within single files and overlapping definitions across modules. The unified build collapses these into one compile-time definition per namespace while preserving the original copies in `legacy/original`.

High-impact duplicates and merge targets:

| Original duplicate/overlap | Unified target |
| --- | --- |
| `ATMInfo`, `JournalSyncRecord`, `JournalSyncState` | `EJLive.Core.Models.UnifiedModels` |
| `AlertPayload`, `TransactionAnalysisReport`, `ATMTransaction`, `ATMError`, `RetainedCard` | `EJLive.Core.Models.UnifiedModels` |
| `TxType`, `TxResult`, `TransactionType`, `TransactionStatus`, `ATMOperationalState` | `EJLive.Core.Models.UnifiedModels` |
| `Constants`, `Protocol`, `NetworkConfig`, `SecurityConfig`, `NCRFiles`, `EJPatterns` | `EJLive.Core.Constants` |
| `JournalOutbox` | `EJLive.Core.Engine.JournalOutbox` |
| `NetworkEngine`, `NetworkManager`, `AdvancedNetworkManager` | `EJLive.Core.Engine.NetworkEngine`, with client UI orchestration in `EJLive.Client.WinForms` |
| `JournalProcessor`, `AdvancedJournalProcessor` | `JournalOutbox`, `JournalSyncService`, and `TransactionAnalysisEngine` |
| `RemoteCommandHandler`, `AdvancedRemoteCommandHandler` | `RemoteCommand`, `RemoteCommandEnvelope`, `CommunicationProtocol`, and UI command queue actions |
| `GhostRemoteEngine` | `EJLive.Core.Engine.OperationalEngines` |
| `TransactionAnalysisEngine` | `EJLive.Core.Engine.OperationalEngines` |
| `Xfs*` models/adapters | `EJLive.Core.Xfs.XfsModels` |
| Server forms/designer fragments | `EJLive.Server.WinForms.ServerMainForm` with detail/viewer/dashboard child forms |

## Missing Types Added To The Unified Runtime

The following previously referenced or requested types are now defined in the buildable source:

- `JournalOutbox`
- `RetryPolicy`
- `DatabaseManager`
- `SecurityHelper`
- `ClientConfig`
- `RemoteCommand`
- `GhostRemoteEngine`
- `TransactionAnalysisEngine`
- `ATMType`
- `ATMStatus`
- `SyncStatus`
- `AlertSeverity`
- `LiveSyncProgress`

