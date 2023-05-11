using Library.Parser;
using Library.Checks;
using Library;

namespace CVRP.Command;

public sealed class ValidateSol : ICommand
{
    private static void WriteHelp()
    {
        Console.Error.WriteLine("""
        CVRP validate-sol <problem file> <solution file>

        CVRP validate-sol reads the problem and solution files and checks if the solution file fits
        the problem file. If the checks succeed it will print "VALID" to stdout. Otherwise a short
        error description.

        You can also use "-" to read from stdin. It is not allowed to use this both for the
        problem and solution file.
        """);
    }

    public int Run(ReadOnlySpan<string> args)
    {
        if (args.Length != 2)
        {
            WriteHelp();
            return 1;
        }

        if (args[0] == args[1])
        {
            Console.Error.WriteLine("Both inputs cannot point to the same file");
            return 1;
        }

        DataFile dataFile;
        SolutionFile solutionFile;

        try { dataFile = new DataFileParser().Parse(args[0]); }
        catch (ParseException e)
        {
            Console.Error.WriteLine($"{e.GetType()}: {e.Message}");
            return 2;
        }

        try { solutionFile = new SolutionFileParser().Parse(args[1]); }
        catch (ParseException e)
        {
            Console.Error.WriteLine($"{e.GetType()}: {e.Message}");
            return 2;
        }

        try { new SolutionFileValidator().Validate(dataFile, solutionFile); }
        catch (ValidationException e)
        {
            Console.WriteLine($"{e.GetType()}: {e.Message}");
            return 3;
        }

        Console.WriteLine("VALID");
        return 0;
    }
}
