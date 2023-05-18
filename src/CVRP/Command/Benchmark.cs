using System.Runtime.InteropServices;

namespace CVRP.Command;

public sealed class Benchmark : ICommand
{
    private string? inputDir;
    private string? outputDir;
    private string? metricDir;
    private readonly HashSet<string> solverExplicit = new();
    private readonly HashSet<string> solverBlacklist = new();
    private int cycles = 1;
    private int? time;
    private readonly List<string> flags = new();

    private static void WriteHelp()
    {
        Console.Error.WriteLine("""
        CVRP benchmark [<-s solver>] [<-x solver>] [<-i input dir>] [<-o output dir>] [<-m metrics dir>] [<-c cycles>]

        CVRP benchmark reads all problem files in the input directory and apply all solver algorithms on
        it. All output will be written to the output directory (there is no option to write them to
        stdout). All metrics will be written to the metrics directory.

            -s <solver>
            --solver <solver>       Add a solver to the whitelist. This option can used more than once. If
                                    this option isn't used it will use all available solver.

            -x <solver>
            --exclude <solver>      Remove a solver from the list of available solver. This option can
                                    be used more than once.

            -i <input dir>
            --input <input dir>     All supported problem files in this directory will be scanned and used
                                    for benchmarks. There is no recursive lookup. This option is mandatory.
                                    Supported files are: *.vrp

            -o <output dir>
            --output <output dir>   Specifies the output directory to store the results. If this option is
                                    not used there will be no output at all.

            -m <metric dir>
            --metrics <metric dir>  Output some metrics about the solving process in a JSON format.
                                    This option is mandatory.

            -c <cycles>
            --cycles <cycles>       The number of repetition the solving algorithm should be executed. This
                                    will only repeat the solving process. The output contains the average
                                    time of all solving steps. The default value is 1.

            -t <time sec>
            --time <time sec>       Similar to --cycles but will repeat a single run for the specified time
                                    limit. The number is defined in seconds. Setting this option will ignore
                                    any setting defined with --cycles. The benchmark will always execute a
                                    test run for each solver first to determine the number of cycles for each
                                    run.

            -f <solver flag>
            --flag <solver flag>    Sets additional flags or option for the internal solver. Check the
                                    documentation of CVRP solve for more details. This flag won't set any
                                    arguments. If any flag needs an argument just use "-f <name> -f <value>".
                                    It isn't recommended to overwrite any options that this benchmark tool sets.

        CVRP benchmark uses CVRP solve under the hood. Check the documentation there to see a list of
        available solver.
        """);
    }

    private bool ParseArgs(ReadOnlySpan<string> args)
    {
        while (args.Length > 0)
        {
            if (args[0].StartsWith('-'))
            {
                if (args.Length == 1)
                {
                    Console.Error.WriteLine($"{args[0]} expects one argument");
                    return false;
                }
                switch (args[0])
                {
                    case "-s" or "--solver":
                        if (!solverExplicit.Add(args[1]))
                        {
                            Console.Error.WriteLine($"solver {args[1]} already added to list");
                            return false;
                        }
                        break;
                    case "-x" or "--exclude":
                        if (!solverBlacklist.Add(args[1]))
                        {
                            Console.Error.WriteLine($"solver {args[1]} already added to blacklist");
                            return false;
                        }
                        break;
                    case "-i" or "--input":
                        if (inputDir is not null)
                        {
                            Console.Error.WriteLine($"input directory already set");
                            return false;
                        }
                        if (!Directory.Exists(args[1]))
                        {
                            Console.Error.WriteLine($"input directory {args[1]} not found");
                            return false;
                        }
                        inputDir = args[1];
                        break;
                    case "-o" or "--output":
                        if (outputDir is not null)
                        {
                            Console.Error.WriteLine("output directory already set");
                            return false;
                        }
                        outputDir = args[1];
                        break;
                    case "-m" or "--metrics":
                        if (metricDir is not null)
                        {
                            Console.Error.WriteLine("metrics directory already set");
                            return false;
                        }
                        metricDir = args[1];
                        break;
                    case "-c" or "--cycles":
                        if (!int.TryParse(args[1], out int cycles) || cycles < 0)
                        {
                            Console.Error.WriteLine($"invalid cycles argument: {args[1]}");
                            return false;
                        }
                        this.cycles = cycles;
                        break;
                    case "-t" or "--time":
                        if (!int.TryParse(args[1], out int timeSecs) || timeSecs <= 0)
                        {
                            Console.Error.WriteLine($"invalid time argument: {args[1]}");
                            return false;
                        }
                        this.time = timeSecs;
                        break;
                    case "-f" or "--flags":
                        flags.Add(args[1]);
                        break;
                    default:
                        Console.Error.WriteLine($"Unknown option {args[0]}");
                        return false;
                }
                args = args[2..];
            }
            else
            {
                Console.Error.WriteLine($"unexpected value {args[0]}");
                return false;
            }
        }
        if (inputDir is null)
        {
            Console.Error.WriteLine("input directory expected");
            return false;
        }
        if (metricDir is null)
        {
            Console.Error.WriteLine("metrics directory expected");
            return false;
        }
        if (time is not null)
        {
            cycles = 1;
        }
        return true;
    }

    public int Run(ReadOnlySpan<string> args)
    {
        if (!ParseArgs(args) || inputDir is null || metricDir is null)
        {
            WriteHelp();
            return 1;
        }

        if (solverExplicit.Count == 0)
        {
            solverExplicit.Add("greedy-simple");
            solverExplicit.Add("back-to-depot");
        }
        solverExplicit.ExceptWith(solverBlacklist);
        if (solverExplicit.Count == 0)
        {
            Console.WriteLine("No solver left for testing");
            return 0;
        }

        if (outputDir is not null && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);
        if (!Directory.Exists(metricDir))
            Directory.CreateDirectory(metricDir);

        foreach (var file in Directory.EnumerateFiles(inputDir, "*.vrp"))
        {
            var baseName = Path.GetFileName(file);
            Console.WriteLine($"Test {baseName}");
            foreach (var solver in solverExplicit)
            {
                Console.WriteLine($"\t{solver}");
                string outputFile;
                if (outputDir is null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        outputFile = "NUL";
                    else outputFile = "/dev/null";
                }
                else
                {
                    outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(baseName)}-{solver}.sol");
                }

                var metricFile = Path.Combine(metricDir, $"{Path.GetFileNameWithoutExtension(baseName)}-{solver}.json");

                ReadOnlySpan<string> solveBasicArgs = new[]
                {
                    "-s", solver,
                    "-o", outputFile,
                    "-m", metricFile,
                    file,
                };
                var solveFullArgs = new string[solveBasicArgs.Length + flags.Count];
                solveBasicArgs.CopyTo(solveFullArgs);
                flags.CopyTo(solveFullArgs, solveBasicArgs.Length);

                var result = new Solve(cycles, time).Run(solveFullArgs);

                if (result != 0)
                {
                    Console.Error.WriteLine("Error during execution. Abort benchmark");
                    return result;
                }
            }
        }

        return 0;
    }
}
