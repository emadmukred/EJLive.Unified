using System.Diagnostics;
using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Client.WinForms.Services;
using EJLive.Setup;

namespace EJLive.Installer.WinForms;

public sealed class InstallerForm : Form
{
    private readonly ListBox _steps = new() { Dock = DockStyle.Left, Width = 220 };
    private readonly RichTextBox _details = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F) };

    public InstallerForm()
    {
        Text = "EJLive Unified Installer";
        MinimumSize = new Size(840, 560);
        Size = new Size(940, 640);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        _steps.Items.AddRange(new object[] { "Overview", "Prerequisites", "Database", "Client Runtime", "Server Runtime", "Windows Services", "Finish" });
        _steps.SelectedIndexChanged += (_, _) => ShowStep();
        _steps.SelectedIndex = 0;

        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8), FlowDirection = FlowDirection.RightToLeft };
        actions.Controls.Add(Button("Install", Install));
        actions.Controls.Add(Button("Uninstall", UninstallRuntime));
        actions.Controls.Add(Button("Validate", ValidateEnvironment));
        actions.Controls.Add(Button("Setup Wizard", () => new SetupWizardForm().Show(this)));
        actions.Controls.Add(Button("Open Data Folder", () => OpenFolder(Path.GetDirectoryName(AppConstants.DefaultDatabasePath)!)));

        Controls.Add(_details);
        Controls.Add(_steps);
        Controls.Add(actions);
    }

    private void ShowStep()
    {
        _details.Text = _steps.SelectedItem?.ToString() switch
        {
            "Overview" => "EJLive Unified installs Shared, Core, Client, Server, Monitoring, and Installer components.",
            "Prerequisites" => ".NET 8 Windows Desktop runtime, SQLite package, and write access to ProgramData are required.",
            "Database" => $"SQLite database: {AppConstants.DefaultDatabasePath}",
            "Client Runtime" => $"Client outbox: {AppConstants.DefaultClientOutboxPath}{Environment.NewLine}Client inbox: {AppConstants.DefaultClientInboxPath}",
            "Server Runtime" => $"Server share: {AppConstants.DefaultServerSharePath}{Environment.NewLine}Archive: {AppConstants.DefaultArchivePath}",
            "Windows Services" => "Installer deploys EJLive.Client.Service, configures service recovery, and applies Windows remote-admin baseline (RDP/NLA/WinRM/RemoteRegistry).",
            "Finish" => "Validation complete. Use Visual Studio to build or run the unified solution.",
            _ => string.Empty
        };
    }

    private void ValidateEnvironment()
    {
        _details.AppendText(Environment.NewLine + "Validation:" + Environment.NewLine);
        foreach (var path in new[] { AppConstants.DefaultLogPath, AppConstants.DefaultArchivePath, AppConstants.DefaultReportsPath, AppConstants.DefaultClientOutboxPath, AppConstants.DefaultClientInboxPath })
        {
            Directory.CreateDirectory(path);
            _details.AppendText($"OK {path}{Environment.NewLine}");
        }
    }

    private void Install()
    {
        _details.AppendText(Environment.NewLine + "Install started..." + Environment.NewLine);
        ValidateEnvironment();
        DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);
        var config = AppConfig.Load();
        config.ApplyDefaults();
        EnforceClientAdminDefaults(config);
        config.Save();
        AgentConfigurationXmlService.SaveAppConfig(config);

        var baseline = ApplyWindowsRemoteBaseline(config);
        _details.AppendText(baseline + Environment.NewLine);

        var serviceInstallResult = InstallClientServiceWindowsRuntime();
        _details.AppendText(serviceInstallResult + Environment.NewLine);

        var serviceRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive",
            "ClientService");
        var servicePort = config.ScopedFirewallPort > 0
            ? config.ScopedFirewallPort
            : (config.ServerPort > 0 ? config.ServerPort : AppConstants.DefaultPort);
        var remoteAddresses = !string.IsNullOrWhiteSpace(config.ScopedFirewallRemoteAddresses)
            ? config.ScopedFirewallRemoteAddresses
            : (string.IsNullOrWhiteSpace(config.ServerIP) ? "Any" : config.ServerIP);
        var hardeningResult = InstallerOperationalSecurityService.Apply(new InstallerOperationalSecurityOptions
        {
            ProgramRoot = serviceRoot,
            DatabasePath = AppConstants.DefaultDatabasePath,
            ServicePort = servicePort,
            RemoteAddresses = remoteAddresses,
            ConfigureDefenderExclusion = config.ConfigureDefenderExclusions,
            SystemOnlyDatabaseAcl = true,
            CleanupInstallerTempArtifacts = true
        });
        _details.AppendText((hardeningResult.Success ? "Security hardening: OK" : "Security hardening: WARNING") + Environment.NewLine);
        foreach (var note in hardeningResult.Notes)
            _details.AppendText("  - " + note + Environment.NewLine);

        var readiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
        _details.AppendText("Windows remote readiness: " + readiness.ToSummary() + Environment.NewLine);
        _details.AppendText("Install actions completed." + Environment.NewLine);
    }

    private void UninstallRuntime()
    {
        var decision = MessageBox.Show(
            "This will remove EJLive client service registration, startup hooks, security rules, and client runtime data under ProgramData. Continue?",
            "EJLive Uninstall",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (decision != DialogResult.Yes)
            return;

        _details.AppendText(Environment.NewLine + "Uninstall started..." + Environment.NewLine);

        var stop = WindowsServiceRegistrationService.StopService();
        _details.AppendText("Service stop: " + stop.Message + Environment.NewLine);

        var removeService = WindowsServiceRegistrationService.UninstallService();
        _details.AppendText("Service uninstall: " + removeService.Message + Environment.NewLine);

        var unregisterTask = WindowsStartupService.UnregisterClientAutostart();
        _details.AppendText("Startup task cleanup: " + unregisterTask.Message + Environment.NewLine);

        var unregisterCompanion = WindowsStartupService.UnregisterUserSessionCompanion();
        _details.AppendText("Companion startup cleanup: " + unregisterCompanion.Message + Environment.NewLine);

        var serviceRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive",
            "ClientService");
        var uninstall = InstallerOperationalSecurityService.Uninstall(
            programRoot: serviceRoot,
            databasePath: AppConstants.DefaultDatabasePath,
            removeClientRuntimeFolders: true);
        _details.AppendText((uninstall.Success ? "Security cleanup: OK" : "Security cleanup: WARNING") + Environment.NewLine);
        foreach (var note in uninstall.Notes)
            _details.AppendText("  - " + note + Environment.NewLine);

        _details.AppendText("Uninstall actions completed." + Environment.NewLine);
    }

    private void EnforceClientAdminDefaults(AppConfig config)
    {
        var changed = false;
        if (!config.AutoEnableRemoteAccess)
        {
            config.AutoEnableRemoteAccess = true;
            changed = true;
        }

        if (!config.AllowLocalWindowsPasswordChange)
        {
            config.AllowLocalWindowsPasswordChange = true;
            changed = true;
        }

        if (!config.RequireEncryptedWindowsPasswordPayload)
        {
            config.RequireEncryptedWindowsPasswordPayload = true;
            changed = true;
        }

        if (!config.AutoPrepareWindowsRuntime)
        {
            config.AutoPrepareWindowsRuntime = true;
            changed = true;
        }

        if (!config.EnableWinRmBootstrap)
        {
            config.EnableWinRmBootstrap = true;
            changed = true;
        }

        if (!config.EnableRemoteRegistryBootstrap)
        {
            config.EnableRemoteRegistryBootstrap = true;
            changed = true;
        }

        if (!config.EnforceScopedFirewallRule)
        {
            config.EnforceScopedFirewallRule = true;
            changed = true;
        }

        if (!config.ConfigureDefenderExclusions)
        {
            config.ConfigureDefenderExclusions = true;
            changed = true;
        }

        const int enforcedRepairInterval = 5;
        if (config.WindowsBaselineRepairIntervalMin != enforcedRepairInterval)
        {
            config.WindowsBaselineRepairIntervalMin = enforcedRepairInterval;
            changed = true;
        }

        var accounts = ParseAccountList(config.AllowedPasswordAccounts);
        accounts.Add("Administrator");
        accounts.Add("Helpdesk");
        var currentUser = (Environment.UserName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(currentUser))
            accounts.Add(currentUser);

        var merged = string.Join(",", accounts.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        if (!string.Equals(config.AllowedPasswordAccounts, merged, StringComparison.Ordinal))
        {
            config.AllowedPasswordAccounts = merged;
            changed = true;
        }

        if (changed)
            _details.AppendText("Client policy defaults enforced for remote admin runtime." + Environment.NewLine);
    }

    private static HashSet<string> ParseAccountList(string accountsCsv)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(accountsCsv))
            return set;

        foreach (var item in accountsCsv.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var value = item.Trim();
            if (!string.IsNullOrWhiteSpace(value))
                set.Add(value);
        }

        return set;
    }

    private static string ApplyWindowsRemoteBaseline(AppConfig config)
    {
        var enforcer = new WindowsPolicyEnforcer(() => config);
        var result = enforcer.EnforceBaseline();
        return result.Success
            ? "Windows baseline applied: " + result.Message
            : "Windows baseline warning: " + result.Message;
    }

    private string InstallClientServiceWindowsRuntime()
    {
        try
        {
            var serviceRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "ClientService");
            Directory.CreateDirectory(serviceRoot);
            _details.AppendText($"Preparing service payload: {serviceRoot}{Environment.NewLine}");

            var stop = WindowsServiceRegistrationService.StopService();
            if (!stop.Success)
                _details.AppendText("Service stop note: " + stop.Message + Environment.NewLine);

            if (!DeployServicePayload(serviceRoot, out var deploymentDetail))
                return "Service deployment failed: " + deploymentDetail;

            _details.AppendText(deploymentDetail + Environment.NewLine);

            var serviceExePath = Path.Combine(serviceRoot, "EJLive.Client.Service.exe");
            if (!File.Exists(serviceExePath))
                return "Service deployment failed: EJLive.Client.Service.exe was not found in target directory.";

            var register = WindowsServiceRegistrationService.InstallOrUpdateService(serviceExePath);
            if (!register.Success)
                return "Service registration failed: " + register.Message;

            _details.AppendText("Service registration: " + register.Message + Environment.NewLine);

            var start = WindowsServiceRegistrationService.StartService();
            if (!start.Success)
                return "Service registered, but start failed: " + start.Message;

            _details.AppendText("Service start: " + start.Message + Environment.NewLine);

            var query = WindowsServiceRegistrationService.QueryService();
            _details.AppendText("Service query: " + query.Message + Environment.NewLine);

            var cleanupTask = WindowsStartupService.UnregisterClientAutostart();
            if (!cleanupTask.Success)
                _details.AppendText("Autostart task cleanup note: " + cleanupTask.Message + Environment.NewLine);

            var companionExePath = Path.Combine(serviceRoot, "EJLive.Client.exe");
            if (File.Exists(companionExePath))
            {
                var companionStartup = WindowsStartupService.RegisterUserSessionCompanion(companionExePath);
                _details.AppendText("Session companion startup: " + companionStartup.Message + Environment.NewLine);
            }
            else
            {
                _details.AppendText("Session companion startup skipped: EJLive.Client.exe not found in service payload." + Environment.NewLine);
            }

            return "Windows service deployment + registration + auto-start completed successfully.";
        }
        catch (Exception ex)
        {
            return "Service install error: " + ex.Message;
        }
    }

    private static bool DeployServicePayload(string targetDirectory, out string detail)
    {
        if (TryPublishService(targetDirectory, out detail))
            return true;

        var candidates = BuildServicePayloadCandidates();
        foreach (var sourceDirectory in candidates)
        {
            if (TryCopyPayloadDirectory(sourceDirectory, targetDirectory, out detail))
                return true;
        }

        detail = "No valid service payload source was found.";
        return false;
    }

    private static bool TryPublishService(string targetDirectory, out string detail)
    {
        detail = "Service publish was skipped.";
        var root = FindSolutionRoot();
        if (string.IsNullOrWhiteSpace(root))
            return false;

        var projectPath = Path.Combine(root, "src", "EJLive.Client.Service", "EJLive.Client.Service.csproj");
        if (!File.Exists(projectPath))
            return false;

        var publish = RunProcess(
            "dotnet",
            $"publish \"{projectPath}\" -c Release -o \"{targetDirectory}\" --nologo",
            timeoutMs: 240000);

        if (!publish.Success)
        {
            detail = "dotnet publish failed: " + publish.Output;
            return false;
        }

        var serviceExe = Path.Combine(targetDirectory, "EJLive.Client.Service.exe");
        if (!File.Exists(serviceExe))
        {
            detail = "dotnet publish finished but service executable was not produced.";
            return false;
        }

        detail = "Service published from source project to installer target directory.";
        return true;
    }

    private static string[] BuildServicePayloadCandidates()
    {
        var candidates = new List<string>();
        candidates.Add(AppContext.BaseDirectory);

        var root = FindSolutionRoot();
        if (!string.IsNullOrWhiteSpace(root))
        {
            candidates.Add(Path.Combine(root, "src", "EJLive.Client.Service", "bin", "Release", "net8.0-windows"));
            candidates.Add(Path.Combine(root, "src", "EJLive.Client.Service", "bin", "Debug", "net8.0-windows"));
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static bool TryCopyPayloadDirectory(string sourceDirectory, string targetDirectory, out string detail)
    {
        detail = "No payload copied.";
        if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory))
            return false;

        var serviceExe = Path.Combine(sourceDirectory, "EJLive.Client.Service.exe");
        if (!File.Exists(serviceExe))
            return false;

        Directory.CreateDirectory(targetDirectory);
        CopyDirectoryRecursive(sourceDirectory, targetDirectory);
        detail = $"Service payload copied from: {sourceDirectory}";
        return File.Exists(Path.Combine(targetDirectory, "EJLive.Client.Service.exe"));
    }

    private static void CopyDirectoryRecursive(string sourceDirectory, string targetDirectory)
    {
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(targetDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destination = Path.Combine(targetDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
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

        return string.Empty;
    }

    private static (bool Success, int ExitCode, string Output) RunProcess(string fileName, string arguments, int timeoutMs)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process == null)
                return (false, -1, $"{fileName} did not start.");

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(); } catch { }
                return (false, -2, $"{fileName} timed out.");
            }

            var output = (process.StandardOutput.ReadToEnd() + " " + process.StandardError.ReadToEnd()).Trim();
            return (process.ExitCode == 0, process.ExitCode, output);
        }
        catch (Exception ex)
        {
            return (false, -3, ex.Message);
        }
    }

    private static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4) };
        button.Click += (_, _) => action();
        return button;
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
    }
}
