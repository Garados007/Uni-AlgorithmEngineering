namespace Library.Solver;

[System.Serializable]
public class SolverNotApplicableException : SolverException
{
    public SolverNotApplicableException() { }
    public SolverNotApplicableException(string message) : base(message) { }
    public SolverNotApplicableException(string message, System.Exception inner) : base(message, inner) { }
    protected SolverNotApplicableException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
