namespace Library;

public sealed class Demand
{
    private readonly Memory<int> data;

    public int this[int index]
    {
        get => data.Span[index];
        set => data.Span[index] = value;
    }

    public Demand(int length)
    {
        data = new int[length];
    }

    public void WriteTo(TextWriter writer)
    {
        var data = this.data.Span;
        writer.WriteLine("DEMAND_SECTION");
        for (int i = 0; i < data.Length; ++i)
        {
            writer.Write(i + 1);
            writer.Write(' ');
            writer.WriteLine(data[i]);
        }
    }
}
