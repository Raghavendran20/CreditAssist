using CreditAssist.Core.Applicants;

namespace CreditAssist.Infrastructure.Applicants;

// Fictional test data. Replaced by a real database in a later step.
public sealed class InMemoryApplicantRepository : IApplicantRepository
{
    private static readonly Dictionary<string, ApplicantProfile> Applicants = new()
    {
        ["ABCDE1234F"] = new("Priya Sharma", 120000, 10000, 500000, 36, 12, 790, "Salaried", 5),
        ["PQRST5678K"] = new("Arun Kumar", 45000, 18000, 400000, 24, 14, 640, "Self-employed", 1),
        ["LMNOP4321Z"] = new("Meena Iyer", 95000, 25000, 1500000, 60, 10.5m, 735, "Salaried", 3)
    };

    public Task<ApplicantProfile?> GetByPanAsync(string pan, CancellationToken ct = default)
    {
        Applicants.TryGetValue(pan.Trim().ToUpperInvariant(), out var applicant);
        return Task.FromResult(applicant);
    }
}