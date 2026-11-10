using CreditAssist.Core.Calculations;

namespace CreditAssist.Tests;

public class CreditMathTests
{
    [Fact]
    public void Emi_For_5Lakh_At_12Percent_For_36Months_Is_Correct()
    {
        var emi = CreditMath.CalculateEmi(500000m, 12m, 36);
        Assert.Equal(16607.15m, emi, 0);
    }

    [Fact]
    public void Foir_Is_Total_Emi_Divided_By_Income()
    {
        Assert.Equal(37.5m, CreditMath.CalculateFoir(80000m, 30000m));
    }

    [Theory]
    [InlineData(0, 12, 36)]
    [InlineData(500000, 12, 0)]
    public void Emi_Is_Zero_For_Invalid_Input(decimal principal, decimal rate, int months)
    {
        Assert.Equal(0m, CreditMath.CalculateEmi(principal, rate, months));
    }

    [Fact]
    public void Foir_Is_Zero_When_Income_Is_Zero()
    {
        Assert.Equal(0m, CreditMath.CalculateFoir(0m, 10000m));
    }
}