using System.Text.Json.Serialization;

namespace Library;

/// <summary>
/// Specifies how a graphical display of the nodes can be obtained. <br/>
/// The default value is <see cref="DisplayDataType.COORD_DISPLAY" />
/// if node coordinates are specified and <see cref="DisplayDataType.NO_DISPLAY" />
/// otherwise.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayDataType
{
    /// <summary>
    /// Display is generated from the node coordinates
    /// </summary>
    COORD_DISPLAY,
    /// <summary>
    /// Explicit coordinates in 2-D are given
    /// </summary>
    TWOD_DISPLAY,
    /// <summary>
    /// No graphical display is possible
    /// </summary>
    NO_DISPLAY,
}
