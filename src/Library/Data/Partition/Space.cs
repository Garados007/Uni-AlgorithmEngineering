using System.Numerics;

namespace Library.Data.Partition;

public sealed class Space2D
{
    public Vector2 MinCoords { get; }

    public Vector2 MaxCoords { get; }

    public HashSet<int> Nodes { get; } = new();

    public Space2D? Parent { get; }

    public Space2DTiling? Tiling { get; }

    public NodeCoord2D Coords { get; }

    private readonly Memory<Space2D?> accessTable;

    public ReadOnlyMemory<Space2D?> AccessTable => accessTable;

    public Space2D(NodeCoord2D coords, int tilingCapacity = 1000)
    {
        if (tilingCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(tilingCapacity));
        Coords = coords;
        if (coords.Length == 0)
            return;

        accessTable = new Space2D[coords.Length];
        Nodes.EnsureCapacity(coords.Length);
        Nodes.Add(0);
        accessTable.Span[0] = this;
        var min = coords[0];
        var max = min;
        for (int i = 1; i < coords.Length; ++i)
        {
            var pos = coords[i];
            min = Vector2.Min(min, pos);
            max = Vector2.Max(max, pos);
            Nodes.Add(i);
            accessTable.Span[i] = this;
        }

        MinCoords = min;
        MaxCoords = max + Vector2.One;
        if (Nodes.Count >= tilingCapacity)
        {
            Tiling = new Space2DTiling(this, (MinCoords + MaxCoords) / 2, tilingCapacity);
        }
    }

    internal Space2D(Space2D parent, Vector2 min, Vector2 max, int tilingCapacity)
    {
        Parent = parent;
        MinCoords = min;
        MaxCoords = max;
        Coords = parent.Coords;
        accessTable = parent.accessTable;

        if (parent.Nodes.Count >= 16)
            Nodes.EnsureCapacity(Parent.Nodes.Count / 2);
        foreach (var node in parent.Nodes)
        {
            var pos = Coords[node];
            if (MinCoords.X <= pos.X && MinCoords.Y <= pos.Y && pos.X < MaxCoords.X && pos.Y < MaxCoords.Y)
            {
                Nodes.Add(node);
                accessTable.Span[node] = this;
            }
        }

        if (Nodes.Count >= tilingCapacity)
        {
            Tiling = new Space2DTiling(this, (MinCoords + MaxCoords) / 2, tilingCapacity);
        }
    }

    public void WriteRepresentation(TextWriter writer)
    => WriteRepresentation(writer, "");

    private void WriteRepresentation(TextWriter writer, string prefix)
    {
        writer.WriteLine($"Space ({Nodes.Count}): ({MinCoords}) - ({MaxCoords})");
        if (Tiling is not null)
        {
            prefix += "\t";
            for (int y = 0; y < 2; ++y)
                for (int x = 0; x < 2; ++x)
                {
                    writer.Write($"{prefix}[{x},{y}] ");
                    Tiling[x, y].WriteRepresentation(writer, prefix);
                }
        }
    }

    public void Remove(int node)
    {
        if (node < 0 || node >= Coords.Length)
            throw new ArgumentOutOfRangeException(nameof(node));
        var space = accessTable.Span[node];
        while (space is not null)
        {
            space.Nodes.Remove(node);
            space = space.Parent;
        }
    }
}
