using Library.EdgeWeights;

namespace Library.Parser;

public sealed class EdgeWeightParser : ParserBase<EdgeWeight>
{
    public EdgeWeightParser(Specification specification)
    {
        Specification = specification;
    }

    public Specification Specification { get; }

    public override EdgeWeight? Parse(ref State state)
    {
        switch (Specification.EdgeWeightFormat)
        {
            case EdgeWeightFormat.FUNCTION:
                throw new ParseException("edge weight format function does not allow EDGE_WEIGHT_SECTION");
            case EdgeWeightFormat.LOWER_ROW:
                if (Specification.Dimension is null)
                    throw new ParseException("dimension expected for EDGE_WEIGHT_SECTION");
                return new EdgeWeightLowerRowParser(Specification.Dimension.Value).Parse(ref state);
            default:
                throw new ParseException($"edge weight format {Specification.EdgeWeightFormat} not supported");
        }
    }
}

public sealed class EdgeWeightLowerRowParser : ParserBase<EdgeWeight>
{
    public EdgeWeightLowerRowParser(int dimension)
    {
        if (dimension < 0)
            throw new ParseException("Dimension has to be at least 0");
        Dimension = dimension;
    }

    public int Dimension { get; }

    public override EdgeWeight? Parse(ref State state)
    {
        var data = new EdgeWeightLowerRow(Dimension);
        for (int from = 1; from < Dimension; ++from)
        {
            for (int to = 0; to < from; ++to)
            {
                var num = ReadInt32(ref state);
                if (num is null)
                {
                    if (to == 0)
                        return data;
                    else throw new ParseException(ref state, $"node weight expected for ({from}, {to})");
                }
                data[from, to] = num.Value;
                if (to + 1 < from && !SkipWhitespace(ref state))
                    throw new ParseException(ref state, "whitespace expected");
            }
            SkipUntilEndOfLine(ref state);
        }
        return data;
    }
}
