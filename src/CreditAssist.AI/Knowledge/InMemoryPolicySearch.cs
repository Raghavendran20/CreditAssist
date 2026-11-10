using CreditAssist.Core.Knowledge;
using Microsoft.Extensions.AI;

namespace CreditAssist.AI.Knowledge;

public sealed class InMemoryPolicySearch(IEmbeddingGenerator<string, Embedding<float>> embedder) : IPolicySearch
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private List<(PolicySnippet Snippet, float[] Vector)>? _index;

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top = 3, CancellationToken ct = default)
    {
        var index = await GetIndexAsync(ct);

        // nomic-embed-text works best with these prefixes
        var queryVector = await embedder.GenerateVectorAsync($"search_query: {query}", cancellationToken: ct);

        return index
            .Select(entry => new SearchHit(
                entry.Snippet.Id,
                entry.Snippet.Title,
                entry.Snippet.Text,
                Math.Round(VectorMath.CosineSimilarity(queryVector.Span, entry.Vector), 3)))
            .OrderByDescending(hit => hit.Score)
            .Take(top)
            .ToList();
    }

    // Embeds all policy snippets ONCE, on the first search, then keeps them in memory
    private async Task<List<(PolicySnippet Snippet, float[] Vector)>> GetIndexAsync(CancellationToken ct)
    {
        if (_index is not null) return _index;

        await _lock.WaitAsync(ct);
        try
        {
            if (_index is not null) return _index;   // another request may have built it

            var embeddings = await embedder.GenerateAsync(
                PolicyData.Snippets.Select(s => $"search_document: {s.Title}. {s.Text}"),
                cancellationToken: ct);

            _index = PolicyData.Snippets
                .Zip(embeddings, (snippet, embedding) => (Snippet: snippet, Vector: embedding.Vector.ToArray()))
                .ToList();

            return _index;
        }
        finally
        {
            _lock.Release();
        }
    }
}