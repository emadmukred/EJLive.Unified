using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class UnifiedJournalStorageService
{
    private readonly UnifiedJournalEvidenceAnalyzer _analyzer;

    public UnifiedJournalStorageService(UnifiedJournalEvidenceAnalyzer? analyzer = null)
    {
        _analyzer = analyzer ?? new UnifiedJournalEvidenceAnalyzer();
    }

    public JournalStorageResult StoreJournalData(
        string storagePath,
        string atmId,
        string atmType,
        string fileName,
        byte[] data,
        string checksum = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(data);

        var monthFolder = Path.Combine(storagePath, SafePath(atmId), DateTime.UtcNow.ToString("yyyy-MM"));
        Directory.CreateDirectory(monthFolder);

        var fullPath = Path.Combine(monthFolder, SafePath(fileName));
        File.WriteAllBytes(fullPath, data);

        var text = DecodeJournalText(data);
        var evidence = _analyzer.Analyze(atmId, atmType, text);

        return new JournalStorageResult(
            atmId,
            fileName,
            fullPath,
            data.LongLength,
            string.IsNullOrWhiteSpace(checksum) ? EJLive.Shared.SecurityHelper.MD5Hash(data) : checksum,
            DateTime.UtcNow,
            evidence);
    }

    public string ArchiveMonth(string storagePath, string archivePath, string atmId, string yearMonth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(yearMonth);

        var source = Path.Combine(storagePath, SafePath(atmId), yearMonth);
        if (!Directory.Exists(source))
            return string.Empty;

        Directory.CreateDirectory(archivePath);
        var zipPath = Path.Combine(archivePath, $"{SafePath(atmId)}_{yearMonth.Replace("-", string.Empty, StringComparison.Ordinal)}.zip");
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(source, zipPath);
        return zipPath;
    }

    public string ExportCsvReport(IEnumerable<JournalStorageResult> records, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("ATM_ID,FileName,FileSize,ReceivedAtUtc,Checksum,Approved,Declined,CapturedCards,CashErrors,TotalCash");
        foreach (var record in records)
        {
            writer.WriteLine(string.Join(",",
                Csv(record.ATM_ID),
                Csv(record.FileName),
                record.FileSize,
                Csv(record.ReceivedAtUtc.ToString("O")),
                Csv(record.Checksum),
                record.Evidence.ApprovedTransactions,
                record.Evidence.DeclinedTransactions,
                record.Evidence.CapturedCards,
                record.Evidence.CashErrorEvents,
                record.Evidence.TotalCashDispensed));
        }

        return filePath;
    }

    public string ExportHtmlReport(IEnumerable<JournalStorageResult> records, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        var rows = records.ToArray();
        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html><head><meta charset=\"utf-8\"><title>EJLive Journal Report</title></head><body>");
        sb.AppendLine("<h1>EJLive Journal Report</h1>");
        sb.AppendLine($"<p>Files: {rows.Length} | Approved: {rows.Sum(r => r.Evidence.ApprovedTransactions)} | Errors: {rows.Sum(r => r.Evidence.CashErrorEvents)}</p>");
        sb.AppendLine("<table><thead><tr><th>ATM</th><th>File</th><th>Size</th><th>Approved</th><th>Cash Errors</th></tr></thead><tbody>");
        foreach (var record in rows)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{Html(record.ATM_ID)}</td>");
            sb.Append($"<td>{Html(record.FileName)}</td>");
            sb.Append($"<td>{record.FileSize}</td>");
            sb.Append($"<td>{record.Evidence.ApprovedTransactions}</td>");
            sb.Append($"<td>{record.Evidence.CashErrorEvents}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table></body></html>");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }

    private static string DecodeJournalText(byte[] data)
    {
        try
        {
            return Encoding.UTF8.GetString(data);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Default.GetString(data);
        }
    }

    private static string SafePath(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }

    private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string Html(string value)
    {
        return (value ?? string.Empty)
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal);
    }
}

public sealed class UnifiedJournalRoutingService
{
    private readonly UnifiedJournalStorageService _journalStorage;
    private readonly ConcurrentDictionary<string, JournalDeliveryReceipt> _receipts = new(StringComparer.OrdinalIgnoreCase);

    public UnifiedJournalRoutingService(UnifiedJournalStorageService? journalStorage = null)
    {
        _journalStorage = journalStorage ?? new UnifiedJournalStorageService();
    }

    public IReadOnlyList<JournalDeliveryReceipt> Receipts =>
        _receipts.Values
            .OrderByDescending(item => item.ReceivedAtUtc)
            .ToArray();

    public JournalDeliveryReceipt RegisterPending(
        string transferId,
        string atmId,
        string atmType,
        string fileName,
        long expectedBytes,
        string checksum = "",
        string routeHint = "")
    {
        var effectiveTransferId = NormalizeTransferId(transferId, atmId, fileName);
        var category = ResolveCategory(fileName, routeHint);
        var now = DateTime.UtcNow;

        var receipt = _receipts.AddOrUpdate(
            effectiveTransferId,
            _ => new JournalDeliveryReceipt(
                effectiveTransferId,
                SafePath(atmId),
                SafePath(atmType),
                SafePath(fileName),
                category,
                string.Empty,
                Math.Max(0, expectedBytes),
                checksum ?? string.Empty,
                now,
                false,
                "Pending transfer started."),
            (_, existing) => existing with
            {
                ATM_ID = SafePath(atmId),
                ATM_Type = SafePath(atmType),
                FileName = SafePath(fileName),
                Category = category,
                FileSize = Math.Max(existing.FileSize, expectedBytes),
                Checksum = string.IsNullOrWhiteSpace(checksum) ? existing.Checksum : checksum,
                ReceivedAtUtc = now,
                Confirmed = false,
                Detail = "Pending transfer updated."
            });

        return receipt;
    }

    public JournalDeliveryReceipt RegisterFailed(string transferId, string detail)
    {
        var effectiveTransferId = NormalizeTransferId(transferId, "UNKNOWN", "unknown.bin");
        var now = DateTime.UtcNow;

        return _receipts.AddOrUpdate(
            effectiveTransferId,
            _ => new JournalDeliveryReceipt(
                effectiveTransferId,
                "UNKNOWN",
                "UNKNOWN",
                "unknown.bin",
                "unknown",
                string.Empty,
                0,
                string.Empty,
                now,
                false,
                string.IsNullOrWhiteSpace(detail) ? "Transfer failed." : detail),
            (_, existing) => existing with
            {
                ReceivedAtUtc = now,
                Confirmed = false,
                Detail = string.IsNullOrWhiteSpace(detail) ? "Transfer failed." : detail
            });
    }

    public JournalDeliveryReceipt StoreInbound(
        string storageRoot,
        string atmId,
        string atmType,
        string fileName,
        byte[] data,
        string checksum = "",
        string transferId = "",
        string routeHint = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(data);

        var effectiveTransferId = NormalizeTransferId(transferId, atmId, fileName);
        var safeAtmId = SafePath(atmId);
        var safeAtmType = SafePath(string.IsNullOrWhiteSpace(atmType) ? "UNKNOWN" : atmType);
        var safeFileName = SafePath(fileName);
        var category = ResolveCategory(fileName, routeHint);
        var now = DateTime.UtcNow;
        var checksumValue = string.IsNullOrWhiteSpace(checksum)
            ? EJLive.Shared.SecurityHelper.MD5Hash(data)
            : checksum;
        var bytes = Math.Max(0, data.LongLength);
        string storagePath;

        if (string.Equals(category, "journals", StringComparison.OrdinalIgnoreCase))
        {
            var result = _journalStorage.StoreJournalData(
                Path.Combine(storageRoot, safeAtmType),
                safeAtmId,
                safeAtmType,
                safeFileName,
                data,
                checksumValue);
            storagePath = result.StoragePath;
        }
        else
        {
            var folder = Path.Combine(
                storageRoot,
                safeAtmType,
                safeAtmId,
                category,
                now.ToString("yyyy-MM"),
                now.ToString("dd"));
            Directory.CreateDirectory(folder);
            storagePath = Path.Combine(folder, safeFileName);
            File.WriteAllBytes(storagePath, data);
        }

        var detail = $"Stored in {category} partition.";
        var receipt = new JournalDeliveryReceipt(
            effectiveTransferId,
            safeAtmId,
            safeAtmType,
            safeFileName,
            category,
            storagePath,
            bytes,
            checksumValue,
            now,
            true,
            detail);

        _receipts[effectiveTransferId] = receipt;
        return receipt;
    }

    public IReadOnlyList<JournalDeliveryReceipt> FindPending(TimeSpan olderThan)
    {
        var threshold = DateTime.UtcNow - olderThan;
        return Receipts
            .Where(item => !item.Confirmed && item.ReceivedAtUtc <= threshold)
            .ToArray();
    }

    public IReadOnlyList<JournalDeliveryReceipt> FindRecentFailures(TimeSpan window)
    {
        var threshold = DateTime.UtcNow - window;
        return Receipts
            .Where(item => !item.Confirmed &&
                           item.ReceivedAtUtc >= threshold &&
                           item.Detail.Contains("fail", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string NormalizeTransferId(string transferId, string atmId, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(transferId))
            return transferId.Trim();
        return $"{SafePath(atmId)}:{SafePath(fileName)}:{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static string ResolveCategory(string fileName, string routeHint)
    {
        var hint = (routeHint ?? string.Empty).Trim().ToLowerInvariant();
        if (hint.Contains("screenshot", StringComparison.Ordinal) || hint.Contains("screen", StringComparison.Ordinal))
            return "screenshots";
        if (hint.Contains("image", StringComparison.Ordinal) || hint.Contains("photo", StringComparison.Ordinal))
            return "images";
        if (hint.Contains("backup", StringComparison.Ordinal) || hint.Contains("archive", StringComparison.Ordinal))
            return "backups";
        if (hint.Contains("journal", StringComparison.Ordinal) || hint.Contains("log", StringComparison.Ordinal))
            return "journals";

        var name = (fileName ?? string.Empty).Trim();
        if (name.StartsWith("SCR_", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
        {
            return "screenshots";
        }

        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return "backups";

        if (name.Contains("EJDATA", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".log", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".jrn", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".ej", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            return "journals";
        }

        return "files";
    }

    private static string SafePath(string value)
    {
        var source = string.IsNullOrWhiteSpace(value) ? "UNKNOWN" : value.Trim();
        var invalid = Path.GetInvalidFileNameChars();
        var chars = source.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }
}

public sealed class UnifiedRemoteCommandOrchestrator
{
    private readonly UnifiedRemoteCommandPolicy _policy;
    private readonly ConcurrentDictionary<string, RemoteCommandDispatch> _history = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<ScheduledRemoteCommand> _schedule = new();

    public UnifiedRemoteCommandOrchestrator(UnifiedRemoteCommandPolicy? policy = null)
    {
        _policy = policy ?? new UnifiedRemoteCommandPolicy();
    }

    public IReadOnlyList<RemoteCommandDispatch> History =>
        _history.Values.OrderByDescending(item => item.Command.CreatedAtUtc).ToArray();

    public IReadOnlyList<ScheduledRemoteCommand> Scheduled => _schedule.ToArray();

    public RemoteCommandDispatch Queue(
        string atmId,
        string commandType,
        string payload = "",
        string role = "Admin",
        bool operatorConfirmed = true,
        bool maintenanceWindow = true)
    {
        var command = new RemoteCommand
        {
            ATM_ID = atmId,
            CommandType = commandType,
            Payload = payload,
            RequiresConfirmation = AppConstants.CommandsRequireConfirmation.Contains(commandType, StringComparer.OrdinalIgnoreCase)
        };

        var decision = _policy.Evaluate(command, role, operatorConfirmed, maintenanceWindow);
        command.Status = decision.Allowed ? RemoteCommandStatus.Sent : RemoteCommandStatus.Failed;
        command.SentAtUtc = decision.Allowed ? DateTime.UtcNow : null;
        command.Result = decision.Allowed ? "Queued for transport." : decision.Reason;

        var dispatch = new RemoteCommandDispatch(command, decision, DateTime.UtcNow);
        _history[command.CommandId] = dispatch;
        return dispatch;
    }

    public ScheduledRemoteCommand Schedule(
        string atmId,
        string commandType,
        DateTime executeAtUtc,
        string payload = "",
        string role = "Admin")
    {
        var scheduled = new ScheduledRemoteCommand(
            Guid.NewGuid().ToString("N"),
            atmId,
            commandType,
            payload,
            role,
            executeAtUtc,
            DateTime.UtcNow);
        _schedule.Enqueue(scheduled);
        return scheduled;
    }

    public bool Complete(string commandId, bool success, string result)
    {
        if (!_history.TryGetValue(commandId, out var dispatch))
            return false;

        dispatch.Command.CompletedAtUtc = DateTime.UtcNow;
        dispatch.Command.Status = success ? RemoteCommandStatus.Completed : RemoteCommandStatus.Failed;
        dispatch.Command.Result = result;
        return true;
    }
}

public sealed class UnifiedClientServiceSupervisor
{
    private readonly ConcurrentDictionary<string, ClientServiceState> _states = new(StringComparer.OrdinalIgnoreCase);

    public UnifiedClientServiceSupervisor()
    {
        foreach (var service in DefaultServices)
            _states[service] = new ClientServiceState(service, ClientServiceStatus.Stopped, "Ready", DateTime.UtcNow);
    }

    public IReadOnlyList<ClientServiceState> Services =>
        _states.Values.OrderBy(service => service.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    public ClientServiceState Start(string serviceName, string detail = "Running")
    {
        return Update(serviceName, ClientServiceStatus.Running, detail);
    }

    public ClientServiceState Stop(string serviceName, string detail = "Stopped")
    {
        return Update(serviceName, ClientServiceStatus.Stopped, detail);
    }

    public ClientServiceState MarkFaulted(string serviceName, string detail)
    {
        return Update(serviceName, ClientServiceStatus.Faulted, detail);
    }

    public ClientServiceSupervisorReport BuildReport()
    {
        var services = Services;
        return new ClientServiceSupervisorReport(
            services.Count,
            services.Count(service => service.Status == ClientServiceStatus.Running),
            services.Count(service => service.Status == ClientServiceStatus.Faulted),
            services);
    }

    private ClientServiceState Update(string serviceName, ClientServiceStatus status, string detail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);
        var state = new ClientServiceState(serviceName.Trim(), status, detail, DateTime.UtcNow);
        _states[serviceName] = state;
        return state;
    }

    private static readonly string[] DefaultServices =
    [
        "Agent Controller",
        "File Watcher",
        "Socket Data",
        "Socket Files",
        "Screenshot",
        "Ghost Access",
        "Windows Startup",
        "Supabase Sync",
        "Network Monitor",
        "Time Sync",
        "Log Backup",
        "Journal Sync"
    ];
}

public sealed class UnifiedProjectIntegrationAuditService
{
    private static readonly Regex TypePattern = new(
        @"(?m)^\s*(?:public|internal|private|protected)?\s*(?:sealed|static|abstract|partial)?\s*(?:class|record|interface|enum)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    private static readonly ActiveServiceReplacement[] ReplacementMap =
    [
        new("src/EJLive.Client.WinForms/Agent/", "UnifiedClientServiceSupervisor", "Agent, tray, boot, network monitor, screenshot, time sync, and backup functions are active through the unified supervisor and current client runtime."),
        new("src/EJLive.Client.WinForms/Services/Advanced", "NetworkEngine + UnifiedRemoteCommandOrchestrator", "Advanced client network and command behavior is active through the unified network/command services."),
        new("src/EJLive.Client.WinForms/Services/Journal", "JournalSyncService + JournalOutbox", "Journal processing and sync agent behavior is active through the compiled sync service and outbox."),
        new("src/EJLive.Client.WinForms/Services/Network", "NetworkEngine", "Client networking is active through the compiled core NetworkEngine."),
        new("src/EJLive.Client.WinForms/Services/Remote", "UnifiedRemoteCommandOrchestrator", "Remote command behavior is active through policy and orchestration services."),
        new("src/EJLive.Client.WinForms/Services/CashTelemetryService.cs", "UnifiedJournalEvidenceAnalyzer", "Cash and transaction telemetry is active through journal evidence analysis."),
        new("src/EJLive.Client.WinForms/Services/JournalProcessor.cs", "UnifiedJournalEvidenceAnalyzer", "Journal parsing behavior is active through the unified analyzer."),
        new("src/EJLive.Client.WinForms/Services/UIBinder.cs", "ClientMainForm", "UI binding is active in the rebuilt WinForms main form."),
        new("src/EJLive.Server/Services/", "UnifiedJournalStorageService + UnifiedRemoteCommandOrchestrator", "Old server services are active through compiled storage, archive, report, and command orchestration."),
        new("src/EJLive.Server.WinForms/Services/", "ServerMainForm + ServerEngine + UnifiedJournalStorageService", "Server UI and service behavior is active in the rebuilt server runtime and unified services."),
        new("src/EJLive.Core/Services/", "CoreServices + UnifiedOperationalFusion + UnifiedServiceOperations + UnifiedServiceGateway", "Core service variants are consolidated into compiled service modules with an active unified gateway bridge."),
        new("src/EJLive.Core/Engine/", "OperationalEngines + NetworkEngine + CommunicationProtocol", "Engine variants are consolidated into compiled operational engines."),
        new("src/EJLive.Core/Xfs/", "XfsModels + UnifiedJournalEvidenceAnalyzer", "XFS adapters are represented by compiled normalized adapters and journal evidence detection."),
        new("src/EJLive.Core/Models/", "UnifiedModels", "Model variants are consolidated into the compiled unified model surface."),
        new("src/EJLive.Shared/", "AppLogger + SecurityHelper + DateTimeHelper + RetryPolicy", "Shared legacy helpers are represented by compiled shared utilities.")
    ];

    public ProjectIntegrationAuditReport Analyze(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        var root = Path.GetFullPath(rootPath);
        var sourceFiles = Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(path => !IsBuildOutput(path))
                .Select(path => Normalize(Path.GetRelativePath(root, path)))
                .Where(path => path.StartsWith("src/", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        var referenceOnly = sourceFiles
            .Where(IsReferenceOnlyServiceCandidate)
            .Select(path => new ReferenceOnlyServiceFile(path, FindReplacement(path)?.ActiveService ?? string.Empty))
            .ToArray();

        var uncovered = referenceOnly
            .Where(file => string.IsNullOrWhiteSpace(file.ActiveReplacement))
            .ToArray();

        var duplicateTypes = BuildDuplicateTypeFindings(root, referenceOnly.Select(file => file.Path));

        return new ProjectIntegrationAuditReport(
            root,
            sourceFiles.Length,
            referenceOnly.Length,
            ReplacementMap,
            referenceOnly,
            duplicateTypes,
            uncovered);
    }

    private static IReadOnlyList<DuplicateTypeFinding> BuildDuplicateTypeFindings(string root, IEnumerable<string> relativePaths)
    {
        var groups = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var relativePath in relativePaths)
        {
            var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                continue;

            var text = File.ReadAllText(path);
            foreach (Match match in TypePattern.Matches(text))
            {
                var typeName = match.Groups["name"].Value;
                if (!groups.TryGetValue(typeName, out var files))
                {
                    files = new List<string>();
                    groups[typeName] = files;
                }
                files.Add(relativePath);
            }
        }

        return groups
            .Where(pair => pair.Value.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(pair => new DuplicateTypeFinding(pair.Key, pair.Value.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()))
            .OrderBy(finding => finding.TypeName, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsReferenceOnlyServiceCandidate(string path)
    {
        if (path.StartsWith("src/EJLive.Client.WinForms/Agent/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (path.StartsWith("src/EJLive.Client.WinForms/Services/", StringComparison.OrdinalIgnoreCase))
        {
            return !path.EndsWith("WindowsStartupService.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("WindowsServiceRegistrationService.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("ServiceRegistry.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("LabelMappingService.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("RuntimeAgentConfigResolver.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("ClientConstants.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("FileDeliveryConfirmationTracker.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("WindowsRemoteAccessService.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("WindowsPolicyEnforcer.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("SessionCompanionIpc.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("LegacySnippetAdapters.cs", StringComparison.OrdinalIgnoreCase);
        }

        if (path.StartsWith("src/EJLive.Server/Services/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (path.StartsWith("src/EJLive.Server.WinForms/Services/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (path.StartsWith("src/EJLive.Core/Services/", StringComparison.OrdinalIgnoreCase))
        {
            // Core services are now compiled operational modules. Only the historical
            // CoreServices.cs aggregate remains as reference-only migration material.
            return path.EndsWith("CoreServices.cs", StringComparison.OrdinalIgnoreCase);
        }

        if (path.StartsWith("src/EJLive.Core/Engine/", StringComparison.OrdinalIgnoreCase))
        {
            return !path.EndsWith("CommunicationProtocol.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("JournalOutbox.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("NetworkEngine.cs", StringComparison.OrdinalIgnoreCase) &&
                   !path.EndsWith("OperationalEngines.cs", StringComparison.OrdinalIgnoreCase);
        }

        if (path.StartsWith("src/EJLive.Core/Xfs/", StringComparison.OrdinalIgnoreCase))
            return !path.EndsWith("XfsModels.cs", StringComparison.OrdinalIgnoreCase);

        if (path.StartsWith("src/EJLive.Core/Models/", StringComparison.OrdinalIgnoreCase))
            return !path.EndsWith("UnifiedModels.cs", StringComparison.OrdinalIgnoreCase);

        if (path.StartsWith("src/EJLive.Shared/", StringComparison.OrdinalIgnoreCase))
            return path.EndsWith("Logger.cs", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("LightUiTheme.cs", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("MonitoringState.cs", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith("MonitoringStateStore.cs", StringComparison.OrdinalIgnoreCase);

        return false;
    }

    private static ActiveServiceReplacement? FindReplacement(string path)
    {
        return ReplacementMap.FirstOrDefault(item => path.StartsWith(item.PathPrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsBuildOutput(string path)
    {
        var normalized = Normalize(path);
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}

public enum ClientServiceStatus
{
    Stopped,
    Running,
    Faulted
}

public sealed record JournalStorageResult(
    string ATM_ID,
    string FileName,
    string StoragePath,
    long FileSize,
    string Checksum,
    DateTime ReceivedAtUtc,
    JournalEvidenceReport Evidence);

public sealed record JournalDeliveryReceipt(
    string TransferId,
    string ATM_ID,
    string ATM_Type,
    string FileName,
    string Category,
    string StoragePath,
    long FileSize,
    string Checksum,
    DateTime ReceivedAtUtc,
    bool Confirmed,
    string Detail);

public sealed record RemoteCommandDispatch(
    RemoteCommand Command,
    RemoteCommandPolicyDecision Policy,
    DateTime QueuedAtUtc);

public sealed record ScheduledRemoteCommand(
    string ScheduleId,
    string ATM_ID,
    string CommandType,
    string Payload,
    string Role,
    DateTime ExecuteAtUtc,
    DateTime CreatedAtUtc);

public sealed record ClientServiceState(
    string Name,
    ClientServiceStatus Status,
    string Detail,
    DateTime UpdatedAtUtc);

public sealed record ClientServiceSupervisorReport(
    int Total,
    int Running,
    int Faulted,
    IReadOnlyList<ClientServiceState> Services);

public sealed record ActiveServiceReplacement(
    string PathPrefix,
    string ActiveService,
    string Coverage);

public sealed record ReferenceOnlyServiceFile(
    string Path,
    string ActiveReplacement);

public sealed record DuplicateTypeFinding(
    string TypeName,
    IReadOnlyList<string> Files);

public sealed record ProjectIntegrationAuditReport(
    string RootPath,
    int SourceFileCount,
    int ReferenceOnlyServiceFileCount,
    IReadOnlyList<ActiveServiceReplacement> ActiveReplacements,
    IReadOnlyList<ReferenceOnlyServiceFile> ReferenceOnlyFiles,
    IReadOnlyList<DuplicateTypeFinding> DuplicateTypeFindings,
    IReadOnlyList<ReferenceOnlyServiceFile> UncoveredReferenceOnlyFiles)
{
    public bool AllReferenceOnlyServicesCovered => UncoveredReferenceOnlyFiles.Count == 0;
}

public enum ServiceActivationStatusKind
{
    ActiveCompiled,
    ReferenceCovered,
    NeedsActivation
}

public sealed record ServiceActivationStatus(
    string Path,
    string ProjectName,
    ServiceActivationStatusKind Status,
    bool Compiled,
    string ActiveReplacement,
    string Detail);

public sealed record ServiceActivationAuditReport(
    int TotalCandidates,
    int ActiveCompiledCandidates,
    int ReferenceCoveredCandidates,
    int NeedsActivationCandidates,
    IReadOnlyList<ServiceActivationStatus> Candidates);

public sealed class UnifiedServiceActivationAuditService
{
    public ServiceActivationAuditReport Analyze(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        var root = Path.GetFullPath(rootPath);

        var integrationAudit = new UnifiedProjectIntegrationAuditService();
        var report = integrationAudit.Analyze(root);
        var compiledMap = BuildCompiledFileMap(root);
        var statuses = report.ReferenceOnlyFiles
            .Select(file => BuildStatus(file, compiledMap))
            .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ServiceActivationAuditReport(
            statuses.Length,
            statuses.Count(item => item.Status == ServiceActivationStatusKind.ActiveCompiled),
            statuses.Count(item => item.Status == ServiceActivationStatusKind.ReferenceCovered),
            statuses.Count(item => item.Status == ServiceActivationStatusKind.NeedsActivation),
            statuses);
    }

    private static ServiceActivationStatus BuildStatus(
        ReferenceOnlyServiceFile file,
        IReadOnlyDictionary<string, string> compiledMap)
    {
        var normalized = Normalize(file.Path);
        var compiled = compiledMap.TryGetValue(normalized, out var projectName);
        var replacement = file.ActiveReplacement ?? string.Empty;

        if (compiled)
        {
            return new ServiceActivationStatus(
                normalized,
                projectName ?? string.Empty,
                ServiceActivationStatusKind.ActiveCompiled,
                true,
                replacement,
                "Candidate file is compiled directly in the active runtime.");
        }

        if (!string.IsNullOrWhiteSpace(replacement))
        {
            return new ServiceActivationStatus(
                normalized,
                ResolveProjectName(normalized),
                ServiceActivationStatusKind.ReferenceCovered,
                false,
                replacement,
                "Candidate file remains reference-preserved and is covered through unified active replacement.");
        }

        return new ServiceActivationStatus(
            normalized,
            ResolveProjectName(normalized),
            ServiceActivationStatusKind.NeedsActivation,
            false,
            replacement,
            "Candidate file is neither compiled nor mapped to an active replacement.");
    }

    private static Dictionary<string, string> BuildCompiledFileMap(string rootPath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var srcRoot = Path.Combine(rootPath, "src");
        if (!Directory.Exists(srcRoot))
            return result;

        foreach (var projectFile in Directory.EnumerateFiles(srcRoot, "*.csproj", SearchOption.AllDirectories))
        {
            if (IsBuildOutput(projectFile))
                continue;

            var compiled = ResolveProjectCompiledFiles(projectFile, rootPath);
            foreach (var path in compiled)
            {
                // The first owner wins; duplicated includes are tracked separately by duplicate-type analysis.
                if (!result.ContainsKey(path))
                    result[path] = Path.GetFileNameWithoutExtension(projectFile);
            }
        }

        return result;
    }

    private static IReadOnlyCollection<string> ResolveProjectCompiledFiles(string projectFilePath, string rootPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(projectDirectory) || !Directory.Exists(projectDirectory))
            return Array.Empty<string>();

        var projectFiles = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .Select(path => Normalize(Path.GetRelativePath(projectDirectory, path)))
            .ToArray();

        var document = XDocument.Load(projectFilePath);
        var rootElement = document.Root;
        if (rootElement is null)
            return Array.Empty<string>();

        var enableDefaultItems = !rootElement
            .Descendants()
            .Where(element => element.Name.LocalName == "EnableDefaultItems")
            .Select(element => (element.Value ?? string.Empty).Trim())
            .Any(value => value.Equals("false", StringComparison.OrdinalIgnoreCase));

        var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (enableDefaultItems)
        {
            foreach (var path in projectFiles)
                resolved.Add(path);
        }

        var removePatterns = rootElement
            .Descendants()
            .Where(element => element.Name.LocalName == "Compile")
            .Select(element => element.Attribute("Remove")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(SplitPatterns)
            .ToArray();

        foreach (var compileNode in rootElement.Descendants().Where(element => element.Name.LocalName == "Compile"))
        {
            var includeRaw = compileNode.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(includeRaw))
                continue;

            var includes = SplitPatterns(includeRaw);
            var excludes = SplitPatterns(compileNode.Attribute("Exclude")?.Value);
            foreach (var pattern in includes)
            {
                foreach (var path in MatchPattern(projectFiles, pattern))
                {
                    if (excludes.Any(exclude => IsMatch(path, exclude)))
                        continue;
                    resolved.Add(path);
                }
            }
        }

        if (removePatterns.Length > 0)
        {
            var toRemove = resolved.Where(path => removePatterns.Any(pattern => IsMatch(path, pattern))).ToArray();
            foreach (var path in toRemove)
                resolved.Remove(path);
        }

        return resolved
            .Select(path => Normalize(Path.GetRelativePath(rootPath, Path.Combine(projectDirectory, path))))
            .Where(path => path.StartsWith("src/", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static string[] SplitPatterns(string? patternList)
    {
        if (string.IsNullOrWhiteSpace(patternList))
            return Array.Empty<string>();

        return patternList
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .ToArray();
    }

    private static IEnumerable<string> MatchPattern(IEnumerable<string> projectFiles, string pattern)
    {
        var normalizedPattern = Normalize(pattern);
        if (string.IsNullOrWhiteSpace(normalizedPattern))
            return Array.Empty<string>();

        return projectFiles.Where(path => IsMatch(path, normalizedPattern));
    }

    private static bool IsMatch(string value, string pattern)
    {
        var normalizedValue = Normalize(value);
        var normalizedPattern = Normalize(pattern);
        if (string.IsNullOrWhiteSpace(normalizedPattern))
            return false;

        var regex = WildcardRegexCache.GetOrAdd(
            normalizedPattern,
            key =>
            {
                var escaped = Regex.Escape(key)
                    .Replace(@"\*\*", "__DOUBLESTAR__", StringComparison.Ordinal)
                    .Replace(@"\*", @"[^/]*", StringComparison.Ordinal)
                    .Replace(@"\?", @"[^/]", StringComparison.Ordinal)
                    .Replace("__DOUBLESTAR__", ".*", StringComparison.Ordinal);
                return new Regex($"^{escaped}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            });

        return regex.IsMatch(normalizedValue);
    }

    private static string ResolveProjectName(string normalizedPath)
    {
        if (!normalizedPath.StartsWith("src/", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var tail = normalizedPath["src/".Length..];
        var slash = tail.IndexOf('/');
        return slash <= 0 ? tail : tail[..slash];
    }

    private static bool IsBuildOutput(string path)
    {
        var normalized = Normalize(path);
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string value) => value.Replace('\\', '/').Trim();

    private static readonly ConcurrentDictionary<string, Regex> WildcardRegexCache = new(StringComparer.OrdinalIgnoreCase);
}
