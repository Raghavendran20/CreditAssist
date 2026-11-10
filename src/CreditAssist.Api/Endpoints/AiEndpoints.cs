using CreditAssist.Core.AI;
using CreditAssist.Core.Applicants;

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

        return app;
    }
}

public record AskRequest(string Question);
public record ChatRequest(List<ChatTurn> Messages);