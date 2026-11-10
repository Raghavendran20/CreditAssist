namespace CreditAssist.Core.Calculations;

public static class CreditMath
{
    // EMI = P × r × (1+r)^n / ((1+r)^n − 1), where r = monthly rate
    public static decimal CalculateEmi(decimal principal, decimal annualRatePercent, int tenureMonths)
    {
        if (principal <= 0 || tenureMonths <= 0) return 0;
        if (annualRatePercent == 0) return Math.Round(principal / tenureMonths, 2);

        double r = (double)annualRatePercent / 12 / 100;
        double factor = Math.Pow(1 + r, tenureMonths);
        double emi = (double)principal * r * factor / (factor - 1);
        return Math.Round((decimal)emi, 2);
    }

    // FOIR = total monthly EMIs ÷ monthly income × 100
    public static decimal CalculateFoir(decimal monthlyIncome, decimal totalEmis)
    {
        if (monthlyIncome <= 0) return 0;
        return Math.Round(totalEmis / monthlyIncome * 100, 1);
    }
}