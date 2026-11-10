using System.Security.Cryptography;
using System.Text;
using CreditAssist.Core.Knowledge;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace CreditAssist.AI.Knowledge;

public sealed class QdrantPolicyStore(
    QdrantClient qdrant,
    IEmbeddingGenerator<string, Embedding<float>> embedder) : IPolicySearch, IPolicyIndexer
{
    public const string CollectionName = "credit-policies";
    private const ulong VectorSize = 768;   // nomic-embed-text
    private const int BatchSize = 32;       // embed + upsert 32 chunks at a time

    // ---------- Write ----------

    public async Task<int> IndexAsync(IReadOnlyList<PolicySnippet> snippets, CancellationToken ct = default)
    {
        if (snippets.Count == 0) return 0;
        await EnsureCollectionAsync(ct);

        int total = 0;
        foreach (var batch in snippets.Chunk(BatchSize))   // large PDFs → many small batches
        {
            var embeddings = await embedder.GenerateAsync(
                batch.Select(s => $"search_document: {s.Title}. {s.Text}"),
                cancellationToken: ct);

            var points = batch.Zip(embeddings, ToPoint).ToList();
            await qdrant.UpsertAsync(CollectionName, points, cancellationToken: ct);
            total += points.Count;
        }
        return total;
    }

    public async Task DeleteBySourceAsync(string source, CancellationToken ct = default)
    {
        if (!await qdrant.CollectionExistsAsync(CollectionName, ct)) return;
        await qdrant.DeleteAsync(CollectionName, MatchKeyword("source", source), cancellationToken: ct);
    }

    // ---------- Read ----------

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top = 3, CancellationToken ct = default)
    {
        if (!await qdrant.CollectionExistsAsync(CollectionName, ct))
            return [];

        var queryVector = await embedder.GenerateVectorAsync($"search_query: {query}", cancellationToken: ct);

        var results = await qdrant.QueryAsync(
            CollectionName,
            query: queryVector.ToArray(),
            limit: (ulong)top,
            payloadSelector: true,
            cancellationToken: ct);

        return results.Select(ToSearchHit).ToList();
    }

    // ---------- Mapping ----------

    private static PointStruct ToPoint(PolicySnippet snippet, Embedding<float> embedding)
    {
        var point = new PointStruct
        {
            Id = ToPointId(snippet.Id),
            Vectors = embedding.Vector.ToArray(),
            Payload =
            {
                ["id"] = snippet.Id,
                ["title"] = snippet.Title,
                ["text"] = snippet.Text
            }
        };

        if (snippet.Source is not null) point.Payload["source"] = snippet.Source;
        if (snippet.Page is not null) point.Payload["page"] = (long)snippet.Page.Value;
        return point;
    }

    private static SearchHit ToSearchHit(ScoredPoint point)
    {
        var payload = point.Payload;
        return new SearchHit(
            payload["id"].StringValue,
            payload["title"].StringValue,
            payload["text"].StringValue,
            Math.Round(point.Score, 3),
            payload.TryGetValue("source", out var source) ? source.StringValue : null,
            payload.TryGetValue("page", out var page) ? (int)page.IntegerValue : null);
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

    // Same key → same Guid → re-indexing overwrites instead of duplicating
    private static Guid ToPointId(string key) => new(MD5.HashData(Encoding.UTF8.GetBytes(key)));
}