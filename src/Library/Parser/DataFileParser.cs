namespace Library.Parser;

public sealed class DataFileParser : ParserBase<DataFile>
{
    public override DataFile? Parse(ref State state)
    {
        var spec = new SpecificationParser().Parse(ref state);
        if (spec is null)
            return null;

        var file = new DataFile(spec);

        var stateCopy = state;
        string? line;
        while ((line = ReadString(ref state)) != null)
        {
            switch (line)
            {
                case "EDGE_WEIGHT_SECTION":
                    if (file.EdgeWeight is not null)
                        throw new ParseException(ref state, "EDGE_WEIGHT_SECTION was already defined");
                    SkipUntilEndOfLine(ref state);
                    var edgeWeight = new EdgeWeightParser(file.Specification).Parse(ref state);
                    file = file with { EdgeWeight = edgeWeight };
                    break;
                case "EOF":
                    return file;
                default:
                    return file;
                    // throw new ParseException(ref stateCopy, $"unknown data tag \"{line}\" found");
            }
            stateCopy = state;
        }

        return file;
    }
}
