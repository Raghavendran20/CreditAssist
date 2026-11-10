namespace CreditAssist.Core.Knowledge;

public interface IPolicyIndexer
{
    /// Embeds and stores policy snippets. Re-indexing the same Id overwrites it.
    Task<int> IndexAsync(IReadOnlyList<PolicySnippet> snippets, CancellationToken ct = default);
}