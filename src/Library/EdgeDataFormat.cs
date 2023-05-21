using System.Text.Json.Serialization;

namespace Library;

/// <summary>
/// Describes the format in which the edges of a graph are given, if the graph is not complete.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdgeDataFormat
{
    /// <summary>
    /// The graph is given by an edge list
    /// </summary>
    EDGE_LIST,
    /// <summary>
    /// The graph is given as an adjacency list
    /// </summary>
    ADJ_LIST,
}
