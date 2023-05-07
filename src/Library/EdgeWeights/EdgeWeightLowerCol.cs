namespace Library.EdgeWeights;

public sealed class EdgeWeightLowerCol : EdgeWeight
{
    private readonly Memory<int> values;
    private readonly Memory<int> offsets;

    public EdgeWeightLowerCol(int length) : base(length)
    {
        values = new int[length * (length - 1) / 2];
        offsets = new int[length - 1];
        var offset = 0;
        for (int i = 0; i < length - 1; ++i)
        {
            offsets.Span[i] = offset - i - 1;
            offset += length - i - 1;
        }
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
            o   2|  1   3   \
            m   3|  2   4   5   \
         */

        return offsets.Span[to] + from;
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
        var values = this.values.Span;
        for (int i = 0; i < values.Length; ++i)
        {
            if (i > 0)
            {
                if (i % 10 == 0)
                    writer.WriteLine();
                else writer.Write(' ');
            }
            writer.Write(values[i]);
        }
        writer.WriteLine();
    }
}
