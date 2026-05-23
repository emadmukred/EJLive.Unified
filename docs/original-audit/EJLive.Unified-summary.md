# EJLive.Unified Deep Audit

Generated from `legacy/original/EJLive.Unified`. Build output folders are excluded.

## Counts

| Metric | Value |
| --- | ---: |
| Files | 180 |
| C# files | 141 |
| Projects | 9 |
| Solutions | 1 |
| Forms/UI files | 17 |
| Service/manager/engine files | 72 |
| Files missing from active by name | 1 |
| C# files different from active compiled source | 7 |
| Files containing Arabic text | 34 |
| Declared type count | 404 |
| Method-like member count | 1207 |

## Layer Candidates

| Layer | Files |
| --- | ---: |
| Business Layer | 72 |
| Data/Core Layer | 38 |
| Presentation Layer | 17 |
| Project Structure | 7 |
| Reference/Future Development | 44 |
| Verification | 2 |

## Categories

| Category | Files |
| --- | ---: |
| Configuration/Data | 14 |
| CSharp Project | 9 |
| CSharp Source | 31 |
| Data/Core Source | 21 |
| Reference Artifact | 15 |
| Service/Manager/Engine | 72 |
| Solution | 1 |
| WinForms UI | 17 |

## Top Folders

| Folder | Files |
| --- | ---: |
| src | 179 |
| (root) | 1 |

## UI Surfaces

- `src/EJLive.Client.WinForms/ClientMainForm.cs`
- `src/EJLive.Client.WinForms/Controls/ATMCardControl.cs`
- `src/EJLive.Client.WinForms/Controls/StatusCard.cs`
- `src/EJLive.Client.WinForms/Controls/ToastNotification.cs`
- `src/EJLive.Installer.WinForms/InstallerForm.cs`
- `src/EJLive.Monitor/MonitoringDashboard.cs`
- `src/EJLive.Monitoring.WinForms/MainDashboardForm.cs`
- `src/EJLive.Server.WinForms/ATMCardPanel.cs`
- `src/EJLive.Server.WinForms/ATMDetailDrawerForm.cs`
- `src/EJLive.Server.WinForms/ATMDetailForm.cs`
- `src/EJLive.Server.WinForms/JournalSyncDashboardForm.cs`
- `src/EJLive.Server.WinForms/JournalViewerForm.cs`
- `src/EJLive.Server.WinForms/ServerMainForm.cs`
- `src/EJLive.Server.WinForms/SyncDashboardForm.cs`
- `src/EJLive.Server/ServerMainForm.cs`
- `src/EJLive.Setup/SetupWizardForm.cs`
- `src/EJLive.Shared/LightUiTheme.cs`

## Services, Managers, Engines, Processors

- `src/EJLive.Client.WinForms/Services/AdvancedJournalProcessor.cs`
- `src/EJLive.Client.WinForms/Services/AdvancedNetworkManager.cs`
- `src/EJLive.Client.WinForms/Services/AdvancedRemoteCommandHandler.cs`
- `src/EJLive.Client.WinForms/Services/CashTelemetryService.cs`
- `src/EJLive.Client.WinForms/Services/ClientConstants.cs`
- `src/EJLive.Client.WinForms/Services/JournalProcessor.cs`
- `src/EJLive.Client.WinForms/Services/JournalSyncAgent.cs`
- `src/EJLive.Client.WinForms/Services/NetworkEngine.cs`
- `src/EJLive.Client.WinForms/Services/NetworkManager.cs`
- `src/EJLive.Client.WinForms/Services/RemoteCommandHandler.cs`
- `src/EJLive.Client.WinForms/Services/WindowsRemoteAccessService.cs`
- `src/EJLive.Client.WinForms/Services/WindowsStartupService.cs`
- `src/EJLive.Core/Engine/CommunicationProtocol.cs`
- `src/EJLive.Core/Engine/FileWatcherEngine.cs`
- `src/EJLive.Core/Engine/FleetPredictionEngine.cs`
- `src/EJLive.Core/Engine/GhostRemoteEngine.cs`
- `src/EJLive.Core/Engine/ImageSyncEngine.cs`
- `src/EJLive.Core/Engine/JournalOutbox.cs`
- `src/EJLive.Core/Engine/JournalSyncTracker.cs`
- `src/EJLive.Core/Engine/NetworkEngine.cs`
- `src/EJLive.Core/Engine/OperationalEngines.cs`
- `src/EJLive.Core/Engine/Protocol.cs`
- `src/EJLive.Core/Engine/ReportExportEngine.cs`
- `src/EJLive.Core/Engine/ServerEngine.cs`
- `src/EJLive.Core/Engine/TransactionAnalysisEngine.cs`
- `src/EJLive.Core/Services/AgentConfigurationXmlService.cs`
- `src/EJLive.Core/Services/AlertManager.cs`
- `src/EJLive.Core/Services/AuditLogger.cs`
- `src/EJLive.Core/Services/CoreServices.cs`
- `src/EJLive.Core/Services/DatabaseManager.cs`
- `src/EJLive.Core/Services/JournalSyncAlertService.cs`
- `src/EJLive.Core/Services/JournalSyncDashboardService.cs`
- `src/EJLive.Core/Services/JournalSyncMonitorService.cs`
- `src/EJLive.Core/Services/JournalSyncService.cs`
- `src/EJLive.Core/Services/JournalSyncStateService.cs`
- `src/EJLive.Core/Services/JournalSyncStateStore.cs`
- `src/EJLive.Core/Services/JournalSyncTracker.cs`
- `src/EJLive.Core/Services/JournalSyncTrackerService.cs`
- `src/EJLive.Core/Services/JournalSyncTrackingService.cs`
- `src/EJLive.Core/Services/MergedTraceCorrelationService.cs`
- `src/EJLive.Core/Services/NcrConfigCapabilityParser.cs`
- `src/EJLive.Core/Services/NcrConfigurationCapabilityParser.cs`
- `src/EJLive.Core/Services/NcrReferenceCapabilityFactory.cs`
- `src/EJLive.Core/Services/OperationalStateStore.cs`
- `src/EJLive.Core/Services/RoleBasedAccess.cs`
- `src/EJLive.Core/Services/VendorRootCapabilityService.cs`
- `src/EJLive.Core/Services/VendorRootProfileCatalogService.cs`
- `src/EJLive.Core/Services/XfsLogAnalysisService.cs`
- `src/EJLive.Core/Xfs/Adapters/CardReaderTraceAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/DebugTraceAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/DieboldMdsAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/GrgJournalAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/GrgXfsAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/HostMessageInAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/HostMessageOutAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/IXfsVendorAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrCardReaderTraceAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrDebugTraceAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrHostMessageInAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrHostMessageOutAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrJournalAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrOoxfsRuntimeAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/NcrXfsAdapter.cs`
- `src/EJLive.Core/Xfs/Adapters/OoxfsRuntimeAdapter.cs`
- `src/EJLive.Core/Xfs/IXfsVendorAdapter.cs`
- `src/EJLive.Core/Xfs/XfsAdapterRegistry.cs`
- `src/EJLive.Server.WinForms/Services/ArchiveManager.cs`
- `src/EJLive.Server.WinForms/Services/EJServer.cs`
- `src/EJLive.Server.WinForms/Services/EJServerService.cs`
- `src/EJLive.Server/Services/JournalAnalyticsService.cs`
- `src/EJLive.Server/Services/RemoteControlService.cs`
- `src/EJLive.Shared/MonitoringStateStore.cs`

## Dependency Records

Dependency rows written: 20. See `docs/original-audit/EJLive.Unified-project-dependencies.csv`.

## File Manifest

Full file-level role map written to `docs/original-audit/EJLive.Unified-file-manifest.csv`.
