# CodexMarege Deep Audit

Generated from `legacy/original/CodexMarege`. Build output folders are excluded.

## Counts

| Metric | Value |
| --- | ---: |
| Files | 672 |
| C# files | 134 |
| Projects | 7 |
| Solutions | 1 |
| Forms/UI files | 17 |
| Service/manager/engine files | 70 |
| Files missing from active by name | 20 |
| C# files different from active compiled source | 50 |
| Files containing Arabic text | 56 |
| Declared type count | 314 |
| Method-like member count | 1415 |

## Layer Candidates

| Layer | Files |
| --- | ---: |
| Business Layer | 70 |
| Data/Core Layer | 211 |
| Presentation Layer | 17 |
| Project Structure | 6 |
| Reference/Future Development | 368 |

## Categories

| Category | Files |
| --- | ---: |
| Binary Artifact | 140 |
| Build/Tool Script | 26 |
| Configuration/Data | 68 |
| CSharp Project | 7 |
| CSharp Source | 28 |
| Data/Core Source | 19 |
| Documentation/Requirements | 11 |
| Reference Artifact | 282 |
| Service/Manager/Engine | 70 |
| Solution | 1 |
| Visual Asset | 3 |
| WinForms UI | 17 |

## Top Folders

| Folder | Files |
| --- | ---: |
| packages | 464 |
| EJLive.Core | 74 |
| EJLive.Client.WinForms | 28 |
| EJLive.Server.WinForms | 23 |
| (root) | 15 |
| EJLive.Shared | 14 |
| xl | 13 |
| EJLive.Monitoring.WinForms | 10 |
| EJLive.Installer.WinForms | 9 |
| EJLive.Monitor | 6 |
| ocr-document-processor | 6 |
| docs | 4 |
| EJLive.Server | 3 |
| _rels | 1 |
| .continue | 1 |
| EJLive.Setup | 1 |

## UI Surfaces

- `EJLive.Client.WinForms/ClientMainForm.cs`
- `EJLive.Client.WinForms/Controls/ATMCardControl.cs`
- `EJLive.Client.WinForms/Controls/StatusCard.cs`
- `EJLive.Client.WinForms/Controls/ToastNotification.cs`
- `EJLive.Installer.WinForms/InstallerForm.cs`
- `EJLive.Monitor/MonitoringDashboard.cs`
- `EJLive.Monitoring.WinForms/MainDashboardForm.cs`
- `EJLive.Server.WinForms/ATMCardPanel.cs`
- `EJLive.Server.WinForms/ATMDetailDrawerForm.cs`
- `EJLive.Server.WinForms/ATMDetailForm.cs`
- `EJLive.Server.WinForms/JournalSyncDashboardForm.cs`
- `EJLive.Server.WinForms/JournalViewerForm.cs`
- `EJLive.Server.WinForms/ServerMainForm.cs`
- `EJLive.Server.WinForms/SyncDashboardForm.cs`
- `EJLive.Server/ServerMainForm.cs`
- `EJLive.Setup/SetupWizardForm.cs`
- `EJLive.Shared/LightUiTheme.cs`

## Services, Managers, Engines, Processors

- `EJLive.Client.WinForms/Services/AdvancedJournalProcessor.cs`
- `EJLive.Client.WinForms/Services/AdvancedNetworkManager.cs`
- `EJLive.Client.WinForms/Services/AdvancedRemoteCommandHandler.cs`
- `EJLive.Client.WinForms/Services/CashTelemetryService.cs`
- `EJLive.Client.WinForms/Services/ClientConstants.cs`
- `EJLive.Client.WinForms/Services/JournalProcessor.cs`
- `EJLive.Client.WinForms/Services/JournalSyncAgent.cs`
- `EJLive.Client.WinForms/Services/NetworkEngine.cs`
- `EJLive.Client.WinForms/Services/NetworkManager.cs`
- `EJLive.Client.WinForms/Services/RemoteCommandHandler.cs`
- `EJLive.Client.WinForms/Services/WindowsRemoteAccessService.cs`
- `EJLive.Client.WinForms/Services/WindowsStartupService.cs`
- `EJLive.Core/Engine/CommunicationProtocol.cs`
- `EJLive.Core/Engine/FileWatcherEngine.cs`
- `EJLive.Core/Engine/FleetPredictionEngine.cs`
- `EJLive.Core/Engine/GhostRemoteEngine.cs`
- `EJLive.Core/Engine/ImageSyncEngine.cs`
- `EJLive.Core/Engine/JournalOutbox.cs`
- `EJLive.Core/Engine/JournalSyncTracker.cs`
- `EJLive.Core/Engine/NetworkEngine.cs`
- `EJLive.Core/Engine/Protocol.cs`
- `EJLive.Core/Engine/ReportExportEngine.cs`
- `EJLive.Core/Engine/ServerEngine.cs`
- `EJLive.Core/Engine/TransactionAnalysisEngine.cs`
- `EJLive.Core/Services/AgentConfigurationXmlService.cs`
- `EJLive.Core/Services/AlertManager.cs`
- `EJLive.Core/Services/AuditLogger.cs`
- `EJLive.Core/Services/DatabaseManager.cs`
- `EJLive.Core/Services/JournalSyncAlertService.cs`
- `EJLive.Core/Services/JournalSyncDashboardService.cs`
- `EJLive.Core/Services/JournalSyncMonitorService.cs`
- `EJLive.Core/Services/JournalSyncService.cs`
- `EJLive.Core/Services/JournalSyncStateService.cs`
- `EJLive.Core/Services/JournalSyncStateStore.cs`
- `EJLive.Core/Services/JournalSyncTracker.cs`
- `EJLive.Core/Services/JournalSyncTrackerService.cs`
- `EJLive.Core/Services/JournalSyncTrackingService.cs`
- `EJLive.Core/Services/MergedTraceCorrelationService.cs`
- `EJLive.Core/Services/NcrConfigCapabilityParser.cs`
- `EJLive.Core/Services/NcrConfigurationCapabilityParser.cs`
- `EJLive.Core/Services/NcrReferenceCapabilityFactory.cs`
- `EJLive.Core/Services/OperationalStateStore.cs`
- `EJLive.Core/Services/RoleBasedAccess.cs`
- `EJLive.Core/Services/VendorRootCapabilityService.cs`
- `EJLive.Core/Services/VendorRootProfileCatalogService.cs`
- `EJLive.Core/Services/XfsLogAnalysisService.cs`
- `EJLive.Core/Xfs/Adapters/CardReaderTraceAdapter.cs`
- `EJLive.Core/Xfs/Adapters/DebugTraceAdapter.cs`
- `EJLive.Core/Xfs/Adapters/DieboldMdsAdapter.cs`
- `EJLive.Core/Xfs/Adapters/GrgJournalAdapter.cs`
- `EJLive.Core/Xfs/Adapters/GrgXfsAdapter.cs`
- `EJLive.Core/Xfs/Adapters/HostMessageInAdapter.cs`
- `EJLive.Core/Xfs/Adapters/HostMessageOutAdapter.cs`
- `EJLive.Core/Xfs/Adapters/IXfsVendorAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrCardReaderTraceAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrDebugTraceAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrHostMessageInAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrHostMessageOutAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrJournalAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrOoxfsRuntimeAdapter.cs`
- `EJLive.Core/Xfs/Adapters/NcrXfsAdapter.cs`
- `EJLive.Core/Xfs/Adapters/OoxfsRuntimeAdapter.cs`
- `EJLive.Core/Xfs/IXfsVendorAdapter.cs`
- `EJLive.Core/Xfs/XfsAdapterRegistry.cs`
- `EJLive.Server.WinForms/Services/ArchiveManager.cs`
- `EJLive.Server.WinForms/Services/EJServer.cs`
- `EJLive.Server.WinForms/Services/EJServerService.cs`
- `EJLive.Server/Services/JournalAnalyticsService.cs`
- `EJLive.Server/Services/RemoteControlService.cs`
- `EJLive.Shared/MonitoringStateStore.cs`

## Dependency Records

Dependency rows written: 78. See `docs/original-audit/CodexMarege-project-dependencies.csv`.

## File Manifest

Full file-level role map written to `docs/original-audit/CodexMarege-file-manifest.csv`.
