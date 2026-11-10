using CreditAssist.Core.Applicants;

namespace CreditAssist.Core.AI;

public record ChatTurn(string Role, string Content);

public record AgentAnswer(string Answer, List<string> ToolsUsed);

public interface ICreditAssistant
{
    Task<string> AskAsync(string question, CancellationToken ct = default);
    IAsyncEnumerable<string> AskStreamingAsync(string question, CancellationToken ct = default);
    Task<string> ChatAsync(IReadOnlyList<ChatTurn> history, CancellationToken ct = default);
    Task<AssessmentResult> AssessAsync(ApplicantProfile applicant, CancellationToken ct = default);
    Task<AgentAnswer> AskWithToolsAsync(string question, CancellationToken ct = default);
}