using CreditAssist.Core.Applicants;
using CreditAssist.Infrastructure.Applicants;
using Microsoft.Extensions.DependencyInjection;

namespace CreditAssist.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddCreditAssistInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IApplicantRepository, InMemoryApplicantRepository>();
        return services;
    }
}