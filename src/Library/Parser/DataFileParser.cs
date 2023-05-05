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
                case "NODE_COORD_SECTION":
                    SkipUntilEndOfLine(ref state);
                    switch (file.Specification.NodeCoordType)
                    {
                        case NodeCoordType.NO_COORDS:
                            throw new ParseException(ref state, "NODE_COORD_SECTION wasn't expected according specification");
                        case NodeCoordType.TWOD_COORDS:
                            if (file.NodeCoord2D is not null)
                                throw new ParseException(ref state, "NODE_COORD_SECTION was already defined");
                            if (file.Specification.Dimension is null)
                                throw new ParseException(ref state, "Cannot read NODE_COORD_SECTION if dimension wasn't specified");
                            var nodeCoord2D = new NodeCoord2DParser(file.Specification.Dimension.Value).Parse(ref state);
                            file = file with { NodeCoord2D = nodeCoord2D };
                            break;
                        case NodeCoordType.THREED_COORDS:
                            if (file.NodeCoord3D is not null)
                                throw new ParseException(ref state, "NODE_COORD_SECTION was already defined");
                            if (file.Specification.Dimension is null)
                                throw new ParseException(ref state, "Cannot read NODE_COORD_SECTION if dimension wasn't specified");
                            var nodeCoord3D = new NodeCoord3DParser(file.Specification.Dimension.Value).Parse(ref state);
                            file = file with { NodeCoord3D = nodeCoord3D };
                            break;
                        default:
                            throw new ParseException($"NODE_COORD_SECTION not supported for {file.Specification.NodeCoordType}");
                    }
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
