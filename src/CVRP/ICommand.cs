namespace CVRP;

public interface ICommand
{
    int Run(ReadOnlySpan<string> args);
}
