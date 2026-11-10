using System.Text;
using System.Text.RegularExpressions;

namespace CreditAssist.Core.Knowledge;

public static class TextChunker
{
    /// Splits text into chunks of at most maxChars, keeping sentences together
    /// and repeating the last ~overlapChars of each chunk at the start of the next.
    public static IReadOnlyList<string> Split(string? text, int maxChars = 800, int overlapChars = 150)
    {
        if (maxChars <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxChars));
        if (overlapChars < 0 || overlapChars >= maxChars / 2)
            throw new ArgumentOutOfRangeException(nameof(overlapChars), "Overlap must be less than half of maxChars.");

        // 1. Normalise whitespace (PDFs contain many line breaks and double spaces)
        var clean = Regex.Replace(text ?? "", @"\s+", " ").Trim();
        if (clean.Length == 0) return [];

        // 2. Split into sentences (after . ! or ? followed by a space)
        var sentences = Regex.Split(clean, @"(?<=[.!?])\s+").Where(s => s.Length > 0);

        // A single piece must fit even after an overlap has been added in front of it
        int maxPiece = maxChars - overlapChars - 1;

        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var sentence in sentences)
        {
            foreach (var piece in HardWrap(sentence, maxPiece))   // very long sentences are cut
            {
                if (current.Length > 0 && current.Length + 1 + piece.Length > maxChars)
                {
                    var finished = current.ToString();
                    chunks.Add(finished);

                    current.Clear();
                    current.Append(Tail(finished, overlapChars));  // start next chunk with the overlap
                }

                if (current.Length > 0) current.Append(' ');
                current.Append(piece);
            }
        }

        if (current.Length > 0) chunks.Add(current.ToString());
        return chunks;
    }

    // Last n characters, starting at a word boundary so no word is cut in half
    private static string Tail(string text, int n)
    {
        if (n == 0) return "";
        if (text.Length <= n) return text;

        var tail = text[^n..];
        var firstSpace = tail.IndexOf(' ');
        return firstSpace >= 0 ? tail[(firstSpace + 1)..] : tail;
    }

    private static IEnumerable<string> HardWrap(string text, int max)
    {
        for (int i = 0; i < text.Length; i += max)
            yield return text.Substring(i, Math.Min(max, text.Length - i));
    }
}