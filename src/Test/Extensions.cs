namespace Test;

public static class Extensions
{
    public static void AssertEqual<T>(this ReadOnlySpan<T> current, ReadOnlySpan<T> expected)
    {
        Assert.AreEqual(expected.Length, current.Length, "check length");
        for (int i = 0; i < expected.Length; ++i)
            Assert.AreEqual(expected[i], current[i], $"check at {i}");
    }
}
