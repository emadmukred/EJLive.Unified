using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// متتبع حالة المزامنة الكامل — JournalSyncTracker
    /// يدير آلة الحالة: Pending → Syncing → [Completed | Failed → ReSyncing]
    /// يضمن: Idempotency (L-03), عدم فقدان السجلات (L-01), إعادة الإرسال (L-05)
    /// يوفر: GetPendingForATM, MarkSyncing, MarkCompleted, MarkFailed, ScheduleResync
    /// </summary>
    public class JournalSyncTracker
    {
        // حالة في الذاكرة لكل صراف
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>> _state
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, JournalSyncRecord>>();

        public event EventHandler<JournalSyncRecord> OnStateChanged;

        // ==========================================
        // إضافة سجل جديد
        // ==========================================

        public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
        {
            var records = _state.GetOrAdd(atmId, _ => new ConcurrentDictionary<string, JournalSyncRecord>());
            var key     = $"{fileName}|{checksum}";

            if (records.TryGetValue(key, out var existing))
                return existing;

            // Idempotency: هل مزامن من قبل؟ (L-03)
            if (DatabaseManager.Instance.IsDuplicateSync(atmId, fileName, checksum))
            {
                AppLogger.Instance.Debug($"Idempotency: already synced {fileName} [{checksum.Substring(0,8)}]", "SyncTracker");
                return null;
            }

            var record = new JournalSyncRecord
            {
                ATM_ID          = atmId,
                FileName        = fileName,
                FileSize        = fileSize,
                FileOffset      = offset,
                Checksum        = checksum,
                State           = JournalSyncState.Pending,
                ProgressPercent = 0,
                CreatedAtUtc    = DateTime.UtcNow
            };

            // تسجيل في قاعدة البيانات
            DatabaseManager.Instance.InsertSyncRecord(record);
            records[key] = record;
            OnStateChanged?.Invoke(this, record);
            return record;
        }

        // ==========================================
        // تحديث الحالة
        // ==========================================

        public void MarkSyncing(string atmId, string syncId, int chunkPercent)
        {
            UpdateState(atmId, syncId, r =>
            {
                r.State           = JournalSyncState.Syncing;
                r.ProgressPercent = chunkPercent;
                r.UpdatedAtUtc    = DateTime.UtcNow;
            });
            DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Syncing, chunkPercent);
        }

        public void MarkCompleted(string atmId, string syncId, string sha256 = null)
        {
            UpdateState(atmId, syncId, r =>
            {
                r.State           = JournalSyncState.Completed;
                r.ProgressPercent = 100;
                r.SHA256Hash      = sha256;
                r.CompletedAtUtc  = DateTime.UtcNow;
                r.UpdatedAtUtc    = DateTime.UtcNow;
            });
            DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Completed, 100);
            AppLogger.Instance.Info($"Sync completed: {syncId}", "SyncTracker");
        }

        public void MarkFailed(string atmId, string syncId, string reason)
        {
            UpdateState(atmId, syncId, r =>
            {
                r.State       = JournalSyncState.Failed;
                r.Message     = reason;
                r.RetryCount++;
                r.UpdatedAtUtc = DateTime.UtcNow;
            });
            DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Failed, 0);
            AppLogger.Instance.Warning($"Sync failed: {syncId} - {reason}", "SyncTracker");
        }

        public void MarkReSyncing(string atmId, string syncId)
        {
            UpdateState(atmId, syncId, r =>
            {
                r.State = JournalSyncState.ReSyncing;
                r.UpdatedAtUtc = DateTime.UtcNow;
            });
            DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.ReSyncing, 0);
        }

        public void MarkArchived(string syncId)
        {
            DatabaseManager.Instance.UpdateSyncState(syncId, JournalSyncState.Archived, 100);
        }

        // ==========================================
        // الاستعلام
        // ==========================================

        public List<JournalSyncRecord> GetAllForATM(string atmId)
        {
            if (!_state.TryGetValue(atmId, out var records)) return new List<JournalSyncRecord>();
            return new List<JournalSyncRecord>(records.Values);
        }

        public List<JournalSyncRecord> GetPendingForATM(string atmId)
            => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.Failed || r.State == JournalSyncState.ReSyncing);

        public List<JournalSyncRecord> GetFailedForATM(string atmId)
            => GetAllForATM(atmId).FindAll(r => r.State == JournalSyncState.Failed);

        public int GetPendingCount(string atmId) => GetPendingForATM(atmId).Count;

        public (int completed, int failed, int pending) GetStats(string atmId)
        {
            var all = GetAllForATM(atmId);
            return (
                all.Count(r => r.State == JournalSyncState.Completed),
                all.Count(r => r.State == JournalSyncState.Failed),
                all.Count(r => r.State == JournalSyncState.Pending || r.State == JournalSyncState.ReSyncing)
            );
        }

        // ==========================================
        // تحميل من قاعدة البيانات
        // ==========================================

        public void LoadFromDatabase(string atmId)
        {
            var records = DatabaseManager.Instance.GetPendingSyncRecords(atmId);
            if (!_state.ContainsKey(atmId))
                _state[atmId] = new ConcurrentDictionary<string, JournalSyncRecord>();

            foreach (var r in records)
            {
                var key = $"{r.FileName}|{r.Checksum}";
                _state[atmId][key] = r;
            }
            AppLogger.Instance.Info($"SyncTracker loaded {records.Count} records for {atmId}", "SyncTracker");
        }

        // مساعد تحديث الحالة
        private void UpdateState(string atmId, string syncId, Action<JournalSyncRecord> updater)
        {
            if (!_state.TryGetValue(atmId, out var records)) return;
            foreach (var r in records.Values)
            {
                if (r.SyncId == syncId)
                {
                    updater(r);
                    OnStateChanged?.Invoke(this, r);
                    return;
                }
            }
        }

        // إعادة ضبط حالة ATM بعد إعادة الاتصال
        public void ResetInFlightToResync(string atmId)
        {
            UpdateAllForATM(atmId, r =>
            {
                if (r.State == JournalSyncState.Syncing)
                {
                    r.State = JournalSyncState.ReSyncing;
                    r.UpdatedAtUtc = DateTime.UtcNow;
                }
            });
        }

        private void UpdateAllForATM(string atmId, Action<JournalSyncRecord> updater)
        {
            if (!_state.TryGetValue(atmId, out var records)) return;
            foreach (var r in records.Values) updater(r);
        }
    }
}
