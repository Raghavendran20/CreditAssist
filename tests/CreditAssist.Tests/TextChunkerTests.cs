using CreditAssist.Core.Knowledge;

namespace CreditAssist.Tests;

public class TextChunkerTests
{
    private static string LongText(int sentences) =>
        string.Join(" ", Enumerable.Range(1, sentences)
            .Select(i => $"This is sentence number {i} about the personal loan policy."));

    [Fact]
    public void Empty_Text_Returns_No_Chunks()
    {
        Assert.Empty(TextChunker.Split(""));
        Assert.Empty(TextChunker.Split("   \n\t  "));
        Assert.Empty(TextChunker.Split(null));
    }

    [Fact]
    public void Short_Text_Returns_One_Chunk()
    {
        var chunks = TextChunker.Split("Minimum CIBIL score is 700.");
        Assert.Single(chunks);
        Assert.Equal("Minimum CIBIL score is 700.", chunks[0]);
    }

    [Fact]
    public void No_Chunk_Is_Longer_Than_Max()
    {
        var chunks = TextChunker.Split(LongText(100), maxChars: 300, overlapChars: 60);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c => Assert.True(c.Length <= 300, $"Chunk length {c.Length} exceeds 300"));
    }

    [Fact]
    public void Consecutive_Chunks_Overlap()
    {
        var chunks = TextChunker.Split(LongText(50), maxChars: 300, overlapChars: 60);

        // The start of chunk 2 must appear in chunk 1 (that is the overlap)
        var startOfSecond = chunks[1][..20];
        Assert.Contains(startOfSecond, chunks[0]);
    }

    [Fact]
    public void Whitespace_And_Line_Breaks_Are_Normalised()
    {
        var chunks = TextChunker.Split("FOIR   limit\n\nis 50%.");
        Assert.Equal("FOIR limit is 50%.", chunks[0]);
    }

    [Fact]
    public void Very_Long_Sentence_Is_Cut_To_Fit()
    {
        var oneHugeSentence = new string('a', 1000);
        var chunks = TextChunker.Split(oneHugeSentence, maxChars: 300, overlapChars: 50);

        Assert.All(chunks, c => Assert.True(c.Length <= 300));
    }
}