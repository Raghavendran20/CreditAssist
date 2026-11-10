using CreditAssist.AI.Knowledge;
using CreditAssist.Core.AI;
using CreditAssist.Core.Applicants;
using CreditAssist.Core.Knowledge;
using Microsoft.Extensions.AI;

namespace CreditAssist.Api.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai").WithTags("AI");

        // 1. Single question
        group.MapPost("/ask", async (AskRequest req, ICreditAssistant assistant, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Question))
                return Results.BadRequest(new { error = "Question is required." });

            return Results.Ok(new { answer = await assistant.AskAsync(req.Question, ct) });
        });

        // 2. Streaming answer (Server-Sent Events)
        group.MapGet("/ask/stream", (string question, ICreditAssistant assistant, CancellationToken ct) =>
            TypedResults.ServerSentEvents(assistant.AskStreamingAsync(question, ct)));

        // 3. Chat with history
        group.MapPost("/chat", async (ChatRequest req, ICreditAssistant assistant, CancellationToken ct) =>
        {
            if (req.Messages is null || req.Messages.Count == 0)
                return Results.BadRequest(new { error = "At least one message is required." });

            return Results.Ok(new { answer = await assistant.ChatAsync(req.Messages, ct) });
        });

        // 4. Structured loan assessment
        group.MapPost("/assess", async (ApplicantProfile applicant, ICreditAssistant assistant, CancellationToken ct) =>
        {
            if (applicant.MonthlyIncome <= 0 || applicant.RequestedLoanAmount <= 0 || applicant.TenureMonths <= 0)
                return Results.BadRequest(new { error = "Income, loan amount and tenure must be greater than 0." });
            if (applicant.CibilScore is < 300 or > 900)
                return Results.BadRequest(new { error = "CIBIL score must be between 300 and 900." });

            try
            {
                return Results.Ok(await assistant.AssessAsync(applicant, ct));
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(ex.Message, statusCode: 502);
            }
        });

        // 5. Agent with tool calling
        group.MapPost("/agent", async (AskRequest req, ICreditAssistant assistant, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Question))
                return Results.BadRequest(new { error = "Question is required." });

            return Results.Ok(await assistant.AskWithToolsAsync(req.Question, ct));
        });

        // 6. See what an embedding looks like
        group.MapGet("/embed", async (string text, IEmbeddingGenerator<string, Embedding<float>> embedder, CancellationToken ct) =>
        {
            var vector = await embedder.GenerateVectorAsync(text, cancellationToken: ct);
            return Results.Ok(new
            {
                text,
                dimensions = vector.Length,
                first5Values = vector.Span[..5].ToArray()
            });
        });

        // 7. Semantic search over policy rules + PDF chunks (Qdrant)
        group.MapGet("/policy/search", async (string q, IPolicySearch search, CancellationToken ct, int top = 3) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest(new { error = "Query 'q' is required." });

            return Results.Ok(await search.SearchAsync(q, Math.Clamp(top, 1, 10), ct));
        });

        // 8. Load the 10 sample policy rules into Qdrant
        group.MapPost("/policy/reindex", async (IPolicyIndexer indexer, CancellationToken ct) =>
        {
            var count = await indexer.IndexAsync(PolicyData.Snippets, ct);
            return Results.Ok(new { indexed = count, collection = QdrantPolicyStore.CollectionName });
        });

        // 9. Upload a policy PDF → extract, chunk, embed, store (Day 9)
        group.MapPost("/documents/upload", async (IFormFile file, IDocumentIngestor ingestor, CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "Please upload a PDF file." });
            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { error = "Only PDF files are supported." });
            if (file.Length > 10 * 1024 * 1024)
                return Results.BadRequest(new { error = "The file is larger than 10 MB." });

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await ingestor.IngestPdfAsync(stream, Path.GetFileName(file.FileName), ct);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .DisableAntiforgery();

        return app;   // ← always the LAST line of the method
    }
}

public record AskRequest(string Question);
public record ChatRequest(List<ChatTurn> Messages);