namespace Library.EdgeWeights;

public sealed class EdgeWeightFullMatrix : EdgeWeight
{
    private readonly Memory<int> values;

    public EdgeWeightFullMatrix(int length) : base(length)
    {
        values = new int[length * length];
    }

    private int GetIndex(int from, int to)
    {
        if (from < 0 || from >= Length)
            throw new ArgumentOutOfRangeException(nameof(from));
        if (to < 0 || to >= Length)
            throw new ArgumentOutOfRangeException(nameof(to));

        /*              t   o
                  __0___1___2___3
            f   0|  0   1   2   3
            r   1|  4   5   6   7
            o   2|  8   9  10  11
            m   3| 12  13  14  15
         */

        return from * Length + to;
    }

    public override int this[int from, int to]
    {
        get => values.Span[GetIndex(from, to)];
        set => values.Span[GetIndex(from, to)] = value;
    }

    public override EdgeWeightFormat Format => EdgeWeightFormat.FULL_MATRIX;

    public override void WriteTo(TextWriter writer)
    {
        writer.WriteLine("EDGE_WEIGHT_SECTION");
        for (int from = 0; from < Length; ++from)
        {
            for (int to = 0; to < Length; ++to)
            {
                if (to > 0)
                    writer.Write(' ');
                writer.Write(this[from, to]);
            }
            writer.WriteLine();
        }
    }
}
