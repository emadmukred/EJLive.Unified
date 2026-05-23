using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using EJLive.Core.Models;


namespace EJLive.Core.Engine
{
    /// <summary>
    /// محرك مزامنة الصور الكامل — Image Sync Engine
    /// يُزامن: شاشات التوقف، التوجيهات، شاشات تعريفية
    /// يطبق: نفس بروتوكول الجورنال المُشفر (START_FILE + CHUNK + COMPLETE)
    ///        تحقق MD5 + حفظ في مجلد الصور
    /// مُستخدم من الخادم → العميل وليس العكس
    /// </summary>
    public class ImageSyncEngine
    {
        private readonly string _imagesPath;
        private readonly byte[] _sessionKey;

        public event EventHandler<string> OnImageSynced;
        public event EventHandler<string> OnImageError;
        public event EventHandler<string> OnLog;

        // إحصائيات
        public int    ImagesSynced    { get; private set; }
        public long   TotalBytesSent  { get; private set; }

        public ImageSyncEngine(string imagesPath = null, byte[] sessionKey = null)
        {
            _imagesPath = imagesPath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Images");
            _sessionKey = sessionKey;
            if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
        }

        // ==========================================
        // إرسال صورة للعميل عبر NetworkStream
        // ==========================================

        public bool SendImageToClient(NetworkStream stream, string imagePath, string atmId, object sendLock)
        {
            if (!File.Exists(imagePath))
            {
                Log($"Image not found: {imagePath}");
                return false;
            }

            try
            {
                var data     = SecurityHelper.ReadFileSafe(imagePath);
                var checksum = SecurityHelper.MD5Hash(data);
                var fileName = Path.GetFileName(imagePath);

                // START_FILE
                var startMsg = CommunicationProtocol.BuildStartFile(atmId, fileName, data.Length, 0, checksum);
                lock (sendLock) stream.Write(startMsg, 0, startMsg.Length);

                // إرسال على أجزاء
                int totalChunks = (int)Math.Ceiling((double)data.Length / AppConstants.ChunkSizeBytes);
                for (int i = 0; i < totalChunks; i++)
                {
                    var off   = i * (int)AppConstants.ChunkSizeBytes;
                    var len   = Math.Min((int)AppConstants.ChunkSizeBytes, data.Length - off);
                    var chunk = new byte[len];
                    Buffer.BlockCopy(data, off, chunk, 0, len);
                    var chunkMsg = CommunicationProtocol.BuildChunk(i, chunk, _sessionKey);
                    lock (sendLock) stream.Write(chunkMsg, 0, chunkMsg.Length);
                    TotalBytesSent += len;
                }

                // COMPLETE
                var sha256  = SecurityHelper.SHA256Hash(data);
                var complete = CommunicationProtocol.BuildComplete(fileName, checksum, sha256);
                lock (sendLock) stream.Write(complete, 0, complete.Length);

                ImagesSynced++;
                OnImageSynced?.Invoke(this, imagePath);
                Log($"✓ Image sent: {fileName} [{data.Length / 1024.0:F1} KB]");
                return true;
            }
            catch (Exception ex)
            {
                OnImageError?.Invoke(this, ex.Message);
                Log($"✗ Image send failed: {ex.Message}");
                return false;
            }
        }

        // ==========================================
        // استلام صورة من السيرفر (Client Side)
        // ==========================================

        public bool ReceiveImage(NetworkStream stream, string fileName, long fileSize)
        {
            try
            {
                var buffer  = new MemoryStream();
                var chunkCount = (int)Math.Ceiling((double)fileSize / AppConstants.ChunkSizeBytes);

                for (int i = 0; i < chunkCount; i++)
                {
                    var msg = CommunicationProtocol.ReadMessage(stream, _sessionKey);
                    if (!CommunicationProtocol.IsChunk(msg)) return false;
                    var (seqNum, data) = CommunicationProtocol.ParseChunk(msg);
                    buffer.Write(data, 0, data.Length);

                    // CHUNK_ACK
                    var ack = CommunicationProtocol.BuildChunkAck(seqNum);
                    stream.Write(ack, 0, ack.Length);
                }

                // قراءة COMPLETE
                var complete = CommunicationProtocol.ReadMessage(stream, _sessionKey);
                var parts    = complete.Text.Split('|');
                var checksum = parts.Length > 2 ? parts[2] : "";

                var imageData   = buffer.ToArray();
                var actualMd5   = SecurityHelper.MD5Hash(imageData);
                var verified    = string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase);

                if (verified)
                {
                    var savePath = Path.Combine(_imagesPath, SanitizeName(fileName));
                    File.WriteAllBytes(savePath, imageData);
                    ImagesSynced++;
                    OnImageSynced?.Invoke(this, savePath);
                    Log($"✓ Image received & verified: {fileName}");
                }
                else
                {
                    Log($"✗ Checksum mismatch for image: {fileName}");
                }

                // إرسال ACK
                var imgAck = CommunicationProtocol.BuildJournalAck(fileName, verified);
                stream.Write(imgAck, 0, imgAck.Length);
                return verified;
            }
            catch (Exception ex)
            {
                OnImageError?.Invoke(this, ex.Message);
                return false;
            }
        }

        // ==========================================
        // مزامنة جماعية
        // ==========================================

        public int SyncAllImagesToClient(NetworkStream stream, string serverImagesFolder, string atmId, object sendLock)
        {
            if (!Directory.Exists(serverImagesFolder)) return 0;
            int count = 0;
            foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" })
                foreach (var file in Directory.GetFiles(serverImagesFolder, ext))
                    if (SendImageToClient(stream, file, atmId, sendLock)) count++;
            Log($"✓ Image sync complete: {count} images sent to {atmId}");
            return count;
        }

        public List<string> GetLocalImages()
        {
            var images = new List<string>();
            if (!Directory.Exists(_imagesPath)) return images;
            foreach (var ext in new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" })
                images.AddRange(Directory.GetFiles(_imagesPath, ext));
            return images;
        }

        private string SanitizeName(string name) =>
            string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

        private void Log(string msg) => OnLog?.Invoke(this, msg);
    
    /// <summary>
    /// محرك مزامنة الصور - نقل الصور من السرفر إلى الصرافات
    /// مشاركة الصور في مجلد السرفر حسب نوع الصرافات
    /// مزامنة الصور إلى كل الصرافات حسب مسار ملف الصور ونوعية الصراف
    /// </summary>
    public class ImageSyncEngine
    {
        #region Events
        public event Action<string> OnLog;
        public event Action<ImageSyncItem, string> OnImageSent; // item, atmId
        public event Action<ImageSyncItem, string, string> OnImageFailed; // item, atmId, error
        public event Action<Exception> OnError;
        public Func<string, byte[], string, bool>? DeliveryBridge;
        #endregion

        #region Fields
        private readonly string _imageBasePath;
        private readonly Dictionary<string, List<string>> _atmsByType; // type -> list of ATM IDs
        private readonly List<ImageSyncItem> _syncQueue;
        private readonly object _lock = new object();
        private FileSystemWatcher _watcher;
        private bool _isRunning;
        private Thread _syncThread;
        #endregion

        #region Configuration
        public string NCRImagePath { get; set; } = @"D:\EJOURNAL Files\Images\NCR";
        public string GRGImagePath { get; set; } = @"D:\EJOURNAL Files\Images\GRG";
        public string WNImagePath { get; set; } = @"D:\EJOURNAL Files\Images\WN";
        public string SharedImagePath { get; set; } = @"D:\EJOURNAL Files\Images\Shared";
        public int SyncIntervalMs { get; set; } = 10000;
        #endregion

        #region Constructor
        public ImageSyncEngine(string imageBasePath)
        {
            _imageBasePath = imageBasePath;
            _atmsByType = new Dictionary<string, List<string>>
            {
                { "NCR", new List<string>() },
                { "GRG", new List<string>() },
                { "WN", new List<string>() }
            };
            _syncQueue = new List<ImageSyncItem>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// تسجيل صراف في قائمة المزامنة
        /// </summary>
        public void RegisterATM(string atmId, string atmType)
        {
            lock (_lock)
            {
                if (_atmsByType.ContainsKey(atmType))
                {
                    if (!_atmsByType[atmType].Contains(atmId))
                        _atmsByType[atmType].Add(atmId);
                }
            }
            OnLog?.Invoke($"[ImageSync] Registered ATM: {atmId} ({atmType})");
        }

        /// <summary>
        /// إلغاء تسجيل صراف
        /// </summary>
        public void UnregisterATM(string atmId)
        {
            lock (_lock)
            {
                foreach (var list in _atmsByType.Values)
                    list.Remove(atmId);
            }
        }

        /// <summary>
        /// إضافة صورة للمزامنة
        /// </summary>
        public void QueueImage(string filePath, string targetType = "ALL", List<string> specificATMs = null)
        {
            if (!File.Exists(filePath))
            {
                OnLog?.Invoke($"[ImageSync] File not found: {filePath}");
                return;
            }

            var item = new ImageSyncItem
            {
                ImageID = Guid.NewGuid().ToString("N").Substring(0, 8),
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length,
                Checksum = ComputeMD5(filePath),
                TargetATMType = targetType,
                TargetATMs = specificATMs ?? new List<string>(),
                ScheduledTime = DateTime.Now,
                Status = ImageSyncStatus.Pending
            };

            lock (_lock) { _syncQueue.Add(item); }
            OnLog?.Invoke($"[ImageSync] Queued: {item.FileName} -> {targetType} ({item.FileSize} bytes)");
        }

        /// <summary>
        /// بدء المزامنة
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            // مراقبة مجلد الصور المشتركة
            SetupWatcher();

            // بدء thread المزامنة
            _syncThread = new Thread(SyncLoop) { IsBackground = true, Name = "ImageSyncThread" };
            _syncThread.Start();

            OnLog?.Invoke("[ImageSync] Engine started");
        }

        /// <summary>
        /// إيقاف المزامنة
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; }
            OnLog?.Invoke("[ImageSync] Engine stopped");
        }

        /// <summary>
        /// الحصول على حالة قائمة المزامنة
        /// </summary>
        public List<ImageSyncItem> GetSyncQueue()
        {
            lock (_lock) { return _syncQueue.ToList(); }
        }

        /// <summary>
        /// إرسال صورة لصراف محدد عبر الشبكة
        /// </summary>
        public bool SendImageToATM(string atmId, byte[] imageData, string fileName, NetworkEngine networkEngine)
        {
            try
            {
                if (networkEngine == null || !networkEngine.IsConnected)
                {
                    OnLog?.Invoke($"[ImageSync] Cannot send to {atmId}: not connected");
                    return false;
                }

                string checksum = ComputeMD5Bytes(imageData);
                bool sent = networkEngine.SendFile(fileName, imageData, checksum);
                if (sent) OnLog?.Invoke($"[ImageSync] Sent {fileName} to {atmId} ({imageData.Length} bytes)");
                return sent;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }
        #endregion

        #region Private Methods
        private void SetupWatcher()
        {
            try
            {
                string watchPath = _imageBasePath;
                if (!Directory.Exists(watchPath)) Directory.CreateDirectory(watchPath);

                _watcher = new FileSystemWatcher(watchPath)
                {
                    Filter = "*.*",
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
                };
                _watcher.Created += Watcher_Created;
                _watcher.EnableRaisingEvents = true;
                OnLog?.Invoke($"[ImageSync] Watching: {watchPath}");
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            string ext = Path.GetExtension(e.FullPath).ToLower();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
            {
                // تحديد النوع المستهدف من المسار
                string targetType = "ALL";
                if (e.FullPath.Contains("NCR")) targetType = "NCR";
                else if (e.FullPath.Contains("GRG")) targetType = "GRG";
                else if (e.FullPath.Contains("WN")) targetType = "WN";

                Thread.Sleep(500); // انتظار اكتمال الكتابة
                QueueImage(e.FullPath, targetType);
            }
        }

        private void SyncLoop()
        {
            while (_isRunning)
            {
                try
                {
                    List<ImageSyncItem> pending;
                    lock (_lock) { pending = _syncQueue.Where(i => i.Status == ImageSyncStatus.Pending).ToList(); }

                    foreach (var item in pending)
                    {
                        if (!_isRunning) break;
                        ProcessSyncItem(item);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
                Thread.Sleep(SyncIntervalMs);
            }
        }

        private void ProcessSyncItem(ImageSyncItem item)
        {
            item.Status = ImageSyncStatus.Syncing;
            List<string> targetATMs;

            lock (_lock)
            {
                if (item.TargetATMs.Count > 0)
                    targetATMs = item.TargetATMs.ToList();
                else if (item.TargetATMType == "ALL")
                    targetATMs = _atmsByType.Values.SelectMany(l => l).ToList();
                else if (_atmsByType.ContainsKey(item.TargetATMType))
                    targetATMs = _atmsByType[item.TargetATMType].ToList();
                else
                    targetATMs = new List<string>();
            }

            if (targetATMs.Count == 0)
            {
                item.Status = ImageSyncStatus.Failed;
                OnLog?.Invoke($"[ImageSync] No target ATMs for: {item.FileName}");
                return;
            }

            byte[] payload;
            try
            {
                payload = File.ReadAllBytes(item.FilePath);
                if (payload.Length == 0)
                {
                    item.Status = ImageSyncStatus.Failed;
                    OnLog?.Invoke($"[ImageSync] Empty payload for: {item.FileName}");
                    return;
                }
            }
            catch (Exception ex)
            {
                item.Status = ImageSyncStatus.Failed;
                OnError?.Invoke(ex);
                return;
            }

            int success = 0;
            foreach (string atmId in targetATMs)
            {
                try
                {
                    var delivered = DeliveryBridge?.Invoke(atmId, payload, item.FileName) ?? true;
                    item.DeliveryStatus[atmId] = delivered;

                    if (delivered)
                    {
                        success++;
                        OnImageSent?.Invoke(item, atmId);
                    }
                    else
                    {
                        OnImageFailed?.Invoke(item, atmId, "Delivery bridge returned false.");
                    }
                }
                catch (Exception ex)
                {
                    item.DeliveryStatus[atmId] = false;
                    OnImageFailed?.Invoke(item, atmId, ex.Message);
                }
            }

            item.Status = success == targetATMs.Count ? ImageSyncStatus.Completed :
                          success > 0 ? ImageSyncStatus.PartiallyCompleted : ImageSyncStatus.Failed;
        }

        private string ComputeMD5(string filePath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private string ComputeMD5Bytes(byte[] data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        #endregion
    }
}
