using CreditAssist.AI;
using CreditAssist.Api.Endpoints;
using CreditAssist.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCreditAssistInfrastructure();
builder.Services.AddCreditAssistAI();

// Allow the Angular dev server to call this API
builder.Services.AddCors(options =>
    options.AddPolicy("Angular", policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Angular");     // must come BEFORE the endpoints
app.MapAiEndpoints();

app.Run();