using EJLive.Business;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Application;

public sealed class EJLiveApplicationHost : IDisposable
{
    private readonly UnifiedBusinessRuntime _runtime;

    public EJLiveApplicationHost(UnifiedBusinessRuntime runtime)
    {
        _runtime = runtime;
    }

    public UnifiedBusinessRuntime Runtime => _runtime;

    public static EJLiveApplicationHost Create(string? databasePath = null)
    {
        return new EJLiveApplicationHost(UnifiedBusinessRuntime.CreateInitialized(databasePath));
    }

    public ApplicationReadinessReport ValidateReadiness()
    {
        var snapshot = _runtime.BuildSnapshot();
        var fusion = _runtime.BuildOperationalFusion(
            "NCR EJDATA APPROVED AMOUNT 500\nM-18 CASH ERROR\nHOST MESSAGE OUT",
            new[] { "src/EJLive.Core/Services/UnifiedOperationalFusion.cs", "legacy/original/Coder01/README.md" });
        var rootPath = FindSolutionRoot();
        var activation = _runtime.ActivateAllReferenceServices(
            rootPath,
            "ATM-APP-HOST",
            Path.Combine(Path.GetTempPath(), "ejlive-runtime-gateway-activation"));
        var coverage = _runtime.BuildReferenceCoverage(rootPath);
        _runtime.ClientServiceSupervisor.Start("Journal Sync", "Compiled journal sync service registered.");
        var serviceReport = _runtime.ClientServiceSupervisor.BuildReport();
        var checks = new List<ApplicationReadinessCheck>
        {
            new("Database initialized", _runtime.Database.IsInitialized, "SQLite schema and indexes are ready."),
            new("Functional map populated", snapshot.Capabilities.Count >= 8, $"{snapshot.Capabilities.Count} capabilities mapped."),
            new("Original source feature coverage", OriginalSourceCatalog.ProjectsWithoutFeatureCoverage().Count == 0, $"{snapshot.SourceFeatures.Count} feature groups cover {OriginalSourceCatalog.Projects.Count} source roots."),
            new("Unified fusion services active", fusion.JournalEvidence.Signals.Count >= 3 && fusion.FileBindings.UnclassifiedCount == 0, "Journal, command, fleet, and file-binding fusion services are loaded."),
            new("Unified service operations active", serviceReport.Total >= 10 && serviceReport.Running >= 1, $"Services tracked: {serviceReport.Total}, running: {serviceReport.Running}."),
            new("Reference services fully activated", activation.RequestedReferencePaths >= 50 && activation.UnclassifiedActivations == 0, $"Activated={activation.ActivatedReferencePaths}/{activation.RequestedReferencePaths}, unclassified={activation.UnclassifiedActivations}."),
            new("Reference coverage complete", coverage.TotalReferenceFiles >= 50 && coverage.UncoveredFiles == 0, $"Covered={coverage.CoveredFiles}/{coverage.TotalReferenceFiles}, uncovered={coverage.UncoveredFiles}."),
            new("Vendor catalog available", _runtime.VendorCapabilities.GetCapabilities(AppConstants.ATM_TYPE_NCR).Count > 0, "NCR capabilities resolved."),
            new("Access policy available", _runtime.Access.Can("Admin", "remote"), "Admin role can execute remote permission.")
        };

        return new ApplicationReadinessReport(checks);
    }

    public IReadOnlyList<DataFlowStep> DescribeDataFlow()
    {
        return new[]
        {
            new DataFlowStep(1, "Presentation", "WinForms button or timer event collects operator input or runtime signal."),
            new DataFlowStep(2, "Application", "Application host validates the request and selects the workflow."),
            new DataFlowStep(3, "Business", "Runtime services update ATM state, queue journal work, raise alerts, or issue remote commands."),
            new DataFlowStep(4, "Core", "Protocol, security, engines, and models normalize the payload."),
            new DataFlowStep(5, "Data", "DatabaseManager persists audit and sync records in SQLite."),
            new DataFlowStep(6, "Presentation", "Dashboards read snapshots and refresh grids, cards, logs, and reports.")
        };
    }

    public ATMInfo SeedDemoAtm(string atmId = "ATM-DEMO")
    {
        return _runtime.RegisterAtm(atmId, "Demo Terminal", AppConstants.ATM_TYPE_NCR, "127.0.0.1");
    }

    public void Dispose()
    {
        _runtime.Dispose();
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EJLive.Unified.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}

public sealed record DataFlowStep(int Order, string Layer, string Description);

public sealed record ApplicationReadinessCheck(string Name, bool Passed, string Detail);

public sealed class ApplicationReadinessReport
{
    public ApplicationReadinessReport(IReadOnlyList<ApplicationReadinessCheck> checks)
    {
        Checks = checks;
    }

    public IReadOnlyList<ApplicationReadinessCheck> Checks { get; }
    public bool Passed => Checks.All(check => check.Passed);
}
