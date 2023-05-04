namespace Library;

/// <summary>
/// Describes the format of the edge weights if they are given explicitly.
/// </summary>
public enum EdgeWeightFormat
{
    /// <summary>
    /// Weights are given by a function
    /// </summary>
    FUNCTION,
    /// <summary>
    /// Weights are given by a full matrix
    /// </summary>
    FULL_MATRIX,
    /// <summary>
    /// Upper triangular matrix (row-wise without diagonal entries)
    /// </summary>
    UPPER_ROW,
    /// <summary>
    /// Lower triangular matrix (row-wise without diagonal entries)
    /// </summary>
    LOWER_ROW,
    /// <summary>
    /// Upper triangular matrix (row-wise including diagonal entries)
    /// </summary>
    UPPER_DIAG_ROW,
    /// <summary>
    /// Lower triangular matrix (row-wise including diagonal entries)
    /// </summary>
    LOWER_DIAG_ROW,
    /// <summary>
    /// Upper triangular matrix (column-wise without diagonal entries)
    /// </summary>
    UPPER_COL,
    /// <summary>
    /// Lower triangular matrix (column-wise without diagonal entries)
    /// </summary>
    LOWER_COL,
    /// <summary>
    /// Upper triangular matrix (column-wise including diagonal entries)
    /// </summary>
    UPPER_DIAG_COL,
    /// <summary>
    /// Lower triangular matrix (column-wise including diagonal entries)
    /// </summary>
    LOWER_DIAG_COL,

}
