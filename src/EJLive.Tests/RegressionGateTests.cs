using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Windows.Forms;

namespace EJLive.Tests;

[TestClass]
public sealed class RegressionGateTests
{
    [TestMethod]
    public void ClientServicePath_HasNoWindowsFormsDependency()
    {
        var serviceAssembly = typeof(EJLive.Client.Service.ClientAgentWindowsService).Assembly;
        var referenced = serviceAssembly.GetReferencedAssemblies();
        var hasWinForms = referenced.Any(r => r.Name.Equals("System.Windows.Forms", StringComparison.OrdinalIgnoreCase));

        Assert.IsFalse(hasWinForms, "Client.Service must not reference System.Windows.Forms. Headless services must remain UI-free.");
    }

    [TestMethod]
    public void ClientServicePath_HasNoDirectUiTypes()
    {
        var serviceAssembly = typeof(EJLive.Client.Service.ClientAgentWindowsService).Assembly;
        var uiTypes = serviceAssembly.GetTypes()
            .Where(t => typeof(Control).IsAssignableFrom(t) || typeof(Form).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToArray();

        Assert.AreEqual(0, uiTypes.Length,
            $"Client.Service contains UI types: {string.Join(", ", uiTypes)}. Service path must be headless.");
    }

    [TestMethod]
    public void ActiveCompileMap_ExistsAndHasEntries()
    {
        var solutionRoot = FindSolutionRoot();
        var mapPath = Path.Combine(solutionRoot, "artifacts", "ActiveCompileMap.csv");

        Assert.IsTrue(File.Exists(mapPath), "ActiveCompileMap.csv must exist.");
        var lines = File.ReadAllLines(mapPath);
        Assert.IsTrue(lines.Length > 1, "ActiveCompileMap.csv must contain data rows.");
    }

    [TestMethod]
    public void ServiceActivationStatus_ExistsAndCoversReferenceServices()
    {
        var solutionRoot = FindSolutionRoot();
        var statusPath = Path.Combine(solutionRoot, "docs", "12-service-activation-status.csv");

        Assert.IsTrue(File.Exists(statusPath), "12-service-activation-status.csv must exist.");
        var lines = File.ReadAllLines(statusPath);
        Assert.IsTrue(lines.Length > 50, $"Service activation status must cover at least 50 reference services. Found {lines.Length - 1}.");
    }

    [TestMethod]
    public void NoUnsafeTermsInProductionNamespace()
    {
        var unsafeTerms = new[] { "Ghost", "Stealth", "Hidden", "Bypass", "DisableDefender", "KillProcess" };
        var productionAssemblies = new[]
        {
            typeof(EJLive.Application.EJLiveApplicationHost).Assembly,
            typeof(EJLive.Business.UnifiedBusinessRuntime).Assembly,
            typeof(EJLive.Core.Constants).Assembly,
            typeof(EJLive.Shared.SecurityHelper).Assembly,
            typeof(EJLive.Client.Service.ClientServiceHost).Assembly,
            typeof(EJLive.Client.WinForms.ClientMainForm).Assembly,
            typeof(EJLive.Server.WinForms.ServerMainForm).Assembly,
        };

        var violations = new List<string>();
        foreach (var assembly in productionAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var term in unsafeTerms)
                {
                    if (type.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                    {
                        violations.Add($"{type.FullName} contains unsafe term '{term}'");
                    }
                }
            }
        }

        Assert.AreEqual(0, violations.Count,
            $"Production namespace contains unsafe terms: {string.Join("; ", violations)}");
    }

    [TestMethod]
    public void ProjectDependencyGraph_Exists()
    {
        var solutionRoot = FindSolutionRoot();
        var graphPath = Path.Combine(solutionRoot, "artifacts", "ProjectDependencyGraph.md");

        Assert.IsTrue(File.Exists(graphPath), "ProjectDependencyGraph.md must exist.");
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EJLive.Unified.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate solution root.");
    }
}
