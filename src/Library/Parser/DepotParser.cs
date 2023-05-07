namespace Library.Parser;

public sealed class DepotParser : ParserBase<Depot>
{
    public override Depot? Parse(ref State state)
    {
        var data = new Depot();
        while (!state.EOF)
        {
            SkipWhitespace(ref state); // some test data is a mess
            var index = ReadInt32(ref state);
            if (index is null)
                throw new ParseException(ref state, "integer value expected");
            if (index.Value < 0)
            {
                SkipUntilEndOfLine(ref state);
                return data;
            }
            if (!data.Data.Add(index.Value - 1))
                throw new ParseException(ref state, "node already marked as depot node");

            SkipUntilEndOfLine(ref state);
        }
        return data;
    }
}
