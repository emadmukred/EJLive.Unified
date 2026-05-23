namespace EJLive.Core.Services;

public sealed class MergedTraceCorrelationService
{
    public IReadOnlyList<string> Correlate(IEnumerable<string> hostMessages, IEnumerable<string> xfsEvents)
    {
        return (hostMessages ?? Array.Empty<string>())
            .Concat(xfsEvents ?? Array.Empty<string>())
            .OrderBy(line => line, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
