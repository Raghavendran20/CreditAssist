using CreditAssist.Core.AI;

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

        // 2. Streaming answer (word by word, Server-Sent Events)
        group.MapGet("/ask/stream", (string question, ICreditAssistant assistant, CancellationToken ct) =>
            TypedResults.ServerSentEvents(assistant.AskStreamingAsync(question, ct)));

        // 3. Chat with history
        group.MapPost("/chat", async (ChatRequest req, ICreditAssistant assistant, CancellationToken ct) =>
        {
            if (req.Messages is null || req.Messages.Count == 0)
                return Results.BadRequest(new { error = "At least one message is required." });

            return Results.Ok(new { answer = await assistant.ChatAsync(req.Messages, ct) });
        });

        return app;
    }
}

public record AskRequest(string Question);
public record ChatRequest(List<ChatTurn> Messages);