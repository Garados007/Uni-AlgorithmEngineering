using System.Numerics;

namespace Library.Data.Partition;

public sealed class Space2DTiling
{
    public Space2D Parent { get; }

    public Vector2 Cut { get; }

    private readonly ReadOnlyMemory<Space2D> tiles = new Space2D[4];

    public Space2D this[int x, int y]
    {
        get
        {
            if (x < 0 || x > 1)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y > 1)
                throw new ArgumentOutOfRangeException(nameof(y));
            return tiles.Span[y * 2 + x];
        }
    }

    public Space2DTiling(Space2D parent, Vector2 cut, int tilingCapacity)
    {
        if (cut.X < parent.MinCoords.X || cut.X > parent.MaxCoords.X)
            throw new ArgumentOutOfRangeException(nameof(cut));
        if (cut.Y < parent.MinCoords.Y || cut.Y > parent.MaxCoords.Y)
            throw new ArgumentOutOfRangeException(nameof(cut));
        Parent = parent;
        Cut = cut;

        Memory<Space2D> tilesMemory = new Space2D[4];
        var tiles = tilesMemory.Span;
        tiles[0] = new Space2D(parent, parent.MinCoords, cut, tilingCapacity);
        tiles[1] = new Space2D(parent, new Vector2(cut.X, parent.MinCoords.Y), new Vector2(parent.MaxCoords.X, cut.Y), tilingCapacity);
        tiles[2] = new Space2D(parent, new Vector2(parent.MinCoords.X, cut.Y), new Vector2(cut.X, parent.MaxCoords.Y), tilingCapacity);
        tiles[3] = new Space2D(parent, cut, parent.MaxCoords, tilingCapacity);

        this.tiles = tilesMemory;
    }

    public IEnumerable<Space2D> GetSpaces()
    {
        for (int i = 0; i < 4; ++i)
            yield return tiles.Span[i];
    }
}
