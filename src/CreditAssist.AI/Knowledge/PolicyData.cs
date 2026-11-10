using CreditAssist.Core.Knowledge;

namespace CreditAssist.AI.Knowledge;

// Sample personal-loan policy for a fictional bank (for learning only).
public static class PolicyData
{
    public static readonly IReadOnlyList<PolicySnippet> Snippets =
    [
        new("FOIR-LIMIT", "FOIR limit",
            "FOIR including the proposed EMI must not exceed 50% for salaried applicants and 45% for self-employed applicants."),
        new("CIBIL-MIN", "Minimum CIBIL score",
            "Minimum CIBIL score is 700 for unsecured personal loans. Scores from 650 to 699 need credit manager approval. Scores below 650 are declined."),
        new("SELF-EMP-DOCS", "Self-employed documents",
            "Self-employed applicants must provide income tax returns for the last 2 years, 12 months of bank statements and proof that the business has run for at least 3 years."),
        new("SALARIED-DOCS", "Salaried documents",
            "Salaried applicants must provide the last 3 months of salary slips, 6 months of bank statements and Form 16."),
        new("MIN-INCOME", "Minimum income",
            "Minimum net monthly income is ₹25,000 for salaried applicants and an average of ₹40,000 per month for self-employed applicants."),
        new("JOB-STABILITY", "Job stability",
            "Salaried applicants need at least 1 year in the current job and 2 years of total work experience."),
        new("AGE", "Age criteria",
            "The applicant must be at least 21 years old when applying and not older than 60 when the loan ends."),
        new("LOAN-AMOUNT", "Maximum loan amount",
            "The maximum personal loan is 20 times the net monthly income, capped at ₹25,00,000."),
        new("BOUNCES", "Payment bounces",
            "More than 2 EMI or cheque bounces in the last 6 months is a red flag and the application needs manual review."),
        new("CO-APPLICANT", "Co-applicant income",
            "A co-applicant's income can be added only if they are an immediate family member with a CIBIL score of 700 or above.")
    ];
}