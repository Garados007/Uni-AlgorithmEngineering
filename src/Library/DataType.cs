using System.Text.Json.Serialization;

namespace Library;

/// <summary>
/// Specifies the type of the data.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DataType
{
    /// <summary>
    /// Data for a symmetric traveling salesman problem
    /// </summary>
    TSP,
    /// <summary>
    /// Data for an asymmetric traveling salesman problem
    /// </summary>
    ATSP,
    /// <summary>
    /// Data for a sequential ordering problem
    /// </summary>
    SOP,
    /// <summary>
    /// Hamiltonian cycle problem data
    /// </summary>
    HCP,
    /// <summary>
    /// Capacitated vehicle routing problem data
    /// </summary>
    CVRP,
    /// <summary>
    /// A collection of tours
    /// </summary>
    TOUR,
}
