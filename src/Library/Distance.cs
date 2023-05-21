using System.Numerics;

namespace Library;

public abstract class Distance
{
    public abstract int this[int from, int to] { get; }

    public static Distance? GetDistance(DataFile dataFile)
    {
        if (dataFile.EdgeWeight is not null)
            return new WeightsDistance(dataFile.EdgeWeight);

        if (dataFile.NodeCoord2D is not null && dataFile.Specification.EdgeWeightType is not null)
        {
            var func = DistanceMetrics.GetFunc2D(
                dataFile.Specification.EdgeWeightType.Value
            );
            if (func is not null)
                return new Coord2DDistance(dataFile.NodeCoord2D, func);
        }

        if (dataFile.NodeCoord3D is not null && dataFile.Specification.EdgeWeightType is not null)
        {
            var func = DistanceMetrics.GetFunc3D(
                dataFile.Specification.EdgeWeightType.Value
            );
            if (func is not null)
                return new Coord3DDistance(dataFile.NodeCoord3D, func);
        }

        return null;
    }

    public sealed class WeightsDistance : Distance
    {
        private readonly EdgeWeight weight;

        public WeightsDistance(EdgeWeight weight)
        {
            this.weight = weight;
        }

        public override int this[int from, int to]
        => weight[from, to];
    }

    public sealed class Coord2DDistance : Distance
    {
        private readonly NodeCoord2D coord;

        private readonly Func<Vector2, Vector2, int> func;

        public Coord2DDistance(NodeCoord2D coord, Func<Vector2, Vector2, int> func)
        {
            this.coord = coord;
            this.func = func;
        }

        public override int this[int from, int to]
        => func(coord[from], coord[to]);
    }

    public sealed class Coord3DDistance : Distance
    {
        private readonly NodeCoord3D coord;

        private readonly Func<Vector3, Vector3, int> func;

        public Coord3DDistance(NodeCoord3D coord, Func<Vector3, Vector3, int> func)
        {
            this.coord = coord;
            this.func = func;
        }

        public override int this[int from, int to]
        => func(coord[from], coord[to]);
    }
}
