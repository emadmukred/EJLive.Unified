using System.Diagnostics;
using RuntimeAgentConfig = EJLive.Client.WinForms.Services.RuntimeAgentConfig;
using RuntimeAgentConfigResolver = EJLive.Client.WinForms.Services.RuntimeAgentConfigResolver;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Client.WinForms;

public sealed class ClientMainForm : Form
{
    private readonly AppConfig _config;
    private readonly ATMInfo _atmInfo;
    private readonly JournalOutbox _outbox = new();
    private readonly FileWatcherEngine _fileWatcher = new();
    private readonly GhostRemoteEngine _ghostEngine = new();
    private readonly RuntimeAgentConfigResolver _runtimeConfigResolver = new();
    private NetworkEngine? _networkEngine;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };

    private Label _statusLabel = null!;
    private Label _sessionLabel = null!;
    private Label _clockLabel = null!;
    private Label _connectionStateValue = null!;
    private Label _queueValue = null!;
    private Label _healthValue = null!;
    private Label _lastDataValue = null!;
    private RichTextBox _connectionLog = null!;
    private DataGridView _syncGrid = null!;
    private RichTextBox _journalText = null!;
    private RichTextBox _remoteLog = null!;
    private PictureBox _remotePreview = null!;
    private Label _remoteStatus = null!;
    private DataGridView _pendingCommandsGrid = null!;
    private DataGridView _serviceGrid = null!;
    private DataGridView _agentConfigGrid = null!;
    private TextBox _serverIp = null!;
    private NumericUpDown _serverPort = null!;
    private TextBox _atmId = null!;
    private TextBox _atmName = null!;
    private ComboBox _atmType = null!;
    private ComboBox _networkType = null!;
    private TextBox _sourcePath = null!;
    private TextBox _backupPath = null!;
    private string _lastSyncSignature = string.Empty;
    private string _lastScreenshotPath = string.Empty;
    private bool _fileWatcherSubscribed;
    private readonly Dictionary<string, string> _serviceStates = new(StringComparer.OrdinalIgnoreCase);

    public ClientMainForm()
    {
        _config = AgentConfigurationXmlService.LoadAppConfig(AppConfig.Load());
        _config.ApplyDefaults();
        if (_runtimeConfigResolver.TryResolve(_config, out var runtimeConfig, out _))
        {
            _runtimeConfigResolver.ApplyTo(_config, runtimeConfig);
        }
        _atmInfo = new ATMInfo
        {
            ATM_ID = _config.ATM_ID,
            ATM_Name = _config.ATM_Name,
            ATM_Type = _config.ATM_Type,
            ServerIP = _config.ServerIP,
            ServerPort = _config.ServerPort,
            NetworkType = _config.NetworkType
        };

        AppLogger.Instance.Initialize(AppConstants.DefaultLogPath, "client");
        AppLogger.Instance.OnLog += (_, e) => AppendLog(_connectionLog, e.FormattedForUI);
        DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);

        InitializeForm();
        Shown += (_, _) =>
        {
            if (_config.AutoConnect)
                BeginInvoke(StartConnection);
        };
        _timer.Tick += (_, _) => RefreshRuntimeState();
        _timer.Start();
    }

    private void InitializeForm()
    {
        Text = $"EJLive Enterprise Client - {_config.ATM_ID}";
        MinimumSize = new Size(1080, 720);
        Size = new Size(1180, 780);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildConnectionTab());
        tabs.TabPages.Add(BuildSyncTab());
        tabs.TabPages.Add(BuildJournalTab());
        tabs.TabPages.Add(BuildRemoteControlTab());
        tabs.TabPages.Add(BuildServicesTab());
        tabs.TabPages.Add(BuildSettingsTab());
        tabs.TabPages.Add(BuildAgentConfigTab());

        var status = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Disconnected") { Spring = true, TextAlign = ContentAlignment.MiddleLeft }.AsLabel();
        _sessionLabel = new ToolStripStatusLabel("Session: -").AsLabel();
        _clockLabel = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm:ss")).AsLabel();
        status.Items.Add(new ToolStripControlHost(_statusLabel));
        status.Items.Add(new ToolStripControlHost(_sessionLabel));
        status.Items.Add(new ToolStripControlHost(_clockLabel));

        Controls.Add(tabs);
        Controls.Add(status);
    }

    private TabPage BuildConnectionTab()
    {
        var tab = new TabPage("Connection");
        var root = Ui.Stack(DockStyle.Fill);
        var top = Ui.Flow();
        top.Controls.Add(Ui.Button("Connect", StartConnection));
        top.Controls.Add(Ui.Button("Disconnect", Disconnect));
        top.Controls.Add(Ui.Button("Ping", PingServer));
        top.Controls.Add(Ui.Button("Open Journal Folder", () => OpenFolder(_config.SourcePath)));
        top.Controls.Add(Ui.Button("Open Log Folder", () => OpenFolder(AppConstants.DefaultLogPath)));

        var metrics = Ui.Grid();
        metrics.Columns.Add("Metric", "Metric");
        metrics.Columns.Add("Value", "Value");
        metrics.Rows.Add("ATM", _config.ATM_ID);
        metrics.Rows.Add("Server", $"{_config.ServerIP}:{_config.ServerPort}");
        metrics.Rows.Add("Network", _config.NetworkType);
        metrics.Rows.Add("Protocol", AppConstants.ProtocolVersion);
        metrics.Rows.Add("Heartbeat", $"{AppConstants.HeartbeatIntervalSec}s");

        var summary = Ui.CardRow(4);
        _connectionStateValue = Ui.AddMetricCard(summary, "Connection", "Disconnected", Color.FromArgb(46, 134, 222));
        _queueValue = Ui.AddMetricCard(summary, "Outbox", "0 queued", Color.FromArgb(255, 159, 67));
        _healthValue = Ui.AddMetricCard(summary, "Health", "100%", Color.FromArgb(16, 172, 132));
        _lastDataValue = Ui.AddMetricCard(summary, "Last Data", "-", Color.FromArgb(95, 39, 205));

        _connectionLog = Ui.LogBox();
        root.Controls.Add(_connectionLog);
        root.Controls.Add(metrics);
        root.Controls.Add(summary);
        root.Controls.Add(top);
        tab.Controls.Add(root);
        UpdateOverviewCards();
        return tab;
    }

    private TabPage BuildSyncTab()
    {
        var tab = new TabPage("Sync");
        var root = Ui.Stack(DockStyle.Fill);
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Force Send", ForceSend));
        actions.Controls.Add(Ui.Button("Clear Failed", () => { _outbox.ClearFailed(); RefreshSyncGrid(true); }));
        actions.Controls.Add(Ui.Button("Pause Sync", () => AppendLog(_connectionLog, "Sync paused.")));
        actions.Controls.Add(Ui.Button("Resume Sync", () => AppendLog(_connectionLog, "Sync resumed.")));

        _syncGrid = Ui.Grid();
        _syncGrid.Columns.Add("ItemId", "Item Id");
        _syncGrid.Columns.Add("FileName", "File");
        _syncGrid.Columns.Add("Status", "Status");
        _syncGrid.Columns.Add("RetryCount", "Retries");
        _syncGrid.Columns.Add("Bytes", "Bytes");

        root.Controls.Add(_syncGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshSyncGrid();
        return tab;
    }

    private TabPage BuildJournalTab()
    {
        var tab = new TabPage("Journal");
        var root = Ui.Stack(DockStyle.Fill);
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Load Journal", LoadJournal));
        actions.Controls.Add(Ui.Button("Send For Server Analysis", AnalyzeJournal));
        actions.Controls.Add(Ui.Button("Export Journal", ExportJournal));
        actions.Controls.Add(Ui.Button("Approved", () => HighlightText("APPROVED")));
        actions.Controls.Add(Ui.Button("Declined", () => HighlightText("DECLINED")));
        actions.Controls.Add(Ui.Button("Capture", () => HighlightText("CAPTURE")));
        actions.Controls.Add(Ui.Button("Cash Error", () => HighlightText("CASH")));
        actions.Controls.Add(Ui.Button("Search Error", () => HighlightText("ERROR")));
        _journalText = Ui.LogBox();
        root.Controls.Add(_journalText);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildRemoteControlTab()
    {
        var tab = new TabPage("Remote Control");
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 5,
            BackColor = Color.FromArgb(203, 213, 225)
        };

        var previewHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(246, 248, 250) };
        var previewFrame = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black, Padding = new Padding(1) };
        _remotePreview = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
        previewFrame.Controls.Add(_remotePreview);

        var previewToolbar = Ui.Flow();
        previewToolbar.Height = 58;
        previewToolbar.Controls.Add(Ui.Button("Capture Screen", RequestScreen));
        previewToolbar.Controls.Add(Ui.Button("Stop Screen", StopScreen));
        previewToolbar.Controls.Add(Ui.Button("Open Screenshots", OpenScreenshotsFolder));
        _remoteStatus = new Label
        {
            Text = "Screen preview idle",
            AutoSize = true,
            Padding = new Padding(10, 8, 0, 0),
            ForeColor = Color.FromArgb(100, 116, 139)
        };
        previewToolbar.Controls.Add(_remoteStatus);
        previewHost.Controls.Add(previewFrame);
        previewHost.Controls.Add(previewToolbar);
        split.Panel1.Controls.Add(previewHost);

        var commandHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.White };
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Restart ATM", () => QueueRemoteCommand(AppConstants.CMD_RESTART)));
        actions.Controls.Add(Ui.Button("Sync Time", () => QueueRemoteCommand(AppConstants.CMD_SYNC_TIME)));
        actions.Controls.Add(Ui.Button("Change Password", () => QueueRemoteCommand(AppConstants.CMD_CHANGE_PASSWORD)));
        actions.Controls.Add(Ui.Button("Windows Remote Start", () => QueueRemoteCommand(AppConstants.CMD_WINDOWS_REMOTE_START)));
        _pendingCommandsGrid = Ui.Grid();
        _pendingCommandsGrid.Columns.Add("Command", "Command");
        _pendingCommandsGrid.Columns.Add("CommandId", "Command Id");
        _pendingCommandsGrid.Columns.Add("Status", "Status");
        _pendingCommandsGrid.Columns.Add("QueuedAt", "Queued At");
        _pendingCommandsGrid.Columns.Add("Result", "Result");
        _remoteLog = Ui.LogBox();
        _remoteLog.Height = 150;
        _remoteLog.Dock = DockStyle.Bottom;
        commandHost.Controls.Add(_pendingCommandsGrid);
        commandHost.Controls.Add(_remoteLog);
        commandHost.Controls.Add(actions);
        split.Panel2.Controls.Add(commandHost);
        split.HandleCreated += (_, _) => split.BeginInvoke(new Action(() => split.SplitterDistance = Math.Max(420, split.Width * 58 / 100)));
        tab.Controls.Add(split);
        return tab;
    }

    private TabPage BuildServicesTab()
    {
        var tab = new TabPage("Services");
        _serviceGrid = Ui.Grid();
        _serviceGrid.Columns.Add("Service", "Service");
        _serviceGrid.Columns.Add("Status", "Status");
        _serviceGrid.Columns.Add("Details", "Details");
        foreach (var service in new[] { "Agent Controller", "File Watcher", "Socket Data", "Socket Files", "Screenshot", "Ghost Access", "Windows Startup" })
            SetServiceState(service, "Stopped", "Ready");

        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Start Agent", () => SetServiceState("Agent Controller", "Running", "UI agent loop active")));
        actions.Controls.Add(Ui.Button("Refresh Agent", () => SetServiceState("Agent Controller", "Running", "Agent status refresh requested")));
        actions.Controls.Add(Ui.Button("Start File Watcher", StartFileWatcher));
        actions.Controls.Add(Ui.Button("Restart File Watcher", StartFileWatcher));
        actions.Controls.Add(Ui.Button("Capture Screen", () => { CaptureAndDisplayScreenshot(sendFrame: false); SetServiceState("Screenshot", "Running", "Last capture requested"); }));
        actions.Controls.Add(Ui.Button("Open Screenshots", OpenScreenshotsFolder));
        actions.Controls.Add(Ui.Button("Run Diagnostics", RunDiagnostics));
        actions.Controls.Add(Ui.Button("Ensure Startup", () => SetStartupRegistration(enabled: true)));
        actions.Controls.Add(Ui.Button("Activate Service", ActivateWindowsService));
        actions.Controls.Add(Ui.Button("Start Service", StartWindowsService));
        actions.Controls.Add(Ui.Button("Service Status", QueryWindowsService));

        var root = Ui.Stack(DockStyle.Fill);
        root.Controls.Add(_serviceGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildSettingsTab()
    {
        var tab = new TabPage("Settings");
        var panel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(16) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _serverIp = Ui.Text(_config.ServerIP);
        _serverPort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = _config.ServerPort, Width = 160 };
        _atmId = Ui.Text(_config.ATM_ID);
        _atmName = Ui.Text(_config.ATM_Name);
        _atmType = Ui.Combo(AppConstants.GetSupportedATMTypes(), _config.ATM_Type);
        _networkType = Ui.Combo(new[] { "LAN", "ADSL", "CDMA", "GSM" }, _config.NetworkType);
        _sourcePath = Ui.Text(_config.SourcePath);
        _backupPath = Ui.Text(_config.BackupPath);

        AddRow(panel, "Server IP", _serverIp);
        AddRow(panel, "Server Port", _serverPort);
        AddRow(panel, "ATM Id", _atmId);
        AddRow(panel, "ATM Name", _atmName);
        AddRow(panel, "ATM Type", _atmType);
        AddRow(panel, "Network Type", _networkType);
        AddRow(panel, "Source Path", _sourcePath);
        AddRow(panel, "Backup Path", _backupPath);

        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Save Settings", SaveSettings));
        actions.Controls.Add(Ui.Button("Browse Source", () => BrowseTo(_sourcePath)));
        actions.Controls.Add(Ui.Button("Browse Backup", () => BrowseTo(_backupPath)));

        var root = Ui.Stack(DockStyle.Fill);
        root.Controls.Add(actions);
        root.Controls.Add(panel);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildAgentConfigTab()
    {
        var tab = new TabPage("Agent Config");
        var root = Ui.Stack(DockStyle.Fill);
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Load Agent Config", LoadAgentConfig));
        actions.Controls.Add(Ui.Button("Apply Agent Config", () => AppendLog(_connectionLog, "Agent configuration applied to runtime state.")));
        actions.Controls.Add(Ui.Button("Save Agent Config", SaveSettings));
        actions.Controls.Add(Ui.Button("Open Config Folder", () => OpenFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client"))));
        _agentConfigGrid = Ui.Grid();
        _agentConfigGrid.Columns.Add("Key", "Key");
        _agentConfigGrid.Columns.Add("Value", "Value");
        root.Controls.Add(_agentConfigGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        LoadAgentConfig();
        return tab;
    }

    private void StartConnection()
    {
        if (_networkEngine?.IsConnected == true)
        {
            AppendLog(_connectionLog, "Connection already active.");
            return;
        }

        if (!TryResolveRuntimeConfigFromInputs(out var runtimeConfig, preferEnvironment: true, out var reason))
        {
            AppendLog(_connectionLog, "Connection start blocked: " + reason);
            MessageBox.Show(reason, "Runtime Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _runtimeConfigResolver.ApplyTo(_config, runtimeConfig);
        _atmInfo.ATM_ID = runtimeConfig.AtmId;
        _atmInfo.ServerIP = runtimeConfig.ServerIp;
        _atmInfo.ServerPort = runtimeConfig.ServerPort;

        _networkEngine?.Dispose();
        _statusLabel.Text = "Connecting";
        _atmInfo.ConnectionStatus = ConnectionStatus.Connecting;
        UpdateOverviewCards();
        _networkEngine = new NetworkEngine(runtimeConfig.ServerIp, runtimeConfig.ServerPort, runtimeConfig.AtmId, _config.ATM_Type, _config.NetworkType, _outbox);
        _networkEngine.OnConnectionChanged += (_, connected) =>
        {
            RunOnUi(() =>
            {
                _atmInfo.ConnectionStatus = connected ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
                _statusLabel.Text = connected ? "Connected" : "Disconnected";
                UpdateOverviewCards();
            });
        };
        _networkEngine.OnSessionEstablished += (_, session) => RunOnUi(() => _sessionLabel.Text = $"Session: {session}");
        _networkEngine.OnMessageReceived += (_, message) => HandleServerMessage(message);
        _networkEngine.OnJournalAcknowledged += (_, ack) => HandleJournalAcknowledgement(ack);
        _networkEngine.OnLog += (_, message) => AppendLog(_connectionLog, message);
        _networkEngine.OnError += (_, message) =>
        {
            RunOnUi(() =>
            {
                _statusLabel.Text = "Disconnected";
                _atmInfo.ConnectionStatus = ConnectionStatus.Disconnected;
                UpdateOverviewCards();
            });
            AppendLog(_connectionLog, message);
        };
        AppendLog(_connectionLog, $"Connecting to {runtimeConfig.ConnectionDisplay} as {runtimeConfig.AtmId}.");
        _ = Task.Run(() => _networkEngine.Connect());
    }

    private void HandleJournalAcknowledgement(string ack)
    {
        var retryPolicy = RetryPolicy.ForNetwork(_config.NetworkType);
        if (!_outbox.TryApplyAcknowledgement(ack, retryPolicy, out var item, out var success, out var detail))
        {
            RunOnUi(() => AppendLog(_connectionLog, $"Journal ACK unmatched: {ack}"));
            return;
        }

        RunOnUi(() =>
        {
            AppendLog(_connectionLog, success
                ? $"Journal confirmed by ACK: {item.FileName} ({detail})"
                : $"Journal ACK failed, re-queued: {item.FileName} ({detail})");
            RefreshSyncGrid(true);
            UpdateOverviewCards();
        });
    }

    private void Disconnect()
    {
        _networkEngine?.Disconnect();
        _statusLabel.Text = "Disconnected";
        AppendLog(_connectionLog, "Disconnected.");
    }

    private void HandleServerMessage(EJMessage message)
    {
        if (message.Type == CommunicationProtocol.MsgType.Broadcast)
        {
            AppendLog(_connectionLog, $"Server broadcast: {message.Text}");
            return;
        }

        if (message.Type != CommunicationProtocol.MsgType.Command)
        {
            AppendLog(_connectionLog, $"Server message {message.Type}: {message.Text}");
            return;
        }

        if (!RemoteCommandEnvelope.TryParse(message.Text, out var command))
        {
            AppendLog(_remoteLog, $"Invalid remote command payload: {message.Text}");
            return;
        }

        RunOnUi(() => AddCommandRow(command, "Running", "Received from server"));
        var result = ExecuteRemoteCommand(command);
        _ = _networkEngine?.SendMessageAsync(CommunicationProtocol.BuildCommandResult(command.CommandId, result.Success, result.Message));
        RunOnUi(() => AddCommandRow(command, result.Success ? "Completed" : "Failed", result.Message));
    }

    private (bool Success, string Message) ExecuteRemoteCommand(RemoteCommandEnvelope command)
    {
        switch (command.CommandType)
        {
            case AppConstants.CMD_PING:
                return (true, "Ping acknowledged.");
            case AppConstants.CMD_SYNC_TIME:
                return (true, $"Local time observed: {DateTime.Now:O}");
            case AppConstants.CMD_SCREENSHOT:
            case AppConstants.CMD_GHOST_START:
                RunOnUi(() => CaptureAndDisplayScreenshot(sendFrame: true));
                return (true, "Screen capture requested.");
            case AppConstants.CMD_GHOST_STOP:
                RunOnUi(StopScreen);
                return (true, "Remote screen stopped.");
            case AppConstants.CMD_FORCE_SYNC:
                RunOnUi(ForceSend);
                return (true, "Force sync requested.");
            case AppConstants.CMD_WINDOWS_REMOTE_START:
                RunOnUi(() => SetServiceState("Ghost Access", "Running", "Windows remote start requested"));
                return (true, "Windows remote start acknowledged.");
            case AppConstants.CMD_WINDOWS_REMOTE_STOP:
                RunOnUi(() => SetServiceState("Ghost Access", "Stopped", "Windows remote stop requested"));
                return (true, "Windows remote stop acknowledged.");
            case AppConstants.CMD_RESTART:
            case AppConstants.CMD_SHUTDOWN:
            case AppConstants.CMD_CHANGE_PASSWORD:
                return (false, $"{command.CommandType} requires local/operator confirmation and was not executed automatically.");
            default:
                return (true, $"{command.CommandType} acknowledged.");
        }
    }

    private async void PingServer()
    {
        if (!TryResolveRuntimeConfigFromInputs(out var runtimeConfig, preferEnvironment: true, out var reason))
        {
            AppendLog(_connectionLog, "Ping blocked: " + reason);
            return;
        }

        try
        {
            AppendLog(_connectionLog, $"Pinging server {runtimeConfig.ServerIp}...");
            var result = await Task.Run(() =>
            {
                using var ping = new System.Net.NetworkInformation.Ping();
                var reply = ping.Send(runtimeConfig.ServerIp, NetworkConfig.PING_TIMEOUT_MS);
                return $"{reply.Status} in {reply.RoundtripTime}ms";
            });
            AppendLog(_connectionLog, $"Ping {result}.");
        }
        catch (Exception ex)
        {
            AppendLog(_connectionLog, $"Ping failed: {ex.Message}");
        }
    }

    private async void ForceSend()
    {
        var payload = System.Text.Encoding.UTF8.GetBytes($"Sample journal sync from {_config.ATM_ID} at {DateTime.UtcNow:O}");
        var item = _outbox.Enqueue(_config.ATM_ID, $"sample-{DateTime.UtcNow:yyyyMMddHHmmss}.ej", payload, 0, SecurityHelper.SHA256Hash(payload));
        _atmInfo.LastDataReceivedUtc = DateTime.UtcNow;
        if (_networkEngine?.IsConnected == true)
        {
            var sent = await _networkEngine.SendJournalFileAsync(item.FileName, item.Data, item.Offset, item.Checksum);
            if (sent)
            {
                _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));
                AppendLog(_connectionLog, $"Sent {item.FileName}; awaiting server ACK.");
            }
        }
        else
        {
            AppendLog(_connectionLog, $"{item.FileName} queued until the connection is available.");
        }
        RefreshSyncGrid(true);
        UpdateOverviewCards();
    }

    private async void LoadJournal()
    {
        try
        {
            _journalText.Text = "Loading journal...";
            var text = await Task.Run(() =>
            {
                var path = Directory.Exists(_config.SourcePath)
                    ? Directory.EnumerateFiles(_config.SourcePath).FirstOrDefault()
                    : null;
                return path is not null ? File.ReadAllText(path) : "No journal file found in configured source path.";
            });
            _journalText.Text = text;
        }
        catch (Exception ex)
        {
            _journalText.Text = $"Failed to load journal: {ex.Message}";
        }
    }

    private async void AnalyzeJournal()
    {
        await QueueJournalForServerAnalysis().ConfigureAwait(false);
    }

    private void ExportJournal()
    {
        Directory.CreateDirectory(AppConstants.DefaultReportsPath);
        var path = Path.Combine(AppConstants.DefaultReportsPath, $"client-journal-{DateTime.Now:yyyyMMddHHmmss}.txt");
        File.WriteAllText(path, _journalText.Text);
        AppendLog(_connectionLog, $"Journal exported to {path}");
    }

    private void HighlightText(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(_journalText.Text))
            return;

        var originalStart = _journalText.SelectionStart;
        var originalLength = _journalText.SelectionLength;
        _journalText.SelectAll();
        _journalText.SelectionBackColor = _journalText.BackColor;

        var index = 0;
        var count = 0;
        while (index < _journalText.TextLength)
        {
            var found = _journalText.Find(text, index, RichTextBoxFinds.None);
            if (found < 0)
                break;
            _journalText.SelectionBackColor = Color.FromArgb(255, 245, 157);
            index = found + text.Length;
            count++;
        }

        _journalText.Select(Math.Min(originalStart, _journalText.TextLength), Math.Min(originalLength, Math.Max(0, _journalText.TextLength - originalStart)));
        AppendLog(_connectionLog, $"Journal filter '{text}' matched {count} item(s).");
    }

    private void RequestScreen()
    {
        _ghostEngine.Start(_config.ATM_ID);
        CaptureAndDisplayScreenshot(sendFrame: true);
        AppendLog(_remoteLog, "Screen request started.");
    }

    private void StopScreen()
    {
        _ghostEngine.Stop();
        if (_networkEngine?.IsConnected == true)
            _ = _networkEngine.SendMessageAsync(CommunicationProtocol.BuildGhostStop(_config.ATM_ID));
        if (_remoteStatus is not null)
        {
            _remoteStatus.Text = "Screen preview stopped";
            _remoteStatus.ForeColor = Color.FromArgb(180, 35, 24);
        }
        AppendLog(_remoteLog, "Screen request stopped.");
    }

    private void QueueRemoteCommand(string commandType)
    {
        var command = new RemoteCommand { ATM_ID = _config.ATM_ID, CommandType = commandType, RequiresConfirmation = AppConstants.CommandsRequireConfirmation.Contains(commandType) };
        AddCommandRow(new RemoteCommandEnvelope { CommandId = command.CommandId, CommandType = command.CommandType, RequiresConfirmation = command.RequiresConfirmation, Payload = command.Payload }, command.Status.ToString(), "Queued locally");
        AppendLog(_remoteLog, $"Queued {command.CommandType} ({command.CommandId}).");
    }

    private void AddCommandRow(RemoteCommandEnvelope command, string status, string result)
    {
        if (_pendingCommandsGrid is null)
            return;
        if (_pendingCommandsGrid.InvokeRequired)
        {
            _pendingCommandsGrid.BeginInvoke(() => AddCommandRow(command, status, result));
            return;
        }

        var shortId = command.CommandId.Length > 10 ? command.CommandId[..10] : command.CommandId;
        var index = _pendingCommandsGrid.Rows.Add(command.CommandType, shortId, status, DateTime.Now.ToString("HH:mm:ss"), result);
        _pendingCommandsGrid.Rows[index].DefaultCellStyle.BackColor = status switch
        {
            "Completed" => Color.FromArgb(239, 252, 246),
            "Failed" => Color.FromArgb(255, 239, 239),
            "Running" => Color.FromArgb(239, 247, 255),
            _ => Color.White
        };
    }

    private void OpenScreenshotsFolder()
    {
        OpenFolder(GetScreenshotFolder());
    }

    private string GetScreenshotFolder()
    {
        var root = string.IsNullOrWhiteSpace(_config.BackupPath)
            ? AppConstants.DefaultClientOutboxPath
            : _config.BackupPath;
        return Path.Combine(root, "Screenshots");
    }

    private void CaptureAndDisplayScreenshot(bool sendFrame)
    {
        try
        {
            var folder = GetScreenshotFolder();
            Directory.CreateDirectory(folder);
            var bytes = _ghostEngine.CaptureScreenJpeg();
            _lastScreenshotPath = Path.Combine(folder, $"{_config.ATM_ID}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
            File.WriteAllBytes(_lastScreenshotPath, bytes);

            using var stream = new MemoryStream(bytes);
            using var image = Image.FromStream(stream);
            _remotePreview.Image?.Dispose();
            _remotePreview.Image = new Bitmap(image);

            if (sendFrame && _networkEngine?.IsConnected == true)
            {
                _networkEngine.SendMessage(CommunicationProtocol.BuildGhostStart(_config.ATM_ID));
                _ = _networkEngine.SendMessageAsync(CommunicationProtocol.BuildGhostFrame(bytes));
            }

            _remoteStatus.Text = $"Last capture: {DateTime.Now:HH:mm:ss} ({Math.Max(1, bytes.Length / 1024)} KB)";
            _remoteStatus.ForeColor = Color.FromArgb(25, 135, 84);
            AppendLog(_remoteLog, $"Screenshot saved: {_lastScreenshotPath}");
        }
        catch (Exception ex)
        {
            if (_remoteStatus is not null)
            {
                _remoteStatus.Text = "Screen capture failed";
                _remoteStatus.ForeColor = Color.FromArgb(180, 35, 24);
            }
            AppendLog(_remoteLog, $"Screen capture failed: {ex.Message}");
        }
    }

    private void StartFileWatcher()
    {
        if (!_fileWatcherSubscribed)
        {
            _fileWatcher.FileChanged += (_, file) => AppendLog(_connectionLog, $"File changed: {file}");
            _fileWatcherSubscribed = true;
        }
        _fileWatcher.Start(_config.SourcePath);
        SetServiceState("File Watcher", "Running", _config.SourcePath);
        AppendLog(_connectionLog, "File watcher started.");
    }

    private void RunDiagnostics()
    {
        AppendLog(_connectionLog, $"Runtime folders: {AppConstants.DefaultClientOutboxPath}; {AppConstants.DefaultClientInboxPath}");
        AppendLog(_connectionLog, $"SQLite initialized: {DatabaseManager.Instance.IsInitialized}");
        AppendLog(_connectionLog, $"Startup registered: {IsStartupRegistered()}");
        AppendLog(_connectionLog, $"Network connected: {_networkEngine?.IsConnected == true}");
        AppendLog(_connectionLog, $"Queued outbox items: {_outbox.Count}");
    }

    private void SetStartupRegistration(bool enabled)
    {
        try
        {
            if (enabled)
            {
                var startupTask = EJLive.Client.WinForms.Services.WindowsStartupService.RegisterClientAutostart(Application.ExecutablePath);
                if (startupTask.Success)
                {
                    SetServiceState("Windows Startup", "Enabled", "System startup task registered");
                    AppendLog(_connectionLog, startupTask.Message);
                    return;
                }
            }

            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true)
                ?? Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true);
            if (enabled)
            {
                key.SetValue("EJLive.Client", $"\"{Application.ExecutablePath}\"", Microsoft.Win32.RegistryValueKind.String);
                SetServiceState("Windows Startup", "Enabled", "Runs at user logon");
                AppendLog(_connectionLog, "Windows startup registration enabled for EJLive.Client.");
            }
            else
            {
                key.DeleteValue("EJLive.Client", throwOnMissingValue: false);
                SetServiceState("Windows Startup", "Disabled", "Startup value removed");
                AppendLog(_connectionLog, "Windows startup registration removed for EJLive.Client.");
            }
        }
        catch (Exception ex)
        {
            AppendLog(_connectionLog, $"Startup registration failed: {ex.Message}");
        }
    }

    private void SetServiceState(string service, string status, string details)
    {
        _serviceStates[service] = status;
        if (_serviceGrid is null)
            return;
        if (_serviceGrid.InvokeRequired)
        {
            _serviceGrid.BeginInvoke(() => SetServiceState(service, status, details));
            return;
        }

        foreach (DataGridViewRow row in _serviceGrid.Rows)
        {
            if (string.Equals(Convert.ToString(row.Cells[0].Value), service, StringComparison.OrdinalIgnoreCase))
            {
                row.Cells[1].Value = status;
                row.Cells[2].Value = details;
                row.DefaultCellStyle.BackColor = ServiceStatusColor(status);
                return;
            }
        }

        var index = _serviceGrid.Rows.Add(service, status, details);
        _serviceGrid.Rows[index].DefaultCellStyle.BackColor = ServiceStatusColor(status);
    }

    private static Color ServiceStatusColor(string status)
    {
        return status switch
        {
            "Running" or "Enabled" => Color.FromArgb(239, 252, 246),
            "Stopped" or "Disabled" => Color.FromArgb(255, 239, 239),
            _ => Color.White
        };
    }

    private static bool IsStartupRegistered()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: false);
            return key?.GetValue("EJLive.Client") is not null;
        }
        catch
        {
            return false;
        }
    }

    private void SaveSettings()
    {
        if (!TryResolveRuntimeConfigFromInputs(out var runtimeConfig, preferEnvironment: false, out var reason))
        {
            MessageBox.Show(reason, "Settings Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _runtimeConfigResolver.ApplyTo(_config, runtimeConfig);
        _config.ATM_Name = _atmName.Text;
        _config.ATM_Type = Convert.ToString(_atmType.SelectedItem) ?? AppConstants.ATM_TYPE_NCR;
        _config.NetworkType = Convert.ToString(_networkType.SelectedItem) ?? "LAN";
        _config.SourcePath = _sourcePath.Text;
        _config.BackupPath = _backupPath.Text;
        _config.Save();
        AgentConfigurationXmlService.SaveAppConfig(_config);
        AppendLog(_connectionLog, $"Settings saved ({runtimeConfig.ConnectionDisplay}).");
    }

    private bool TryResolveRuntimeConfigFromInputs(out RuntimeAgentConfig runtimeConfig, bool preferEnvironment, out string reason)
    {
        var serverPortInput = _serverPort is null ? (int?)null : Decimal.ToInt32(_serverPort.Value);
        return _runtimeConfigResolver.TryResolve(
            _config,
            out runtimeConfig,
            out reason,
            _serverIp?.Text,
            serverPortInput,
            _atmId?.Text,
            preferEnvironment);
    }

    private async Task QueueJournalForServerAnalysis()
    {
        var journalText = _journalText.Text;
        if (string.IsNullOrWhiteSpace(journalText))
        {
            AppendLog(_connectionLog, "No journal data loaded. Load a journal first.");
            return;
        }

        var payload = System.Text.Encoding.UTF8.GetBytes(journalText);
        var fileName = $"ANLREQ_{_config.ATM_ID}_{DateTime.UtcNow:yyyyMMddHHmmss}.log";
        var checksum = SecurityHelper.MD5Hash(payload);
        var item = _outbox.Enqueue(_config.ATM_ID, fileName, payload, 0, checksum);
        AppendLog(_connectionLog, $"Queued for server analysis: {fileName} ({payload.Length} bytes).");

        if (_networkEngine?.IsConnected == true)
        {
            var sent = await _networkEngine.SendJournalFileAsync(item.FileName, item.Data, item.Offset, item.Checksum).ConfigureAwait(false);
            if (sent)
            {
                _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));
                AppendLog(_connectionLog, $"Sent to server analytics (awaiting ACK): {item.FileName}.");
            }
            else
            {
                AppendLog(_connectionLog, $"Send pending for analytics file: {item.FileName}.");
            }
        }
        else
        {
            AppendLog(_connectionLog, "Client is offline. Analysis file will be sent automatically on reconnect.");
        }

        RefreshSyncGrid(true);
        UpdateOverviewCards();
    }

    private void ActivateWindowsService()
    {
        var path = ResolveServiceExecutablePath();
        if (string.IsNullOrWhiteSpace(path))
        {
            AppendLog(_connectionLog, "Service executable not found. Deploy EJLive.Client.Service first.");
            return;
        }

        var result = EJLive.Client.WinForms.Services.WindowsServiceRegistrationService.InstallOrUpdateService(path);
        AppendLog(_connectionLog, result.Message);
        if (result.Success)
            SetServiceState("Agent Controller", "Running", "Windows service installed/updated.");
    }

    private void StartWindowsService()
    {
        var result = EJLive.Client.WinForms.Services.WindowsServiceRegistrationService.StartService();
        AppendLog(_connectionLog, result.Message);
        if (result.Success)
            SetServiceState("Agent Controller", "Running", "Windows service started.");
    }

    private void QueryWindowsService()
    {
        var result = EJLive.Client.WinForms.Services.WindowsServiceRegistrationService.QueryService();
        AppendLog(_connectionLog, result.Message);
    }

    private static string ResolveServiceExecutablePath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "ClientService", "EJLive.Client.Service.exe"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EJLive.Client.Service.exe"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "EJLive.Client.Service.exe")
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return string.Empty;
    }

    private void LoadAgentConfig()
    {
        var record = AgentConfigurationXmlService.LoadOrCreate(_config);
        _agentConfigGrid.Rows.Clear();
        _agentConfigGrid.Rows.Add("ConfigPath", record.ConfigPath);
        foreach (var pair in record.Values)
            _agentConfigGrid.Rows.Add(pair.Key, pair.Value);
    }

    private void RefreshRuntimeState()
    {
        _clockLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        UpdateOverviewCards();
        RefreshSyncGrid();
    }

    private void RefreshSyncGrid(bool force = false)
    {
        if (_syncGrid is null)
            return;

        var snapshot = _outbox.Snapshot;
        var signature = string.Join(";", snapshot.Select(i => $"{i.ItemId}:{i.Status}:{i.RetryCount}:{i.Data.Length}"));
        if (!force && string.Equals(signature, _lastSyncSignature, StringComparison.Ordinal))
            return;

        _lastSyncSignature = signature;
        _syncGrid.SuspendLayout();
        try
        {
            _syncGrid.Rows.Clear();
            foreach (var item in snapshot)
                _syncGrid.Rows.Add(item.ItemId, item.FileName, item.Status, item.RetryCount, item.Data.Length);
        }
        finally
        {
            _syncGrid.ResumeLayout();
        }
    }

    private void UpdateOverviewCards()
    {
        if (_connectionStateValue is null)
            return;

        _atmInfo.RecalculateHealthScore();
        var queued = _outbox.Count;
        _connectionStateValue.Text = _networkEngine?.IsConnected == true ? "Connected" : _atmInfo.ConnectionStatus.ToString();
        _queueValue.Text = $"{queued} queued";
        _healthValue.Text = $"{_atmInfo.HealthScore}%";
        _lastDataValue.Text = _atmInfo.LastDataReceivedUtc == DateTime.MinValue
            ? "-"
            : _atmInfo.LastDataReceivedUtc.ToLocalTime().ToString("HH:mm:ss");
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed)
            return;
        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private static void BrowseTo(TextBox target)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
            target.Text = dialog.SelectedPath;
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
    }

    private static void AppendLog(RichTextBox? box, string message)
    {
        if (box is null)
            return;
        if (box.InvokeRequired)
        {
            box.BeginInvoke(() => AppendLog(box, message));
            return;
        }
        box.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        box.ScrollToCaret();
    }

    private static void AddRow(TableLayoutPanel panel, string label, Control control)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(control);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        _fileWatcher.Dispose();
        _networkEngine?.Dispose();
        _remotePreview?.Image?.Dispose();
        base.OnFormClosed(e);
    }
}

internal static class Ui
{
    public static FlowLayoutPanel Flow() => new() { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
    public static Panel Stack(DockStyle dock) => new() { Dock = dock, Padding = new Padding(8) };
    public static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4), FlatStyle = FlatStyle.System };
        button.Click += (_, _) => action();
        return button;
    }
    public static TableLayoutPanel CardRow(int columns)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Top, Height = 92, ColumnCount = columns, Padding = new Padding(4) };
        for (var i = 0; i < columns; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        return row;
    }

    public static Label AddMetricCard(TableLayoutPanel row, string title, string value, Color accent)
    {
        var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White };
        var accentBar = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
        var titleLabel = new Label { Text = title, Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F) };
        var valueLabel = new Label { Text = value, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(35, 35, 35), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        row.Controls.Add(card);
        return valueLabel;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
        grid.EnableDoubleBuffering();
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        return grid;
    }
    public static RichTextBox LogBox() => new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), BorderStyle = BorderStyle.FixedSingle };
    public static TextBox Text(string text) => new() { Text = text, Dock = DockStyle.Fill };
    public static ComboBox Combo(IEnumerable<string> values, string selected)
    {
        var combo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        combo.Items.AddRange(values.Cast<object>().ToArray());
        combo.SelectedItem = selected;
        if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
            combo.SelectedIndex = 0;
        return combo;
    }
}

internal static class ControlRenderingExtensions
{
    public static void EnableDoubleBuffering(this Control control)
    {
        var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property?.SetValue(control, true, null);
    }
}

internal static class ToolStripExtensions
{
    public static Label AsLabel(this ToolStripStatusLabel label)
    {
        return new Label { Text = label.Text, AutoSize = true, Padding = new Padding(8, 3, 8, 3) };
    }
}
