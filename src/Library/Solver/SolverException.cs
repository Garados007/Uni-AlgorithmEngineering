namespace Library.Solver;

[System.Serializable]
public class SolverException : System.Exception
{
    public SolverException() { }
    public SolverException(string message) : base(message) { }
    public SolverException(string message, System.Exception inner) : base(message, inner) { }
    protected SolverException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
