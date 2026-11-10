using CreditAssist.AI.Knowledge;

namespace CreditAssist.Tests;

public class VectorMathTests
{
    [Fact]
    public void Identical_Vectors_Have_Similarity_One()
    {
        float[] a = [1, 2, 3];
        Assert.Equal(1.0, VectorMath.CosineSimilarity(a, a), 5);
    }

    [Fact]
    public void Perpendicular_Vectors_Have_Similarity_Zero()
    {
        float[] a = [1, 0];
        float[] b = [0, 1];
        Assert.Equal(0.0, VectorMath.CosineSimilarity(a, b), 5);
    }

    [Fact]
    public void Opposite_Vectors_Have_Similarity_Minus_One()
    {
        float[] a = [1, 2];
        float[] b = [-1, -2];
        Assert.Equal(-1.0, VectorMath.CosineSimilarity(a, b), 5);
    }

    [Fact]
    public void Different_Lengths_Throw()
    {
        float[] a = [1, 2];
        float[] b = [1, 2, 3];
        Assert.Throws<ArgumentException>(() => VectorMath.CosineSimilarity(a, b));
    }
}