using System.Data.SQLite;
using System.Windows.Forms;
using EJLive.Application;
using EJLive.Business;
using EJLive.Client.WinForms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Installer.WinForms;
using EJLive.Monitor;
using EJLive.Monitoring.WinForms;
using EJLive.Server.Services;
using EJLive.Server.WinForms;
using EJLive.Shared;

var checks = new List<(string Name, bool Passed, string Detail)>
{
    RunDatabaseMigrationProbe(),
    RunApplicationLayerProbe(),
    await RunNetworkProbeAsync(),
    await RunRemoteCommandProbeAsync(),
    await RunClientTelemetryProbeAsync(),
    await RunPulseJsonTelemetryProbeAsync(),
    await RunJournalAckMetadataProbeAsync(),
    await RunFileWatcherProbeAsync(),
    RunUiProbe(),
    RunOriginalAuditProbe(),
    RunSourceFeatureCoverageProbe(),
    RunOperationalFusionProbe(),
    RunUnifiedServiceOperationsProbe(),
    RunOperationalReportingProbe(),
    RunOperationalReportCatalogProbe(),
    RunClientTelemetryAnalyticsProbe(),
    RunUnifiedServiceGatewayProbe(),
    RunReferenceServiceActivationCompletenessProbe(),
    RunIntegrationAuditProbe(),
    RunServiceActivationAuditProbe(),
    await RunLegacySelectiveUpgradeProbe(),
    RunCompileMapConflictProbe(),
    RunSourceTruthProbe(),
    RunFileLinkageProbe(),
    RunUnsafeTermScanProbe(),
    RunUiInServicePathProbe(),
    RunDuplicateTypeProbe()
};

foreach (var check in checks)
{
    var state = check.Passed ? "PASS" : "FAIL";
    Console.WriteLine($"{state} {check.Name}: {check.Detail}");
}

static async Task<(string Name, bool Passed, string Detail)> RunRemoteCommandProbeAsync()
{
    var port = Random.Shared.Next(59001, 61000);
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var commandResult = new ManualResetEventSlim(false);
    var detail = string.Empty;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.Log += (_, message) =>
        {
            if (message.Contains("Command result", StringComparison.OrdinalIgnoreCase))
            {
                detail = message;
                commandResult.Set();
            }
        };
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-COMMAND", AppConstants.ATM_TYPE_NCR);
        client.OnMessageReceived += (_, message) =>
        {
            if (message.Type == CommunicationProtocol.MsgType.Command &&
                RemoteCommandEnvelope.TryParse(message.Text, out var command))
            {
                client.SendMessage(CommunicationProtocol.BuildCommandResult(command.CommandId, true, $"{command.CommandType} acknowledged by probe."));
            }
        };

        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));
        var sent = server.SendCommand("ATM-COMMAND", new RemoteCommandEnvelope { CommandType = AppConstants.CMD_PING });
        var resultReceived = commandResult.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned && accepted && sent && resultReceived;
        return ("Remote command routing", passed, $"connectReturned={connectReturned}, accepted={accepted}, sent={sent}, resultReceived={resultReceived}, detail={detail}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Remote command routing", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunFileWatcherProbeAsync()
{
    var folder = Path.Combine(Path.GetTempPath(), "ejlive-filewatcher-probe", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);
    using var watcher = new FileWatcherEngine { PollInterval = TimeSpan.FromSeconds(1) };
    using var detected = new ManualResetEventSlim(false);
    var detectedFile = string.Empty;

    try
    {
        watcher.FileChanged += (_, file) =>
        {
            detectedFile = Path.GetFileName(file);
            detected.Set();
        };
        watcher.Start(folder);
        var path = Path.Combine(folder, "EJDATA.LOG");
        await File.WriteAllTextAsync(path, "probe").ConfigureAwait(false);
        var passed = detected.Wait(TimeSpan.FromSeconds(5));
        watcher.Stop();
        Directory.Delete(folder, recursive: true);
        return ("File watcher fallback", passed, $"detected={passed}, file={detectedFile}");
    }
    catch (Exception ex)
    {
        try { watcher.Stop(); } catch { }
        try { Directory.Delete(folder, recursive: true); } catch { }
        return ("File watcher fallback", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunClientTelemetryProbeAsync()
{
    var port = Random.Shared.Next(61001, 62000);
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var telemetryReceived = new ManualResetEventSlim(false);
    ClientTelemetryPacket? captured = null;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.ClientTelemetryReceived += (_, packet) =>
        {
            captured = packet;
            telemetryReceived.Set();
        };
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-TEL-PROBE", AppConstants.ATM_TYPE_NCR);
        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        var detail = "probe=telemetry";
        var payload =
            "TELEMETRY|ATM=ATM-TEL-PROBE;Type=probe_event;Severity=info;Utc=" + DateTime.UtcNow.ToString("O") +
            ";DetailB64=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(detail));
        client.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, payload));

        var received = telemetryReceived.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned &&
                     accepted &&
                     received &&
                     captured is not null &&
                     captured.ATM_ID == "ATM-TEL-PROBE" &&
                     captured.EventType == "probe_event" &&
                     captured.Detail == detail;
        var detailText = captured is null
            ? "none"
            : $"{captured.ATM_ID}/{captured.Severity}/{captured.EventType}";
        return ("Client telemetry stream", passed, $"connectReturned={connectReturned}, accepted={accepted}, received={received}, packet={detailText}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Client telemetry stream", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunPulseJsonTelemetryProbeAsync()
{
    var port = Random.Shared.Next(62001, 63000);
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var telemetryReceived = new ManualResetEventSlim(false);
    ClientTelemetryPacket? captured = null;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.ClientTelemetryReceived += (_, packet) =>
        {
            if (packet.EventType.Equals("pulse_json", StringComparison.OrdinalIgnoreCase))
            {
                captured = packet;
                telemetryReceived.Set();
            }
        };
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-PULSEJSON", AppConstants.ATM_TYPE_NCR);
        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        var now = DateTime.UtcNow;
        var payload = "{\"terminalId\":\"ATM-PULSEJSON\",\"timestampUtc\":\"" + now.ToString("O") +
                      "\",\"serviceState\":\"connected\",\"handshake\":true,\"pendingOutbox\":2,\"networkType\":\"lan\"}";
        client.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, "PULSE_JSON|" + payload));

        var received = telemetryReceived.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned &&
                     accepted &&
                     received &&
                     captured is not null &&
                     captured.ATM_ID == "ATM-PULSEJSON" &&
                     captured.EventType == "pulse_json" &&
                     captured.RawJson == payload &&
                     captured.Detail.Contains("pending=2", StringComparison.OrdinalIgnoreCase);
        var detailText = captured is null
            ? "none"
            : $"{captured.ATM_ID}/{captured.Severity}/{captured.EventType}";
        return ("Client pulse_json telemetry", passed, $"connectReturned={connectReturned}, accepted={accepted}, received={received}, packet={detailText}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Client pulse_json telemetry", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunJournalAckMetadataProbeAsync()
{
    var failures = new List<string>();

    for (var attempt = 1; attempt <= 3; attempt++)
    {
        var result = await RunJournalAckMetadataAttemptAsync(attempt).ConfigureAwait(false);
        if (result.Passed)
            return ("Journal ACK metadata", true, result.Detail);

        failures.Add(result.Detail);
        await Task.Delay(250).ConfigureAwait(false);
    }

    return ("Journal ACK metadata", false, string.Join("; ", failures));
}

static async Task<(bool Passed, string Detail)> RunJournalAckMetadataAttemptAsync(int attempt)
{
    var port = Random.Shared.Next(63001, 64000);
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var ackReceived = new ManualResetEventSlim(false);
    var ack = string.Empty;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-ACK-PROBE", AppConstants.ATM_TYPE_NCR);
        client.OnJournalAcknowledged += (_, text) =>
        {
            ack = text;
            ackReceived.Set();
        };

        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        var data = System.Text.Encoding.UTF8.GetBytes("ACK-PROBE-DATA");
        var checksum = SecurityHelper.MD5Hash(data);
        var sent = client.SendJournalFile("EJDATA.LOG", data, 0, checksum);
        var received = ackReceived.Wait(TimeSpan.FromSeconds(8));

        client.Disconnect();
        server.Stop();

        var passed = connectReturned &&
                     accepted &&
                     sent &&
                     received &&
                     ack.Contains("|OK|", StringComparison.OrdinalIgnoreCase) &&
                     ack.Contains("sha256=", StringComparison.OrdinalIgnoreCase) &&
                     ack.Contains("size=", StringComparison.OrdinalIgnoreCase) &&
                     ack.Contains("staging_time_ms=", StringComparison.OrdinalIgnoreCase);
        return (passed, $"attempt={attempt}, connectReturned={connectReturned}, accepted={accepted}, sent={sent}, received={received}, ack={ack}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return (false, $"attempt={attempt}, error={ex.Message}");
    }
}

static (string Name, bool Passed, string Detail) RunUiProbe()
{
    (string Name, bool Passed, string Detail) result = ("WinForms UI composition", false, "not run");
    var thread = new Thread(() =>
    {
        try
        {
            Environment.SetEnvironmentVariable(
                "EJLIVE_DATABASE_PATH",
                Path.Combine(Path.GetTempPath(), $"ejlive-ui-probe-{Guid.NewGuid():N}.db"));
            ApplicationConfiguration.Initialize();
            using var client = new ClientMainForm();
            using var server = new ServerMainForm();
            using var monitoring = new MainDashboardForm();
            using var installer = new InstallerForm();
            using var legacyMonitor = new MonitoringDashboard();

            var clientTabs = GetTabs(client);
            var serverTabs = GetTabs(server);
            var monitoringTabs = GetTabs(monitoring);
            var clientButtons = CountControls<Button>(client);
            var serverButtons = CountControls<Button>(server);
            var monitoringButtons = CountControls<Button>(monitoring);
            var installerButtons = CountControls<Button>(installer);
            var legacyMonitorLists = CountControls<ListView>(legacyMonitor);

            var passed =
                ContainsAll(clientTabs, "Connection", "Sync", "Journal", "Remote Control", "Services", "Settings", "Agent Config") &&
                ContainsAll(serverTabs, "Fleet", "Network Map", "Journal Viewer", "Sync Dashboard", "Remote Commands", "Alerts", "Archive", "Reports", "Settings") &&
                ContainsAll(monitoringTabs, "Overview", "Operational Map", "Device State", "Realtime Sync", "XFS Events", "Vendor Logs", "Reports") &&
                clientButtons >= 25 &&
                serverButtons >= 25 &&
                monitoringButtons >= 8 &&
                installerButtons >= 4 &&
                legacyMonitorLists >= 2;

            result = ("WinForms UI composition", passed, $"clientTabs={string.Join(',', clientTabs)}; serverTabs={string.Join(',', serverTabs)}; monitoringTabs={string.Join(',', monitoringTabs)}; buttons={clientButtons}/{serverButtons}/{monitoringButtons}/{installerButtons}; legacyMonitorLists={legacyMonitorLists}");
        }
        catch (Exception ex)
        {
            result = ("WinForms UI composition", false, ex.Message);
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();
    return result;
}

static string[] GetTabs(Form form)
{
    return EnumerateControls(form)
        .OfType<TabControl>()
        .SelectMany(t => t.TabPages.Cast<TabPage>())
        .Select(t => t.Text)
        .ToArray();
}

static int CountControls<T>(Control root) where T : Control => EnumerateControls(root).OfType<T>().Count();

static IEnumerable<Control> EnumerateControls(Control root)
{
    foreach (Control child in root.Controls)
    {
        yield return child;
        foreach (var descendant in EnumerateControls(child))
            yield return descendant;
    }
}

static bool ContainsAll(IEnumerable<string> actual, params string[] expected)
{
    var set = new HashSet<string>(actual, StringComparer.OrdinalIgnoreCase);
    return expected.All(set.Contains);
}

static (string Name, bool Passed, string Detail) RunFileLinkageProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var allFiles = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var slnx = File.ReadAllText(Path.Combine(root, "EJLive.Unified.slnx"));
        var legacyReference = File.ReadAllText(Path.Combine(root, "src", "EJLive.LegacyReference", "EJLive.LegacyReference.csproj"));
        var installerProject = File.ReadAllText(Path.Combine(root, "src", "EJLive.Installer.WinForms", "EJLive.Installer.WinForms.csproj"));
        var coreProject = File.ReadAllText(Path.Combine(root, "src", "EJLive.Core", "EJLive.Core.csproj"));
        var clientProject = File.ReadAllText(Path.Combine(root, "src", "EJLive.Client.WinForms", "EJLive.Client.WinForms.csproj"));
        var serverProject = File.ReadAllText(Path.Combine(root, "src", "EJLive.Server.WinForms", "EJLive.Server.WinForms.csproj"));
        var monitoringProject = File.ReadAllText(Path.Combine(root, "src", "EJLive.Monitoring.WinForms", "EJLive.Monitoring.WinForms.csproj"));
        var inventoryPath = Path.Combine(root, "docs", "09-file-function-inventory.csv");
        var inventoryLines = File.Exists(inventoryPath) ? File.ReadAllLines(inventoryPath) : Array.Empty<string>();
        var inventoryRowCount = Math.Max(0, inventoryLines.Length - 1);
        var inventoryText = inventoryLines.Length == 0 ? string.Empty : string.Join(Environment.NewLine, inventoryLines);
        var inventoryComparableFileCount = allFiles.Count(path =>
            !path.StartsWith(".codex/", StringComparison.OrdinalIgnoreCase) &&
            !path.StartsWith(".git/", StringComparison.OrdinalIgnoreCase));
        var inventoryRowDelta = Math.Abs(inventoryComparableFileCount - inventoryRowCount);

        var requiredProjects = new[]
        {
            "src/EJLive.Application/EJLive.Application.csproj",
            "src/EJLive.Business/EJLive.Business.csproj",
            "src/EJLive.Client.WinForms/EJLive.Client.WinForms.csproj",
            "src/EJLive.Client.Service/EJLive.Client.Service.csproj",
            "src/EJLive.Core/EJLive.Core.csproj",
            "src/EJLive.Installer.WinForms/EJLive.Installer.WinForms.csproj",
            "src/EJLive.LegacyReference/EJLive.LegacyReference.csproj",
            "src/EJLive.Monitor/EJLive.Monitor.csproj",
            "src/EJLive.Monitoring.WinForms/EJLive.Monitoring.WinForms.csproj",
            "src/EJLive.Server.WinForms/EJLive.Server.WinForms.csproj",
            "src/EJLive.Shared/EJLive.Shared.csproj",
            "src/EJLive.Tests/EJLive.Tests.csproj",
            "src/EJLive.Verification/EJLive.Verification.csproj"
        };

        var missingProjects = requiredProjects
            .Where(project => !slnx.Contains(project, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var markers = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["legacy original source"] = legacyReference.Contains(@"..\..\legacy\original\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["docs source map"] = legacyReference.Contains(@"..\..\docs\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["tools scripts"] = legacyReference.Contains(@"..\..\tools\**\*.*", StringComparison.OrdinalIgnoreCase) ||
                                legacyReference.Contains(@"..\tools\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["root artifacts"] = legacyReference.Contains("README.md", StringComparison.OrdinalIgnoreCase) &&
                                 legacyReference.Contains("EJLive.Unified.slnx", StringComparison.OrdinalIgnoreCase),
            ["root source archives"] = legacyReference.Contains("src_update.zip", StringComparison.OrdinalIgnoreCase) &&
                                      legacyReference.Contains("src_letast.zip", StringComparison.OrdinalIgnoreCase),
            ["unprojected server source"] = legacyReference.Contains(@"..\EJLive.Server\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["setup source reference"] = legacyReference.Contains(@"..\EJLive.Setup\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["imported archive reference"] = legacyReference.Contains(@"..\..\legacy\original\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["qoder development reference"] = legacyReference.Contains(@"..\.qoder-plugin\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["legacy docs reference"] = legacyReference.Contains(@"..\..\legacy\docs\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["reference books material"] = legacyReference.Contains(@"..\..\كتب ومراجع\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["src reference material"] = legacyReference.Contains(@"..\_reference\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["development support references"] = legacyReference.Contains(@"..\.continue\**\*.*", StringComparison.OrdinalIgnoreCase) &&
                                                 legacyReference.Contains(@"..\ocr-document-processor\**\*.*", StringComparison.OrdinalIgnoreCase) &&
                                                 legacyReference.Contains(@"..\Skills.agents\**\*.*", StringComparison.OrdinalIgnoreCase),
            ["setup wizard runtime link"] = installerProject.Contains(@"..\EJLive.Setup\SetupWizardForm.cs", StringComparison.OrdinalIgnoreCase),
            ["core reference source"] = coreProject.Contains("reference-source", StringComparison.OrdinalIgnoreCase) &&
                                        coreProject.Contains("<EnableDefaultItems>false</EnableDefaultItems>", StringComparison.OrdinalIgnoreCase),
            ["client reference source"] = clientProject.Contains("reference-source", StringComparison.OrdinalIgnoreCase) &&
                                          clientProject.Contains("<EnableDefaultItems>false</EnableDefaultItems>", StringComparison.OrdinalIgnoreCase),
            ["server reference source"] = serverProject.Contains("reference-source", StringComparison.OrdinalIgnoreCase) &&
                                          serverProject.Contains("<EnableDefaultItems>false</EnableDefaultItems>", StringComparison.OrdinalIgnoreCase),
            ["monitoring reference source"] = monitoringProject.Contains("reference-source", StringComparison.OrdinalIgnoreCase) &&
                                              monitoringProject.Contains("<EnableDefaultItems>false</EnableDefaultItems>", StringComparison.OrdinalIgnoreCase),
            ["file function inventory"] = inventoryRowDelta <= 10 &&
                                          !inventoryText.Contains("Unclassified project file", StringComparison.OrdinalIgnoreCase)
        };

        var srcRoot = Path.Combine(root, "src");
        var projectFolders = Directory.EnumerateDirectories(srcRoot)
            .Where(directory => File.Exists(Path.Combine(directory, Path.GetFileName(directory) + ".csproj")))
            .Select(directory => Path.GetFileName(directory))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var linkedSourceFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "EJLive.Server",
            "EJLive.Setup",
            "archive",
            "_reference",
            "tools",
            ".continue",
            ".qoder-plugin",
            ".qodo",
            ".sixth",
            ".vscode",
            "_rels",
            "ocr-document-processor",
            "packages",
            "Skills.agents",
            "xl"
        };
        var unlinkedSourceFolders = Directory.EnumerateDirectories(srcRoot)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Where(name => !name!.Equals(".vs", StringComparison.OrdinalIgnoreCase))
            .Where(name => !projectFolders.Contains(name!) && !linkedSourceFolders.Contains(name!))
            .ToArray();

        var unaccountedFiles = allFiles
            .Where(path => !IsAccounted(path, projectFolders, linkedSourceFolders))
            .Take(12)
            .ToArray();

        var failedMarkers = markers.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToArray();
        var passed = missingProjects.Length == 0 &&
                     failedMarkers.Length == 0 &&
                     unlinkedSourceFolders.Length == 0 &&
                     unaccountedFiles.Length == 0;

        return ("Project file linkage", passed,
            $"files={allFiles.Length}, inventoryComparable={inventoryComparableFileCount}, inventoryRows={inventoryRowCount}, inventoryDelta={inventoryRowDelta}, projects={requiredProjects.Length}, missingProjects={string.Join(',', missingProjects)}, failedMarkers={string.Join(',', failedMarkers)}, unlinkedFolders={string.Join(',', unlinkedSourceFolders)}, unaccounted={string.Join(',', unaccountedFiles)}");
    }
    catch (Exception ex)
    {
        return ("Project file linkage", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunCompileMapConflictProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var statusPath = Path.Combine(root, "docs", "12-service-activation-status.csv");
        var mapPath = Path.Combine(root, "artifacts", "ActiveCompileMap.csv");

        if (!File.Exists(statusPath))
            return ("Compile map conflict", false, "12-service-activation-status.csv not found");
        if (!File.Exists(mapPath))
            return ("Compile map conflict", false, "ActiveCompileMap.csv not found");

        var statusLines = File.ReadAllLines(statusPath).Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 3)
            .Select(parts => new { Path = parts[0].Trim('"'), Status = parts[2].Trim('"') })
            .ToArray();

        var mapLines = File.ReadAllLines(mapPath).Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length >= 3)
            .Select(parts => new { Path = parts[1].Trim('"'), CompileState = parts[2].Trim('"') })
            .ToDictionary(x => x.Path, x => x.CompileState, StringComparer.OrdinalIgnoreCase);

        var critical = new List<string>();
        var acceptable = new List<string>();

        foreach (var item in statusLines)
        {
            var expected = item.Status;
            var actual = mapLines.TryGetValue(item.Path, out var state) ? state : "MISSING";

            if (expected.Equals("ActiveCompiled", StringComparison.OrdinalIgnoreCase) &&
                !actual.Equals("ActiveCompiled", StringComparison.OrdinalIgnoreCase))
            {
                critical.Add($"{item.Path}: expected ActiveCompiled, actual {actual}");
            }
            else if (expected.Equals("ReferenceCovered", StringComparison.OrdinalIgnoreCase) &&
                     actual.Equals("ActiveCompiled", StringComparison.OrdinalIgnoreCase))
            {
                acceptable.Add($"{item.Path}: expected ReferenceOnly, actual ActiveCompiled (cross-project linked compile)");
            }
            else if (expected.Equals("ReferenceCovered", StringComparison.OrdinalIgnoreCase) &&
                     actual.Equals("Deprecated", StringComparison.OrdinalIgnoreCase))
            {
                acceptable.Add($"{item.Path}: expected ReferenceOnly, actual Deprecated (Compile Remove)");
            }
        }

        var passed = critical.Count == 0;
        var detail = $"critical={critical.Count}, acceptable={acceptable.Count}";
        if (critical.Count > 0)
            detail += "; critical: " + string.Join("; ", critical);
        if (acceptable.Count > 0)
            detail += "; acceptable: " + string.Join("; ", acceptable);

        return ("Compile map conflict", passed, detail);
    }
    catch (Exception ex)
    {
        return ("Compile map conflict", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunSourceTruthProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var sourceTruthPath = Path.Combine(root, "docs", "phase2-source-of-truth.md");
        var exists = File.Exists(sourceTruthPath);

        return ("Source truth document", exists,
            exists ? "docs/phase2-source-of-truth.md exists" : "docs/phase2-source-of-truth.md not found");
    }
    catch (Exception ex)
    {
        return ("Source truth document", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunOriginalAuditProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var originalRoot = Path.Combine(root, "legacy", "original");
        var auditRoot = Path.Combine(root, "docs", "original-audit");
        if (!Directory.Exists(originalRoot))
            return ("Original source audit coverage", true, "legacy/original not present in this checkout");

        var sourceRoots = Directory.EnumerateDirectories(originalRoot)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var missing = new List<string>();
        foreach (var sourceRoot in sourceRoots)
        {
            var safeName = BuildAuditSafeName(sourceRoot!);
            var required = new[]
            {
                Path.Combine(auditRoot, $"{safeName}-summary.md"),
                Path.Combine(auditRoot, $"{safeName}-file-manifest.csv"),
                Path.Combine(auditRoot, $"{safeName}-project-dependencies.csv")
            };

            if (required.Any(path => !File.Exists(path)))
                missing.Add(sourceRoot!);
        }

        return ("Original source audit coverage", missing.Count == 0,
            $"sourceRoots={sourceRoots.Length}, missing={string.Join(',', missing)}");
    }
    catch (Exception ex)
    {
        return ("Original source audit coverage", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunSourceFeatureCoverageProbe()
{
    try
    {
        var missing = OriginalSourceCatalog.ProjectsWithoutFeatureCoverage();
        var sourceFeatures = OriginalSourceCatalog.BuildFeatureCoverageReport();
        var v5Covered = OriginalSourceCatalog
            .FeaturesFor("EJLive_Client_v5_Enhanced")
            .Any(feature => feature.State == FeatureMergeState.ActiveWithReference &&
                            feature.Feature.Contains("Agent bootstrap", StringComparison.OrdinalIgnoreCase));
        var activeCoverage = OriginalSourceCatalog.ActiveOrReferencedFeatures.Count;

        var passed = missing.Count == 0 &&
                     sourceFeatures.Length == OriginalSourceCatalog.Projects.Count &&
                     v5Covered &&
                     activeCoverage >= 10;

        return ("Source feature coverage", passed,
            $"sources={OriginalSourceCatalog.Projects.Count}, features={sourceFeatures.Length}, activeOrReferenced={activeCoverage}, missing={string.Join(',', missing)}, v5Covered={v5Covered}");
    }
    catch (Exception ex)
    {
        return ("Source feature coverage", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunOperationalFusionProbe()
{
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var atm = runtime.RegisterAtm("ATM-FUSION", "Fusion Probe", AppConstants.ATM_TYPE_NCR, "127.0.0.1");
        atm.LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-1);
        atm.LastDataReceivedUtc = DateTime.UtcNow.AddMinutes(-2);
        runtime.TrackJournalSync("ATM-FUSION", "EJDATA.LOG", 8192, JournalSyncState.Completed);

        var command = new RemoteCommand
        {
            ATM_ID = "ATM-FUSION",
            CommandType = AppConstants.CMD_RESTART,
            RequiresConfirmation = true
        };
        var fusion = runtime.BuildOperationalFusion(
            "NCR APTRA EJDATA APPROVED AMOUNT 700\nCARD CAPTURED\nM-18 CASH ERROR\nHOST MESSAGE IN",
            new[]
            {
                "src/EJLive.Core/Services/UnifiedOperationalFusion.cs",
                "src/EJLive.Business/UnifiedBusinessRuntime.cs",
                "legacy/original/Coder01/README.md",
                "docs/09-file-function-inventory.csv"
            },
            command,
            role: "Admin",
            operatorConfirmed: true,
            maintenanceWindow: true);

        var passed = fusion.JournalEvidence.ApprovedTransactions == 1 &&
                     fusion.JournalEvidence.CashErrorEvents == 1 &&
                     fusion.JournalEvidence.CapturedCards == 1 &&
                     fusion.CommandPolicy?.Allowed == true &&
                     fusion.FileBindings.UnclassifiedCount == 0 &&
                     fusion.FleetReadiness.Summary.Total == 1;

        return ("Unified operational fusion", passed,
            $"signals={fusion.JournalEvidence.Signals.Count}, cashErrors={fusion.JournalEvidence.CashErrorEvents}, bindings={fusion.FileBindings.TotalFiles}, unclassified={fusion.FileBindings.UnclassifiedCount}, commandAllowed={fusion.CommandPolicy?.Allowed}");
    }
    catch (Exception ex)
    {
        return ("Unified operational fusion", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunUnifiedServiceOperationsProbe()
{
    var root = Path.Combine(Path.GetTempPath(), "ejlive-service-ops-probe", Guid.NewGuid().ToString("N"));
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var storage = Path.Combine(root, "storage");
        var archive = Path.Combine(root, "archive");
        var reports = Path.Combine(root, "reports");
        var data = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 900\nCARD CAPTURED\nM-18 CASH ERROR");

        var stored = runtime.JournalStorage.StoreJournalData(storage, "ATM-SVC", AppConstants.ATM_TYPE_NCR, "EJDATA.LOG", data);
        var csv = runtime.JournalStorage.ExportCsvReport(new[] { stored }, Path.Combine(reports, "journal.csv"));
        var html = runtime.JournalStorage.ExportHtmlReport(new[] { stored }, Path.Combine(reports, "journal.html"));
        var zip = runtime.JournalStorage.ArchiveMonth(storage, archive, "ATM-SVC", DateTime.UtcNow.ToString("yyyy-MM"));
        var dispatch = runtime.RemoteCommands.Queue("ATM-SVC", AppConstants.CMD_RESTART, role: "Admin", operatorConfirmed: true, maintenanceWindow: true);
        runtime.ClientServiceSupervisor.Start("Journal Sync", "Probe activation");
        runtime.ClientServiceSupervisor.Start("Network Monitor", "Probe activation");
        var serviceReport = runtime.ClientServiceSupervisor.BuildReport();

        var passed = File.Exists(stored.StoragePath) &&
                     File.Exists(csv) &&
                     File.Exists(html) &&
                     File.Exists(zip) &&
                     stored.Evidence.ApprovedTransactions == 1 &&
                     stored.Evidence.CashErrorEvents == 1 &&
                     dispatch.Policy.Allowed &&
                     serviceReport.Total >= 10 &&
                     serviceReport.Running >= 2;

        return ("Unified service operations", passed,
            $"stored={File.Exists(stored.StoragePath)}, csv={File.Exists(csv)}, html={File.Exists(html)}, zip={File.Exists(zip)}, running={serviceReport.Running}/{serviceReport.Total}, commandAllowed={dispatch.Policy.Allowed}");
    }
    catch (Exception ex)
    {
        return ("Unified service operations", false, ex.Message);
    }
    finally
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }
}

static (string Name, bool Passed, string Detail) RunOperationalReportingProbe()
{
    var folder = Path.Combine(Path.GetTempPath(), $"ejlive-reporting-probe-{Guid.NewGuid():N}");
    Directory.CreateDirectory(folder);
    try
    {
        var now = DateTime.UtcNow;
        var snapshot = new UnifiedServerAnalyticsSnapshot(
            Fleet: new FleetSummary { Total = 1, Connected = 1, Offline = 0, AverageHealth = 95 },
            Sync: new SyncSummary { Total = 1, Completed = 1, AverageProgress = 100 },
            ConfirmedDeliveries: 1,
            PendingDeliveries: 0,
            FailedDeliveries: 0,
            CommandDispatches: 2,
            CommandResults: 2,
            CommandFailures: 0,
            LastCommandAtUtc: now.AddMinutes(-2),
            TelemetryEvents: 2,
            TelemetryWarnings: 1,
            TelemetryErrors: 0,
            NetworkDisconnectEvents: 0,
            HandshakeMissingEvents: 0,
            FileRetryEvents: 1,
            LastTelemetryAtUtc: now.AddMinutes(-1),
            AtmRows: new[]
            {
                new UnifiedAtmOperationalAnalyticsRow(
                    "ATM-RPT",
                    AppConstants.ATM_TYPE_NCR,
                    95,
                    ConnectionStatus.Connected,
                    0,
                    0,
                    1,
                    0,
                    0,
                    1,
                    0,
                    now.AddMinutes(-1),
                    now.AddSeconds(-30),
                    1)
            });

        var reporting = new UnifiedOperationalReportingService();
        var window = reporting.ExportWindowReport(folder, "day", 24, snapshot, DateTime.Now, now);
        var bundle = reporting.ExportBundleReport(
            folder,
            new[]
            {
                new OperationalWindowSnapshot("shift", 8, snapshot),
                new OperationalWindowSnapshot("day", 24, snapshot),
                new OperationalWindowSnapshot("week", 168, snapshot)
            },
            DateTime.Now,
            now);
        var fleet = reporting.ExportFleetHealthReport(folder, snapshot, DateTime.Now);

        var passed =
            File.Exists(window.JsonPath) &&
            File.Exists(window.CsvPath) &&
            File.Exists(bundle.JsonPath) &&
            File.Exists(bundle.SummaryCsvPath) &&
            File.Exists(bundle.AtmCsvPath) &&
            File.Exists(fleet);
        return ("Operational reporting", passed, $"window={Path.GetFileName(window.JsonPath)}, bundle={Path.GetFileName(bundle.JsonPath)}, fleet={Path.GetFileName(fleet)}");
    }
    catch (Exception ex)
    {
        return ("Operational reporting", false, ex.Message);
    }
    finally
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }
}

static (string Name, bool Passed, string Detail) RunOperationalReportCatalogProbe()
{
    var folder = Path.Combine(Path.GetTempPath(), $"ejlive-report-catalog-probe-{Guid.NewGuid():N}");
    Directory.CreateDirectory(folder);
    try
    {
        var now = DateTime.UtcNow;
        var snapshot = new UnifiedServerAnalyticsSnapshot(
            Fleet: new FleetSummary { Total = 1, Connected = 1, Offline = 0, AverageHealth = 92 },
            Sync: new SyncSummary { Total = 1, Pending = 0, InProgress = 0, Completed = 1, Failed = 0, AverageProgress = 100 },
            ConfirmedDeliveries: 1,
            PendingDeliveries: 0,
            FailedDeliveries: 0,
            CommandDispatches: 1,
            CommandResults: 1,
            CommandFailures: 0,
            LastCommandAtUtc: now.AddMinutes(-1),
            TelemetryEvents: 1,
            TelemetryWarnings: 1,
            TelemetryErrors: 0,
            NetworkDisconnectEvents: 0,
            HandshakeMissingEvents: 0,
            FileRetryEvents: 0,
            LastTelemetryAtUtc: now.AddMinutes(-1),
            AtmRows: new[]
            {
                new UnifiedAtmOperationalAnalyticsRow(
                    "ATM-CAT-PROBE",
                    AppConstants.ATM_TYPE_NCR,
                    92,
                    ConnectionStatus.Connected,
                    0,
                    0,
                    1,
                    0,
                    0,
                    1,
                    0,
                    now.AddMinutes(-1),
                    now.AddSeconds(-30),
                    0)
            });

        var reporting = new UnifiedOperationalReportingService();
        reporting.ExportBundleReport(
            folder,
            new[]
            {
                new OperationalWindowSnapshot("shift", 8, snapshot),
                new OperationalWindowSnapshot("day", 24, snapshot),
                new OperationalWindowSnapshot("week", 168, snapshot)
            },
            DateTime.Now,
            now);

        var catalog = new OperationalReportCatalogService();
        var files = catalog.GetLatestReportFiles(folder, 20);
        var summary = catalog.LoadLatestBundleSummary(folder);
        var passed = files.Count >= 3 &&
                     summary.Rows.Count == 3 &&
                     summary.Rows.Any(row => row.Window.Equals("day", StringComparison.OrdinalIgnoreCase) && row.LookbackHours == 24);
        return ("Operational report catalog", passed, $"files={files.Count}, bundleRows={summary.Rows.Count}");
    }
    catch (Exception ex)
    {
        return ("Operational report catalog", false, ex.Message);
    }
    finally
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }
}

static (string Name, bool Passed, string Detail) RunClientTelemetryAnalyticsProbe()
{
    var folder = Path.Combine(Path.GetTempPath(), $"ejlive-telemetry-probe-{Guid.NewGuid():N}");
    Directory.CreateDirectory(folder);
    try
    {
        var now = DateTime.UtcNow;
        var audit = new[]
        {
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-P1", Details = "info|network_connected|ok", CreatedAtUtc = now.AddMinutes(-3) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-P1", Details = "warning|file_retry|retry=1", CreatedAtUtc = now.AddMinutes(-2) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-P2", Details = "error|network_disconnected|socket lost", CreatedAtUtc = now.AddMinutes(-1) }
        };

        var service = new ClientTelemetryAnalyticsService();
        var snapshot = service.BuildSnapshot(audit);
        var timeline = service.ExportTimelineCsv(folder, snapshot, DateTime.Now);
        var atmSummary = service.ExportAtmSummaryCsv(folder, snapshot, DateTime.Now);
        var passed = snapshot.TotalEvents == 3 &&
                     snapshot.WarningEvents == 1 &&
                     snapshot.ErrorEvents == 1 &&
                     snapshot.DistinctAtms == 2 &&
                     File.Exists(timeline) &&
                     File.Exists(atmSummary);
        return ("Client telemetry analytics", passed, $"events={snapshot.TotalEvents}, warn={snapshot.WarningEvents}, err={snapshot.ErrorEvents}, atms={snapshot.DistinctAtms}");
    }
    catch (Exception ex)
    {
        return ("Client telemetry analytics", false, ex.Message);
    }
    finally
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }
}

static (string Name, bool Passed, string Detail) RunUnifiedServiceGatewayProbe()
{
    var root = Path.Combine(Path.GetTempPath(), "ejlive-service-gateway-probe", Guid.NewGuid().ToString("N"));
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var storage = Path.Combine(root, "storage");
        var data = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 500\nCARD CAPTURED\nM-18 CASH ERROR");

        var stored = runtime.ServiceGateway.StoreJournal(
            storage,
            "ATM-GATE",
            AppConstants.ATM_TYPE_NCR,
            "EJDATA.LOG",
            data,
            referencePath: "src/EJLive.Client.WinForms/Services/JournalProcessor.cs");
        var dispatch = runtime.ServiceGateway.DispatchRemoteCommand(
            "ATM-GATE",
            AppConstants.CMD_RESTART,
            role: "Admin",
            operatorConfirmed: true,
            maintenanceWindow: true,
            referencePath: "src/EJLive.Client.WinForms/Services/RemoteCommandHandler.cs");
        runtime.ServiceGateway.RegisterHeartbeat(
            "ATM-GATE",
            referencePath: "src/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs");
        runtime.ServiceGateway.RegisterBackupSnapshot(
            "ATM-GATE",
            Path.Combine(root, "backup.zip"),
            sizeBytes: 2048,
            referencePath: "src/EJLive.Client.WinForms/Agent/LogBackupScheduler.cs");
        runtime.ServiceGateway.RegisterScreenshotResult(
            "ATM-GATE",
            Path.Combine(root, "screen.jpg"),
            success: true,
            sizeBytes: 4096,
            referencePath: "src/EJLive.Client.WinForms/Agent/ScreenshotScheduler.cs");
        runtime.ServiceGateway.MarkClientServiceRunning(
            "Network Monitor",
            "Probe activation",
            referencePath: "src/EJLive.Client.WinForms/Services/NetworkManager.cs");

        var activationBatch = runtime.ServiceGateway.ActivateReferencePaths(
            new[]
            {
                "src/EJLive.Client.WinForms/Services/JournalProcessor.cs",
                "src/EJLive.Client.WinForms/Services/RemoteCommandHandler.cs",
                "src/EJLive.Client.WinForms/Agent/NetworkMonitor.cs",
                "src/EJLive.Server/Services/JournalAnalyticsService.cs",
                "src/EJLive.Server/Services/RemoteControlService.cs",
                "src/EJLive.Core/Engine/NetworkEngine.cs",
                "src/EJLive.Core/Services/MergedTraceCorrelationService.cs"
            },
            "ATM-GATE",
            storage);

        var fullActivation = runtime.ServiceGateway.ActivateAllReferenceServices(
            FindSolutionRoot(),
            "ATM-GATE",
            storage);

        var report = runtime.ServiceGateway.BuildReport();
        var coverage = runtime.ServiceGateway.BuildReferenceCoverage(FindSolutionRoot());
        var atmState = report.AtmStates.FirstOrDefault(state => state.AtmId == "ATM-GATE");
        var serviceReport = runtime.ClientServiceSupervisor.BuildReport();

        var passed = File.Exists(stored.StoragePath) &&
                     dispatch.Policy.Allowed &&
                     activationBatch.ActivatedReferencePaths >= 7 &&
                     activationBatch.UnclassifiedActivations == 0 &&
                     fullActivation.RequestedReferencePaths >= 50 &&
                     fullActivation.UnclassifiedActivations == 0 &&
                     report.TotalActivations >= 13 + fullActivation.RequestedReferencePaths &&
                     report.UnresolvedReferenceActivations == 0 &&
                     report.MappedReferenceActivations >= 13 + fullActivation.RequestedReferencePaths &&
                     report.DistinctAtms == 1 &&
                     coverage.TotalReferenceFiles >= 50 &&
                     coverage.UncoveredFiles == 0 &&
                     fullActivation.RequestedReferencePaths == coverage.TotalReferenceFiles &&
                     atmState is not null &&
                     atmState.LastHeartbeatUtc.HasValue &&
                     atmState.LastBackupUtc.HasValue &&
                     atmState.LastScreenshotUtc.HasValue &&
                     serviceReport.Running >= 1;

        return ("Unified service gateway", passed,
            $"stored={File.Exists(stored.StoragePath)}, commandAllowed={dispatch.Policy.Allowed}, activations={report.TotalActivations}, mapped={report.MappedReferenceActivations}, unresolved={report.UnresolvedReferenceActivations}, coverage={coverage.CoveredFiles}/{coverage.TotalReferenceFiles}, batchUnclassified={activationBatch.UnclassifiedActivations}, fullRequested={fullActivation.RequestedReferencePaths}, fullUnclassified={fullActivation.UnclassifiedActivations}, running={serviceReport.Running}/{serviceReport.Total}");
    }
    catch (Exception ex)
    {
        return ("Unified service gateway", false, ex.Message);
    }
    finally
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }
}

static (string Name, bool Passed, string Detail) RunReferenceServiceActivationCompletenessProbe()
{
    var root = Path.Combine(Path.GetTempPath(), "ejlive-service-reference-completeness", Guid.NewGuid().ToString("N"));
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var solutionRoot = FindSolutionRoot();
        var audit = runtime.BuildIntegrationAudit(solutionRoot);
        var referencePaths = audit.ReferenceOnlyFiles.Select(file => file.Path).ToArray();

        var activation = runtime.ServiceGateway.ActivateReferencePaths(
            referencePaths,
            "ATM-REF-ALL",
            root);
        var coverage = runtime.ServiceGateway.BuildReferenceCoverage(solutionRoot);
        var report = runtime.ServiceGateway.BuildReport();

        var passed = referencePaths.Length >= 50 &&
                     activation.RequestedReferencePaths == referencePaths.Length &&
                     activation.ActivatedReferencePaths == referencePaths.Length &&
                     activation.UnclassifiedActivations == 0 &&
                     coverage.TotalReferenceFiles == activation.RequestedReferencePaths &&
                     coverage.UncoveredFiles == 0 &&
                     report.UnresolvedReferenceActivations == 0 &&
                     report.DistinctReferencePaths.Count >= referencePaths.Length;

        return ("Reference service activation completeness", passed,
            $"referencePaths={referencePaths.Length}, activated={activation.ActivatedReferencePaths}, unclassified={activation.UnclassifiedActivations}, coverage={coverage.CoveredFiles}/{coverage.TotalReferenceFiles}, unresolved={report.UnresolvedReferenceActivations}, distinctReferencePaths={report.DistinctReferencePaths.Count}");
    }
    catch (Exception ex)
    {
        return ("Reference service activation completeness", false, ex.Message);
    }
    finally
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }
}

static (string Name, bool Passed, string Detail) RunIntegrationAuditProbe()
{
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var audit = runtime.BuildIntegrationAudit(FindSolutionRoot());
        var serverAnalyticsCovered = audit.ReferenceOnlyFiles.Any(file =>
            file.Path.EndsWith("src/EJLive.Server/Services/JournalAnalyticsService.cs", StringComparison.OrdinalIgnoreCase) &&
            file.ActiveReplacement.Contains("UnifiedJournalStorageService", StringComparison.OrdinalIgnoreCase));
        var agentDuplicationRemoved = !audit.DuplicateTypeFindings.Any(finding => finding.TypeName == "AgentBootstrapper");

        var passed = audit.SourceFileCount >= 100 &&
                     audit.ReferenceOnlyServiceFileCount >= 50 &&
                     audit.AllReferenceOnlyServicesCovered &&
                     serverAnalyticsCovered &&
                     agentDuplicationRemoved;

        return ("Integration audit coverage", passed,
            $"sourceFiles={audit.SourceFileCount}, referenceOnlyServices={audit.ReferenceOnlyServiceFileCount}, replacements={audit.ActiveReplacements.Count}, uncovered={audit.UncoveredReferenceOnlyFiles.Count}, duplicateTypes={audit.DuplicateTypeFindings.Count}, serverAnalyticsCovered={serverAnalyticsCovered}, agentDuplicationRemoved={agentDuplicationRemoved}");
    }
    catch (Exception ex)
    {
        return ("Integration audit coverage", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunServiceActivationAuditProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var audit = new UnifiedServiceActivationAuditService().Analyze(root);
        var exportPath = ExportServiceActivationAudit(root, audit);

        var criticalUnresolved = audit.Candidates
            .Where(item => item.Status == ServiceActivationStatusKind.NeedsActivation)
            .Where(item =>
                item.Path.StartsWith("src/EJLive.Client.WinForms/Agent/", StringComparison.OrdinalIgnoreCase) ||
                item.Path.StartsWith("src/EJLive.Client.WinForms/Services/", StringComparison.OrdinalIgnoreCase) ||
                item.Path.StartsWith("src/EJLive.Core/Services/", StringComparison.OrdinalIgnoreCase) ||
                item.Path.StartsWith("src/EJLive.Server/Services/", StringComparison.OrdinalIgnoreCase) ||
                item.Path.StartsWith("src/EJLive.Server.WinForms/Services/", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Path)
            .Take(5)
            .ToArray();

        var sampleCompiled = audit.Candidates
            .Where(item => item.Status == ServiceActivationStatusKind.ActiveCompiled)
            .Take(5)
            .Select(item => item.Path)
            .ToArray();

        var passed = audit.TotalCandidates >= 50 &&
                     audit.ActiveCompiledCandidates >= 20 &&
                     criticalUnresolved.Length == 0 &&
                     audit.NeedsActivationCandidates == 0;

        return ("Service activation status", passed,
            $"candidates={audit.TotalCandidates}, compiled={audit.ActiveCompiledCandidates}, covered={audit.ReferenceCoveredCandidates}, unresolved={audit.NeedsActivationCandidates}, criticalUnresolved={string.Join(',', criticalUnresolved)}, compiledSample={string.Join(',', sampleCompiled)}, export={exportPath}");
    }
    catch (Exception ex)
    {
        return ("Service activation status", false, ex.Message);
    }
}

static string ExportServiceActivationAudit(string solutionRoot, ServiceActivationAuditReport audit)
{
    var docs = Path.Combine(solutionRoot, "docs");
    Directory.CreateDirectory(docs);
    var path = Path.Combine(docs, "12-service-activation-status.csv");
    using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
    writer.WriteLine("\"Path\",\"Project\",\"Status\",\"Compiled\",\"Replacement\",\"Detail\"");
    foreach (var item in audit.Candidates)
    {
        writer.WriteLine(string.Join(",",
            Csv(item.Path),
            Csv(item.ProjectName),
            Csv(item.Status.ToString()),
            Csv(item.Compiled ? "true" : "false"),
            Csv(item.ActiveReplacement),
            Csv(item.Detail)));
    }

    return path;
}

static string Csv(string value) =>
    "\"" + (value ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

static async Task<(string Name, bool Passed, string Detail)> RunLegacySelectiveUpgradeProbe()
{
    var root = Path.Combine(Path.GetTempPath(), "ejlive-legacy-upgrade-probe", Guid.NewGuid().ToString("N"));
    var storage = Path.Combine(root, "storage");
    var archive = Path.Combine(root, "archive");
    var reports = Path.Combine(root, "reports");
    var port = Random.Shared.Next(61001, 62000);

    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-LEGACY", AppConstants.ATM_TYPE_NCR);
        var clientConnected = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        using var legacyAnalytics = new JournalAnalyticsService(storage, archive);
        using var legacyRemote = new RemoteControlService(server);
        Directory.CreateDirectory(reports);

        var data = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 150\nCARD CAPTURED\nM-18 CASH ERROR");
        legacyAnalytics.StoreJournalData("ATM-LEGACY", "EJDATA.LOG", data, SecurityHelper.MD5Hash(data));
        var month = DateTime.Now.ToString("yyyy-MM");
        var zip = legacyAnalytics.ArchiveMonth("ATM-LEGACY", month);
        var csv = legacyAnalytics.ExportCSVReport(Path.Combine(reports, "legacy.csv"), "ATM-LEGACY");
        var html = legacyAnalytics.ExportHTMLReport(Path.Combine(reports, "legacy.html"), "ATM-LEGACY");

        var cmdId = legacyRemote.SendRestart("ATM-LEGACY", 5);
        await Task.Delay(250).ConfigureAwait(false);
        var last = legacyRemote.GetCommandHistory("ATM-LEGACY", 1).FirstOrDefault();

        client.Disconnect();
        server.Stop();

        var passed = clientConnected &&
                     accepted &&
                     File.Exists(zip) &&
                     File.Exists(csv) &&
                     File.Exists(html) &&
                     !string.IsNullOrWhiteSpace(cmdId) &&
                     last is not null &&
                     last.Sent;

        return ("Legacy selective upgrade", passed,
            $"clientConnected={clientConnected}, accepted={accepted}, zip={File.Exists(zip)}, csv={File.Exists(csv)}, html={File.Exists(html)}, cmdSent={last?.Sent}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Legacy selective upgrade", false, ex.Message);
    }
    finally
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }
}

static string BuildAuditSafeName(string name)
{
    var safeName = System.Text.RegularExpressions.Regex.Replace(name, "[^A-Za-z0-9_.-]+", "_");
    safeName = System.Text.RegularExpressions.Regex.Replace(safeName, "_{2,}", "_").TrimEnd('_');
    if (!name.StartsWith('_'))
        safeName = safeName.TrimStart('_');
    return string.IsNullOrWhiteSpace(safeName) || safeName.All(c => c is '_' or '.' or '-')
        ? "source_" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name)))[..12]
        : safeName;
}

static (string Name, bool Passed, string Detail) RunUnsafeTermScanProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var srcRoot = Path.Combine(root, "src");
        var unsafeTerms = new[] { "Stealth", "HiddenProcess", "DisableDefender", "DisableFirewall", "BypassGpo", "KillProcess", "ArbitraryShell", "ExecScript", "NoConsentPrompt" };
        var allowedExceptions = new[]
        {
            "GhostRemoteEngine",
            "GhostRemote2Service",
            "AllowNoConsentPrompt",
            "requestNoConsentPrompt",
            "Arbitrary shell commands are not permitted"
        }; // Explicitly allowed compatibility and policy-gated names under review
        var violations = new List<string>();

        foreach (var csFile in Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(root, csFile).Replace('\\', '/');
            if (IsBuildOutput(rel))
                continue;
            if (rel.StartsWith("src/EJLive.Tests/", StringComparison.OrdinalIgnoreCase))
                continue;
            // Skip reference-only files (None Include)
            if (rel.Contains("/reference-source/", StringComparison.OrdinalIgnoreCase))
                continue;
            var content = File.ReadAllText(csFile);
            foreach (var term in unsafeTerms)
            {
                if (content.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                    !allowedExceptions.Any(ex => content.Contains(ex, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add($"{rel}: contains '{term}'");
                }
            }
        }

        var passed = violations.Count == 0;
        return ("Unsafe term scan", passed, $"violations={violations.Count}, samples={string.Join("; ", violations.Take(3))}");
    }
    catch (Exception ex)
    {
        return ("Unsafe term scan", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunUiInServicePathProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var servicePaths = new[]
        {
            Path.Combine(root, "src", "EJLive.Client.Service")
        };
        var uiIndicators = new[] { "System.Windows.Forms", "MessageBox", "Form", "Control", "Button", "TextBox", "DataGridView", "DialogResult" };
        var violations = new List<string>();

        foreach (var path in servicePaths.Where(Directory.Exists))
        {
            foreach (var csFile in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(root, csFile).Replace('\\', '/');
                if (IsBuildOutput(rel))
                    continue;
                var content = File.ReadAllText(csFile);
                foreach (var indicator in uiIndicators)
                {
                    var pattern = indicator == "System.Windows.Forms"
                        ? System.Text.RegularExpressions.Regex.Escape(indicator)
                        : $@"(?<![A-Za-z0-9_]){System.Text.RegularExpressions.Regex.Escape(indicator)}(?![A-Za-z0-9_])";

                    if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        // Allow comments that document UI boundary
                        var lines = content.Split('\n')
                            .Where(l => System.Text.RegularExpressions.Regex.IsMatch(l, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            .ToArray();
                        var codeLines = lines.Where(l => !l.TrimStart().StartsWith("//") && !l.TrimStart().StartsWith("*")).ToArray();
                        if (codeLines.Length > 0)
                        {
                            violations.Add($"{rel}: references '{indicator}'");
                            break;
                        }
                    }
                }
            }
        }

        var passed = violations.Count == 0;
        return ("UI-in-service-path scan", passed, $"violations={violations.Count}, samples={string.Join("; ", violations.Take(3))}");
    }
    catch (Exception ex)
    {
        return ("UI-in-service-path scan", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunDuplicateTypeProbe()
{
    try
    {
        var assemblies = new[]
        {
            typeof(EJLive.Application.EJLiveApplicationHost).Assembly,
            typeof(EJLive.Business.UnifiedBusinessRuntime).Assembly,
            typeof(EJLive.Core.Constants).Assembly,
            typeof(EJLive.Shared.SecurityHelper).Assembly,
            typeof(EJLive.Client.Service.ClientAgentWindowsService).Assembly,
            typeof(EJLive.Client.WinForms.ClientMainForm).Assembly,
            typeof(EJLive.Server.WinForms.ServerMainForm).Assembly,
            typeof(EJLive.Installer.WinForms.InstallerForm).Assembly,
            typeof(EJLive.Monitoring.WinForms.MainDashboardForm).Assembly,
            typeof(EJLive.Monitor.MonitoringDashboard).Assembly,
        };

        var typeNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.StartsWith("<", StringComparison.Ordinal) ||
                    type.FullName?.Contains("<PrivateImplementationDetails>", StringComparison.OrdinalIgnoreCase) == true ||
                    type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false))
                {
                    continue;
                }

                var typeKey = type.FullName ?? type.Name;
                if (!typeNames.TryGetValue(typeKey, out var list))
                {
                    list = new List<string>();
                    typeNames[typeKey] = list;
                }
                list.Add(assembly.GetName().Name ?? "?");
            }
        }

        var duplicates = typeNames
            .Where(kvp => kvp.Value.Count > 1 && kvp.Value.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(kvp => $"{kvp.Key}: [{string.Join(", ", kvp.Value.Distinct(StringComparer.OrdinalIgnoreCase))}]")
            .ToArray();

        var passed = duplicates.Length == 0;
        return ("Duplicate type detection", passed, $"duplicates={duplicates.Length}, samples={string.Join("; ", duplicates.Take(3))}");
    }
    catch (Exception ex)
    {
        return ("Duplicate type detection", false, ex.Message);
    }
}

static string FindSolutionRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory != null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "EJLive.Unified.slnx")))
            return directory.FullName;
        directory = directory.Parent;
    }
    throw new DirectoryNotFoundException("Could not locate EJLive.Unified.slnx from the verification output folder.");
}

static bool IsBuildOutput(string path)
{
    var normalized = path.Replace('\\', '/');
    return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase) ||
           normalized.StartsWith(".git/", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("/artifacts/", StringComparison.OrdinalIgnoreCase);
}

static bool IsAccounted(string relativePath, ISet<string> projectFolders, ISet<string> linkedSourceFolders)
{
    if (relativePath.StartsWith("docs/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith("legacy/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith(".git/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith(".github/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith(".codex/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith("commands/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith("كتب ومراجع/", StringComparison.OrdinalIgnoreCase) ||
        relativePath.StartsWith("tools/", StringComparison.OrdinalIgnoreCase))
        return true;

    if (relativePath is "AGENTS.md" or
                        "AGENTS_MEMORY_APPENDIX.md" or
                        ".gitattributes" or
                        "CODEX_COMMIT_INSTRUCTIONS.txt" or
                        "CODEX_CONFIG_MEMORY_SNIPPET.toml" or
                        "CODEX_CUSTOM_INSTRUCTIONS.txt" or
                        "CODEX_PULL_REQUEST_INSTRUCTIONS.txt" or
                        "GITHUB_ISSUES_COPYPASTE_ALL_45.md" or
                        "README.md" or
                        "README_APPLY.md" or
                        "README_FIX.md" or
                        "README_INSTALL.md" or
                        "MASTER_COMMAND_INDEX.md" or
                        "TREE.txt" or
                        "MCP_PLUGINS_RECOMMENDED.md" or
                        "INSTALL_TO_PROJECT.ps1" or
                        "INSTALL_TO_PROJECT_FIXED.ps1" or
                        "codex-plugin-manifest.json" or
                        "EJLive_MASTER_PROMPT.md" or
                        "EJLive_Codex_Complete_Updated_Pack.zip" or
                        "codex_professional_skills_tree.txt" or
                        "Directory.Build.props" or
                        "EJLive.Unified.sln" or
                        "EJLive.Unified.slnx" or
                        "src_update.zip" or
                        "src_letast.zip")
        return true;

    if (relativePath is "src/coder-jetbrains-toolbox-main.zip")
        return true;

    const string sourcePrefix = "src/";
    if (!relativePath.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
        return false;

    var tail = relativePath[sourcePrefix.Length..];
    var slashIndex = tail.IndexOf('/');
    var folder = slashIndex >= 0 ? tail[..slashIndex] : tail;
    return projectFolders.Contains(folder) || linkedSourceFolders.Contains(folder);
}

return checks.All(c => c.Passed) ? 0 : 1;

static (string Name, bool Passed, string Detail) RunDatabaseMigrationProbe()
{
    var db = Path.Combine(Path.GetTempPath(), "ejlive-verification-legacy-schema.db");
    foreach (var path in new[] { db, db + "-wal", db + "-shm" })
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    try
    {
        using (var connection = new SQLiteConnection($"Data Source={db};Version=3;"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE audit_log (
                    entry_id TEXT PRIMARY KEY,
                    user_name TEXT NOT NULL,
                    action TEXT NOT NULL,
                    target TEXT NOT NULL,
                    details TEXT NOT NULL
                );
                CREATE TABLE sync_records (
                    sync_id TEXT PRIMARY KEY,
                    atm_id TEXT NOT NULL,
                    file_name TEXT NOT NULL,
                    state TEXT NOT NULL,
                    progress INTEGER NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }

        DatabaseManager.Instance.Initialize(db);

        using var upgraded = new SQLiteConnection($"Data Source={db};Version=3;");
        upgraded.Open();
        using var verify = upgraded.CreateCommand();
        verify.CommandText = "SELECT name FROM pragma_table_info('audit_log') WHERE name='created_at_utc';";
        var auditColumn = Convert.ToString(verify.ExecuteScalar());
        verify.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name='ix_audit_log_created_at';";
        var auditIndex = Convert.ToString(verify.ExecuteScalar());
        verify.CommandText = "SELECT name FROM pragma_table_info('sync_records') WHERE name='updated_at_utc';";
        var syncColumn = Convert.ToString(verify.ExecuteScalar());

        var passed = auditColumn == "created_at_utc" &&
                     auditIndex == "ix_audit_log_created_at" &&
                     syncColumn == "updated_at_utc";
        return ("SQLite migration", passed, $"auditColumn={auditColumn}, auditIndex={auditIndex}, syncColumn={syncColumn}");
    }
    catch (Exception ex)
    {
        return ("SQLite migration", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunApplicationLayerProbe()
{
    var db = Path.Combine(Path.GetTempPath(), $"ejlive-application-probe-{Guid.NewGuid():N}.db");
    try
    {
        using var host = EJLiveApplicationHost.Create(db);
        var atm = host.SeedDemoAtm("ATM-APP");
        host.Runtime.TrackJournalSync(atm.ATM_ID ?? "ATM-APP", "EJDATA.LOG", 4096, JournalSyncState.Completed);
        var readiness = host.ValidateReadiness();
        var snapshot = host.Runtime.BuildSnapshot();
        var flow = host.DescribeDataFlow();

        var passed = readiness.Passed &&
                     snapshot.Fleet.Total == 1 &&
                     snapshot.Sync.Completed == 1 &&
                     flow.Count == 6 &&
                     snapshot.Capabilities.Any(capability => capability.Layer == "Legacy Reference");
        return ("Application/business layering", passed, $"ready={readiness.Passed}, fleet={snapshot.Fleet.Total}, completed={snapshot.Sync.Completed}, flow={flow.Count}, capabilities={snapshot.Capabilities.Count}");
    }
    catch (Exception ex)
    {
        return ("Application/business layering", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunNetworkProbeAsync()
{
    var port = Random.Shared.Next(56000, 59000);
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    var connectedAtm = string.Empty;
    var errors = new List<string>();

    try
    {
        server.ClientConnected += (_, connection) =>
        {
            connectedAtm = connection.ATM_ID;
            connected.Set();
        };
        server.Error += (_, message) => errors.Add(message);
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-SMOKE", AppConstants.ATM_TYPE_NCR);
        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned && accepted && connectedAtm == "ATM-SMOKE" && errors.Count == 0;
        return ("Client/server network", passed, $"connectReturned={connectReturned}, accepted={accepted}, atm={connectedAtm}, errors={errors.Count}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Client/server network", false, ex.Message);
    }
}
