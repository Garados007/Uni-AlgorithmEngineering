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
