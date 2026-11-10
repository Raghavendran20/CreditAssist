using System.Runtime.CompilerServices;
using System.Text.Json;
using CreditAssist.AI.Prompts;
using CreditAssist.AI.Tools;
using CreditAssist.Core.AI;
using CreditAssist.Core.Applicants;
using CreditAssist.Core.Calculations;
using Microsoft.Extensions.AI;

namespace CreditAssist.AI;

public sealed class CreditAssistant(IChatClient chatClient, CreditTools creditTools) : ICreditAssistant
{
    // ---------- Day 2: Q&A, streaming, chat ----------

    public async Task<string> AskAsync(string question, CancellationToken ct = default)
    {
        var response = await chatClient.GetResponseAsync(
            BuildMessages([new ChatTurn("user", question)]), cancellationToken: ct);
        return response.Text;
    }

    public async IAsyncEnumerable<string> AskStreamingAsync(
        string question, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var update in chatClient.GetStreamingResponseAsync(
            BuildMessages([new ChatTurn("user", question)]), cancellationToken: ct))
        {
            if (!string.IsNullOrEmpty(update.Text))
                yield return update.Text;
        }
    }

    public async Task<string> ChatAsync(IReadOnlyList<ChatTurn> history, CancellationToken ct = default)
    {
        var response = await chatClient.GetResponseAsync(BuildMessages(history), cancellationToken: ct);
        return response.Text;
    }

    // ---------- Day 3: structured loan assessment ----------

    public async Task<AssessmentResult> AssessAsync(ApplicantProfile applicant, CancellationToken ct = default)
    {
        var newEmi = CreditMath.CalculateEmi(
            applicant.RequestedLoanAmount, applicant.AnnualInterestRate, applicant.TenureMonths);
        var foir = CreditMath.CalculateFoir(applicant.MonthlyIncome, applicant.ExistingEmis + newEmi);

        var prompt = $"""
            Applicant profile:
            {JsonSerializer.Serialize(applicant)}

            System-calculated figures:
            - New EMI: ₹{newEmi:N0}
            - FOIR (existing + new EMI): {foir}%  (bank limit: 50%)

            Assess this applicant.
            """;

        var response = await chatClient.GetResponseAsync<LoanAssessment>(
            [
                new ChatMessage(ChatRole.System, SystemPrompts.Assessor),
                new ChatMessage(ChatRole.User, prompt)
            ],
            new ChatOptions { Temperature = 0.1f },
            cancellationToken: ct);

        if (!response.TryGetResult(out var assessment) || assessment is null)
            throw new InvalidOperationException("The AI did not return a valid assessment.");

        if ((foir > 50 || applicant.CibilScore < 650) && assessment.RiskLevel == RiskLevel.Low)
            assessment = assessment with { RiskLevel = RiskLevel.Medium };

        return new AssessmentResult(newEmi, foir, assessment);
    }

    // ---------- Day 4: tool calling ----------

    public async Task<AgentAnswer> AskWithToolsAsync(string question, CancellationToken ct = default)
    {
        var options = new ChatOptions
        {
            Tools = creditTools.AsTools(),
            Temperature = 0.1f
        };

        var response = await chatClient.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompts.Agent),
                new ChatMessage(ChatRole.User, question)
            ],
            options,
            cancellationToken: ct);

        // Collect every tool call the AI made (for transparency and audit)
        var toolsUsed = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionCallContent>()
            .Select(call => $"{call.Name}({string.Join(", ",
                call.Arguments?.Select(a => $"{a.Key}={a.Value}") ?? [])})")
            .ToList();

        return new AgentAnswer(response.Text, toolsUsed);
    }

    // ---------- Helpers ----------

    private static List<ChatMessage> BuildMessages(IEnumerable<ChatTurn> turns)
    {
        var messages = new List<ChatMessage> { new(ChatRole.System, SystemPrompts.CreditAssistant) };

        foreach (var turn in turns)
        {
            var role = turn.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                ? ChatRole.Assistant
                : ChatRole.User;
            messages.Add(new ChatMessage(role, turn.Content));
        }
        return messages;
    }
}