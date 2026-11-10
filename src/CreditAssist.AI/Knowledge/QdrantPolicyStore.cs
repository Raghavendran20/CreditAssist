using System.Security.Cryptography;
using System.Text;
using CreditAssist.Core.Knowledge;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace CreditAssist.AI.Knowledge;

public sealed class QdrantPolicyStore(
    QdrantClient qdrant,
    IEmbeddingGenerator<string, Embedding<float>> embedder) : IPolicySearch, IPolicyIndexer
{
    public const string CollectionName = "credit-policies";
    private const ulong VectorSize = 768;   // nomic-embed-text produces 768 numbers

    // ---------- Write: embed + store ----------

    public async Task<int> IndexAsync(IReadOnlyList<PolicySnippet> snippets, CancellationToken ct = default)
    {
        if (snippets.Count == 0) return 0;

        await EnsureCollectionAsync(ct);

        // 1. Embed all snippets in ONE batch call
        var embeddings = await embedder.GenerateAsync(
            snippets.Select(s => $"search_document: {s.Title}. {s.Text}"),
            cancellationToken: ct);

        // 2. A "point" in Qdrant = id + vector + payload (the original text)
        var points = snippets.Zip(embeddings, (snippet, embedding) => new PointStruct
        {
            Id = ToPointId(snippet.Id),
            Vectors = embedding.Vector.ToArray(),
            Payload =
            {
                ["id"] = snippet.Id,
                ["title"] = snippet.Title,
                ["text"] = snippet.Text
            }
        }).ToList();

        // 3. Upsert = insert new, or overwrite existing with the same id
        await qdrant.UpsertAsync(CollectionName, points, cancellationToken: ct);
        return points.Count;
    }

    // ---------- Read: semantic search ----------

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top = 3, CancellationToken ct = default)
    {
        if (!await qdrant.CollectionExistsAsync(CollectionName, ct))
            return [];   // nothing indexed yet

        var queryVector = await embedder.GenerateVectorAsync($"search_query: {query}", cancellationToken: ct);

              var results = await qdrant.QueryAsync(
            CollectionName,
            query: queryVector.ToArray(),      // nearest-neighbour search with this vector
            limit: (ulong)top,
            payloadSelector: true,             // return the stored text (id, title, text)
            cancellationToken: ct);

        return results
            .Select(point => new SearchHit(
                point.Payload["id"].StringValue,
                point.Payload["title"].StringValue,
                point.Payload["text"].StringValue,
                Math.Round(point.Score, 3)))
            .ToList();
    }

    // ---------- Helpers ----------

    private async Task EnsureCollectionAsync(CancellationToken ct)
    {
        if (await qdrant.CollectionExistsAsync(CollectionName, ct)) return;

        await qdrant.CreateCollectionAsync(
            CollectionName,
            new VectorParams { Size = VectorSize, Distance = Distance.Cosine },
            cancellationToken: ct);
    }

    // Same snippet Id → same Guid → re-indexing overwrites instead of duplicating
    private static Guid ToPointId(string key) => new(MD5.HashData(Encoding.UTF8.GetBytes(key)));
}