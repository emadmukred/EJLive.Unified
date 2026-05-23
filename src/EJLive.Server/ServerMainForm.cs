using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;

namespace EJLive.Server
{
    /// <summary>
    /// EJLive Server v3.4 - واجهة السيرفر المحسّنة
    /// تبويبات: Server | Connections | Archive | Remote | Analytics | Log
    /// لوحة NOC مع 8 مقاييس + بطاقات ATM ملونة متقدمة
    /// نظام بطاقات: أخضر (متصل/مزامن) | أصفر (متصل/غير مزامن) | أحمر (خطأ) | رمادي (غير متصل) | برتقالي (Supervisor)
    /// </summary>
    public class ServerMainForm : Form
    {
        #region Controls
        private TabControl tabMain;
        private TabPage tabServer, tabConnections, tabArchive, tabRemote, tabAnalytics, tabLog;

        // Tab Server - NOC Dashboard
        private Panel pnlNOC;
        private Label lblTotalATMs, lblConnected, lblSyncing, lblErrors, lblOffline, lblSupervisor;
        private Label lblTotalATMsVal, lblConnectedVal, lblSyncingVal, lblErrorsVal, lblOfflineVal, lblSupervisorVal;
        private Label lblBandwidth, lblBandwidthVal, lblUptime, lblUptimeVal;
        private FlowLayoutPanel flpATMCards;
        private GroupBox grpServerControl;
        private Button btnStartServer, btnStopServer;
        private NumericUpDown numPort;
        private Label lblServerStatus, lblPort;
        private TextBox txtArchivePath;
        private Button btnBrowseArchive;

        // Tab Connections
        private DataGridView dgvConnections;
        private Panel pnlConnectionToolbar;
        private Button btnRefreshConnections, btnDisconnectSelected, btnBroadcastMsg;
        private Label lblConnectionCount;

        // Tab Archive
        private DataGridView dgvArchive;
        private Panel pnlArchiveToolbar;
        private DateTimePicker dtpArchiveFrom, dtpArchiveTo;
        private ComboBox cmbArchiveATM;
        private Button btnArchiveSearch, btnArchiveExport, btnArchiveOpen;
        private Label lblArchiveStats;

        // Tab Remote
        private ComboBox cmbRemoteATM;
        private Button btnCmdRestart, btnCmdScreenshot, btnCmdTimeSync, btnCmdSysInfo;
        private Button btnCmdGhostStart, btnCmdGhostStop, btnCmdImageSync;
        private Button btnBroadcastRestart, btnBroadcastTimeSync;
        private PictureBox picGhostView;
        private RichTextBox rtbRemoteResult;
        private Label lblRemoteStatus;

        // Tab Analytics
        private ComboBox cmbAnalyticsATM;
        private DateTimePicker dtpAnalyticsFrom, dtpAnalyticsTo;
        private Button btnRunAnalysis, btnExportReport;
        private DataGridView dgvAnalytics;
        private Label lblAnalyticsSummary;

        // Tab Log
        private RichTextBox rtbLog;
        private Panel pnlLogToolbar;
        private Button btnClearLog, btnSaveLog;
        private CheckBox chkAutoScroll;

        // Status Bar
        private StatusStrip statusBar;
        private ToolStripStatusLabel lblStatus, lblServerState, lblClientsCount, lblVersion, lblClock;
        #endregion

        #region Engine References
        private ServerEngine _serverEngine;
        private TransactionAnalysisEngine _analysisEngine;
        private GhostRemoteEngine _ghostEngine;
        private ImageSyncEngine _imageSyncEngine;
        private ReportExportEngine _reportEngine;
        private Dictionary<string, ATMCardPanel> _atmCards;
        private System.Windows.Forms.Timer _refreshTimer, _clockTimer;
        private bool _serverRunning;
        private string _archivePath = @"D:\EJOURNAL Files\Archive";
        #endregion

        #region Constructor
        public ServerMainForm()
        {
            _atmCards = new Dictionary<string, ATMCardPanel>();
            InitializeComponent();
            SetupTimers();
            Log("[System] EJLive Server v3.4.0 initialized", Color.Cyan);
        }
        #endregion

        #region UI Construction
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Text = "EJLive Server v3.4.0 - NOC Dashboard";
            this.Size = new Size(1050, 720);
            this.MinimumSize = new Size(1000, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.Icon = SystemIcons.Shield;

            tabMain = new TabControl { Dock = DockStyle.Fill };
            tabServer = new TabPage("Server");
            tabConnections = new TabPage("Connections");
            tabArchive = new TabPage("Archive");
            tabRemote = new TabPage("Remote");
            tabAnalytics = new TabPage("Analytics");
            tabLog = new TabPage("Log");
            tabMain.TabPages.AddRange(new[] { tabServer, tabConnections, tabArchive, tabRemote, tabAnalytics, tabLog });

            BuildServerTab();
            BuildConnectionsTab();
            BuildArchiveTab();
            BuildRemoteTab();
            BuildAnalyticsTab();
            BuildLogTab();
            BuildStatusBar();

            this.Controls.Add(tabMain);
            this.Controls.Add(statusBar);
            this.ResumeLayout(false);
        }

        private void BuildServerTab()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            tabServer.Controls.Add(mainPanel);

            // === NOC Dashboard - 8 مقاييس ===
            pnlNOC = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(26, 35, 60) };
            int mx = 15;
            AddNOCMetric(pnlNOC, "Total ATMs", "0", Color.FromArgb(33, 150, 243), ref mx, out lblTotalATMs, out lblTotalATMsVal);
            AddNOCMetric(pnlNOC, "Connected", "0", Color.FromArgb(76, 175, 80), ref mx, out lblConnected, out lblConnectedVal);
            AddNOCMetric(pnlNOC, "Syncing", "0", Color.FromArgb(0, 188, 212), ref mx, out lblSyncing, out lblSyncingVal);
            AddNOCMetric(pnlNOC, "Errors", "0", Color.FromArgb(244, 67, 54), ref mx, out lblErrors, out lblErrorsVal);
            AddNOCMetric(pnlNOC, "Offline", "0", Color.FromArgb(158, 158, 158), ref mx, out lblOffline, out lblOfflineVal);
            AddNOCMetric(pnlNOC, "Supervisor", "0", Color.FromArgb(255, 152, 0), ref mx, out lblSupervisor, out lblSupervisorVal);
            AddNOCMetric(pnlNOC, "Bandwidth", "0 KB/s", Color.FromArgb(156, 39, 176), ref mx, out lblBandwidth, out lblBandwidthVal);
            AddNOCMetric(pnlNOC, "Uptime", "00:00:00", Color.FromArgb(121, 85, 72), ref mx, out lblUptime, out lblUptimeVal);
            mainPanel.Controls.Add(pnlNOC);

            // === Server Control ===
            grpServerControl = new GroupBox { Text = "Server Control", Location = new Point(5, 95), Size = new Size(1010, 60), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lblPort = new Label { Text = "Port:", Location = new Point(10, 25), AutoSize = true };
            numPort = new NumericUpDown { Location = new Point(45, 22), Width = 70, Minimum = 1024, Maximum = 65535, Value = 5656 };
            btnStartServer = new Button { Text = "Start Server", Location = new Point(130, 20), Size = new Size(110, 28), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnStopServer = new Button { Text = "Stop Server", Location = new Point(250, 20), Size = new Size(100, 28), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Enabled = false };
            lblServerStatus = new Label { Text = "Status: Stopped", Location = new Point(370, 25), AutoSize = true, ForeColor = Color.Gray };
            new Label { Text = "Archive:", Location = new Point(530, 25), AutoSize = true, Parent = grpServerControl };
            txtArchivePath = new TextBox { Location = new Point(585, 22), Width = 320, Text = _archivePath, ReadOnly = true };
            btnBrowseArchive = new Button { Text = "...", Location = new Point(910, 20), Size = new Size(40, 28) };
            btnStartServer.Click += BtnStartServer_Click;
            btnStopServer.Click += BtnStopServer_Click;
            btnBrowseArchive.Click += (s, e) => { using (var fbd = new FolderBrowserDialog()) { if (fbd.ShowDialog() == DialogResult.OK) { txtArchivePath.Text = fbd.SelectedPath; _archivePath = fbd.SelectedPath; } } };
            grpServerControl.Controls.AddRange(new Control[] { lblPort, numPort, btnStartServer, btnStopServer, lblServerStatus, txtArchivePath, btnBrowseArchive });
            mainPanel.Controls.Add(grpServerControl);

            // === ATM Cards Flow ===
            flpATMCards = new FlowLayoutPanel { Location = new Point(5, 160), Size = new Size(1010, 480), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, AutoScroll = true, BackColor = Color.FromArgb(240, 242, 245) };
            mainPanel.Controls.Add(flpATMCards);
        }

        private void AddNOCMetric(Panel parent, string label, string value, Color color, ref int x, out Label lblLabel, out Label lblValue)
        {
            var panel = new Panel { Location = new Point(x, 10), Size = new Size(115, 70), BackColor = Color.FromArgb(35, 45, 75) };
            panel.Paint += (s, e) => { using (var pen = new Pen(color, 2)) e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1); };
            lblValue = new Label { Text = value, Location = new Point(5, 8), Size = new Size(105, 30), ForeColor = color, Font = new Font("Segoe UI", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            lblLabel = new Label { Text = label, Location = new Point(5, 42), Size = new Size(105, 20), ForeColor = Color.LightGray, Font = new Font("Segoe UI", 8), TextAlign = ContentAlignment.MiddleCenter };
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblLabel);
            parent.Controls.Add(panel);
            x += 125;
        }

        private void BuildConnectionsTab()
        {
            pnlConnectionToolbar = new Panel { Dock = DockStyle.Top, Height = 40 };
            btnRefreshConnections = new Button { Text = "Refresh", Location = new Point(5, 7), Size = new Size(80, 26) };
            btnDisconnectSelected = new Button { Text = "Disconnect", Location = new Point(90, 7), Size = new Size(90, 26), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnBroadcastMsg = new Button { Text = "Broadcast Message", Location = new Point(190, 7), Size = new Size(130, 26) };
            lblConnectionCount = new Label { Text = "Connections: 0", Location = new Point(340, 12), AutoSize = true };
            btnRefreshConnections.Click += (s, e) => RefreshConnectionsGrid();
            btnDisconnectSelected.Click += BtnDisconnectSelected_Click;
            btnBroadcastMsg.Click += BtnBroadcastMsg_Click;
            pnlConnectionToolbar.Controls.AddRange(new Control[] { btnRefreshConnections, btnDisconnectSelected, btnBroadcastMsg, lblConnectionCount });
            tabConnections.Controls.Add(pnlConnectionToolbar);

            dgvConnections = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvConnections.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM_ID", HeaderText = "ATM ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "IP", HeaderText = "IP Address", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Connected", HeaderText = "Connected At", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "LastSync", HeaderText = "Last Sync", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "Latency", HeaderText = "Latency", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Network", HeaderText = "Network", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Files", HeaderText = "Files", Width = 50 }
            });
            tabConnections.Controls.Add(dgvConnections);
        }

        private void BuildArchiveTab()
        {
            pnlArchiveToolbar = new Panel { Dock = DockStyle.Top, Height = 45 };
            dtpArchiveFrom = new DateTimePicker { Location = new Point(10, 10), Width = 130, Format = DateTimePickerFormat.Short };
            new Label { Text = "to", Location = new Point(145, 14), AutoSize = true, Parent = pnlArchiveToolbar };
            dtpArchiveTo = new DateTimePicker { Location = new Point(165, 10), Width = 130, Format = DateTimePickerFormat.Short };
            cmbArchiveATM = new ComboBox { Location = new Point(310, 10), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbArchiveATM.Items.Add("All ATMs");
            cmbArchiveATM.SelectedIndex = 0;
            btnArchiveSearch = new Button { Text = "Search", Location = new Point(420, 8), Size = new Size(75, 26), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnArchiveExport = new Button { Text = "Export", Location = new Point(500, 8), Size = new Size(75, 26), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnArchiveOpen = new Button { Text = "Open File", Location = new Point(580, 8), Size = new Size(80, 26) };
            lblArchiveStats = new Label { Text = "0 files", Location = new Point(670, 14), AutoSize = true };
            btnArchiveSearch.Click += BtnArchiveSearch_Click;
            btnArchiveExport.Click += BtnArchiveExport_Click;
            btnArchiveOpen.Click += BtnArchiveOpen_Click;
            pnlArchiveToolbar.Controls.AddRange(new Control[] { dtpArchiveFrom, dtpArchiveTo, cmbArchiveATM, btnArchiveSearch, btnArchiveExport, btnArchiveOpen, lblArchiveStats });
            tabArchive.Controls.Add(pnlArchiveToolbar);

            dgvArchive = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            dgvArchive.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM_ID", HeaderText = "ATM", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "File Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "Size", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Checksum", HeaderText = "Checksum", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Path", Width = 200 }
            });
            tabArchive.Controls.Add(dgvArchive);
        }

        private void BuildRemoteTab()
        {
            var splitRemote = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 350 };
            tabRemote.Controls.Add(splitRemote);

            var pnlCommands = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblSelectATM = new Label { Text = "Select ATM:", Location = new Point(10, 10), AutoSize = true };
            cmbRemoteATM = new ComboBox { Location = new Point(90, 7), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            lblRemoteStatus = new Label { Text = "Ready", Location = new Point(250, 10), AutoSize = true, ForeColor = Color.Green };

            int by = 45;
            btnCmdRestart = CreateCmdButton("Restart ATM", new Point(10, by), Color.FromArgb(244, 67, 54)); by += 35;
            btnCmdScreenshot = CreateCmdButton("Screenshot", new Point(10, by), Color.FromArgb(33, 150, 243)); by += 35;
            btnCmdTimeSync = CreateCmdButton("Time Sync", new Point(10, by), Color.FromArgb(76, 175, 80)); by += 35;
            btnCmdSysInfo = CreateCmdButton("System Info", new Point(10, by), Color.FromArgb(156, 39, 176)); by += 35;
            btnCmdGhostStart = CreateCmdButton("Ghost View Start", new Point(10, by), Color.FromArgb(255, 152, 0)); by += 35;
            btnCmdGhostStop = CreateCmdButton("Ghost View Stop", new Point(10, by), Color.FromArgb(158, 158, 158)); by += 35;
            btnCmdImageSync = CreateCmdButton("Sync Images", new Point(10, by), Color.FromArgb(0, 150, 136)); by += 50;

            new Label { Text = "--- Broadcast ---", Location = new Point(10, by), AutoSize = true, ForeColor = Color.Gray, Parent = pnlCommands }; by += 25;
            btnBroadcastRestart = CreateCmdButton("Broadcast Restart", new Point(10, by), Color.FromArgb(183, 28, 28)); by += 35;
            btnBroadcastTimeSync = CreateCmdButton("Broadcast TimeSync", new Point(10, by), Color.FromArgb(27, 94, 32));

            btnCmdRestart.Click += (s, e) => SendRemoteCommand("CMD_RESTART");
            btnCmdScreenshot.Click += (s, e) => SendRemoteCommand("CMD_SCREENSHOT");
            btnCmdTimeSync.Click += (s, e) => SendRemoteCommand("CMD_TIMESYNC");
            btnCmdSysInfo.Click += (s, e) => SendRemoteCommand("CMD_SYSINFO");
            btnCmdGhostStart.Click += (s, e) => SendRemoteCommand("CMD_GHOST_START");
            btnCmdGhostStop.Click += (s, e) => SendRemoteCommand("CMD_GHOST_STOP");
            btnCmdImageSync.Click += (s, e) => SendRemoteCommand("CMD_IMAGE_SYNC");
            btnBroadcastRestart.Click += (s, e) => BroadcastCommand("CMD_RESTART");
            btnBroadcastTimeSync.Click += (s, e) => BroadcastCommand("CMD_TIMESYNC");

            pnlCommands.Controls.AddRange(new Control[] { lblSelectATM, cmbRemoteATM, lblRemoteStatus, btnCmdRestart, btnCmdScreenshot, btnCmdTimeSync, btnCmdSysInfo, btnCmdGhostStart, btnCmdGhostStop, btnCmdImageSync, btnBroadcastRestart, btnBroadcastTimeSync });
            splitRemote.Panel1.Controls.Add(pnlCommands);

            var splitRight = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350 };
            picGhostView = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            new Label { Text = "Ghost Remote View", Dock = DockStyle.Top, Height = 20, BackColor = Color.FromArgb(40, 40, 50), ForeColor = Color.Orange, TextAlign = ContentAlignment.MiddleCenter, Parent = splitRight.Panel1 };
            splitRight.Panel1.Controls.Add(picGhostView);
            rtbRemoteResult = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(30, 30, 40), ForeColor = Color.LightGreen, Font = new Font("Consolas", 9) };
            splitRight.Panel2.Controls.Add(rtbRemoteResult);
            splitRemote.Panel2.Controls.Add(splitRight);
        }

        private void BuildAnalyticsTab()
        {
            var pnlAnalyticsTop = new Panel { Dock = DockStyle.Top, Height = 80 };
            new Label { Text = "ATM:", Location = new Point(10, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            cmbAnalyticsATM = new ComboBox { Location = new Point(50, 9), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Parent = pnlAnalyticsTop };
            cmbAnalyticsATM.Items.Add("All");
            cmbAnalyticsATM.SelectedIndex = 0;
            new Label { Text = "From:", Location = new Point(185, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            dtpAnalyticsFrom = new DateTimePicker { Location = new Point(225, 9), Width = 130, Format = DateTimePickerFormat.Short, Parent = pnlAnalyticsTop };
            new Label { Text = "To:", Location = new Point(365, 12), AutoSize = true, Parent = pnlAnalyticsTop };
            dtpAnalyticsTo = new DateTimePicker { Location = new Point(390, 9), Width = 130, Format = DateTimePickerFormat.Short, Parent = pnlAnalyticsTop };
            btnRunAnalysis = new Button { Text = "Run Analysis", Location = new Point(535, 7), Size = new Size(100, 26), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Parent = pnlAnalyticsTop };
            btnExportReport = new Button { Text = "Export Report", Location = new Point(645, 7), Size = new Size(100, 26), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Parent = pnlAnalyticsTop };
            lblAnalyticsSummary = new Label { Text = "", Location = new Point(10, 45), Size = new Size(800, 25), ForeColor = Color.DarkBlue, Font = new Font("Segoe UI", 9, FontStyle.Bold), Parent = pnlAnalyticsTop };
            btnRunAnalysis.Click += BtnRunAnalysis_Click;
            btnExportReport.Click += BtnExportReport_Click;
            tabAnalytics.Controls.Add(pnlAnalyticsTop);

            dgvAnalytics = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            dgvAnalytics.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ATM", HeaderText = "ATM", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "Total Tx", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Success", HeaderText = "Success", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Failed", HeaderText = "Failed", Width = 55 },
                new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "Rate %", Width = 55 },
                new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Retained", HeaderText = "Cards", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Errors", HeaderText = "Errors", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "LastTx", HeaderText = "Last Tx", Width = 80 }
            });
            tabAnalytics.Controls.Add(dgvAnalytics);
        }

        private void BuildLogTab()
        {
            pnlLogToolbar = new Panel { Dock = DockStyle.Top, Height = 35 };
            btnClearLog = new Button { Text = "Clear", Location = new Point(5, 5), Size = new Size(60, 25) };
            btnSaveLog = new Button { Text = "Save", Location = new Point(70, 5), Size = new Size(60, 25) };
            chkAutoScroll = new CheckBox { Text = "Auto Scroll", Location = new Point(145, 8), AutoSize = true, Checked = true };
            btnClearLog.Click += (s, e) => rtbLog.Clear();
            btnSaveLog.Click += (s, e) => { using (var sfd = new SaveFileDialog { Filter = "Log|*.log" }) { if (sfd.ShowDialog() == DialogResult.OK) File.WriteAllText(sfd.FileName, rtbLog.Text); } };
            pnlLogToolbar.Controls.AddRange(new Control[] { btnClearLog, btnSaveLog, chkAutoScroll });
            tabLog.Controls.Add(pnlLogToolbar);

            rtbLog = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(25, 25, 35), ForeColor = Color.LightGray, Font = new Font("Consolas", 9) };
            tabLog.Controls.Add(rtbLog);
        }

        private void BuildStatusBar()
        {
            statusBar = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Ready") { Width = 100 };
            lblServerState = new ToolStripStatusLabel("Server: Stopped") { ForeColor = Color.Red };
            lblClientsCount = new ToolStripStatusLabel("Clients: 0");
            lblVersion = new ToolStripStatusLabel("v3.4.0") { Alignment = ToolStripItemAlignment.Right };
            lblClock = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm:ss")) { Alignment = ToolStripItemAlignment.Right };
            statusBar.Items.AddRange(new ToolStripItem[] { lblStatus, lblServerState, lblClientsCount, lblVersion, lblClock });
        }

        private Button CreateCmdButton(string text, Point location, Color color)
        {
            return new Button { Text = text, Location = location, Size = new Size(200, 28), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
        }
        #endregion

        #region Server Control
        private void BtnStartServer_Click(object sender, EventArgs e)
        {
            int port = (int)numPort.Value;
            _serverEngine = new ServerEngine(port, _archivePath);
            _serverEngine.OnLog += (msg) => SafeLog(msg, Color.White);
            _serverEngine.OnError += (ex) => SafeLog($"[Server Error] {ex.Message}", Color.Red);
            _serverEngine.OnClientConnected += ServerEngine_OnClientConnected;
            _serverEngine.OnClientDisconnected += ServerEngine_OnClientDisconnected;
            _serverEngine.OnJournalReceived += ServerEngine_OnJournalReceived;
            _serverEngine.OnCommandResult += ServerEngine_OnCommandResult;
            _serverEngine.Start();

            _imageSyncEngine = new ImageSyncEngine(_archivePath + @"\Images");
            _imageSyncEngine.OnLog += (msg) => SafeLog(msg, Color.White);
            _imageSyncEngine.Start();

            _reportEngine = new ReportExportEngine(_archivePath + @"\Reports");

            _serverRunning = true;
            btnStartServer.Enabled = false; btnStopServer.Enabled = true;
            lblServerStatus.Text = $"Status: Running on port {port}"; lblServerStatus.ForeColor = Color.Green;
            lblServerState.Text = $"Server: Running (:{port})"; lblServerState.ForeColor = Color.Green;
            Log($"[Server] Started on port {port}", Color.LightGreen);
            Log($"[Server] Archive: {_archivePath}", Color.Cyan);
        }

        private void BtnStopServer_Click(object sender, EventArgs e)
        {
            if (_serverEngine != null) { _serverEngine.Stop(); _serverEngine = null; }
            if (_imageSyncEngine != null) { _imageSyncEngine.Stop(); _imageSyncEngine = null; }
            _serverRunning = false;
            btnStartServer.Enabled = true; btnStopServer.Enabled = false;
            lblServerStatus.Text = "Status: Stopped"; lblServerStatus.ForeColor = Color.Gray;
            lblServerState.Text = "Server: Stopped"; lblServerState.ForeColor = Color.Red;
            Log("[Server] Stopped", Color.Yellow);
        }
        #endregion

        #region Server Events
        private void ServerEngine_OnClientConnected(string atmId, string atmType, string ip)
        {
            SafeInvoke(() =>
            {
                AddOrUpdateATMCard(atmId, atmType, ip, ATMCardStatus.Connected);
                if (!cmbRemoteATM.Items.Contains(atmId)) cmbRemoteATM.Items.Add(atmId);
                if (!cmbArchiveATM.Items.Contains(atmId)) cmbArchiveATM.Items.Add(atmId);
                if (!cmbAnalyticsATM.Items.Contains(atmId)) cmbAnalyticsATM.Items.Add(atmId);
                _imageSyncEngine?.RegisterATM(atmId, atmType);
                UpdateNOCMetrics();
            });
            Log($"[Connected] {atmId} ({atmType}) from {ip}", Color.LightGreen);
        }

        private void ServerEngine_OnClientDisconnected(string atmId)
        {
            SafeInvoke(() =>
            {
                if (_atmCards.ContainsKey(atmId)) _atmCards[atmId].SetStatus(ATMCardStatus.Offline);
                _imageSyncEngine?.UnregisterATM(atmId);
                UpdateNOCMetrics();
            });
            Log($"[Disconnected] {atmId}", Color.Red);
        }

        private void ServerEngine_OnJournalReceived(string atmId, string fileName, long size)
        {
            SafeInvoke(() =>
            {
                if (_atmCards.ContainsKey(atmId)) { _atmCards[atmId].SetStatus(ATMCardStatus.Syncing); _atmCards[atmId].UpdateLastSync(DateTime.Now); }
                UpdateNOCMetrics();
            });
            Log($"[Journal] {atmId}: {fileName} ({size} bytes)", Color.Cyan);
        }

        private void ServerEngine_OnCommandResult(string atmId, string command, bool success, string result)
        {
            SafeInvoke(() =>
            {
                rtbRemoteResult.SelectionColor = success ? Color.LightGreen : Color.Red;
                rtbRemoteResult.AppendText($"[{DateTime.Now:HH:mm:ss}] {atmId} > {command}: {(success ? "OK" : "FAILED")} - {result}\n");
            });
            Log($"[Result] {atmId} > {command}: {(success ? "OK" : "FAIL")}", success ? Color.LightGreen : Color.Red);
        }
        #endregion

        #region Remote Commands
        private void SendRemoteCommand(string command)
        {
            if (cmbRemoteATM.SelectedItem == null) { MessageBox.Show("Select ATM first"); return; }
            string atmId = cmbRemoteATM.SelectedItem.ToString();
            if (_serverEngine != null)
            {
                _serverEngine.SendCommand(atmId, command, new string[0]);
                Log($"[Command] Sent {command} to {atmId}", Color.Cyan);
                lblRemoteStatus.Text = $"Sent: {command}"; lblRemoteStatus.ForeColor = Color.Orange;
            }
        }

        private void BroadcastCommand(string command)
        {
            if (_serverEngine == null) return;
            if (MessageBox.Show($"Broadcast '{command}' to ALL connected ATMs?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _serverEngine.BroadcastCommand(command, new string[0]);
                Log($"[Broadcast] {command} to all ATMs", Color.FromArgb(255, 152, 0));
            }
        }

        private void BtnDisconnectSelected_Click(object sender, EventArgs e)
        {
            if (dgvConnections.SelectedRows.Count == 0) return;
            string atmId = dgvConnections.SelectedRows[0].Cells["ATM_ID"].Value?.ToString();
            if (!string.IsNullOrEmpty(atmId) && _serverEngine != null) { _serverEngine.DisconnectClient(atmId); Log($"[Disconnect] {atmId}", Color.Yellow); }
        }

        private void BtnBroadcastMsg_Click(object sender, EventArgs e)
        {
            string msg = "";
            using (var inputForm = new Form { Text = "Broadcast Message", Size = new Size(350, 130), StartPosition = FormStartPosition.CenterParent })
            {
                var txt = new TextBox { Location = new Point(10, 10), Width = 310 };
                var btn = new Button { Text = "Send", Location = new Point(130, 45), Size = new Size(80, 28), DialogResult = DialogResult.OK };
                inputForm.Controls.AddRange(new Control[] { txt, btn });
                inputForm.AcceptButton = btn;
                if (inputForm.ShowDialog() == DialogResult.OK) msg = txt.Text;
            }
            if (!string.IsNullOrEmpty(msg) && _serverEngine != null) { _serverEngine.BroadcastCommand("CMD_MESSAGE", new[] { msg }); Log($"[Broadcast] Message: {msg}", Color.Cyan); }
        }
        #endregion

        #region Archive
        private void BtnArchiveSearch_Click(object sender, EventArgs e)
        {
            dgvArchive.Rows.Clear();
            if (!Directory.Exists(_archivePath)) { MessageBox.Show("Archive path not found"); return; }
            string filter = cmbArchiveATM.SelectedItem?.ToString();
            var files = Directory.GetFiles(_archivePath, "*.*", SearchOption.AllDirectories);
            int count = 0;
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                if (fi.LastWriteTime < dtpArchiveFrom.Value || fi.LastWriteTime > dtpArchiveTo.Value.AddDays(1)) continue;
                string atmId = Path.GetFileName(Path.GetDirectoryName(file));
                if (filter != "All ATMs" && atmId != filter) continue;
                dgvArchive.Rows.Add(atmId, fi.Name, fi.LastWriteTime.ToString("yyyy-MM-dd"), $"{fi.Length / 1024} KB", "", file);
                count++;
            }
            lblArchiveStats.Text = $"{count} files";
            Log($"[Archive] Found {count} files", Color.Cyan);
        }

        private void BtnArchiveExport_Click(object sender, EventArgs e)
        {
            if (dgvArchive.Rows.Count == 0) return;
            using (var fbd = new FolderBrowserDialog { Description = "Select export folder" })
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    int exported = 0;
                    foreach (DataGridViewRow row in dgvArchive.SelectedRows)
                    {
                        string path = row.Cells["Path"].Value?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        { File.Copy(path, Path.Combine(fbd.SelectedPath, Path.GetFileName(path)), true); exported++; }
                    }
                    Log($"[Archive] Exported {exported} files", Color.LightGreen);
                }
            }
        }

        private void BtnArchiveOpen_Click(object sender, EventArgs e)
        {
            if (dgvArchive.SelectedRows.Count == 0) return;
            string path = dgvArchive.SelectedRows[0].Cells["Path"].Value?.ToString();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            { try { System.Diagnostics.Process.Start("notepad.exe", path); } catch { } }
        }
        #endregion

        #region Analytics
        private void BtnRunAnalysis_Click(object sender, EventArgs e)
        {
            dgvAnalytics.Rows.Clear();
            Log("[Analytics] Running analysis...", Color.Cyan);
            if (!Directory.Exists(_archivePath)) { MessageBox.Show("Archive path not found"); return; }
            var atmDirs = Directory.GetDirectories(_archivePath);
            int totalTx = 0, totalSuccess = 0, totalFailed = 0;
            decimal totalAmount = 0;
            foreach (var dir in atmDirs)
            {
                string atmId = Path.GetFileName(dir);
                if (cmbAnalyticsATM.SelectedItem?.ToString() != "All" && atmId != cmbAnalyticsATM.SelectedItem?.ToString()) continue;
                var engine = new TransactionAnalysisEngine("NCR");
                var files = Directory.GetFiles(dir, "*.*");
                var allTx = new List<ATMTransaction>();
                foreach (var file in files) allTx.AddRange(engine.AnalyzeJournalFile(file));
                var filtered = allTx.Where(t => t.Timestamp >= dtpAnalyticsFrom.Value && t.Timestamp <= dtpAnalyticsTo.Value.AddDays(1)).ToList();
                int success = filtered.Count(t => t.IsSuccessful);
                int failed = filtered.Count(t => t.Status == TransactionStatus.Failed);
                decimal amount = filtered.Where(t => t.Type == TransactionType.CashWithdrawal && t.IsSuccessful).Sum(t => t.Amount);
                int retained = filtered.Count(t => t.Type == TransactionType.CardRetained);
                double rate = filtered.Count > 0 ? (double)success / filtered.Count * 100 : 0;
                string lastTx = filtered.Count > 0 ? filtered.Max(t => t.Timestamp).ToString("HH:mm:ss") : "-";
                dgvAnalytics.Rows.Add(atmId, filtered.Count, success, failed, $"{rate:F1}%", amount.ToString("N0"), retained, filtered.Count(t => t.Status == TransactionStatus.Failed), lastTx);
                totalTx += filtered.Count; totalSuccess += success; totalFailed += failed; totalAmount += amount;
            }
            double totalRate = totalTx > 0 ? (double)totalSuccess / totalTx * 100 : 0;
            lblAnalyticsSummary.Text = $"Total: {totalTx} Tx | Success: {totalSuccess} ({totalRate:F1}%) | Failed: {totalFailed} | Amount: {totalAmount:N0}";
            Log($"[Analytics] Complete: {totalTx} transactions analyzed", Color.LightGreen);
        }

        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (dgvAnalytics.Rows.Count == 0) { MessageBox.Show("Run analysis first"); return; }
            using (var sfd = new SaveFileDialog { Filter = "HTML|*.html|CSV|*.csv", FileName = $"Analysis_{DateTime.Now:yyyyMMdd}" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (_reportEngine != null)
                    {
                        var report = new TransactionAnalysisReport { ATM_ID = "ALL", FromDate = dtpAnalyticsFrom.Value, ToDate = dtpAnalyticsTo.Value, GeneratedAt = DateTime.Now };
                        if (sfd.FileName.EndsWith(".html")) _reportEngine.ExportAnalysisReportToHTML(report, Path.GetDirectoryName(sfd.FileName));
                        Log($"[Export] Report: {sfd.FileName}", Color.LightGreen);
                    }
                }
            }
        }
        #endregion

        #region ATM Cards
        private void AddOrUpdateATMCard(string atmId, string atmType, string ip, ATMCardStatus status)
        {
            if (_atmCards.ContainsKey(atmId))
            { _atmCards[atmId].SetStatus(status); _atmCards[atmId].UpdateIP(ip); }
            else
            {
                var card = new ATMCardPanel(atmId, atmType, ip);
                card.SetStatus(status);
                _atmCards[atmId] = card;
                flpATMCards.Controls.Add(card);
            }
        }

        private void UpdateNOCMetrics()
        {
            int total = _atmCards.Count;
            int connected = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Connected || c.Status == ATMCardStatus.Syncing);
            int syncing = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Syncing);
            int errors = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Error);
            int offline = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Offline);
            int supervisor = _atmCards.Values.Count(c => c.Status == ATMCardStatus.Supervisor);

            lblTotalATMsVal.Text = total.ToString();
            lblConnectedVal.Text = connected.ToString();
            lblSyncingVal.Text = syncing.ToString();
            lblErrorsVal.Text = errors.ToString();
            lblOfflineVal.Text = offline.ToString();
            lblSupervisorVal.Text = supervisor.ToString();
            lblClientsCount.Text = $"Clients: {connected}";
        }

        private void RefreshConnectionsGrid()
        {
            dgvConnections.Rows.Clear();
            foreach (var card in _atmCards.Values)
            {
                dgvConnections.Rows.Add(card.ATM_ID, card.IP, card.ATMType, card.Status.ToString(), card.ConnectedAt.ToString("HH:mm:ss"), card.LastSync.ToString("HH:mm:ss"), card.Latency + "ms", card.NetworkType, card.FilesReceived);
            }
            lblConnectionCount.Text = $"Connections: {_atmCards.Count}";
        }
        #endregion

        #region Helpers
        private void SetupTimers()
        {
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) => { if (_serverRunning) { UpdateNOCMetrics(); lblUptimeVal.Text = _serverEngine != null ? (DateTime.Now - _serverEngine.StartTime).ToString(@"hh\:mm\:ss") : "00:00:00"; } };
            _refreshTimer.Start();

            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) => { lblClock.Text = DateTime.Now.ToString("HH:mm:ss"); };
            _clockTimer.Start();
        }

        private void Log(string message, Color color)
        {
            if (rtbLog == null) return;
            if (rtbLog.InvokeRequired) { rtbLog.Invoke((Action)(() => Log(message, color))); return; }
            rtbLog.SelectionColor = Color.Gray;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(message + Environment.NewLine);
            if (chkAutoScroll != null && chkAutoScroll.Checked) rtbLog.ScrollToCaret();
        }

        private void SafeLog(string msg, Color c) { if (this.InvokeRequired) this.BeginInvoke((Action)(() => Log(msg, c))); else Log(msg, c); }
        private void SafeInvoke(Action a) { if (this.InvokeRequired) this.BeginInvoke(a); else a(); }
        protected override void OnFormClosing(FormClosingEventArgs e) { if (_serverEngine != null) _serverEngine.Stop(); base.OnFormClosing(e); }
        #endregion
    }

    #region ATM Card Panel
    public enum ATMCardStatus { Connected, Syncing, Error, Offline, Supervisor }

    /// <summary>
    /// بطاقة ATM ملونة - تعرض حالة الصراف بألوان مميزة
    /// أخضر=متصل | سماوي=مزامن | أحمر=خطأ | رمادي=غير متصل | برتقالي=Supervisor
    /// </summary>
    public class ATMCardPanel : Panel
    {
        public string ATM_ID { get; private set; }
        public string ATMType { get; private set; }
        public string IP { get; private set; }
        public ATMCardStatus Status { get; private set; }
        public DateTime ConnectedAt { get; private set; }
        public DateTime LastSync { get; private set; }
        public int Latency { get; set; }
        public string NetworkType { get; set; } = "LAN";
        public int FilesReceived { get; set; }

        private Label lblId, lblType, lblIP, lblStatus, lblLastSync;

        public ATMCardPanel(string atmId, string atmType, string ip)
        {
            ATM_ID = atmId; ATMType = atmType; IP = ip;
            ConnectedAt = DateTime.Now; LastSync = DateTime.Now;
            this.Size = new Size(180, 110);
            this.Margin = new Padding(5);
            this.BackColor = Color.White;

            lblId = new Label { Text = atmId, Location = new Point(5, 5), Size = new Size(170, 20), Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            lblType = new Label { Text = atmType, Location = new Point(5, 27), Size = new Size(170, 15), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
            lblIP = new Label { Text = ip, Location = new Point(5, 44), Size = new Size(170, 15), ForeColor = Color.DarkGray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };
            lblStatus = new Label { Text = "Connected", Location = new Point(5, 65), Size = new Size(170, 20), ForeColor = Color.Green, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            lblLastSync = new Label { Text = "Sync: --:--:--", Location = new Point(5, 88), Size = new Size(170, 15), ForeColor = Color.DarkGray, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 8) };

            this.Controls.AddRange(new Control[] { lblId, lblType, lblIP, lblStatus, lblLastSync });
            SetStatus(ATMCardStatus.Connected);
        }

        public void SetStatus(ATMCardStatus status)
        {
            Status = status;
            Color borderColor;
            string statusText;
            switch (status)
            {
                case ATMCardStatus.Connected: borderColor = Color.FromArgb(76, 175, 80); statusText = "Connected"; break;
                case ATMCardStatus.Syncing: borderColor = Color.FromArgb(0, 188, 212); statusText = "Syncing"; break;
                case ATMCardStatus.Error: borderColor = Color.FromArgb(244, 67, 54); statusText = "Error"; break;
                case ATMCardStatus.Offline: borderColor = Color.FromArgb(158, 158, 158); statusText = "Offline"; break;
                case ATMCardStatus.Supervisor: borderColor = Color.FromArgb(255, 152, 0); statusText = "Supervisor"; break;
                default: borderColor = Color.Gray; statusText = "Unknown"; break;
            }
            this.BackColor = Color.FromArgb(240, 245, 250);
            lblStatus.ForeColor = borderColor;
            lblStatus.Text = statusText;
            this.Invalidate();
        }

        public void UpdateLastSync(DateTime time) { LastSync = time; lblLastSync.Text = $"Sync: {time:HH:mm:ss}"; }
        public void UpdateIP(string ip) { IP = ip; lblIP.Text = ip; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color c = Status == ATMCardStatus.Connected ? Color.FromArgb(76, 175, 80) :
                      Status == ATMCardStatus.Syncing ? Color.FromArgb(0, 188, 212) :
                      Status == ATMCardStatus.Error ? Color.FromArgb(244, 67, 54) :
                      Status == ATMCardStatus.Supervisor ? Color.FromArgb(255, 152, 0) : Color.Gray;
            using (var pen = new Pen(c, 3)) e.Graphics.DrawRectangle(pen, 1, 1, Width - 3, Height - 3);
        }
    }
    #endregion
}
