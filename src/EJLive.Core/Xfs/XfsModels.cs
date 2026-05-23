using System.Text.RegularExpressions;

namespace EJLive.Core.Xfs;

public enum XfsSeverity { Trace, Info, Warning, Error, Critical }

public sealed class XfsNormalizedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString("N");
    public string Vendor { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public XfsSeverity Severity { get; set; } = XfsSeverity.Info;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class XfsCassetteSnapshot
{
    public string CassetteId { get; set; } = string.Empty;
    public int Denomination { get; set; }
    public int Count { get; set; }
    public string Currency { get; set; } = "SAR";
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
}

public interface IXfsVendorAdapter
{
    string Vendor { get; }
    IEnumerable<XfsNormalizedEvent> Parse(string text);
}

public sealed class XfsAdapterRegistry
{
    private readonly Dictionary<string, IXfsVendorAdapter> _adapters = new(StringComparer.OrdinalIgnoreCase);
    public void Register(IXfsVendorAdapter adapter) => _adapters[adapter.Vendor] = adapter;
    public IXfsVendorAdapter? Resolve(string vendor) => _adapters.TryGetValue(vendor, out var adapter) ? adapter : null;
}

public abstract class SimpleXfsAdapter : IXfsVendorAdapter
{
    protected SimpleXfsAdapter(string vendor) => Vendor = vendor;
    public string Vendor { get; }

    public virtual IEnumerable<XfsNormalizedEvent> Parse(string text)
    {
        foreach (var line in (text ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            yield return ParseLineCore(line);
    }

    protected virtual XfsNormalizedEvent ParseLineCore(string line)
    {
        var trimmed = (line ?? string.Empty).Trim();
        var severity = trimmed.Contains("critical", StringComparison.OrdinalIgnoreCase)
                       || trimmed.Contains("fatal", StringComparison.OrdinalIgnoreCase)
            ? XfsSeverity.Critical
            : trimmed.Contains("error", StringComparison.OrdinalIgnoreCase)
              || trimmed.Contains("fault", StringComparison.OrdinalIgnoreCase)
            ? XfsSeverity.Error
            : trimmed.Contains("warning", StringComparison.OrdinalIgnoreCase)
              || trimmed.Contains("timeout", StringComparison.OrdinalIgnoreCase)
              || trimmed.Contains("offline", StringComparison.OrdinalIgnoreCase)
            ? XfsSeverity.Warning
            : XfsSeverity.Info;

        return new XfsNormalizedEvent
        {
            Vendor = Vendor,
            Component = InferComponent(trimmed),
            EventCode = severity is XfsSeverity.Error or XfsSeverity.Critical ? "ERROR" : "TRACE",
            Message = trimmed,
            Severity = severity
        };
    }

    protected static string InferComponent(string line)
    {
        if (line.Contains("cassette", StringComparison.OrdinalIgnoreCase)
            || line.Contains("dispens", StringComparison.OrdinalIgnoreCase)
            || line.Contains("cdm", StringComparison.OrdinalIgnoreCase))
            return "CashDispenser";
        if (line.Contains("deposit", StringComparison.OrdinalIgnoreCase)
            || line.Contains("cim", StringComparison.OrdinalIgnoreCase))
            return "CashAcceptor";
        if (line.Contains("card", StringComparison.OrdinalIgnoreCase)
            || line.Contains("reader", StringComparison.OrdinalIgnoreCase)
            || line.Contains("idc", StringComparison.OrdinalIgnoreCase))
            return "CardReader";
        if (line.Contains("printer", StringComparison.OrdinalIgnoreCase)
            || line.Contains("receipt", StringComparison.OrdinalIgnoreCase)
            || line.Contains("ptr", StringComparison.OrdinalIgnoreCase))
            return "Printer";
        if (line.Contains("pin", StringComparison.OrdinalIgnoreCase)
            || line.Contains("epp", StringComparison.OrdinalIgnoreCase))
            return "PinPad";
        if (line.Contains("network", StringComparison.OrdinalIgnoreCase)
            || line.Contains("host", StringComparison.OrdinalIgnoreCase)
            || line.Contains("line ", StringComparison.OrdinalIgnoreCase))
            return "Connectivity";
        return "XFS";
    }
}

public abstract class DictionaryXfsAdapter : SimpleXfsAdapter
{
    private readonly IReadOnlyList<XfsDictionaryRule> _rules;
    private readonly string _dictionaryName;

    protected DictionaryXfsAdapter(string vendor, string dictionaryName, IReadOnlyList<XfsDictionaryRule> rules)
        : base(vendor)
    {
        _rules = rules;
        _dictionaryName = dictionaryName;
    }

    protected override XfsNormalizedEvent ParseLineCore(string line)
    {
        var trimmed = (line ?? string.Empty).Trim();
        foreach (var rule in _rules)
        {
            if (!rule.IsMatch(trimmed))
                continue;

            return new XfsNormalizedEvent
            {
                Vendor = Vendor,
                Component = rule.Component,
                EventCode = rule.Code,
                Message = trimmed,
                Severity = rule.Severity,
                Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["dictionary"] = _dictionaryName,
                    ["recommended_action"] = rule.RecommendedAction
                }
            };
        }

        var fallback = base.ParseLineCore(trimmed);
        fallback.Attributes["dictionary"] = _dictionaryName + "_fallback";
        return fallback;
    }
}

public sealed class NcrXfsAdapter : DictionaryXfsAdapter
{
    private static readonly XfsDictionaryRule[] Rules =
    [
        new("NCR_SDC_LINK_FAILURE", "DeviceLink", XfsSeverity.Critical, "(m-146|sdc).*(error|fault|timeout|lost|down)", "Check NCR SDC link and XFS/SP communications."),
        new("NCR_DISPENSE_FAULT", "CashDispenser", XfsSeverity.Critical, "(dispense|cdm|cassette).*(error|fault|reject|timeout|jam)", "Run NCR dispenser diagnostics and verify reject/divert paths."),
        new("NCR_CARD_CAPTURE_TIMEOUT", "CardReader", XfsSeverity.Warning, "(card captured|take card timeout|retain)", "Audit captured-card flow and inspect card reader path."),
        new("NCR_PRINTER_SUPPLY", "Printer", XfsSeverity.Warning, "(printer|receipt|journal).*(paper low|paper out|ribbon|printhead|knife)", "Refill supplies and validate printer module state."),
        new("NCR_SENSOR_TAMPER", "Sensors", XfsSeverity.Critical, "(tamper|safe door|vault door|silent alarm|mode switch)", "Check physical security sensors and supervisor events."),
        new("NCR_POWER_RESET", "Terminal", XfsSeverity.Info, "(power-up/reset|power up|terminal reset)", "Correlate reset marker with previous faults and recovery timeline.")
    ];

    public NcrXfsAdapter() : base("NCR", "NCR_DIAGNOSTIC_V1", Rules) { }
}

public sealed class GrgXfsAdapter : DictionaryXfsAdapter
{
    private static readonly XfsDictionaryRule[] Rules =
    [
        new("GRG_LINE_DOWN", "Connectivity", XfsSeverity.Critical, "(line down|enter offline mode|outofservice)", "Validate host line and GRG terminal connectivity state."),
        new("GRG_DISPENSE_FAIL", "CashDispenser", XfsSeverity.Critical, "(dispense fail|cash unit changed|cdm retract|cash retract)", "Reconcile cassette/reject counters and inspect dispenser transport."),
        new("GRG_TAKE_CASH_TIMEOUT", "CashDispenser", XfsSeverity.Warning, "(take cash timeout|customer no take|retract)", "Track not-taken cash path and verify retract evidence."),
        new("GRG_CIM_RETRACT", "CashAcceptor", XfsSeverity.Warning, "(cim retract|cash deposit retract|deposit retract)", "Check cash-in module retract reasons and bin state."),
        new("GRG_CARD_CAPTURE", "CardReader", XfsSeverity.Warning, "(card captured|take card timeout)", "Audit card capture incident and branch handover process."),
        new("GRG_CASSETTE_LOW", "CashDispenser", XfsSeverity.Warning, "(cassette status|cas\\().*(low|out|empty)", "Plan replenishment and verify cassette availability.")
    ];

    public GrgXfsAdapter() : base("GRG", "GRG_DIAGNOSTIC_V1", Rules) { }
}

public sealed class WincorXfsAdapter : DictionaryXfsAdapter
{
    private static readonly XfsDictionaryRule[] Rules =
    [
        new("WN_CDM_FAULT", "CashDispenser", XfsSeverity.Critical, "(wincor|nixdorf|protopas|wn).*(cdm|cash unit|dispense).*(error|fault|jam|reject)", "Inspect Wincor CDM path and cassette states."),
        new("WN_IDC_FAULT", "CardReader", XfsSeverity.Warning, "(wincor|nixdorf|idc).*(retain|capture|jam|fault)", "Inspect Wincor IDC reader and capture bin."),
        new("WN_PTR_SUPPLY", "Printer", XfsSeverity.Warning, "(wincor|nixdorf|ptr|receipt).*(paper|ribbon|low|out|empty)", "Refill printer media and confirm PTR state."),
        new("WN_SP_ERROR", "ServiceProvider", XfsSeverity.Critical, "(wosa/xfs|service provider|sp).*(error|not available|offline)", "Restart WOSA/XFS services and validate provider bindings."),
        new("WN_EPP_TAMPER", "PinPad", XfsSeverity.Critical, "(epp|pinpad).*(tamper|fault|error)", "Escalate secure PIN device tamper workflow immediately.")
    ];

    public WincorXfsAdapter() : base("WINCOR", "WINCOR_DIAGNOSTIC_V1", Rules) { }
}

public sealed class HyosungXfsAdapter : DictionaryXfsAdapter
{
    private static readonly XfsDictionaryRule[] Rules =
    [
        new("HYO_CDM_FAULT", "CashDispenser", XfsSeverity.Critical, "(hyosung|nautilus|hcdm).*(dispense|cassette|reject|fault|error|jam)", "Run Hyosung CDM diagnostics and inspect reject path."),
        new("HYO_CARD_READER_FAULT", "CardReader", XfsSeverity.Warning, "(hyosung|hcrw|card reader).*(capture|retain|jam|fault)", "Inspect Hyosung card reader transport and throat sensor."),
        new("HYO_TAKE_CASH_TIMEOUT", "CashDispenser", XfsSeverity.Warning, "(take cash timeout|cash retract|customer no take)", "Record not-taken cash event and reconcile retract totals."),
        new("HYO_PTR_SUPPLY", "Printer", XfsSeverity.Warning, "(hyosung|receipt printer|ptr).*(paper|low|out|empty)", "Refill receipt media and validate printer status."),
        new("HYO_EPP_ALERT", "PinPad", XfsSeverity.Critical, "(hyosung|epp|pin).*(tamper|fault|error)", "Follow Hyosung secure PIN fault/tamper procedures.")
    ];

    public HyosungXfsAdapter() : base("HYOSUNG", "HYOSUNG_DIAGNOSTIC_V1", Rules) { }
}

public sealed class DieboldMdsAdapter : SimpleXfsAdapter { public DieboldMdsAdapter() : base("DIEBOLD") { } }
public sealed class NcrJournalAdapter : SimpleXfsAdapter { public NcrJournalAdapter() : base("NCR_JOURNAL") { } }
public sealed class GrgJournalAdapter : SimpleXfsAdapter { public GrgJournalAdapter() : base("GRG_JOURNAL") { } }
public sealed class DebugTraceAdapter : SimpleXfsAdapter { public DebugTraceAdapter() : base("DEBUG_TRACE") { } }
public sealed class CardReaderTraceAdapter : SimpleXfsAdapter { public CardReaderTraceAdapter() : base("CARD_READER") { } }
public sealed class HostMessageInAdapter : SimpleXfsAdapter { public HostMessageInAdapter() : base("HOST_IN") { } }
public sealed class HostMessageOutAdapter : SimpleXfsAdapter { public HostMessageOutAdapter() : base("HOST_OUT") { } }
public sealed class OoxfsRuntimeAdapter : SimpleXfsAdapter { public OoxfsRuntimeAdapter() : base("OOXFS") { } }

public sealed class XfsDictionaryRule
{
    private readonly Regex _pattern;

    public XfsDictionaryRule(string code, string component, XfsSeverity severity, string pattern, string recommendedAction)
    {
        Code = code;
        Component = component;
        Severity = severity;
        RecommendedAction = recommendedAction;
        _pattern = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }

    public string Code { get; }
    public string Component { get; }
    public XfsSeverity Severity { get; }
    public string RecommendedAction { get; }

    public bool IsMatch(string line) => _pattern.IsMatch(line ?? string.Empty);
}
