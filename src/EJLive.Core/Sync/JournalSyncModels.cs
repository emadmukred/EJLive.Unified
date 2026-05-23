using System;
using System.Collections.Generic;

namespace EJLive.Core.Sync
{
    [Serializable]
    public enum JournalSyncStatus
    {
        Unknown,
        ConnectedIdle,
        Syncing,
        SyncHealthy,
        SyncWarning,
        SyncCritical,
        Disconnected
    }

    [Serializable]
    public enum JournalTransferStatus
    {
        Received,
        Failed,
        Archived
    }

    [Serializable]
    public class JournalSyncEntry
    {
        public string EntryId { get; set; }
        public string ATMId { get; set; }
        public string FileName { get; set; }
        public string RelativeStoragePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string Checksum { get; set; }
        public DateTime ReceivedAtUtc { get; set; }
        public JournalTransferStatus Status { get; set; }
        public string ErrorMessage { get; set; }

        public JournalSyncEntry()
        {
            EntryId = Guid.NewGuid().ToString("N");
            ReceivedAtUtc = DateTime.UtcNow;
            Status = JournalTransferStatus.Received;
        }
    }

    [Serializable]
    public class ATMJournalSyncState
    {
        public string ATMId { get; set; }
        public bool IsConnected { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
        public DateTime LastJournalReceivedUtc { get; set; }
        public DateTime LastSuccessfulSyncUtc { get; set; }
        public long TotalJournalBytesReceived { get; set; }
        public int TotalJournalFilesReceived { get; set; }
        public int FailedSyncCount { get; set; }
        public int PendingArchiveCount { get; set; }
        public string LastStoredFile { get; set; }
        public string LastChecksum { get; set; }
        public string LastError { get; set; }
        public JournalSyncStatus CurrentStatus { get; set; }

        public ATMJournalSyncState()
        {
            ATMId = "Unknown";
            CurrentStatus = JournalSyncStatus.Unknown;
        }
    }

    [Serializable]
    public class JournalSyncAlert
    {
        public string AlertId { get; set; }
        public string ATMId { get; set; }
        public string Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public JournalSyncAlert()
        {
            AlertId = Guid.NewGuid().ToString("N");
            CreatedAtUtc = DateTime.UtcNow;
            Severity = "Info";
            Code = "sync-info";
        }
    }

    [Serializable]
    public class JournalSyncStateEnvelope
    {
        public DateTime LastUpdatedUtc { get; set; }
        public List<ATMJournalSyncState> AtmStates { get; set; }
        public List<JournalSyncEntry> Transfers { get; set; }
        public List<JournalSyncAlert> Alerts { get; set; }

        public JournalSyncStateEnvelope()
        {
            LastUpdatedUtc = DateTime.UtcNow;
            AtmStates = new List<ATMJournalSyncState>();
            Transfers = new List<JournalSyncEntry>();
            Alerts = new List<JournalSyncAlert>();
        }
    }

    public class JournalSyncDashboardSnapshot
    {
        public int TotalAtms { get; set; }
        public int ConnectedAtms { get; set; }
        public int WarningAtms { get; set; }
        public int CriticalAtms { get; set; }
        public int TotalTransfers { get; set; }
        public int FailedTransfers { get; set; }
        public List<ATMJournalSyncState> AtmStates { get; set; }
        public List<JournalSyncEntry> RecentTransfers { get; set; }
        public List<JournalSyncAlert> ActiveAlerts { get; set; }

        public JournalSyncDashboardSnapshot()
        {
            AtmStates = new List<ATMJournalSyncState>();
            RecentTransfers = new List<JournalSyncEntry>();
            ActiveAlerts = new List<JournalSyncAlert>();
        }
    }
}
