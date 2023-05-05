namespace Library.EdgeWeights;

public sealed class EdgeWeightLowerRow : EdgeWeight
{
    private readonly Memory<int> values;

    public EdgeWeightLowerRow(int length) : base(length)
    {
        values = new int[length * (length - 1) / 2];
    }

    private int GetIndex(int from, int to)
    {
        if (from < 0 || from >= Length)
            throw new ArgumentOutOfRangeException(nameof(from));
        if (to < 0 || to >= Length)
            throw new ArgumentOutOfRangeException(nameof(to));

        if (from < to)
            return GetIndex(to, from);

        /*              t   o
                  __0___1___2___3
            f   0|  \
            r   1|  0   \
            o   2|  1   2   \
            m   3|  3   4   5   \
         */

        return from * (from - 1) / 2 + to;
    }

    public override int this[int from, int to]
    {
        get
        {
            if (from == to)
                return 0;
            else return values.Span[GetIndex(from, to)];
        }
        set
        {
            if (from != to)
                values.Span[GetIndex(from, to)] = value;
        }
    }

    public override EdgeWeightFormat Format => EdgeWeightFormat.LOWER_ROW;

    public override void WriteTo(TextWriter writer)
    {
        writer.WriteLine("EDGE_WEIGHT_SECTION");
        for (int from = 1; from < Length; ++from)
        {
            for (int to = 0; to < from; ++to)
            {
                if (to > 0)
                    writer.Write(' ');
                writer.Write(this[from, to]);
            }
            writer.WriteLine();
        }
    }
}
