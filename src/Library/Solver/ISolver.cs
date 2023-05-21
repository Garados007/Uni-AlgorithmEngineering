namespace Library;

public interface ISolver<TData, TSolution>
    where TData : notnull
    where TSolution : notnull
{
    TSolution? Solve(TData data);
}
