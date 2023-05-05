namespace Library;

public sealed record DataFile(
    Specification Specification,
    EdgeWeight? EdgeWeight = null
)
{
    public void WriteTo(TextWriter writer)
    {
        Specification.WriteTo(writer);
        EdgeWeight?.WriteTo(writer);

        writer.WriteLine("EOF");
    }
}
