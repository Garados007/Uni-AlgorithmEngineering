namespace Library.Parser;

public sealed class SpecificationParser : ParserBase<Specification>
{
    private bool HasCloseColon(ref State state)
    {
        const int MaxKeySize = 50;
        if (state.read < MaxKeySize)
            ReadMoreData(ref state);
        var data = state.Data;
        if (data.Length > MaxKeySize)
            data = data[..MaxKeySize];
        var colonIndex = data.IndexOf(':');
        if (colonIndex == -1)
            return false;
        var lineIndex = data.IndexOf('\n');
        return lineIndex == -1 || colonIndex < lineIndex;
    }

    public override Specification? Parse(ref State state)
    {
        var dict = new Dictionary<string, List<string>>();
        while (!state.EOF)
        {
            if (!HasCloseColon(ref state))
                break;
            var key = ReadString(ref state)
                ?? throw new ParseException(ref state, "Expected a keyword");
            if (!SkipWhitespace(ref state))
                throw new ParseException(ref state, "Expected whitespace");
            if (!CheckKeyword(ref state, ":"))
                throw new ParseException(ref state, "Expected a colon");
            if (!SkipWhitespace(ref state))
                throw new ParseException(ref state, "Expected whitespace");
            if (key == "EOF")
            {
                ReadLine(ref state);
                break;
            }
            var value = ReadLine(ref state)
                ?? throw new ParseException(ref state, "Expect a value");

            if (!dict.TryGetValue(key, out var list))
                dict.Add(key, list = new());
            list.Add(value);
        }
        if (dict.Count == 0)
            return null;
        return new Specification(dict);
    }
}
