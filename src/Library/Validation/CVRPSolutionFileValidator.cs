namespace Library.Validation;

public sealed class CVRPSolutionFileValidator
{
    public void Validate(DataFile dataFile, SolutionFile solutionFile)
    {
        new CVRPValidator().Validate(dataFile);

        var depots = dataFile.Depot!;
        var dimension = dataFile.Specification.Dimension!.Value;
        var capacity = dataFile.Specification.Capacity!.Value;
        var demands = dataFile.Demand!;

        if (depots.Data.Count >= 2)
            throw new ValidationException("data file has multiple depots defined");
        var depot = depots.Data.First();

        var distance = Distance.GetDistance(dataFile) ??
            throw new ValidationException("data file has unsupported edge weight type or no coords");

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
                cost += distance[prev, route[j]];

                prev = route[j];
            }

            if (demand > capacity)
                throw new ValidationException($"Route {i + 1}: Demand {demand} is higher than the capacity {capacity}");

            cost += distance[prev, depot];
        }

        if (cost != solutionFile.Cost)
            throw new ValidationException($"solution file defines a cost of {solutionFile.Cost} but {cost} was calculated");
    }
}
