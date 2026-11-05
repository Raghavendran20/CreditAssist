using System.Runtime.CompilerServices;
using CreditAssist.AI.Prompts;
using CreditAssist.Core.AI;
using Microsoft.Extensions.AI;

namespace CreditAssist.AI;

public sealed class CreditAssistant(IChatClient chatClient) : ICreditAssistant
{
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