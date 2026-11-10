namespace CreditAssist.Core.Applicants;

public record ApplicantProfile(
    string Name,
    decimal MonthlyIncome,
    decimal ExistingEmis,
    decimal RequestedLoanAmount,
    int TenureMonths,
    decimal AnnualInterestRate,
    int CibilScore,
    string EmploymentType,      // "Salaried" or "Self-employed"
    int YearsInCurrentJob);