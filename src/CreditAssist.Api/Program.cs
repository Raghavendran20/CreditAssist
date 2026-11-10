using CreditAssist.AI;
using CreditAssist.Api.Endpoints;
using CreditAssist.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCreditAssistInfrastructure();
builder.Services.AddCreditAssistAI();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAiEndpoints();

app.Run();