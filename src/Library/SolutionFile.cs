namespace Library;

public sealed class SolutionFile
{
    public List<List<int>> Routes { get; } = new();

    public long Cost { get; set; }

    public void WriteTo(TextWriter writer)
    {
        for (int i = 0; i < Routes.Count; ++i)
        {
            writer.Write("Route #");
            writer.Write(i + 1);
            writer.Write(':');
            foreach (var node in Routes[i])
            {
                writer.Write(' ');
                writer.Write(node + 1);
            }
            writer.WriteLine();
        }
        writer.Write("Cost ");
        writer.WriteLine(Cost);
    }
}
