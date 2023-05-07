using Library.EdgeWeights;

namespace Library.Parser;

public sealed class EdgeWeightLowerColParser : ParserBase<EdgeWeight>
{
    public EdgeWeightLowerColParser(int dimension)
    {
        if (dimension < 0)
            throw new ParseException("Dimension has to be at least 0");
        Dimension = dimension;
    }

    public int Dimension { get; }

    public override EdgeWeight? Parse(ref State state)
    {
        var data = new EdgeWeightLowerCol(Dimension);
        var maxNumbers = Dimension * (Dimension - 1) / 2;
        int from = 1;
        int to = 0;

        for (int i = 0; i < maxNumbers; ++i)
        {
            SkipWhitespace(ref state);
            var num = ReadInt32(ref state);
            if (num is null)
            {
                if (!CheckKeyword(ref state, "\n"))
                    throw new ParseException(ref state, $"node weight expected for ({from}, {to})");
                --i;
                continue;
            }

            data[from, to] = num.Value;
            from++;
            if (from >= Dimension)
            {
                to++;
                from = to + 1;
            }
        }

        SkipUntilEndOfLine(ref state);
        return data;
    }
}
