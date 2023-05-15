using System.Collections;

namespace Library.Validation;

public sealed class CVRPValidator
{
    public void Validate(DataFile dataFile)
    {
        if (dataFile.Specification.Type != DataType.CVRP)
            throw new ArgumentException("a CVRP problem was expected", nameof(dataFile));

        if (dataFile.Depot is null)
            throw new ValidationException("data file is required to contain depots");
        if (dataFile.Depot.Data.Count == 0)
            throw new ValidationException("data file has no depot nodes defined");

        if (dataFile.Specification.Dimension is null)
            throw new ValidationException("data file is required to contain a dimension");
        if (dataFile.Specification.Capacity is null)
            throw new ValidationException("data file is required to have a capacity defined");
        if (dataFile.Demand is null)
            throw new ValidationException("data file is required to have demands defined");

        if (dataFile.NodeCoord2D is not null && dataFile.Specification.EdgeWeightType is null)
            throw new ValidationException("data file has no edge weight type defined");
        if (dataFile.NodeCoord3D is not null && dataFile.Specification.EdgeWeightType is null)
            throw new ValidationException("data file has no edge weight type defined");

        if (dataFile.EdgeWeight is null && dataFile.NodeCoord2D is null && dataFile.NodeCoord3D is null)
            throw new ValidationException("data file has no edge weights or node coordinates defined");
    }
}

public sealed class DataFileValidator
{
    public void Validate(DataFile dataFile)
    {
        switch (dataFile.Specification.Type)
        {
            case DataType.CVRP:
                new CVRPValidator().Validate(dataFile);
                break;
            default:
                throw new ValidationException($"No validation for {dataFile.Specification.Type} supported");
        }
    }
}
