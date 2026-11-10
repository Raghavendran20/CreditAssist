using CreditAssist.AI.Knowledge;
using CreditAssist.AI.Tools;
using CreditAssist.Core.AI;
using CreditAssist.Core.Knowledge;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using Qdrant.Client;

namespace CreditAssist.AI;

public static class AiServiceExtensions
{
    public static IServiceCollection AddCreditAssistAI(this IServiceCollection services, IConfiguration config)
    {
        var ollamaUri      = new Uri(config["AI:OllamaUrl"] ?? "http://localhost:11434");
        var chatModel      = config["AI:ChatModel"] ?? "qwen2.5:3b";
        var embeddingModel = config["AI:EmbeddingModel"] ?? "nomic-embed-text";
        var qdrantHost     = config["Qdrant:Host"] ?? "localhost";
        var qdrantPort     = int.TryParse(config["Qdrant:Port"], out var port) ? port : 6334;

        // LLM + embeddings (Ollama)
        services.AddChatClient(new OllamaApiClient(ollamaUri, chatModel))
                .UseFunctionInvocation();
        services.AddEmbeddingGenerator(new OllamaApiClient(ollamaUri, embeddingModel));

        // Vector database (Qdrant) — one client for the whole app
        services.AddSingleton(new QdrantClient(qdrantHost, qdrantPort));

        // One store, exposed through two interfaces (read + write)
        services.AddSingleton<QdrantPolicyStore>();
        services.AddSingleton<IPolicySearch>(sp => sp.GetRequiredService<QdrantPolicyStore>());
        services.AddSingleton<IPolicyIndexer>(sp => sp.GetRequiredService<QdrantPolicyStore>());

        services.AddScoped<CreditTools>();
        services.AddScoped<ICreditAssistant, CreditAssistant>();
        return services;
    }
}