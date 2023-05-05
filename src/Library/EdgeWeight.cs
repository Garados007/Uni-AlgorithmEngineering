namespace Library;

public abstract class EdgeWeight
{
    public abstract EdgeWeightFormat Format { get; }

    public abstract int this[int from, int to] { get; set; }

    public int Length { get; }

    public EdgeWeight(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));
        Length = length;
    }

    public abstract void WriteTo(TextWriter writer);
}
