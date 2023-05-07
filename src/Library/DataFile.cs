using Library.Parser;

namespace Library;

public sealed record DataFile(
    Specification Specification,
    EdgeWeight? EdgeWeight = null,
    NodeCoord2D? NodeCoord2D = null,
    NodeCoord3D? NodeCoord3D = null,
    Demand? Demand = null,
    Depot? Depot = null
)
{
    /// <summary>
    /// Parses a data file and returns it.
    /// </summary>
    /// <param name="reader">the text reader with the data</param>
    /// <returns>the parsed data file</returns>
    /// <exception cref="ParseException" />
    public static DataFile? Parse(TextReader reader)
    {
        var state = new State
        {
            reader = reader,
            buffer = stackalloc char[1024],
        };
        var parser = new DataFileParser();
        return parser.Parse(ref state);
    }

    public void WriteTo(TextWriter writer)
    {
        Specification.WriteTo(writer);
        EdgeWeight?.WriteTo(writer);
        NodeCoord2D?.WriteTo(writer);
        NodeCoord3D?.WriteTo(writer);
        Demand?.WriteTo(writer);
        Depot?.WriteTo(writer);
        writer.WriteLine("EOF");
    }
}
