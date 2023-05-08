namespace Library;

public sealed class Depot
{
    public HashSet<int> Data { get; } = new();

    /// <summary>
    /// Get the references for consumers out of the total list of nodes. The returning
    /// memory has the size of consumers and each consumer points to the original node
    /// index. A consumer node is a node which isn't a depot.
    /// </summary>
    /// <param name="length">the total number of nodes</param>
    /// <returns>the consumer references</returns>
    public ReadOnlyMemory<int> GetConsumerAssignments(int length)
    {
        if (length < Data.Count)
            throw new ArgumentOutOfRangeException(nameof(length), "the number of nodes to be at least the number of depots");
        Memory<int> consumer = new int[length - Data.Count];
        int index = 0;
        for (int i = 0; i < length; ++i)
        {
            if (Data.Contains(i))
                continue;
            consumer.Span[index++] = i;
        }
        return consumer;
    }

    public void WriteTo(TextWriter writer)
    {
        writer.WriteLine("DEPOT_SECTION");
        foreach (var entry in Data)
            writer.WriteLine(entry + 1);
        writer.WriteLine(-1);
    }
}
