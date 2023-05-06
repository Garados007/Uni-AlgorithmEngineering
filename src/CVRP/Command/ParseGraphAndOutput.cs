using Library.Parser;
using Library;

namespace CVRP.Command;

public sealed class ParseGraphAndOutput : ICommand
{
    private static void WriteHelp()
    {
        Console.Error.WriteLine("""
        CVRP parse-graph-and-output <file>

        CVRP parse-graph-and-output expects one input file which is a valid graph file. The representation
        of the file is written afterwards to stdout.

        You can also use "-" to read from stdin.
        """);
    }

    public int Run(ReadOnlySpan<string> args)
    {
        if (args.Length != 1)
        {
            WriteHelp();
            return 1;
        }

        if (args[0] == "-")
        {
            return Run(Console.In);
        }
        else
        {
            if (!File.Exists(args[0]))
            {
                Console.Error.WriteLine($"File {args[0]} not found");
                return 2;
            }
            using var file = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(file);
            return Run(reader);
        }
    }

    private int Run(TextReader reader)
    {
        var state = new State
        {
            reader = reader,
            buffer = stackalloc char[1024],
        };
        var parser = new DataFileParser();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        DataFile? dataFile;
        try
        {
            dataFile = parser.Parse(ref state);
        }
        catch (Library.ParseException e)
        {
            Console.Error.WriteLine(e);
            return 2;
        }
#if DEBUG
        Console.WriteLine(sw.Elapsed);
#endif

        if (dataFile is null)
        {
            Console.WriteLine("NULL");
            return 2;
        }
        else
        {
            dataFile.WriteTo(Console.Out);
            return 0;
        }
    }
}
