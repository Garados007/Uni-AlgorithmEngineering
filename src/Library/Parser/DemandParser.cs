namespace Library.Parser;

public sealed class DemandParser : ParserBase<Demand>
{
    public DemandParser(int length)
    {
        Length = length;
    }

    public int Length { get; }

    public override Demand? Parse(ref State state)
    {
        var data = new Demand(Length);
        while (!state.EOF)
        {
            var index = ReadInt32(ref state);
            if (index is null)
                return data;
            if (!SkipWhitespace(ref state))
                throw new ParseException(ref state, "whitespace expected");
            var demand = ReadInt32(ref state);
            if (demand is null)
                throw new ParseException(ref state, "integer value for demand expected");
            data[index.Value - 1] = demand.Value;
            SkipUntilEndOfLine(ref state);
        }
        return data;
    }
}
