namespace Library.Parser;

public sealed class SolutionFileParser : ParserBase<SolutionFile>
{
    public override SolutionFile? Parse(ref State state)
    {
        var file = new SolutionFile();
        while (CheckKeyword(ref state, "Route #"))
        {
            var index = ReadInt32(ref state);
            if (index is null)
                throw new ParseException(ref state, "index expected");
            if (index.Value + 1 != file.Routes.Count)
                throw new ParseException(ref state, $"index {file.Routes.Count + 1} expected");
            if (!CheckKeyword(ref state, ":"))
                throw new ParseException(ref state, "colon expected");
            var route = new List<int>();
            while (true)
            {
                SkipWhitespace(ref state);
                index = ReadInt32(ref state);
                if (index is null)
                {
                    if (!CheckKeyword(ref state, "\n"))
                        throw new ParseException(ref state, "consumer index expected");
                    break;
                }
                route.Add(index.Value);
            }
            file.Routes.Add(route);
        }
        if (!CheckKeyword(ref state, "Cost"))
            throw new ParseException(ref state, "Route or Cost keyword expected");
        if (!SkipWhitespace(ref state))
            throw new ParseException(ref state, "whitespace expected");
        var cost = ReadInt32(ref state);
        if (cost is null)
            throw new ParseException(ref state, "Cost value expected");
        file.Cost = cost.Value;
        return file;
    }
}
