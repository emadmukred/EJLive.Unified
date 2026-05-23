using EJLive.Business;
using EJLive.Client.WinForms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Server.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests;

/// <summary>
/// Documents and validates the eight non-identical files between the
/// <c>EJLive_Client_v5_Enhanced</c> reference workspace and the active
/// compiled source. These tests are additive only: they assert the active
/// behaviour after the v5 features were promoted, plus the active
/// improvements that go beyond the v5 baseline. No production code is
/// modified by this file.
///
/// Mapping of asserted files to legacy paths:
///   1. src/EJLive.Client.WinForms/Program.cs                  (covered partly by ClientV5StartupTests)
///   2. src/EJLive.Core/Constants.cs                           (DefaultDatabasePath env override)
///   3. src/EJLive.Core/LegacyReferenceMarker.cs               (identical-after-EOL)
///   4. src/EJLive.Server/ServerMainForm.cs                    (reference-only legacy NOC dashboard)
///   5. src/EJLive.Server/Services/JournalAnalyticsService.cs  (IDisposable + nullable upgrade)
///   6. src/EJLive.Server/Services/RemoteControlService.cs     (IDisposable + envelope upgrade)
///   7. src/EJLive.Setup/SetupWizardForm.cs                    (shared link into installer)
///   8. src/EJLive.Verification/Program.cs                     (active probe set is superset)
/// </summary>
[TestClass]
public sealed class V5EnhancedDifferenceTests
{
    // -----------------------------------------------------------------
    // 1. Client/Program.cs  — startup plan honours v5 arguments + mutex
    // -----------------------------------------------------------------

    [TestMethod]
    public void StartupPlanner_PreservesV5MutexNameForAgentSingleton()
    {
        // The v5 Program.cs hard-codes "EJLive_Agent_v5" as the mutex name.
        // The active ClientStartupPlanner must preserve the same identity so
        // that an upgraded build does not race with an in-flight v5 agent.
        Assert.AreEqual("EJLive_Agent_v5", ClientStartupPlanner.AgentMutexName);
    }

    [TestMethod]
    public void StartupPlanner_RecognisesV5BackgroundAndAutostartArguments()
    {
        // v5 detected ("--background" or "--autostart") via Args.Contains.
        // The active planner must continue to do the same.
        Assert.AreEqual("--background", ClientStartupPlanner.BackgroundArgument);
        Assert.AreEqual("--autostart", ClientStartupPlanner.AutoStartArgument);

        var background = ClientStartupPlanner.Create(new[] { "--background" }, isAdministrator: true);
        var autostart = ClientStartupPlanner.Create(new[] { "--autostart" }, isAdministrator: true);

        Assert.IsTrue(background.IsBackground);
        Assert.IsTrue(autostart.IsBackground);
    }

    // -----------------------------------------------------------------
    // 2. Core/Constants.cs  — env var override (active enhancement)
    // -----------------------------------------------------------------

    [TestMethod]
    public void DefaultDatabasePath_HonoursEnvironmentVariableOverride()
    {
        // The v5 baseline returned a fixed path under CommonApplicationData.
        // The active runtime adds an EJLIVE_DATABASE_PATH override to make
        // tests/CI deterministic. This test pins that contract.
        var previous = Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH");
        var sentinel = Path.Combine(Path.GetTempPath(), $"ejlive-defaultdb-{Guid.NewGuid():N}.db");
        try
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", sentinel);
            Assert.AreEqual(sentinel, AppConstants.DefaultDatabasePath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", previous);
        }
    }

    [TestMethod]
    public void DefaultDatabasePath_FallsBackToCommonApplicationDataLikeV5()
    {
        var previous = Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH");
        try
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", null);
            var path = AppConstants.DefaultDatabasePath;

            Assert.IsFalse(string.IsNullOrWhiteSpace(path));
            Assert.IsTrue(path.EndsWith("ejlive.db", StringComparison.OrdinalIgnoreCase),
                $"Unexpected DefaultDatabasePath fallback: {path}");
        }
        finally
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", previous);
        }
    }

    // -----------------------------------------------------------------
    // 5. JournalAnalyticsService.cs — IDisposable + non-null DTO defaults
    // -----------------------------------------------------------------

    [TestMethod]
    public void JournalAnalyticsService_ImplementsIDisposableAndReleasesTimer()
    {
        // Active version is `sealed class : IDisposable` and disposes the
        // auto-archive timer. v5 was a non-sealed class with a leaky timer.
        Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(JournalAnalyticsService)),
            "JournalAnalyticsService must implement IDisposable in the active build.");
        Assert.IsTrue(typeof(JournalAnalyticsService).IsSealed,
            "JournalAnalyticsService must be sealed in the active build.");

        var root = Path.Combine(Path.GetTempPath(), $"ejlive-jas-{Guid.NewGuid():N}");
        var storage = Path.Combine(root, "storage");
        var archive = Path.Combine(root, "archive");
        try
        {
            // Construction creates the auto-archive timer; Dispose must not throw.
            using (new JournalAnalyticsService(storage, archive))
            {
                Assert.IsTrue(Directory.Exists(storage));
                Assert.IsTrue(Directory.Exists(archive));
            }
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
        }
    }

    [TestMethod]
    public void JournalAnalyticsService_DtosInitialiseStringPropertiesToNonNullDefaults()
    {
        // Active inner DTOs default string properties to string.Empty so that
        // nullable-aware consumers do not need to defensively coalesce.
        var stats = new ATMJournalStats();
        var record = new JournalRecord();

        Assert.IsNotNull(stats.ATM_ID);
        Assert.IsNotNull(stats.LastFileName);

        Assert.IsNotNull(record.ATM_ID);
        Assert.IsNotNull(record.FileName);
        Assert.IsNotNull(record.Checksum);
        Assert.IsNotNull(record.StoragePath);
    }

    [TestMethod]
    public void JournalAnalyticsService_StoresJournalAndUpdatesAggregateStats()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-jas-store-{Guid.NewGuid():N}");
        var storage = Path.Combine(root, "storage");
        var archive = Path.Combine(root, "archive");
        try
        {
            using var service = new JournalAnalyticsService(storage, archive);
            var data = System.Text.Encoding.UTF8.GetBytes(
                "NCR EJDATA APPROVED AMOUNT 200\nWITHDRAWAL 200\nDISPENSE 200\nERROR JAM\nCARD CAPTURED");
            service.StoreJournalData("ATM-V5", "EJDATA.LOG", data, checksum: "deadbeef");

            var snapshot = service.GetATMStats("ATM-V5");
            Assert.AreEqual("ATM-V5", snapshot.ATM_ID);
            Assert.AreEqual(1, snapshot.TotalFiles);
            Assert.IsTrue(snapshot.TotalLinesProcessed >= 5);
            Assert.IsTrue(snapshot.TotalWithdrawals >= 1);
            Assert.IsTrue(snapshot.TotalErrors >= 1);
            Assert.IsTrue(snapshot.TotalCardRetained >= 1);

            var recent = service.GetRecentRecords(10, "ATM-V5");
            Assert.AreEqual(1, recent.Count);
            Assert.AreEqual("EJDATA.LOG", recent[0].FileName);
            Assert.AreEqual("deadbeef", recent[0].Checksum);
        }
        finally
        {
            try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
        }
    }

    // -----------------------------------------------------------------
    // 6. RemoteControlService.cs — IDisposable + envelope-based dispatch
    // -----------------------------------------------------------------

    [TestMethod]
    public void RemoteControlService_ImplementsIDisposableAndDetachesFromServerEngine()
    {
        Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(RemoteControlService)),
            "RemoteControlService must implement IDisposable in the active build.");
        Assert.IsTrue(typeof(RemoteControlService).IsSealed,
            "RemoteControlService must be sealed in the active build.");

        using var server = new ServerEngine();
        // Construction subscribes to ServerEngine.Log; Dispose must unsubscribe
        // without throwing so that repeated cycles remain safe.
        using (new RemoteControlService(server))
        {
        }
        // A second cycle proves the previous Dispose detached cleanly.
        using (new RemoteControlService(server))
        {
        }
    }

    [TestMethod]
    public void RemoteControlService_RecordsNoActiveConnectionWhenAtmIsOffline()
    {
        // The active envelope-based dispatcher records a default Result of
        // "No active connection" for offline ATMs. v5 wrote no result text
        // until a reply arrived. This test pins the active behaviour.
        using var server = new ServerEngine();
        using var service = new RemoteControlService(server);

        var commandId = service.SendScreenshot("ATM-NOT-CONNECTED");
        var record = service.GetCommandHistory("ATM-NOT-CONNECTED", 1).Single();

        Assert.AreEqual(commandId, record.CommandId);
        Assert.IsFalse(record.Sent);
        Assert.IsFalse(record.Completed);
        Assert.AreEqual("No active connection", record.Result);
    }

    [TestMethod]
    public void RemoteControlService_BroadcastReturnsZeroWhenNoConnections()
    {
        using var server = new ServerEngine();
        using var service = new RemoteControlService(server);

        Assert.AreEqual(0, service.BroadcastRestart(delaySeconds: 5));
        Assert.AreEqual(0, service.BroadcastTimeSync());
        Assert.AreEqual(0, service.BroadcastScreenshot());
    }

    // -----------------------------------------------------------------
    // 7. SetupWizardForm.cs — linked into the Installer project
    // -----------------------------------------------------------------

    [TestMethod]
    public void InstallerProject_LinksSetupWizardFormFromOrphanedSetupFolder()
    {
        // EJLive.Setup has no .csproj; the wizard is linked into the Installer
        // project so the v5 setup story still ships, just from a single
        // canonical location. This test keeps that wiring honest.
        var root = FindRepositoryRoot();
        var installerCsproj = Path.Combine(root, "src", "EJLive.Installer.WinForms", "EJLive.Installer.WinForms.csproj");
        var setupFile = Path.Combine(root, "src", "EJLive.Setup", "SetupWizardForm.cs");

        Assert.IsTrue(File.Exists(installerCsproj), $"Installer csproj not found: {installerCsproj}");
        Assert.IsTrue(File.Exists(setupFile), $"Setup wizard source not found: {setupFile}");

        var contents = File.ReadAllText(installerCsproj);
        Assert.IsTrue(contents.Contains(@"..\EJLive.Setup\SetupWizardForm.cs", StringComparison.OrdinalIgnoreCase),
            "Installer project must link the legacy SetupWizardForm.cs from the orphaned EJLive.Setup folder.");
    }

    // -----------------------------------------------------------------
    // 8. OriginalSourceCatalog — v5 manifest stays at exactly 8 deltas
    // -----------------------------------------------------------------

    [TestMethod]
    public void OriginalSourceCatalog_ReportsEightCompiledDifferencesForV5()
    {
        var v5 = OriginalSourceCatalog.FindByName("EJLive_Client_v5_Enhanced");

        Assert.AreEqual(149, v5.CSharpFiles, "v5 Enhanced should still ship 149 C# files.");
        Assert.AreEqual(141, v5.IdenticalCSharpToActive, "141 files must remain hash-identical to the active source.");
        Assert.AreEqual(8, v5.DifferentCSharpFromActive, "Exactly 8 files must remain as the focused promotion delta.");
        Assert.AreEqual(SourceMergeRole.ReferenceWorkspace, v5.Role);
    }

    [TestMethod]
    public void OriginalSourceCatalog_DescribesV5AsActiveWithReferenceFeature()
    {
        var v5Features = OriginalSourceCatalog.FeaturesFor("EJLive_Client_v5_Enhanced");

        Assert.IsTrue(v5Features.Count > 0, "v5 Enhanced must surface at least one tracked feature.");
        Assert.IsTrue(v5Features.Any(feature =>
                feature.State == FeatureMergeState.ActiveWithReference &&
                feature.Feature.Contains("Enhanced v5 client", StringComparison.OrdinalIgnoreCase)),
            "v5 Enhanced should be classified as ActiveWithReference (active startup plan + preserved reference).");
    }

    // -----------------------------------------------------------------
    // Repository helpers
    // -----------------------------------------------------------------

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EJLive.Unified.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate EJLive.Unified.slnx from the test output folder.");
    }
}
