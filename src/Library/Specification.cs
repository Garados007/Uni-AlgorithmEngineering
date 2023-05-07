namespace Library;

public class Specification
{
    /// <summary>
    /// Identifies the data file
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies the type of the data.
    /// </summary>
    public DataType Type { get; }

    /// <summary>
    /// Additional comments (usually the name of the contributor or creator of
    /// the problem instance is given here).
    /// </summary>
    public ReadOnlyMemory<string> Comment { get; }

    /// <summary>
    /// For a <see cref="DataType.TSP" /> or <see cref="DataType.ATSP" />, the
    /// dimension is the number of its nodes. For a <see cref="DataType.CVRP"
    /// />, it is the total number of nodes and depots. For a <see
    /// cref="DataType.TOUR" /> file it is the dimension of the corresponding
    /// problem.
    /// </summary>
    public int? Dimension { get; }

    /// <summary>
    /// Specifies the truck capacity in a <see cref="DataType.CVRP" />.
    /// </summary>
    public int? Capacity { get; }

    /// <summary>
    /// Specifies how the edge weights (or distances) are given.
    /// </summary>
    public EdgeWeightType? EdgeWeightType { get; }

    /// <summary>
    /// Describes the format of the edge weights if they are given explicitly.
    /// </summary>
    public EdgeWeightFormat? EdgeWeightFormat { get; }

    /// <summary>
    /// Describes the format in which the edges of a graph are given, if the
    /// graph is not complete.
    /// </summary>
    public EdgeDataFormat? EdgeDataFormat { get; }

    /// <summary>
    /// Specifies whether coordinates are associated with each node (which, for
    /// example may be used for either graphical display or distance
    /// computations).
    /// </summary>
    public NodeCoordType NodeCoordType { get; }

    /// <summary>
    /// Specifies how a graphical display of the nodes can be obtained.
    /// </summary>
    public DisplayDataType DisplayDataType { get; }

    /// <summary>
    /// This part contains information on the fle format and on its contents.
    /// </summary>
    /// <param name="data">the data that was read from input stream</param>
    /// <exception cref="ParseException" />
    public Specification(Dictionary<string, List<string>> data)
    {
        Name = ExtractSingle(data, "NAME");
        Type = ExtractEnum<DataType>(data, "TYPE");
        Comment = GetList(data, "COMMENT")?.ToArray() ?? Array.Empty<string>();
        Dimension = GetInt(data, "DIMENSION");
        Capacity = GetInt(data, "CAPACITY");
        EdgeWeightType = GetEnum<EdgeWeightType>(data, "EDGE_WEIGHT_TYPE");
        EdgeWeightFormat = GetEnum<EdgeWeightFormat>(data, "EDGE_WEIGHT_FORMAT");
        EdgeDataFormat = GetEnum<EdgeDataFormat>(data, "EDGE_DATA_FORMAT");
        NodeCoordType = GetEnum<NodeCoordType>(data, "NODE_COORD_TYPE") ?? EnforcedNodeCoordType(EdgeWeightType);
        var display = GetEnum<DisplayDataType>(data, "DISPLAY_DATA_TYPE");
        if (display is null)
            DisplayDataType = NodeCoordType == NodeCoordType.NO_COORDS ? DisplayDataType.NO_DISPLAY : DisplayDataType.COORD_DISPLAY;
        else DisplayDataType = display.Value;
    }

    private static NodeCoordType EnforcedNodeCoordType(EdgeWeightType? type)
    {
        // some test data is a mess and needs to be fixed
        return type switch
        {
            Library.EdgeWeightType.EUC_2D => NodeCoordType.TWOD_COORDS,
            Library.EdgeWeightType.MAX_2D => NodeCoordType.TWOD_COORDS,
            Library.EdgeWeightType.MAN_2D => NodeCoordType.TWOD_COORDS,
            Library.EdgeWeightType.CEIL_2D => NodeCoordType.TWOD_COORDS,
            Library.EdgeWeightType.EUC_3D => NodeCoordType.THREED_COORDS,
            Library.EdgeWeightType.MAX_3D => NodeCoordType.THREED_COORDS,
            Library.EdgeWeightType.MAN_3D => NodeCoordType.THREED_COORDS,
            _ => NodeCoordType.NO_COORDS,
        };
    }

    /// <summary>
    /// Returns a single entry from the data. If this entry is found multiple
    /// times or has an invalid format it will throw a <see
    /// cref="ParseException" />.
    /// </summary>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found int or null</returns>
    /// <exception cref="ParseException" />
    private static int? GetInt(Dictionary<string, List<string>> data, string key)
    {
        var num = GetSingle(data, key);
        if (num is null)
            return null;
        if (!int.TryParse(num, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out int value))
            throw new ParseException($"{key} specification has an invalid formated number \"{num}\"");
        return value;
    }

    /// <summary>
    /// Returns a single enum entry from data.  If this entry is found multiple
    /// times or has an invalid format it will throw a <see
    /// cref="ParseException" />.
    /// </summary>
    /// <typeparam name="T">the enum type</typeparam>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found enum entry</returns>
    /// <exception cref="ParseException" />
    private static T? GetEnum<T>(Dictionary<string, List<string>> data, string key)
        where T : struct
    {
        var input = GetSingle(data, key);
        if (input is null)
            return null;
        if (!Enum.TryParse<T>(input, false, out T value))
            throw new ParseException($"{key} specification has invalid value \"{input}\"");
        return value;
    }

    /// <summary>
    /// Extracts a single enum entry which is to be expected to be contained in <paramref name="data"/>.
    /// </summary>
    /// <typeparam name="T">the enum type</typeparam>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found enum entry</returns>
    /// <exception cref="ParseException" />
    private static T ExtractEnum<T>(Dictionary<string, List<string>> data, string key)
        where T : struct
    {
        var input = ExtractSingle(data, key);
        if (!Enum.TryParse<T>(input, false, out T value))
            throw new ParseException($"{key} specification has invalid value \"{input}\"");
        return value;
    }

    /// <summary>
    /// Extracts a single entry which is to be expected to be contained in <paramref name="data"/>.
    /// </summary>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found entry</returns>
    /// <exception cref="ParseException" />
    private static string ExtractSingle(Dictionary<string, List<string>> data, string key)
    {
        var list = ExtractList(data, key);
        if (list.Count > 1)
            throw new ParseException($"{key} specification is called multiple times");
        return list[0];
    }

    /// <summary>
    /// Extracts a list which is to be expected to be contained in <paramref name="data"/>.
    /// </summary>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found list</returns>
    /// <exception cref="ParseException" />
    private static List<string> ExtractList(Dictionary<string, List<string>> data, string key)
    {
        if (!data.TryGetValue(key, out var list))
            throw new ParseException($"no {key} specification found");
        return list;
    }

    /// <summary>
    /// Returns a single entry from the data. If this entry is found multiple times it
    /// will throw a <see cref="ParseException" />.
    /// </summary>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found entry or null</returns>
    /// <exception cref="ParseException" />
    private static string? GetSingle(Dictionary<string, List<string>> data, string key)
    {
        var list = GetList(data, key);
        if (list is null)
            return null;
        if (list.Count > 1)
            throw new ParseException($"{key} specification is called multiple times");
        return list[0];
    }

    /// <summary>
    /// Returns a single list from the data
    /// </summary>
    /// <param name="data">the full data from input</param>
    /// <param name="key">the key to search</param>
    /// <returns>the found list or null</returns>
    private static List<string>? GetList(Dictionary<string, List<string>> data, string key)
    {
        if (!data.TryGetValue(key, out var list))
            return null;
        return list;
    }

    public void WriteTo(TextWriter writer)
    {
        writer.Write("NAME : ");
        writer.WriteLine(Name);
        foreach (var entry in Comment.Span)
        {
            writer.Write("COMMENT : ");
            writer.WriteLine(entry);
        }
        writer.Write("TYPE : ");
        writer.WriteLine(Type);
        if (Dimension is not null)
        {
            writer.Write("DIMENSION : ");
            writer.WriteLine(Dimension);
        }
        if (Capacity is not null)
        {
            writer.Write("CAPACITY : ");
            writer.WriteLine(Capacity);
        }
        if (EdgeWeightType is not null)
        {
            writer.Write("EDGE_WEIGHT_TYPE : ");
            writer.WriteLine(EdgeWeightType);
        }
        if (EdgeWeightFormat is not null)
        {
            writer.Write("EDGE_WEIGHT_FORMAT : ");
            writer.WriteLine(EdgeWeightFormat);
        }
        if (EdgeDataFormat is not null)
        {
            writer.Write("EDGE_DATA_FORMAT : ");
            writer.WriteLine(EdgeDataFormat);
        }
        writer.Write("NODE_COORD_TYPE : ");
        writer.WriteLine(NodeCoordType);
        writer.Write("DISPLAY_DATA_TYPE : ");
        writer.WriteLine(DisplayDataType);
        writer.WriteLine("EOF : ");
    }
}
