namespace Library;

/// <summary>
/// Specifies whether coordinates are associated with each node (which, for
/// example may be used for either graphical display or distance computations). <br/>
/// THe default value is <see cref="NodeCoordType.NO_COORDS" />.
/// </summary>
public enum NodeCoordType
{
    /// <summary>
    /// Nodes are specified by coordinates in 2-D
    /// </summary>
    TWOD_COORDS,
    /// <summary>
    /// Nodes are specified by coordinates in 3-D
    /// </summary>
    THREED_COORDS,
    /// <summary>
    /// The nodes do not have associated coordinates
    /// </summary>
    NO_COORDS,
}
