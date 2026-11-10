namespace CreditAssist.Core.Knowledge;

public record PolicySnippet(
    string Id,
    string Title,
    string Text,
    string? Source = null,   // file name, e.g. "SampleBank_PersonalLoan_Policy.pdf"
    int? Page = null);       // page number in that file

public record SearchHit(
    string Id,
    string Title,
    string Text,
    double Score,
    string? Source = null,
    int? Page = null);

public interface IPolicySearch
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top = 3, CancellationToken ct = default);
}