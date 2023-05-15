namespace CVRP;

public static class Program
{
    private static void WriteHelp()
    {
        Console.Error.WriteLine("""
        CVRP <command> (<optional arguments for command>)

        CVRP supports the following commands:

            parse-graph-and-output:
                Parse a graph file and output it's representation to the std output.
            parse-sol-and-output:
                Parse a solution file and output it's representation to the std output.
            validate-sol:
                Validates a solution file.
            solve:
                Solves a problem file.
            benchmark:
                Executes a benchmark for the various solver.
        """);
    }

    public static int Main(string[] args)
    => Run(args.AsSpan());

    private static int Run(ReadOnlySpan<string> args)
    {
        if (args.Length == 0)
        {
            WriteHelp();
            return 1;
        }

        switch (args[0])
        {
            case "parse-graph-and-output":
                return new Command.ParseGraphAndOutput().Run(args[1..]);
            case "parse-sol-and-output":
                return new Command.ParseSolAndOutput().Run(args[1..]);
            case "validate-sol":
                return new Command.ValidateSol().Run(args[1..]);
            case "solve":
                return new Command.Solve().Run(args[1..]);
            case "benchmark":
                return new Command.Benchmark().Run(args[1..]);
            default:
                Console.Error.WriteLine($"Unknown command {args[0]}");
                WriteHelp();
                return 1;
        }
    }
}
