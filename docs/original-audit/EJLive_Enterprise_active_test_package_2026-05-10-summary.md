# EJLive_Enterprise_active_test_package_2026-05-10 Deep Audit

Generated from `legacy/original/EJLive_Enterprise_active_test_package_2026-05-10`. Build output folders are excluded.

## Counts

| Metric | Value |
| --- | ---: |
| Files | 166 |
| C# files | 95 |
| Projects | 7 |
| Solutions | 1 |
| Forms/UI files | 7 |
| Service/manager/engine files | 54 |
| Files missing from active by name | 45 |
| C# files different from active compiled source | 41 |
| Files containing Arabic text | 18 |
| Declared type count | 200 |
| Method-like member count | 564 |

## Layer Candidates

| Layer | Files |
| --- | ---: |
| Business Layer | 54 |
| Data/Core Layer | 25 |
| Presentation Layer | 7 |
| Project Structure | 6 |
| Reference/Future Development | 74 |

## Categories

| Category | Files |
| --- | ---: |
| Archive Package | 1 |
| Configuration/Data | 16 |
| CSharp Project | 7 |
| CSharp Source | 17 |
| Data/Core Source | 17 |
| Documentation/Requirements | 21 |
| Reference Artifact | 6 |
| Runtime Log Evidence | 16 |
| Service/Manager/Engine | 54 |
| Solution | 1 |
| Visual Asset | 3 |
| WinForms UI | 7 |

## Top Folders

| Folder | Files |
| --- | ---: |
| agent_files | 166 |

## UI Surfaces

- `agent_files/EJLive.Client.WinForms/ClientMainForm.cs`
- `agent_files/EJLive.Installer.WinForms/InstallerForm.cs`
- `agent_files/EJLive.Monitoring.WinForms/MainDashboardForm.cs`
- `agent_files/EJLive.Server.WinForms/ATMDetailForm.cs`
- `agent_files/EJLive.Server.WinForms/JournalSyncDashboardForm.cs`
- `agent_files/EJLive.Server.WinForms/ServerMainForm.cs`
- `agent_files/EJLive.Server.WinForms/SyncDashboardForm.cs`

## Services, Managers, Engines, Processors

- `agent_files/EJLive.Client.WinForms/Services/AdvancedJournalProcessor.cs`
- `agent_files/EJLive.Client.WinForms/Services/AdvancedNetworkManager.cs`
- `agent_files/EJLive.Client.WinForms/Services/AdvancedRemoteCommandHandler.cs`
- `agent_files/EJLive.Client.WinForms/Services/CashTelemetryService.cs`
- `agent_files/EJLive.Client.WinForms/Services/JournalProcessor.cs`
- `agent_files/EJLive.Client.WinForms/Services/JournalSyncAgent.cs`
- `agent_files/EJLive.Client.WinForms/Services/NetworkEngine.cs`
- `agent_files/EJLive.Client.WinForms/Services/NetworkManager.cs`
- `agent_files/EJLive.Client.WinForms/Services/RemoteCommandHandler.cs`
- `agent_files/EJLive.Core/Engine/CommunicationProtocol.cs`
- `agent_files/EJLive.Core/Engine/FileWatcherEngine.cs`
- `agent_files/EJLive.Core/Engine/GhostRemoteEngine.cs`
- `agent_files/EJLive.Core/Engine/ImageSyncEngine.cs`
- `agent_files/EJLive.Core/Engine/NetworkEngine.cs`
- `agent_files/EJLive.Core/Engine/ReportExportEngine.cs`
- `agent_files/EJLive.Core/Engine/ServerEngine.cs`
- `agent_files/EJLive.Core/Engine/TransactionAnalysisEngine.cs`
- `agent_files/EJLive.Core/Services/JournalSyncAlertService.cs`
- `agent_files/EJLive.Core/Services/JournalSyncDashboardService.cs`
- `agent_files/EJLive.Core/Services/JournalSyncMonitorService.cs`
- `agent_files/EJLive.Core/Services/JournalSyncService.cs`
- `agent_files/EJLive.Core/Services/JournalSyncStateService.cs`
- `agent_files/EJLive.Core/Services/JournalSyncStateStore.cs`
- `agent_files/EJLive.Core/Services/JournalSyncTracker.cs`
- `agent_files/EJLive.Core/Services/JournalSyncTrackerService.cs`
- `agent_files/EJLive.Core/Services/JournalSyncTrackingService.cs`
- `agent_files/EJLive.Core/Services/MergedTraceCorrelationService.cs`
- `agent_files/EJLive.Core/Services/NcrConfigCapabilityParser.cs`
- `agent_files/EJLive.Core/Services/NcrConfigurationCapabilityParser.cs`
- `agent_files/EJLive.Core/Services/NcrReferenceCapabilityFactory.cs`
- `agent_files/EJLive.Core/Services/VendorRootCapabilityService.cs`
- `agent_files/EJLive.Core/Services/VendorRootProfileCatalogService.cs`
- `agent_files/EJLive.Core/Services/XfsLogAnalysisService.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/CardReaderTraceAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/DebugTraceAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/DieboldMdsAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/GrgJournalAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/GrgXfsAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/HostMessageInAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/HostMessageOutAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/IXfsVendorAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrCardReaderTraceAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrDebugTraceAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrHostMessageInAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrHostMessageOutAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrJournalAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrOoxfsRuntimeAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/NcrXfsAdapter.cs`
- `agent_files/EJLive.Core/Xfs/Adapters/OoxfsRuntimeAdapter.cs`
- `agent_files/EJLive.Core/Xfs/IXfsVendorAdapter.cs`
- `agent_files/EJLive.Core/Xfs/XfsAdapterRegistry.cs`
- `agent_files/EJLive.Server.WinForms/Services/ArchiveManager.cs`
- `agent_files/EJLive.Server.WinForms/Services/EJServer.cs`
- `agent_files/EJLive.Shared/MonitoringStateStore.cs`

## Dependency Records

Dependency rows written: 63. See `docs/original-audit/EJLive_Enterprise_active_test_package_2026-05-10-project-dependencies.csv`.

## File Manifest

Full file-level role map written to `docs/original-audit/EJLive_Enterprise_active_test_package_2026-05-10-file-manifest.csv`.
