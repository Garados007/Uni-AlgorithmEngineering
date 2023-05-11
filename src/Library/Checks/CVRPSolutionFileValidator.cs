namespace Library.Checks;

public sealed class CVRPSolutionFileValidator
{
    public void Validate(DataFile dataFile, SolutionFile solutionFile)
    {
        var depots = dataFile.Depot ?? throw new ValidationException("data file has no depots defined");
        var dimension = dataFile.Specification.Dimension ?? throw new ValidationException("data file has no dimension defined");
        var capacity = dataFile.Specification.Capacity ?? throw new ValidationException("data file has no capacity defined");
        var demands = dataFile.Demand ?? throw new ValidationException("data file has no demands defined");

        if (depots.Data.Count == 0)
            throw new ValidationException("data file has no depots defined");
        if (depots.Data.Count >= 2)
            throw new ValidationException("data file has multiple depots defined");
        var depot = depots.Data.First();

        var weights = dataFile.EdgeWeight;
        var coord2 = dataFile.NodeCoord2D;
        var coord3 = dataFile.NodeCoord3D;
        var func2 = coord2 is null ? null :
            DistanceMetrics.GetFunc2D(
                dataFile.Specification.EdgeWeightType ??
                    throw new ValidationException("data file has no edge weight type defined")
            );
        var func3 = coord3 is null ? null :
            DistanceMetrics.GetFunc3D(
                dataFile.Specification.EdgeWeightType ??
                    throw new ValidationException("data file has no edge weight type defined")
            );
        if (coord2 is not null && func2 is null)
            throw new ValidationException("data file has unsupported edge weight type");
        if (coord3 is not null && func3 is null)
            throw new ValidationException("data file has unsupported edge weight type");

        Span<bool> usedNodes = new bool[dimension];

        long cost = 0;
        for (int i = 0; i < solutionFile.Routes.Count; ++i)
        {
            var route = solutionFile.Routes[i];
            if (route.Count == 0)
                throw new ValidationException($"Route {i + 1}: no nodes defined");

            var prev = depot;
            long demand = 0;
            for (int j = 0; j < route.Count; ++j)
            {
                if (route[j] == depot)
                    throw new ValidationException($"Route {i + 1}: depot {route[j] + 1} (at {j + 1}) is not allowed to be listed");
                if (route[j] < 0 || route[j] >= dimension)
                    throw new ValidationException($"Route {i + 1}: node {route[j] + 1} (at {j + 1}) has an invalid index");
                if (usedNodes[route[j]])
                    throw new ValidationException($"Route {i + 1}: node {route[j] + 1} (at {j + 1}) is already used on a route");
                usedNodes[route[j]] = true;

                demand += demands[route[j]];

                if (weights is not null)
                {
                    cost += weights[prev, route[j]];
                }
                else if (coord2 is not null)
                {
                    cost += func2!(coord2[prev], coord2[route[j]]);
                }
                else if (coord3 is not null)
                {
                    cost += func3!(coord3[prev], coord3[route[j]]);
                }

                prev = route[j];
            }

            if (demand > capacity)
                throw new ValidationException($"Route {i + 1}: Demand {demand} is higher than the capacity {capacity}");

            if (weights is not null)
            {
                cost += weights[prev, depot];
            }
            else if (coord2 is not null)
            {
                cost += func2!(coord2[prev], coord2[depot]);
            }
            else if (coord3 is not null)
            {
                cost += func3!(coord3[prev], coord3[depot]);
            }
        }

        if (cost != solutionFile.Cost)
            throw new ValidationException($"solution file defines a cost of {solutionFile.Cost} but {cost} was calculated");
    }
}
