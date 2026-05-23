using System;
using System.Collections.Generic;

namespace EJLive.Core.Xfs
{
    public enum XfsVendor
    {
        Unknown,
        NCR,
        GRG,
        WN,
        Hyosung,
        CashWay,
        Diebold,
        Nautilus,
        DelaRue
    }

    public enum XfsSourceLayer
    {
        BusinessJournal,
        XfsStatus,
        DriverError,
        HardwareDiagnostic,
        HostTransport,
        MiddlewareRuntime,
        Configuration
    }

    public enum XfsEventKind
    {
        Unknown,
        TerminalModeTransition,
        NetworkState,
        TransactionLifecycle,
        TransactionRequest,
        HostReply,
        TransactionSerial,
        DeviceStatus,
        DeviceFault,
        CassetteSnapshot,
        CassetteChange,
        CashDispense,
        CashDeposit,
        Retract,
        CardEvent,
        CardReaderFlow,
        CardReaderFitness,
        CardReaderStatistics,
        PrinterEvent,
        Timeout,
        OcrCapture,
        CommandReject,
        Maintenance,
        HostMessageInbound,
        HostMessageOutbound,
        ProtocolKeepalive,
        XfsSessionLifecycle,
        XfsGetInfoCycle,
        XfsCapabilitiesQuery,
        XfsStatusPolling,
        VirtualControllerInbound,
        VirtualControllerOutbound,
        TerminalCommandLifecycle,
        MiddlewareValidationFlow,
        HeartbeatTelemetry,
        UpsState,
        ConfigurationProfile
    }

    public enum XfsSeverity
    {
        Info,
        Warning,
        Critical
    }

    public sealed class XfsNormalizedEvent
    {
        public XfsVendor Vendor { get; set; }
        public XfsSourceLayer SourceLayer { get; set; }
        public XfsEventKind Kind { get; set; }
        public XfsSeverity Severity { get; set; }
        public DateTime? Timestamp { get; set; }
        public string TerminalId { get; set; }
        public string DeviceFamily { get; set; }
        public string DeviceCode { get; set; }
        public string RawCode { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ServiceImpact { get; set; }
        public string CustomerImpact { get; set; }
        public string RecommendedAction { get; set; }
        public string RawLine { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public XfsNormalizedEvent()
        {
            Vendor = XfsVendor.Unknown;
            SourceLayer = XfsSourceLayer.BusinessJournal;
            Kind = XfsEventKind.Unknown;
            Severity = XfsSeverity.Info;
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public sealed class XfsCassetteSnapshot
    {
        public string CassetteId { get; set; }
        public string Currency { get; set; }
        public int Denomination { get; set; }
        public int RemainingCount { get; set; }
        public int RejectCount { get; set; }
        public int LoadedCount { get; set; }
        public int InCount { get; set; }
        public int OutCount { get; set; }
        public string CassetteState { get; set; }
        public string CassetteType { get; set; }
        public string NoteType { get; set; }
    }
}
