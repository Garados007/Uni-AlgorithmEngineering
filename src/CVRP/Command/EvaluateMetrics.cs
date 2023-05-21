using System.Globalization;
using System.Text.Json;
using Library;
using Library.Solver;

namespace CVRP.Command;

public sealed class EvaluateMetrics : ICommand
{
    private string? inputDir;
    private string? outputDir;

    private static void WriteHelp()
    {
        Console.Error.WriteLine("""
        CVRP evaluate-metrics <input dir> <output dir>

        CVRP evaluate-metrics reads all the metrics from the parsing step and prepares some
        CSV files in the output for easy usage in ploting environments.
        """);
    }

    private bool ParseArgs(ReadOnlySpan<string> args)
    {
        if (args.Length != 2)
            return false;

        if (!Directory.Exists(args[0]))
        {
            Console.Error.WriteLine($"input directory {args[1]} not found");
            return false;
        }
        inputDir = args[0];

        if (!Directory.Exists(args[1]))
            Directory.CreateDirectory(args[1]);
        outputDir = args[1];

        return true;
    }

    public int Run(ReadOnlySpan<string> args)
    {
        if (!ParseArgs(args) || inputDir is null || outputDir is null)
        {
            WriteHelp();
            return 1;
        }

        var metrics = new List<Metrics>();
        Console.WriteLine("Read all metric files...");
        foreach (var file in Directory.EnumerateFiles(inputDir, "*.json"))
        {
            Console.WriteLine($"\t{Path.GetFileName(file)}");
            using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            var metric = JsonSerializer.Deserialize<Metrics>(stream);
            if (metric is null)
                Console.WriteLine($"\tcannot read {Path.Combine(file)}");
            else metrics.Add(metric);
        }

        Console.WriteLine($"{metrics.Count} metrics found");

        Console.WriteLine("Calculate node count timings");
        GetNodeCountTimings(metrics, null);
        var edgeWeights = GetEdgeWeights(metrics);
        foreach (var edgeWeight in edgeWeights.Keys)
            GetNodeCountTimings(metrics, edgeWeight);

        Console.WriteLine("Calculate solver performance");
        GetPerformance(metrics);

        return 0;
    }

    private Dictionary<EdgeWeightType, int> GetEdgeWeights(List<Metrics> metrics)
    {
        var res = new Dictionary<EdgeWeightType, int>();

        foreach (var metric in metrics)
        {
            if (metric.DataFile?.EdgeWeightType is null)
                continue;
            if (res.TryGetValue(metric.DataFile.EdgeWeightType.Value, out int count))
                res[metric.DataFile.EdgeWeightType.Value] = count + 1;
            else res[metric.DataFile.EdgeWeightType.Value] = 1;
        }

        using var output = new FileStream(
            Path.Combine(outputDir!, "edge-weight-types.csv"),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Read
        );
        using var writer = new StreamWriter(output);

        writer.WriteLine("Type,Count");
        foreach (var (type, count) in res)
        {
            writer.WriteLine($"\"{type}\",{count}");
        }

        writer.Flush();

        return res;
    }

    private void GetNodeCountTimings(List<Metrics> metrics, EdgeWeightType? type)
    {
        var res = new Dictionary<string, List<(TimeSpan time, int count)>>();
        var header = new Dictionary<int, int>();

        foreach (var metric in metrics)
        {
            if (metric.Solution.Solver is null || metric.DataFile?.Dimension is null)
            {
                continue;
            }
            if (type != null && metric.DataFile.EdgeWeightType != type)
                continue;
            if (!res.TryGetValue(metric.Solution.Solver!, out var list))
                res.Add(metric.Solution.Solver, list = new());

            var nodes = metric.DataFile.Dimension.Value;
            if (!header.TryGetValue(nodes, out int offset))
                header.Add(nodes, offset = header.Count);

            if (list.Count <= offset)
            {
                list.AddRange(Enumerable.Repeat((TimeSpan.Zero, 0), offset - list.Count + 1));
            }
            list[offset] = (list[offset].time + metric.Timings.Solving, list[offset].count + 1);
        }

        using var output = new FileStream(
            Path.Combine(
                outputDir!,
                type is null ? "node-count-timing.csv" : $"node-count-timing-{type}.csv"
            ),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Read
        );
        using var writer = new StreamWriter(output);

        writer.Write("Nodes");
        foreach (var solver in res.Keys)
            writer.Write($",\"{solver}\"");
        writer.WriteLine();
        foreach (var (nodes, offset) in header)
        {
            writer.Write(nodes);
            foreach (var list in res.Values)
            {
                writer.Write(',');
                if (list.Count <= offset)
                    continue;
                var (time, count) = list[offset];
                if (count == 0)
                    continue;
                writer.Write((time.TotalSeconds / count).ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteLine();
        }

        writer.Flush();
    }

    private void GetPerformance(List<Metrics> metrics)
    {
        var res = new Dictionary<string, List<double>>();
        var header = new Dictionary<string, (int row, int nodes)>();

        foreach (var metric in metrics)
        {
            if (metric.Solution.Solver is null || metric.DataFile?.Name is null || metric.DataFile?.Dimension is null)
            {
                continue;
            }

            if (!header.TryGetValue(metric.DataFile.Name, out var headerResult))
                header.Add(metric.DataFile.Name, headerResult = (header.Count, metric.DataFile.Dimension.Value));
            var row = headerResult.row;

            if (!res.TryGetValue(metric.Solution.Solver, out var list))
                res.Add(metric.Solution.Solver, list = new());

            if (list.Count <= row)
                list.AddRange(Enumerable.Repeat(double.NaN, row - list.Count + 1));

            list[row] = metric.Solution.Cost;
        }

        var allowed = new HashSet<string>
        {
            typeof(Library.Solver.CVRP.SimpleHeuristic).FullName!,
            typeof(Library.Solver.CVRP.SpacePartitionHeuristic).FullName!,
        };
        foreach (var name in res.Keys.ToArray())
        {
            if (!allowed.Contains(name))
                res.Remove(name);
        }

        if (res.Count == 0)
            return;

        var mainName = typeof(Library.Solver.CVRP.SimpleHeuristic).FullName;
        if (mainName is null || !res.ContainsKey(mainName))
            mainName = res.Keys.First();

        for (int i = 0; i < header.Count; ++i)
        {
            var baseValue = res[mainName][i];
            foreach (var list in res.Values)
                list[i] /= baseValue;
        }

        using var output = new FileStream(
            Path.Combine(
                outputDir!,
                "node-performance.csv"
            ),
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Read
        );
        using var writer = new StreamWriter(output);

        writer.Write("Name,Nodes");
        foreach (var solver in res.Keys)
            writer.Write($",\"{solver}\"");
        writer.WriteLine();
        foreach (var (name, (row, nodes)) in header)
        {
            writer.Write($"\"{name}\",{nodes}");
            foreach (var list in res.Values)
            {
                writer.Write(",");
                if (!double.IsNaN(list[row]))
                    writer.Write(list[row].ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteLine();
        }
        writer.Flush();
        output.SetLength(output.Position);
    }
}
