using System.ComponentModel;
using System.Text.Json.Serialization;

namespace CreditAssist.Core.AI;

[JsonConverter(typeof(JsonStringEnumConverter<RiskLevel>))]
public enum RiskLevel { Low, Medium, High }

public record LoanAssessment(
    [property: Description("Overall credit risk: Low, Medium or High")] RiskLevel RiskLevel,
    [property: Description("2 to 4 positive points about the applicant")] List<string> Strengths,
    [property: Description("0 to 4 concerns; empty list if none")] List<string> RedFlags,
    [property: Description("Two-sentence summary for the underwriter")] string Summary);

public record AssessmentResult(
    decimal NewEmi,
    decimal FoirPercent,
    LoanAssessment Assessment);