namespace Library;

public sealed record DataFile(
    Specification Specification,
    EdgeWeight? EdgeWeight = null,
    NodeCoord2D? NodeCoord2D = null,
    NodeCoord3D? NodeCoord3D = null
)
{
    public void WriteTo(TextWriter writer)
    {
        Specification.WriteTo(writer);
        EdgeWeight?.WriteTo(writer);
        NodeCoord2D?.WriteTo(writer);
        NodeCoord3D?.WriteTo(writer);

        writer.WriteLine("EOF");
    }
}
