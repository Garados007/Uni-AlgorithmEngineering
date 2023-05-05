namespace Library.Parser;

public sealed class NodeCoord3DParser : ParserBase<NodeCoord3D>
{
    public NodeCoord3DParser(int length)
    {
        Length = length;
    }

    public int Length { get; }

    public override NodeCoord3D? Parse(ref State state)
    {
        var coords = new NodeCoord3D(Length);
        while (!state.EOF)
        {
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
            if (!SkipWhitespace(ref state))
                throw new ParseException(ref state, "whitespace expected");
            var z = ReadDouble(ref state);
            if (z is null)
                throw new ParseException(ref state, "floating point value for z expected");
            coords[index.Value - 1] = new System.Numerics.Vector3((float)x, (float)y, (float)z);
            SkipUntilEndOfLine(ref state);
        }
        return coords;
    }
}
