namespace Library;

/// <summary>
/// Specifies how the edge weights (or distances) are given.
/// </summary>
public enum EdgeWeightType
{
    /// <summary>
    /// Weights are listed explicitly in the corresponding section
    /// </summary>
    EXPLICIT,
    /// <summary>
    /// Weights are Euclidean distances in 2-D
    /// </summary>
    EUC_2D,
    /// <summary>
    /// Weights are Euclidean distances in 3-D
    /// </summary>
    EUC_3D,
    /// <summary>
    /// Weights are maximum distances in 2-D
    /// </summary>
    MAX_2D,
    /// <summary>
    /// Weights are maximum distances in 3-D
    /// </summary>
    MAX_3D,
    /// <summary>
    /// Weights are Manhattan distances in 2-D
    /// </summary>
    MAN_2D,
    /// <summary>
    /// Weights are Manhattan distances in 3-D
    /// </summary>
    MAN_3D,
    /// <summary>
    /// Weights are Euclidean distances in 2-D rounded up
    /// </summary>
    CEIL_2D,
    /// <summary>
    /// Weights are geographical distances
    /// </summary>
    GEO,
    /// <summary>
    /// Special distance function for problems <c>att48</c> and <c>att532</c>
    /// </summary>
    ATT,
    /// <summary>
    /// Special distance function for crystallography problems (Version 1)
    /// </summary>
    XRAY1,
    /// <summary>
    /// Special distance function for crystallography problems (Version 2)
    /// </summary>
    XRAY2,
    /// <summary>
    /// There is a special distance function documented elsewhere
    /// </summary>
    SPECIAL,
}
