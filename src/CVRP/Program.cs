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
            default:
                Console.Error.WriteLine($"Unknown command {args[0]}");
                WriteHelp();
                return 1;
        }
    }
}
