using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Business;

public sealed class UnifiedBusinessRuntime : IDisposable
{
    public DatabaseManager Database { get; } = DatabaseManager.Instance;
    public OperationalStateStore OperationalState { get; } = new();
    public JournalSyncTrackingService SyncTracking { get; } = new();
    public JournalSyncService JournalSync { get; } = new();
    public AlertManager Alerts { get; } = new();
    public RoleBasedAccess Access { get; } = new();
    public VendorRootCapabilityService VendorCapabilities { get; } = new();
    public XfsLogAnalysisService XfsAnalysis { get; } = new();
    public TransactionAnalysisEngine TransactionAnalysis { get; } = new();
    public FileWatcherEngine FileWatcher { get; } = new();
    public ImageSyncEngine ImageSync { get; } = new();
    public ReportExportEngine Reports { get; } = new();
    public GhostRemoteEngine GhostRemote { get; } = new();
    public UnifiedJournalEvidenceAnalyzer JournalEvidence { get; } = new();
    public UnifiedRemoteCommandPolicy RemoteCommandPolicy { get; } = new();
    public UnifiedFleetReadinessService FleetReadiness { get; } = new();
    public UnifiedFileBindingService FileBindings { get; } = new();
    public UnifiedOperationalFusionService OperationalFusion { get; } = new();
    public UnifiedJournalStorageService JournalStorage { get; }
    public UnifiedRemoteCommandOrchestrator RemoteCommands { get; }
    public UnifiedClientServiceSupervisor ClientServiceSupervisor { get; } = new();
    public UnifiedProjectIntegrationAuditService IntegrationAudit { get; } = new();
    public UnifiedServiceGateway ServiceGateway { get; }

    public UnifiedBusinessRuntime()
    {
        JournalStorage = new UnifiedJournalStorageService(JournalEvidence);
        RemoteCommands = new UnifiedRemoteCommandOrchestrator(RemoteCommandPolicy);
        ServiceGateway = new UnifiedServiceGateway(JournalStorage, RemoteCommands, ClientServiceSupervisor, IntegrationAudit);
    }

    public static UnifiedBusinessRuntime CreateInitialized(string? databasePath = null)
    {
        var runtime = new UnifiedBusinessRuntime();
        runtime.Database.Initialize(string.IsNullOrWhiteSpace(databasePath) ? AppConstants.DefaultDatabasePath : databasePath);
        return runtime;
    }

    public ATMInfo RegisterAtm(string atmId, string name, string atmType, string? serverIp = null)
    {
        var atm = new ATMInfo
        {
            ATM_ID = atmId,
            ATM_Name = name,
            ATM_Type = AppConstants.NormalizeATMType(atmType),
            ServerIP = serverIp,
            ConnectionStatus = ConnectionStatus.Connected,
            Status = ATMStatus.Online,
            IsConnected = true,
            ConnectedAtUtc = DateTime.UtcNow,
            LastHeartbeatUtc = DateTime.UtcNow,
            LastDataReceivedUtc = DateTime.UtcNow
        };
        atm.RecalculateHealthScore();
        OperationalState.Upsert(atm);
        return atm;
    }

    public JournalSyncRecord TrackJournalSync(string atmId, string fileName, long fileSize, JournalSyncState state)
    {
        var record = new JournalSyncRecord
        {
            ATM_ID = atmId,
            FileName = fileName,
            FileSize = fileSize,
            State = state,
            ProgressPercent = state == JournalSyncState.Completed ? 100 : 0
        };
        SyncTracking.AddOrUpdate(record);
        return record;
    }

    public UnifiedRuntimeSnapshot BuildSnapshot()
    {
        return new UnifiedRuntimeSnapshot(
            OperationalState.BuildSummary(),
            SyncTracking.BuildSummary(),
            Alerts.Alerts.Count,
            BuildCapabilities(),
            BuildSourceCoverage());
    }

    public IReadOnlyList<FunctionalCapability> BuildCapabilities()
    {
        var sourceCount = OriginalSourceCatalog.Projects.Count;
        var uniqueCSharpCount = OriginalSourceCatalog.TotalDifferentCSharpFromActive;
        var featureCoverageCount = OriginalSourceCatalog.Features.Count;
        var activeOrReferencedCount = OriginalSourceCatalog.ActiveOrReferencedFeatures.Count;

        return new[]
        {
            new FunctionalCapability("Presentation", "Client WinForms", "Connection, sync, journal, remote control, services, settings, and agent configuration tabs."),
            new FunctionalCapability("Presentation", "Server WinForms", "Fleet, map, journal viewer, sync dashboard, remote commands, alerts, archive, reports, and settings tabs."),
            new FunctionalCapability("Presentation", "Monitoring WinForms", "Operational dashboard, map, device state, realtime sync, XFS events, vendor logs, and reports."),
            new FunctionalCapability("Business", "Journal Sync", "Outbox queue, sync tracking, state mapping, alerts, retry, and checksum verification."),
            new FunctionalCapability("Business", "Remote Control", "Command envelopes, command results, screenshot transfer, and ghost session state."),
            new FunctionalCapability("Business", "Vendor Engines", "NCR, GRG, Wincor, Diebold/Nixdorf, Hyosung paths, XFS adapters, trace analysis, and capability catalog."),
            new FunctionalCapability("Data/Core", "SQLite Store", "Audit log and sync record schema with safe additive migrations and indexes."),
            new FunctionalCapability("Core", "Security", "AES, RSA, PBKDF2 password hashing, compression, checksums, and file chunk helpers."),
            new FunctionalCapability("Legacy Reference", "Imported Projects", $"{sourceCount} original source roots are preserved with {uniqueCSharpCount} non-identical C# files tracked for staged promotion."),
            new FunctionalCapability("Legacy Reference", "Feature Coverage", $"{featureCoverageCount} source feature groups are mapped; {activeOrReferencedCount} are active or directly referenced by the unified runtime."),
            new FunctionalCapability("Unified Fusion", "Legacy-to-runtime services", "Journal evidence analysis, fleet readiness, remote command policy, and file binding are rebuilt as compiled services."),
            new FunctionalCapability("Unified Services", "Client/server service operations", "Journal storage/archive/reporting, command orchestration, client service supervision, and integration audit are active compiled services."),
            new FunctionalCapability("Unified Services", "Unified service gateway", "Reference-only service paths are bridged into active runtime operations with activation tracking and audit-ready routing.")
        };
    }

    public IReadOnlyList<OriginalSourceFeature> BuildSourceCoverage()
    {
        return OriginalSourceCatalog.BuildFeatureCoverageReport();
    }

    public UnifiedOperationalFusionSnapshot BuildOperationalFusion(
        string journalText,
        IEnumerable<string>? relativePaths = null,
        RemoteCommand? command = null,
        string role = "Admin",
        bool operatorConfirmed = true,
        bool maintenanceWindow = true)
    {
        return OperationalFusion.Build(
            OperationalState.Snapshot,
            SyncTracking.Records,
            journalText,
            relativePaths ?? Array.Empty<string>(),
            command,
            role,
            operatorConfirmed,
            maintenanceWindow);
    }

    public ProjectIntegrationAuditReport BuildIntegrationAudit(string rootPath)
    {
        return IntegrationAudit.Analyze(rootPath);
    }

    public UnifiedGatewayActivationBatchResult ActivateAllReferenceServices(
        string rootPath,
        string atmId,
        string storagePath,
        string atmType = AppConstants.ATM_TYPE_NCR)
    {
        return ServiceGateway.ActivateAllReferenceServices(rootPath, atmId, storagePath, atmType);
    }

    public UnifiedGatewayReferenceCoverage BuildReferenceCoverage(string rootPath)
    {
        return ServiceGateway.BuildReferenceCoverage(rootPath);
    }

    public void Dispose()
    {
        FileWatcher.Dispose();
    }
}

public sealed record FunctionalCapability(string Layer, string Name, string Responsibility);

public sealed record UnifiedRuntimeSnapshot(
    FleetSummary Fleet,
    SyncSummary Sync,
    int ActiveAlerts,
    IReadOnlyList<FunctionalCapability> Capabilities,
    IReadOnlyList<OriginalSourceFeature> SourceFeatures);
