namespace Library.Solver.CVRP;

public sealed class FurthestFirstTwoSidedHeuristic : ISolver<DataFile, SolutionFile>
{
    public SolutionFile? Solve(DataFile dataFile)
    {
        if (dataFile.Specification.Type != DataType.CVRP)
            throw new SolverException("solver can only used with CVRP problems");

        new Validation.CVRPValidator().Validate(dataFile);

        if (dataFile.Depot!.Data.Count != 1)
            throw new SolverNotApplicableException("This solver requires exactly one depot");

        var distance = Distance.GetDistance(dataFile) ??
            throw new SolverException("data file has no means of calculating a distance");
        var dimension = dataFile.Specification.Dimension!.Value;
        var depots = dataFile.Depot!;
        var depot = depots.Data.First();
        var capacity = dataFile.Specification.Capacity!.Value;
        var demands = dataFile.Demand!;

        if (dimension <= 1)
            return new SolutionFile();

        // build up distance matrix to depot node
        var depotDistanceList = new List<(int id, int distance)>(dimension);
        Span<int> depotDistance = new int[dimension];
        for (int i = 0; i < dimension; ++i)
        {
            var dist = distance[i, depot];
            depotDistance[i] = dist;
            if (i != depot)
                depotDistanceList.Add((i, dist));
        }
        depotDistanceList.Sort((x, y) => -x.distance.CompareTo(y.distance));
        Span<bool> usedNodes = new bool[dimension];
        usedNodes[depot] = true;

        // get maximum demand
        var maxDemand = 0;
        for (int i = 0; i < dimension; ++i)
            maxDemand = Math.Max(maxDemand, demands[i]);

        // build up tours
        var solution = new SolutionFile();
        var highestNodeOffset = 0;
        while (highestNodeOffset < depotDistanceList.Count)
        {
            // prepare tour
            var startId = depotDistanceList[highestNodeOffset].id;
            var path = new LinkedList<int>();
            path.AddFirst(startId);
            usedNodes[startId] = true;
            var fixedCost = 0;
            var remainingCapacity = capacity - demands[startId];

            while (true)
            {
                var best = GetNodeStats(distance, demands, usedNodes,
                    path.First!.Value, path.Last!.Value, remainingCapacity
                );
                if (best is null)
                    break;

                var node = best.Value.id;
                if (best.Value.start == path.First!.Value)
                {
                    path.AddFirst(node);
                }
                else
                {
                    path.AddLast(node);
                }
                usedNodes[node] = true;
                fixedCost += best.Value.startDist;
                remainingCapacity -= best.Value.demand;

            }

            // add tour to solution
            solution.Routes.Add(path.ToList());
            solution.Cost += distance[depot, path.First!.Value] + fixedCost + distance[path.Last!.Value, depot];

            // fix node offset
            for (;
                highestNodeOffset < depotDistanceList.Count &&
                    usedNodes[depotDistanceList[highestNodeOffset].id];
                ++highestNodeOffset
            );

        }

        return solution;
    }

    private static (int id, int startDist, int demand, int start)? GetNodeStats(
        Distance distance, Demand demands, ReadOnlySpan<bool> usedNodes,
        int start1, int start2, int remainingCapacity
    )
    {
        (int id, int startDist, int demand, int start)? best = null;
        var bestCost = int.MaxValue;
        for (int node = 0; node < usedNodes.Length; ++node)
        {
            if (usedNodes[node])
                continue;
            var dem = demands[node];
            if (dem > remainingCapacity)
                continue;
            var startDist1 = distance[start1, node];
            var startDist2 = distance[start2, node];
            var (start, startDist) = startDist1 < startDist2 ? (start1, startDist1) : (start2, startDist2);
            var cost = startDist - dem;
            if (best is null || cost < bestCost)
            {
                best = (node, startDist, dem, start);
                bestCost = cost;
            }
        }
        return best;
    }
}
