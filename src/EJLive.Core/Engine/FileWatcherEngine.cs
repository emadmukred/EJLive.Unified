using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using EJLive.Core.Models;
using EJLive.Shared;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// محرك مراقبة ملفات الجورنال الكامل
    /// استراتيجيات مخصصة: NCR (Overwrite Mirroring + Offset) | GRG (Daily Files) | WN (Daily Files)
    /// يطبق: L-04 (Offset Tracking), T-01 (File Locking Fix), L-02 (Local Backup)
    /// يستخدم: FileSystemWatcher + Polling احتياطي كل 10 ثانية
    /// </summary>
    public class FileWatcherEngine : IDisposable
    {
        private readonly string  _atmId;
        private readonly string  _atmType;
        private readonly string  _sourcePath;
        private readonly string  _backupPath;
        private readonly JournalOutbox _outbox;

        private FileSystemWatcher _watcher;
        private Timer             _pollTimer;
        private volatile bool     _running;

        // تتبع Offset لكل ملف (L-04)
        private readonly Dictionary<string, long> _fileOffsets = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _lastChecksums = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _offsetLock = new object();

        public event EventHandler<JournalFileEvent> OnNewData;
        public event EventHandler<string>           OnError;

        // إحصائيات
        public long TotalBytesRead      { get; private set; }
        public int  TotalFilesDetected  { get; private set; }
        public DateTime LastActivityUtc { get; private set; }
        public bool IsRunning           => _running;
        public string SourcePath        => _sourcePath;
        public string BackupPath        => _backupPath;

        public FileWatcherEngine(string atmId, string atmType, string sourcePath, string backupPath, JournalOutbox outbox)
        {
            _atmId      = atmId;
            _atmType    = AppConstants.NormalizeATMType(atmType);
            _sourcePath = sourcePath;
            _backupPath = backupPath;
            _outbox     = outbox;

            if (!Directory.Exists(_backupPath))
                Directory.CreateDirectory(_backupPath);
        }

        // ==========================================
        // التشغيل والإيقاف
        // ==========================================

        public void Start()
        {
            if (_running) return;
            _running = true;
            AppLogger.Instance.Info($"FileWatcher started for {_atmId} ({_atmType}) — {_sourcePath}", "FileWatcher");

            // تحميل الـ Offsets المحفوظة من قاعدة البيانات
            LoadSavedOffsets();

            // FileSystemWatcher أولًا
            SetupFileSystemWatcher();

            // Polling احتياطي كل 10 ثانية (T-01 fallback)
            _pollTimer = new Timer(PollCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
        }

        public void Stop()
        {
            _running = false;
            _watcher?.Dispose();
            _pollTimer?.Dispose();
            SaveOffsets();
            AppLogger.Instance.Info($"FileWatcher stopped for {_atmId}", "FileWatcher");
        }

        private void SetupFileSystemWatcher()
        {
            try
            {
                if (!Directory.Exists(_sourcePath))
                {
                    AppLogger.Instance.Warning($"Source path not found: {_sourcePath}", "FileWatcher");
                    return;
                }

                _watcher = new FileSystemWatcher(_sourcePath)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter          = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                    EnableRaisingEvents   = true
                };

                // أنماط الملفات حسب نوع الصراف
                _watcher.Filter = "*.*";

                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                AppLogger.Instance.Info($"FileSystemWatcher active: {_sourcePath} [{_atmType} patterns]", "FileWatcher");
            }
            catch (Exception ex)
            {
                ReportError(ex, "FileWatcher.SetupWatcher");
            }
        }

        // ==========================================
        // معالجة أحداث الملفات
        // ==========================================

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!_running) return;
            if (!IsWatchedFile(e.FullPath)) return;
            ProcessFile(e.FullPath);
        }

        private void PollCallback(object state)
        {
            if (!_running) return;
            try
            {
                var files = GetWatchedFiles();
                foreach (var file in files)
                    ProcessFile(file);
            }
            catch (Exception ex)
            {
                ReportError(ex, "FileWatcher.Poll");
            }
        }

        private List<string> GetWatchedFiles()
        {
            var files = new List<string>();
            if (!Directory.Exists(_sourcePath)) return files;

            switch (_atmType)
            {
                case AppConstants.ATM_TYPE_NCR:
                    // NCR: 3 ملفات ثابتة
                    AddIfExists(files, Path.Combine(_sourcePath, "EJDATA.LOG"));
                    AddIfExists(files, Path.Combine(_sourcePath, "EJRCPY.LOG"));
                    AddIfExists(files, Path.Combine(_sourcePath, "EJDATA.LOb"));
                    break;
                case AppConstants.ATM_TYPE_GRG:
                    files.AddRange(Directory.GetFiles(_sourcePath, "EJ_*.dat"));
                    files.AddRange(Directory.GetFiles(_sourcePath, "TRACE*"));
                    break;
                case AppConstants.ATM_TYPE_WN:
                    files.AddRange(Directory.GetFiles(_sourcePath, "*.ej"));
                    files.AddRange(Directory.GetFiles(_sourcePath, "*.log"));
                    break;
                case AppConstants.ATM_TYPE_DN:
                    files.AddRange(Directory.GetFiles(_sourcePath, "*.jrn"));
                    files.AddRange(Directory.GetFiles(_sourcePath, "*.log"));
                    break;
                case AppConstants.ATM_TYPE_HY:
                    files.AddRange(Directory.GetFiles(_sourcePath, "EJ_*.dat"));
                    files.AddRange(Directory.GetFiles(_sourcePath, "*.log"));
                    break;
                default:
                    files.AddRange(Directory.GetFiles(_sourcePath));
                    break;
            }
            return files;
        }

        private void AddIfExists(List<string> list, string path)
        {
            if (File.Exists(path)) list.Add(path);
        }

        private bool IsWatchedFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            switch (_atmType)
            {
                case AppConstants.ATM_TYPE_NCR:
                    return string.Equals(fileName, AppConstants.NCR_EJData, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fileName, AppConstants.NCR_EJRcpy, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(fileName, AppConstants.NCR_EJDataLob, StringComparison.OrdinalIgnoreCase);
                case AppConstants.ATM_TYPE_GRG:
                    return WildcardMatch(fileName, AppConstants.GRG_FilePattern)
                        || WildcardMatch(fileName, AppConstants.GRG_TracePattern);
                case AppConstants.ATM_TYPE_WN:
                    return WildcardMatch(fileName, AppConstants.WN_EJPattern)
                        || WildcardMatch(fileName, AppConstants.WN_LogPattern);
                case AppConstants.ATM_TYPE_DN:
                    return WildcardMatch(fileName, AppConstants.DN_EJPattern)
                        || WildcardMatch(fileName, AppConstants.DN_LogPattern);
                case AppConstants.ATM_TYPE_HY:
                    return WildcardMatch(fileName, AppConstants.HY_EJPattern)
                        || WildcardMatch(fileName, AppConstants.HY_LogPattern);
                default:
                    return true;
            }
        }

        private static bool WildcardMatch(string input, string pattern)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(pattern)) return false;
            var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        // ==========================================
        // معالجة الملف الفردي (L-04 Offset Tracking)
        // ==========================================

        private void ProcessFile(string filePath)
        {
            if (!_running || !File.Exists(filePath)) return;

            // تجنب الاحتفاظ بالملف (T-01)
            var fileLen = SecurityHelper.GetFileLengthSafe(filePath);
            if (fileLen <= 0) return;

            var fileName = Path.GetFileName(filePath);
            lock (_offsetLock)
            {
                _fileOffsets.TryGetValue(fileName, out var lastOffset);

                // NCR: ملف ثابت يتحدث بالكتابة فوق نفسه — Overwrite Mirroring
                // GRG/WN: ملفات يومية تُضاف إليها
                long readFrom = AppConstants.IsOverwriteJournalMode(_atmType)
                    ? GetNCROffset(fileName, fileLen, lastOffset)
                    : lastOffset;

                if (fileLen <= readFrom) return; // لا يوجد بيانات جديدة

                // قراءة الجزء الجديد فقط (T-01: FileShare.ReadWrite)
                var newBytes = SecurityHelper.ReadFileChunk(filePath, readFrom, (int)Math.Min(524288, fileLen - readFrom));
                if (newBytes == null || newBytes.Length == 0) return;

                // فحص Checksum — هل تغيرت البيانات فعلًا؟
                var checksum = SecurityHelper.MD5Hash(newBytes);
                _lastChecksums.TryGetValue(fileName, out var lastChecksum);
                if (checksum == lastChecksum) return; // نفس البيانات — تجاهل

                _lastChecksums[fileName] = checksum;

                // تحديث Offset
                long newOffset = AppConstants.IsOverwriteJournalMode(_atmType)
                    ? readFrom + newBytes.Length
                    : fileLen;
                _fileOffsets[fileName] = newOffset;

                TotalBytesRead    += newBytes.Length;
                TotalFilesDetected++;
                LastActivityUtc    = DateTime.UtcNow;

                // نسخ احتياطي محلي (L-02)
                SaveLocalBackup(fileName, newBytes, readFrom);

                // إضافة للطابور
                var item = new JournalOutboxItem
                {
                    ATM_ID    = _atmId,
                    FileName  = fileName,
                    LocalPath = filePath,
                    Offset    = readFrom,
                    Data      = newBytes,
                    DataSize  = newBytes.Length,
                    Checksum  = checksum,
                    SHA256    = SecurityHelper.SHA256Hash(newBytes)
                };
                _outbox?.Enqueue(item);

                // إطلاق حدث
                OnNewData?.Invoke(this, new JournalFileEvent
                {
                    ATMId       = _atmId,
                    FilePath    = filePath,
                    FileName    = fileName,
                    NewOffset   = newOffset,
                    BytesRead   = newBytes.Length,
                    Checksum    = checksum,
                    DetectedAt  = DateTime.UtcNow
                });
            }
        }

        // ==========================================
        // منطق NCR الخاص (Overwrite Mirroring)
        // ==========================================

        private long GetNCROffset(string fileName, long currentSize, long lastOffset)
        {
            // NCR يكتب فوق نفسه — إذا تقلص الملف يعني كتابة من البداية
            if (currentSize < lastOffset)
            {
                AppLogger.Instance.Info($"NCR file reset detected: {fileName} ({lastOffset} -> {currentSize})", "FileWatcher");
                return 0; // إعادة من البداية
            }
            return lastOffset;
        }

        // ==========================================
        // النسخ الاحتياطي المحلي (L-02)
        // ==========================================

        private void SaveLocalBackup(string fileName, byte[] data, long offset)
        {
            try
            {
                var backupFile = Path.Combine(_backupPath, $"{fileName}.{DateTime.UtcNow:yyyyMMdd}");
                using var fs   = new FileStream(backupFile, FileMode.Append, FileAccess.Write, FileShare.Read);
                fs.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                ReportError(ex, "FileWatcher.SaveBackup");
            }
        }

        private void ReportError(Exception ex, string context)
        {
            AppLogger.Instance.Error(ex, context);
            OnError?.Invoke(this, $"{context}: {ex.Message}");
        }

        // ==========================================
        // حفظ واسترجاع الـ Offsets (L-01)
        // ==========================================

        private void LoadSavedOffsets()
        {
            try
            {
                var pending = Services.DatabaseManager.Instance.GetPendingSyncRecords(_atmId);
                foreach (var rec in pending)
                {
                    if (!_fileOffsets.ContainsKey(rec.FileName) || _fileOffsets[rec.FileName] < rec.FileOffset)
                        _fileOffsets[rec.FileName] = rec.FileOffset;
                }
            }
            catch { }
        }

        private void SaveOffsets()
        {
            // يُحفظ تلقائيًا في قاعدة البيانات عند كل مزامنة ناجحة
        }

        public void Dispose()
        {
            Stop();
        }
    }

    // ==========================================
    // حدث اكتشاف البيانات
    // ==========================================

    public class JournalFileEvent
    {
        public string   ATMId      { get; set; }
        public string   FilePath   { get; set; }
        public string   FileName   { get; set; }
        public long     NewOffset  { get; set; }
        public int      BytesRead  { get; set; }
        public string   Checksum   { get; set; }
        public DateTime DetectedAt { get; set; }
		
    /// <summary>
    /// محرك مراقبة الملفات - يراقب التغييرات في ملفات الجورنال ويطبق الاستراتيجية المناسبة
    /// يدعم: NCR (Overwrite 3 ملفات) / GRG (ملفات يومية EJ+TRACE) / WN (ملفات يومية EJ)
    /// </summary>
    public class FileWatcherEngine
    {
        private FileSystemWatcher _watcher;
        private string _sourcePath;
        private string _backupPath;
        private string _atmType;
        private string _atmId;
        private bool _isRunning;
        private long _lastReadPosition;  // لـ NCR: مؤشر آخر سطر تم قراءته
        private Timer _pollingTimer;
        private readonly object _lockObj = new object();

        // أحداث
        public event Action<string, string> OnFileChanged;      // (filePath, changeType)
        public event Action<string, byte[]> OnNewDataReady;     // (fileName, data)
        public event Action<string> OnNewFileDetected;          // (filePath)
        public event Action<string> OnLog;                      // (message)
        public event Action<Exception> OnError;                 // (exception)

        public bool IsRunning { get { return _isRunning; } }
        public string SourcePath { get { return _sourcePath; } }
        public string BackupPath { get { return _backupPath; } }

        public FileWatcherEngine(string atmId, string atmType, string sourcePath, string backupPath)
        {
            _atmId = atmId;
            _atmType = atmType;
            _sourcePath = sourcePath;
            _backupPath = backupPath;
            _lastReadPosition = 0;
            _isRunning = false;
        }

        /// <summary>
        /// بدء المراقبة
        /// </summary>
        public bool Start()
        {
            try
            {
                if (_isRunning) return true;

                // التأكد من وجود المجلدات
                if (!Directory.Exists(_sourcePath))
                {
                    OnLog?.Invoke("[FileWatcher] Source path not found: " + _sourcePath);
                    return false;
                }

                if (!Directory.Exists(_backupPath))
                {
                    Directory.CreateDirectory(_backupPath);
                    OnLog?.Invoke("[FileWatcher] Created backup path: " + _backupPath);
                }

                // تطبيق الاستراتيجية المناسبة
                switch (_atmType)
                {
                    case AppConstants.ATM_TYPE_NCR:
                        StartNCRStrategy();
                        break;
                    case AppConstants.ATM_TYPE_GRG:
                        StartGRGStrategy();
                        break;
                    case AppConstants.ATM_TYPE_WN:
                        StartWNStrategy();
                        break;
                    default:
                        OnLog?.Invoke("[FileWatcher] Unknown ATM type: " + _atmType);
                        return false;
                }

                _isRunning = true;
                OnLog?.Invoke("[FileWatcher] Started monitoring: " + _sourcePath);
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// إيقاف المراقبة
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
            if (_pollingTimer != null)
            {
                _pollingTimer.Dispose();
                _pollingTimer = null;
            }
            OnLog?.Invoke("[FileWatcher] Stopped monitoring");
        }

        #region NCR Strategy - تحديث فوقي (Overwrite) لـ 3 ملفات

        /// <summary>
        /// استراتيجية NCR: مراقبة 3 ملفات محددة (EJDATA.LOG, EJRCPY.LOG, EJDATA.LOb)
        /// عند أي تغيير يتم نسخها فوقياً إلى مجلد الباك أب (مرآة لحظية)
        /// </summary>
        private void StartNCRStrategy()
        {
            OnLog?.Invoke("[NCR] Strategy: Overwrite mirroring for 3 target files");

            // مراقبة الملفات الثلاثة
            _watcher = new FileSystemWatcher(_sourcePath);
            _watcher.Filter = "*.*";
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
            _watcher.Changed += NCR_OnFileChanged;
            _watcher.EnableRaisingEvents = true;

            // فحص أولي - نسخ الملفات الموجودة
            foreach (string targetFile in NCRFiles.TargetFiles)
            {
                string fullPath = Path.Combine(_sourcePath, targetFile);
                if (File.Exists(fullPath))
                {
                    MirrorFileToBackup(fullPath, targetFile);
                }
            }

            // Polling كل 5 ثواني لاكتشاف التغييرات (احتياطي)
            _pollingTimer = new Timer(NCR_PollChanges, null, 5000, 5000);
        }

        private void NCR_OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                // التحقق أن الملف من الملفات المستهدفة
                bool isTarget = false;
                foreach (string target in NCRFiles.TargetFiles)
                {
                    if (string.Equals(fileName, target, StringComparison.OrdinalIgnoreCase))
                    {
                        isTarget = true;
                        break;
                    }
                }

                if (!isTarget) return;

                lock (_lockObj)
                {
                    // انتظار تحرير الملف من القفل
                    WaitForFileRelease(e.FullPath);

                    // نسخ مرآة (Overwrite) إلى الباك أب
                    MirrorFileToBackup(e.FullPath, fileName);

                    // قراءة البيانات الجديدة فقط (من آخر موضع)
                    byte[] newData = ReadNewDataFromNCR(e.FullPath);
                    if (newData != null && newData.Length > 0)
                    {
                        OnNewDataReady?.Invoke(fileName, newData);
                    }

                    OnFileChanged?.Invoke(e.FullPath, "Modified");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        private void NCR_PollChanges(object state)
        {
            if (!_isRunning) return;
            try
            {
                foreach (string targetFile in NCRFiles.TargetFiles)
                {
                    string fullPath = Path.Combine(_sourcePath, targetFile);
                    if (!File.Exists(fullPath)) continue;

                    string backupPath = Path.Combine(_backupPath, targetFile);
                    if (!File.Exists(backupPath))
                    {
                        MirrorFileToBackup(fullPath, targetFile);
                        continue;
                    }

                    // مقارنة الحجم والتاريخ
                    var srcInfo = new FileInfo(fullPath);
                    var bkpInfo = new FileInfo(backupPath);
                    if (srcInfo.Length != bkpInfo.Length || srcInfo.LastWriteTime > bkpInfo.LastWriteTime)
                    {
                        MirrorFileToBackup(fullPath, targetFile);
                        byte[] newData = ReadNewDataFromNCR(fullPath);
                        if (newData != null && newData.Length > 0)
                        {
                            OnNewDataReady?.Invoke(targetFile, newData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        /// <summary>
        /// قراءة البيانات الجديدة فقط من ملف NCR (من آخر موضع)
        /// </summary>
        private byte[] ReadNewDataFromNCR(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length <= _lastReadPosition)
                    {
                        // الملف لم يكبر - ربما تم إعادة كتابته
                        _lastReadPosition = 0;
                    }

                    fs.Seek(_lastReadPosition, SeekOrigin.Begin);
                    byte[] buffer = new byte[fs.Length - _lastReadPosition];
                    int bytesRead = fs.Read(buffer, 0, buffer.Length);
                    _lastReadPosition = fs.Position;

                    if (bytesRead > 0)
                    {
                        byte[] result = new byte[bytesRead];
                        Array.Copy(buffer, result, bytesRead);
                        OnLog?.Invoke(string.Format("[NCR] Read {0} new bytes from {1}", bytesRead, Path.GetFileName(filePath)));
                        return result;
                    }
                }
            }
            catch (IOException) { /* الملف مقفل */ }
            return null;
        }

        #endregion

        #region GRG Strategy - ملفات يومية (EJ + TRACE)

        /// <summary>
        /// استراتيجية GRG: مراقبة مسار اللوج والتقاط ملفات EJ و TRACE يومياً
        /// </summary>
        private void StartGRGStrategy()
        {
            OnLog?.Invoke("[GRG] Strategy: Daily files monitoring (EJ* + TRACE*)");

            _watcher = new FileSystemWatcher(_sourcePath);
            _watcher.Filter = "*.*";
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
            _watcher.Created += GRG_OnNewFile;
            _watcher.Changed += GRG_OnFileChanged;
            _watcher.EnableRaisingEvents = true;

            // فحص أولي - نسخ الملفات الموجودة اليوم
            ScanExistingGRGFiles();

            // Polling كل 10 ثواني
            _pollingTimer = new Timer(GRG_PollChanges, null, 10000, 10000);
        }

        private void GRG_OnNewFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                if (IsGRGTargetFile(fileName))
                {
                    OnLog?.Invoke("[GRG] New file detected: " + fileName);
                    OnNewFileDetected?.Invoke(e.FullPath);
                    WaitForFileRelease(e.FullPath);
                    MirrorFileToBackup(e.FullPath, fileName);
                }
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        private void GRG_OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                if (!IsGRGTargetFile(fileName)) return;

                lock (_lockObj)
                {
                    WaitForFileRelease(e.FullPath);
                    MirrorFileToBackup(e.FullPath, fileName);

                    byte[] data = ReadFileWithSharing(e.FullPath);
                    if (data != null && data.Length > 0)
                    {
                        OnNewDataReady?.Invoke(fileName, data);
                        OnFileChanged?.Invoke(e.FullPath, "Modified");
                    }
                }
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        private void GRG_PollChanges(object state)
        {
            if (!_isRunning) return;
            ScanExistingGRGFiles();
        }

        private void ScanExistingGRGFiles()
        {
            try
            {
                if (!Directory.Exists(_sourcePath)) return;
                string[] ejFiles = Directory.GetFiles(_sourcePath, FilePatterns.GRG_EJ_PATTERN);
                string[] traceFiles = Directory.GetFiles(_sourcePath, FilePatterns.GRG_TRACE_PATTERN);

                ProcessFileList(ejFiles);
                ProcessFileList(traceFiles);
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        private bool IsGRGTargetFile(string fileName)
        {
            string upper = fileName.ToUpperInvariant();
            return upper.StartsWith("EJ") || upper.StartsWith("TRACE");
        }

        #endregion

        #region WN Strategy - ملفات يومية (EJ)

        /// <summary>
        /// استراتيجية WN: مراقبة والتقاط ملفات EJ اليومية
        /// </summary>
        private void StartWNStrategy()
        {
            OnLog?.Invoke("[WN] Strategy: Daily EJ files monitoring");

            _watcher = new FileSystemWatcher(_sourcePath);
            _watcher.Filter = FilePatterns.WN_EJ_PATTERN;
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
            _watcher.Created += WN_OnNewFile;
            _watcher.Changed += WN_OnFileChanged;
            _watcher.EnableRaisingEvents = true;

            // فحص أولي
            ScanExistingWNFiles();

            // Polling كل 10 ثواني
            _pollingTimer = new Timer(WN_PollChanges, null, 10000, 10000);
        }

        private void WN_OnNewFile(object sender, FileSystemEventArgs e)
        {
            try
            {
                OnLog?.Invoke("[WN] New EJ file detected: " + Path.GetFileName(e.FullPath));
                OnNewFileDetected?.Invoke(e.FullPath);
                WaitForFileRelease(e.FullPath);
                MirrorFileToBackup(e.FullPath, Path.GetFileName(e.FullPath));
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        private void WN_OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string fileName = Path.GetFileName(e.FullPath);
                lock (_lockObj)
                {
                    WaitForFileRelease(e.FullPath);
                    MirrorFileToBackup(e.FullPath, fileName);

                    byte[] data = ReadFileWithSharing(e.FullPath);
                    if (data != null && data.Length > 0)
                    {
                        OnNewDataReady?.Invoke(fileName, data);
                        OnFileChanged?.Invoke(e.FullPath, "Modified");
                    }
                }
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        private void WN_PollChanges(object state)
        {
            if (!_isRunning) return;
            ScanExistingWNFiles();
        }

        private void ScanExistingWNFiles()
        {
            try
            {
                if (!Directory.Exists(_sourcePath)) return;
                string[] ejFiles = Directory.GetFiles(_sourcePath, FilePatterns.WN_EJ_PATTERN);
                ProcessFileList(ejFiles);
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// نسخ ملف إلى مجلد الباك أب (مرآة - Overwrite)
        /// </summary>
        private void MirrorFileToBackup(string sourceFile, string fileName)
        {
            try
            {
                string destFile = Path.Combine(_backupPath, fileName);
                // نسخ مع إعادة المحاولة عند القفل
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        File.Copy(sourceFile, destFile, true);
                        OnLog?.Invoke("[Mirror] " + fileName + " -> Backup");
                        return;
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception ex) { OnError?.Invoke(ex); }
        }

        /// <summary>
        /// انتظار تحرير الملف من القفل
        /// </summary>
        private void WaitForFileRelease(string filePath)
        {
            int attempts = 0;
            while (attempts < 10)
            {
                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return; // الملف متاح
                    }
                }
                catch (IOException)
                {
                    attempts++;
                    Thread.Sleep(200);
                }
            }
        }

        /// <summary>
        /// قراءة ملف مع السماح بالمشاركة (File Sharing)
        /// </summary>
        private byte[] ReadFileWithSharing(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
            catch (IOException) { return null; }
        }

        /// <summary>
        /// معالجة قائمة ملفات - نسخ الجديد منها
        /// </summary>
        private void ProcessFileList(string[] files)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string backupFile = Path.Combine(_backupPath, fileName);

                if (!File.Exists(backupFile))
                {
                    MirrorFileToBackup(file, fileName);
                    OnNewFileDetected?.Invoke(file);
                }
                else
                {
                    var srcInfo = new FileInfo(file);
                    var bkpInfo = new FileInfo(backupFile);
                    if (srcInfo.Length != bkpInfo.Length || srcInfo.LastWriteTime > bkpInfo.LastWriteTime)
                    {
                        MirrorFileToBackup(file, fileName);
                    }
                }
            }
        }

        #endregion
    }
}
