using System.Numerics;

namespace Library;

public sealed class NodeCoord2D : NodeCoord<Vector2>
{
    public NodeCoord2D(int length) : base(length)
    {
    }

    public override void WriteTo(TextWriter writer)
    {
        var data = this.data.Span;
        writer.WriteLine("NODE_COORD_SECTION");
        for (int i = 0; i < Length; ++i)
        {
            writer.Write(i + 1);
            writer.Write(' ');
            writer.Write(data[i].X);
            writer.Write(' ');
            writer.WriteLine(data[i].Y);
        }
    }
}
