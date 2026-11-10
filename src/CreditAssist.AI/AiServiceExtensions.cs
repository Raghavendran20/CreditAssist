using CreditAssist.AI.Knowledge;
using CreditAssist.AI.Tools;
using CreditAssist.Core.AI;
using CreditAssist.Core.Knowledge;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace CreditAssist.AI;

public static class AiServiceExtensions
{
    private static readonly Uri OllamaUri = new("http://localhost:11434");

    public static IServiceCollection AddCreditAssistAI(this IServiceCollection services)
    {
        // Chat model (with tool calling)
        services.AddChatClient(new OllamaApiClient(OllamaUri, "qwen2.5:3b"))
                .UseFunctionInvocation();

        // Embedding model
        services.AddEmbeddingGenerator(new OllamaApiClient(OllamaUri, "nomic-embed-text"));

        services.AddSingleton<IPolicySearch, InMemoryPolicySearch>();   // singleton: index built once
        services.AddScoped<CreditTools>();
        services.AddScoped<ICreditAssistant, CreditAssistant>();
        return services;
    }
}