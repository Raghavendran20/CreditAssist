namespace CreditAssist.Core.Applicants;

public interface IApplicantRepository
{
    Task<ApplicantProfile?> GetByPanAsync(string pan, CancellationToken ct = default);
}