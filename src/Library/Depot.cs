namespace Library;

public sealed class Depot
{
    public HashSet<int> Data { get; } = new();

    public void WriteTo(TextWriter writer)
    {
        writer.WriteLine("DEPOT_SECTION");
        foreach (var entry in Data)
            writer.WriteLine(entry + 1);
        writer.WriteLine(-1);
    }
}
