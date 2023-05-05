namespace Library;

public abstract class NodeCoord<T>
    where T : struct
{
    protected readonly Memory<T> data;

    public int Length => data.Length;

    public T this[int index]
    {
        get => data.Span[index];
        set => data.Span[index] = value;
    }

    public NodeCoord(int length)
    {
        data = new T[length];
    }

    public abstract void WriteTo(TextWriter writer);
}
