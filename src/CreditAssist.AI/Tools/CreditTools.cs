using System.ComponentModel;
using CreditAssist.Core.Applicants;
using CreditAssist.Core.Calculations;
using Microsoft.Extensions.AI;

namespace CreditAssist.AI.Tools;

public sealed class CreditTools(IApplicantRepository repository)
{
    [Description("Gets an applicant's profile (income, existing EMIs, CIBIL score, employment) by PAN number. Returns null if not found.")]
    public async Task<ApplicantProfile?> GetApplicantByPan(
        [Description("10-character PAN, for example ABCDE1234F")] string pan)
        => await repository.GetByPanAsync(pan);

    [Description("Calculates the monthly EMI in INR for a loan.")]
    public decimal CalculateEmi(
        [Description("Loan amount in INR")] decimal loanAmount,
        [Description("Annual interest rate in percent, for example 12")] decimal annualInterestRatePercent,
        [Description("Loan tenure in months")] int tenureMonths)
        => CreditMath.CalculateEmi(loanAmount, annualInterestRatePercent, tenureMonths);

    [Description("Calculates FOIR (Fixed Obligation to Income Ratio) in percent.")]
    public decimal CalculateFoir(
        [Description("Monthly income in INR")] decimal monthlyIncome,
        [Description("Total of ALL monthly EMIs (existing + new) in INR")] decimal totalMonthlyEmis)
        => CreditMath.CalculateFoir(monthlyIncome, totalMonthlyEmis);

    public IList<AITool> AsTools() =>
    [
        AIFunctionFactory.Create(GetApplicantByPan),
        AIFunctionFactory.Create(CalculateEmi),
        AIFunctionFactory.Create(CalculateFoir)
    ];
}