namespace Library.Parser;

public sealed class NodeCoord2DParser : ParserBase<NodeCoord2D>
{
    public NodeCoord2DParser(int length)
    {
        Length = length;
    }

    public int Length { get; }

    public override NodeCoord2D? Parse(ref State state)
    {
        var coords = new NodeCoord2D(Length);
        while (!state.EOF)
        {
            SkipWhitespace(ref state);
            var index = ReadInt32(ref state);
            if (index is null)
                return coords;
            if (!SkipWhitespace(ref state))
                throw new ParseException(ref state, "whitespace expected");
            var x = ReadDouble(ref state);
            if (x is null)
                throw new ParseException(ref state, "floating point value for x expected");
            if (!SkipWhitespace(ref state))
                throw new ParseException(ref state, "whitespace expected");
            var y = ReadDouble(ref state);
            if (y is null)
                throw new ParseException(ref state, "floating point value for y expected");
            coords[index.Value - 1] = new System.Numerics.Vector2((float)x, (float)y);
            SkipUntilEndOfLine(ref state);
        }
        return coords;
    }
}
