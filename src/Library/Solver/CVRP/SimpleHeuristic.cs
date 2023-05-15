using System.Text.RegularExpressions;

namespace Library.Solver.CVRP;

public sealed class SimpleHeuristic : ISolver<DataFile, SolutionFile>
{
    public SolutionFile? Solve(DataFile dataFile)
    {
        if (dataFile.Specification.Type != DataType.CVRP)
            throw new SolverException("solver can only used with CVRP problems");

        new Validation.CVRPValidator().Validate(dataFile);

        var distance = Distance.GetDistance(dataFile) ??
            throw new SolverException("data file has no means of calculating a distance");
        var depots = dataFile.Depot!;
        var dimension = dataFile.Specification.Dimension!.Value;
        var capacity = dataFile.Specification.Capacity!.Value;
        var demands = dataFile.Demand!;

        var remainingNodes = new HashSet<int>(dimension);
        for (int i = 0; i < dimension; ++i)
            if (!depots.Data.Contains(i))
                remainingNodes.Add(i);

        var solution = new SolutionFile();

        while (remainingNodes.Count > 0)
        {
            // get closest node from depot
            (int id, int dist, int depot)? closestFromDepot = null;
            foreach (var depot in depots.Data)
            {
                var closest = GetClosestNode(distance, depot, remainingNodes, capacity, demands);
                if (closest is null)
                    continue;
                if (closestFromDepot is null || closestFromDepot.Value.dist > closest.Value.dist)
                    closestFromDepot = (closest.Value.id, closest.Value.dist, depot);
            }

            // check if there is a closest node (sanity check)
            if (closestFromDepot is null)
                return solution;

            // walk
            var node = closestFromDepot.Value.id;
            var remainingCapacity = capacity;
            solution.Cost += closestFromDepot.Value.dist;
            var path = new List<int>();
            while (true)
            {
                remainingCapacity -= demands[node];
                remainingNodes.Remove(node);
                path.Add(node);

                var closest = GetClosestNode(distance, node, remainingNodes, remainingCapacity, demands);
                if (closest is null)
                    break;

                solution.Cost += closest.Value.dist;
                node = closest.Value.id;
            }

            // go back to closest depot
            var closestDepot = depots.Data.Count > 0 ?
                GetClosestNode(distance, node, depots.Data, int.MaxValue, demands) :
                (closestFromDepot.Value.depot, distance[node, closestFromDepot.Value.depot]);
            if (closestDepot is null)
                return solution;

            solution.Cost += closestDepot.Value.dist;
            solution.Routes.Add(path);
        }

        return solution;
    }

    private static (int id, int dist)? GetClosestNode(Distance distance, int start,
        HashSet<int> remainingTargets, int remainingCapacity, Demand demands)
    {
        (int id, int dist)? closest = null;
        foreach (var target in remainingTargets)
        {
            if (demands[target] > remainingCapacity)
                continue;

            var dist = distance[start, target];
            if (closest is null || closest.Value.dist > dist)
                closest = (target, dist);
        }
        return closest;
    }
}
