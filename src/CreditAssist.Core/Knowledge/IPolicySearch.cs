namespace CreditAssist.Core.Knowledge;

public record PolicySnippet(string Id, string Title, string Text);

public record SearchHit(string Id, string Title, string Text, double Score);

public interface IPolicySearch
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(string query, int top = 3, CancellationToken ct = default);
}