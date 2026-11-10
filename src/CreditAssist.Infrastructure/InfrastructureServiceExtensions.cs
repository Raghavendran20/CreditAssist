using CreditAssist.Core.Applicants;
using CreditAssist.Core.Knowledge;
using CreditAssist.Infrastructure.Applicants;
using CreditAssist.Infrastructure.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace CreditAssist.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddCreditAssistInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IApplicantRepository, InMemoryApplicantRepository>();
        services.AddScoped<IDocumentIngestor, PdfDocumentIngestor>();
        return services;
    }
}