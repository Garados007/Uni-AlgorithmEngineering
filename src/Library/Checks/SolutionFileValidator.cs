namespace Library.Checks;

public sealed class SolutionFileValidator
{
    public void Validate(DataFile dataFile, SolutionFile solutionFile)
    {
        switch (dataFile.Specification.Type)
        {
            case DataType.CVRP:
                new CVRPSolutionFileValidator().Validate(dataFile, solutionFile);
                break;
            default:
                throw new ValidationException($"cannot validate a {dataFile.Specification.Type} file");
        }
    }
}
