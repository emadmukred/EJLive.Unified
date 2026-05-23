using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Parses raw Electronic Journal lines into structured <see cref="EjTransaction"/> records.
/// </summary>
public interface IEjTransactionParser
{
    /// <summary>
    /// Parses the provided raw EJ lines into a list of transactions.
    /// </summary>
    /// <param name="lines">The raw lines from an electronic journal file.</param>
    /// <param name="atmId">The ATM identifier associated with this journal.</param>
    /// <returns>A list of parsed transactions with forensic metadata.</returns>
    List<EjTransaction> Parse(List<string> lines, string atmId);
}
