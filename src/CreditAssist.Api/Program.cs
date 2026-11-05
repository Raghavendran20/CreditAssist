using CreditAssist.AI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCreditAssistAI();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/api/ai/ask", async (AskRequest req, IChatClient ai) =>
{
    var response = await ai.GetResponseAsync(
    [
        new ChatMessage(ChatRole.System,
            "You are a credit underwriting assistant for an Indian bank. Answer briefly and clearly."),
        new ChatMessage(ChatRole.User, req.Question)
    ]);

    return Results.Ok(new { answer = response.Text });
});

app.Run();

record AskRequest(string Question);