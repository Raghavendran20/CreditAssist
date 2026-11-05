namespace CreditAssist.Core.AI;

public record ChatTurn(string Role, string Content);

public interface ICreditAssistant
{
    Task<string> AskAsync(string question, CancellationToken ct = default);
    IAsyncEnumerable<string> AskStreamingAsync(string question, CancellationToken ct = default);
    Task<string> ChatAsync(IReadOnlyList<ChatTurn> history, CancellationToken ct = default);
}