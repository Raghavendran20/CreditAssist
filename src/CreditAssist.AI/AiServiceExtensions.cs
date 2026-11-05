using CreditAssist.Core.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;

namespace CreditAssist.AI;

public static class AiServiceExtensions
{
    public static IServiceCollection AddCreditAssistAI(this IServiceCollection services)
    {
        IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"), "qwen2.5:3b");

        services.AddChatClient(chatClient);
        services.AddScoped<ICreditAssistant, CreditAssistant>();
        return services;
    }
}