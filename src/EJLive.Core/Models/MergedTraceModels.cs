using System;
using System.Collections.Generic;
using EJLive.Core.Xfs;

namespace EJLive.Core.Models
{
    public sealed class NcrMergedTraceEvent
    {
        public DateTime? Timestamp { get; set; }
        public string SourceName { get; set; }
        public XfsVendor Vendor { get; set; }
        public XfsSourceLayer SourceLayer { get; set; }
        public XfsEventKind Kind { get; set; }
        public XfsSeverity Severity { get; set; }
        public string DeviceFamily { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string RawLine { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public NcrMergedTraceEvent()
        {
            Vendor = XfsVendor.NCR;
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public sealed class NcrMergedTraceCorrelationResult
    {
        public string SourceName { get; set; }
        public XfsVendor Vendor { get; set; }
        public int TotalRawLines { get; set; }
        public int TotalEvents { get; set; }
        public List<NcrMergedTraceEvent> Timeline { get; set; }
        public Dictionary<string, int> BySourceLayer { get; set; }
        public Dictionary<string, int> ByKind { get; set; }

        public NcrMergedTraceCorrelationResult()
        {
            Vendor = XfsVendor.NCR;
            SourceName = "NCR MergedTrace";
            Timeline = new List<NcrMergedTraceEvent>();
            BySourceLayer = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ByKind = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
