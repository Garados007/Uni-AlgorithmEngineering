using System.Diagnostics;
using Library.Data.Partition;

namespace Library.Solver.CVRP;

public sealed class SpacePartitionHeuristic : ISolver<DataFile, SolutionFile>
{
    public SolutionFile? Solve(DataFile dataFile)
    {
        if (dataFile.Specification.Type != DataType.CVRP)
            throw new SolverException("solver can only used with CVRP problems");

        new Validation.CVRPValidator().Validate(dataFile);

        if (dataFile.NodeCoord2D is null)
            throw new SolverNotApplicableException("This solver requires 2D coordinates");
        if (dataFile.Depot!.Data.Count != 1)
            throw new SolverNotApplicableException("This solver requires exactly one depot");

        var distance = Distance.GetDistance(dataFile) ??
            throw new SolverException("data file has no means of calculating a distance");
        var depots = dataFile.Depot!;
        var depot = depots.Data.First();
        var capacity = dataFile.Specification.Capacity!.Value;
        var demands = dataFile.Demand!;
        // var start = Stopwatch.GetTimestamp();
        var spaces = new Space2D(dataFile.NodeCoord2D, 500);
        // Console.WriteLine($"Space build: {Stopwatch.GetElapsedTime(start)}");

        // spaces.WriteRepresentation(Console.Out);

        var solution = new SolutionFile();
        var depotSpace = spaces.AccessTable.Span[depot];
        spaces.Remove(depot);

        while (spaces.Nodes.Count > 0)
        {
            // get closest node from depot
            var space = depotSpace;
            var remainingCapacity = capacity;
            var position = depot;
            var path = new List<int>();
            while (remainingCapacity > 0)
            {
                var closest = GetClosestNode(distance, space, demands, position, remainingCapacity);
                if (closest is null)
                    break;

                position = closest.Value.id;
                space = spaces.AccessTable.Span[position];
                path.Add(position);
                solution.Cost += closest.Value.dist;
                remainingCapacity -= demands[position];

                spaces.Remove(position);
            }

            // go back to depot and cleanup
            if (path.Count > 0)
            {
                solution.Routes.Add(path);
                solution.Cost += distance[position, depot];
            }
            else break;
        }

        return solution;
    }

    private static (int id, int dist)? GetClosestNode(Distance distance, Space2D? space, Demand demands,
        int start, int remainingCapacity
    )
    {
        (int id, int dist)? closest = null;
        while (space is not null)
        {
            // look for all nodes in the current space
            foreach (var node in space.Nodes)
            {
                if (demands[node] > remainingCapacity)
                    continue;

                var dist = distance[start, node];
                if (closest is null || closest.Value.dist > dist)
                    closest = (node, dist);
            }
            // return next found node or go one space level upwards
            if (closest is not null)
                return closest;
            space = space.Parent;
        }
        return closest;
    }
}
