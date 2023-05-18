using Library.Parser;
using Library.Validation;
using Library;
using Library.Solver;
using Library.Solver.CVRP;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace CVRP.Command;

public sealed class Solve : ICommand
{
    private ISolver<DataFile, SolutionFile> solver = new SimpleHeuristic();
    private string outputFile = "-";
    private string? inputFile;
    private string? metricFile;
    private bool skipNotApplicable = false;

    private readonly int cycles;
    private readonly int? time;

    public Solve(int cycles = 1, int? time = null)
    {
        if (cycles < 1)
            throw new ArgumentOutOfRangeException(nameof(cycles));
        this.cycles = cycles;
        this.time = time;
    }

    private static void WriteHelp()
    {
        Console.Error.WriteLine("""
        CVRP solve [<-s solver>] [<-o output>] [<-m metrics>] <problem file>

        CVRP solve reads the problem file and tries to find a solution using the specified method.
        The following arguments are supported:

            -s <solver>
            --solver <solver>       Specifies the solver to use. The full list is described below.
                                    Default is "greedy-simple".

            -o <output file>
            --output <output file>  Specifies the output file to store the results. Can be "-" to
                                    print to std output. Default is "-".

            -m <metric file>
            --metrics <metric file> Output some metrics about the solving process in a JSON format.
                                    Can be "-" to print to std output. It wont output anything by
                                    default.

            --skip-not-applicable   Skips processing problem files which haven't the correct data
                                    to be processed. This is only used for some solving algorithms
                                    and doesn't change the basic CVRP verification. If this flag is
                                    enabled and problem is considered as not applicable then this
                                    application won't fail if so. It will also return a Github
                                    Actions style warning for these cases.

            <problem file>          The problem file to read the specification. Must be a CVRP
                                    problem. You can also use "-" to read from stdin.

        CVRP solve supports the following solver:

            greedy-simple           Uses a greedy algorithm which always searches for the closest
                                    unused neighbour until the current capacity is exhausted.

            back-to-depot           Similar to greedy-simple but includes the distance to the
                                    closest depot node to the cost function.

            space-partition         Similar to greedy-simple but uses a 2D space partition into
                                    smaller areas and try to prefere nodes that are closer to the
                                    current one. This does only allow one depot node.

            furthest-first-two-sided
                                    Similar to greedy-simple but tries to use furthest nodes from
                                    the depot first and add afterwards closest nodes to both ends
                                    of the path until the capacity is exhausted. This does only
                                    allow one depot node.

        Examples:

            CVRP solve -
                Solves the problem file from stdin and print the solution to stdout.

            CVRP -s greedy-simple -o solution.sol problem.vrp
                Reads the problem in problem.vrp and solves it using the greedy-simple algorithm.
                The output is written to solution.sol.
        """);
    }

    private bool ParseArgs(ReadOnlySpan<string> args)
    {
        bool solverArgsSet = false;
        bool outputFileArgsSet = false;
        while (args.Length > 0)
        {
            if (args[0].StartsWith('-'))
            {
                switch (args[0])
                {
                    case "-s" or "--solver":
                        if (solverArgsSet)
                        {
                            Console.Error.WriteLine("Cannot set a solver twice");
                            return false;
                        }
                        solverArgsSet = true;
                        if (args.Length == 1)
                        {
                            Console.Error.WriteLine($"{args[0]} expects an argument");
                            return false;
                        }
                        switch (args[1])
                        {
                            case "greedy-simple":
                                solver = new SimpleHeuristic();
                                break;
                            case "back-to-depot":
                                solver = new BackToDepot();
                                break;
                            case "space-partition":
                                solver = new SpacePartitionHeuristic();
                                break;
                            case "furthest-first-two-sided":
                                solver = new FurthestFirstTwoSidedHeuristic();
                                break;
                            default:
                                Console.Error.WriteLine($"unsupported solver \"{args[1]}\"");
                                return false;
                        }
                        args = args[2..];
                        break;
                    case "-o" or "--output":
                        if (outputFileArgsSet)
                        {
                            Console.Error.WriteLine("Cannot set output twice");
                            return false;
                        }
                        outputFileArgsSet = true;
                        if (args.Length == 1)
                        {
                            Console.Error.WriteLine($"{args[0]} expects an argument");
                            return false;
                        }
                        outputFile = args[1];
                        args = args[2..];
                        break;
                    case "-m" or "--metrics":
                        if (metricFile is not null)
                        {
                            Console.Error.WriteLine("Cannot set metrics twice");
                            return false;
                        }
                        if (args.Length == 1)
                        {
                            Console.Error.WriteLine($"{args[0]} expects an argument");
                            return false;
                        }
                        metricFile = args[1];
                        args = args[2..];
                        break;
                    case "--skip-not-applicable":
                        skipNotApplicable = true;
                        args = args[1..];
                        break;
                    default:
                        Console.Error.WriteLine($"Unsupported parameter {args[0]}");
                        break;
                }
            }
            else
            {
                if (inputFile is not null)
                {
                    Console.Error.WriteLine($"Cannot set a set a secondary input file \"{args[0]}\" when \"{inputFile}\" is already set.");
                    return false;
                }
                inputFile = args[0];
                args = args[1..];
            }
        }
        return true;
    }

    public int Run(ReadOnlySpan<string> args)
    {
        if (!ParseArgs(args) || inputFile is null)
        {
            WriteHelp();
            return 1;
        }

        var metrics = new Metrics();
        DataFile dataFile;

        var start = Stopwatch.GetTimestamp();
        try { dataFile = new DataFileParser().Parse(inputFile); }
        catch (ParseException e)
        {
            Console.Error.WriteLine($"{e.GetType()}: {e.Message}");
            return 2;
        }
        metrics.Timings.Parsing = Stopwatch.GetElapsedTime(start);
        metrics.DataFile = dataFile.Specification;

        SolutionFile? solutionFile = null;

        start = Stopwatch.GetTimestamp();
        try
        {
            for (int i = 0; i < cycles; ++i)
                solutionFile = solver.Solve(dataFile);
        }
        catch (SolverNotApplicableException e) when (skipNotApplicable)
        {
            Console.Out.WriteLine($"::warning::{e.GetType()}: {e.Message}");
            return 0;
        }
        catch (Exception e) when (e is ValidationException || e is SolverException)
        {
            Console.Error.WriteLine($"{e.GetType()}: {e.Message}");
            return 3;
        }
        if (solutionFile is null)
        {
            Console.Error.WriteLine("Cannot generate a solution file");
            return 3;
        }
        metrics.Timings.Solving = Stopwatch.GetElapsedTime(start) / cycles;
        metrics.Timings.SolverCycles = cycles;
        metrics.Solution.Solver = solver.GetType().FullName;
        metrics.Solution.Cost = solutionFile.Cost;

        if (time is not null)
        {
            var iteration = (int)Math.Ceiling(time.Value / metrics.Timings.SolvingSec);
            Console.WriteLine($"\t\tSingle Run: {metrics.Timings.Solving}");
            Console.WriteLine($"\t\tIteration: {iteration}");
            start = Stopwatch.GetTimestamp();
            for (int i = 0; i < iteration; ++i)
                solver.Solve(dataFile);
            metrics.Timings.Solving = Stopwatch.GetElapsedTime(start) / iteration;
            metrics.Timings.SolverCycles = iteration;
            Console.WriteLine($"\t\tFull Run: {metrics.Timings.Solving * iteration}");
            Console.WriteLine($"\t\tAverage Run: {metrics.Timings.Solving}");
            Console.WriteLine($"\t\tCost: {solutionFile.Cost}");
        }

        start = Stopwatch.GetTimestamp();
        if (outputFile == "-")
        {
            solutionFile.WriteTo(Console.Out);
        }
        else
        {
            var dir = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using var file = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(file);
            solutionFile.WriteTo(writer);
            writer.Flush();
            if (file.CanSeek)
                try { file.SetLength(file.Position); }
                catch (IOException) {}
        }
        metrics.Timings.Writing = Stopwatch.GetElapsedTime(start);

        if (metricFile is not null)
        {
            if (metricFile == "-")
            {
                using var stream = new MemoryStream();
                WriteMetric(stream, metrics);
                Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
            }
            else
            {
                var dir = Path.GetDirectoryName(metricFile);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using var stream = new FileStream(metricFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                WriteMetric(stream, metrics);
            }
        }

        return 0;
    }

    private void WriteMetric(Stream output, Metrics metrics)
    {
        var writer = new Utf8JsonWriter(output, new JsonWriterOptions
        {
            Indented = true,
        });
        JsonSerializer.Serialize(writer, metrics);
        writer.Flush();
        output.SetLength(output.Position);
    }
}
