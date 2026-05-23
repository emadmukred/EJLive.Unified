using System.Collections.Generic;
using EJLive.Core.Xfs.Models;

namespace EJLive.Core.Xfs.Adapters
{
    public interface IXfsVendorAdapter
    {
        string Vendor { get; }
        bool CanParse(string line);
        IEnumerable<XfsNormalizedEvent> ParseLine(string line);
    }
}
