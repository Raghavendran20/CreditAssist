namespace CreditAssist.Core.Knowledge;

public interface IPolicyIndexer
{
    /// Embeds and stores snippets. Re-indexing the same Id overwrites it.
    Task<int> IndexAsync(IReadOnlyList<PolicySnippet> snippets, CancellationToken ct = default);

    /// Removes every chunk that came from this source file.
    Task DeleteBySourceAsync(string source, CancellationToken ct = default);
}