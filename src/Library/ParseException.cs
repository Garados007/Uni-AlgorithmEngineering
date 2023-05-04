namespace Library;

[System.Serializable]
public class ParseException : System.Exception
{
    public ParseException() { }

    public ParseException(ref Parser.State state, string message)
        : this($"{message} at {state.TotalLine + 1}:{state.TotalRead - state.LastLineStart + 1} (offset {state.TotalRead})")
    {}
    public ParseException(string message) : base(message) { }
    public ParseException(string message, System.Exception inner) : base(message, inner) { }
    protected ParseException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
