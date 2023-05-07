using System.Numerics;

namespace Library;

public static class DistanceMetrics
{
    private static int nint(float value)
    => (int)(value + 0.5f);

    public static int EUC_2D(Vector2 a, Vector2 b)
    => nint((a - b).Length());

    public static int EUC_3D(Vector3 a, Vector3 b)
    => nint((a - b).Length());

    public static int MAN_2D(Vector2 a, Vector2 b)
    {
        var d = Vector2.Abs(a - b);
        return nint(d.X + d.Y);
    }

    public static int MAN_3D(Vector3 a, Vector3 b)
    {
        var d = Vector3.Abs(a - b);
        return nint(d.X + d.Y + d.Z);
    }

    public static int MAX_2D(Vector2 a, Vector2 b)
    {
        var d = Vector2.Abs(a - b);
        return Math.Max(nint(d.X), nint(d.Y));
    }

    public static int MAX_3D(Vector3 a, Vector3 b)
    {
        var d = Vector3.Abs(a - b);
        return Math.Max(Math.Max(nint(d.X), nint(d.Y)), nint(d.Z));
    }

    private static (double longitude, double latitude) GetGeo(Vector2 vec)
    {
        const double PI = 3.141592;
        var deg = nint(vec.X);
        var min = vec.X - deg;
        var latitude = PI * (deg + 5.0 * min / 3.0) / 180.0;
        deg = nint(vec.Y);
        min = vec.Y - deg;
        var longitude = PI * (deg + 5.0 * min / 3.0) / 180.0;
        return (longitude, latitude);
    }

    public static int GEO(Vector2 a, Vector2 b)
    {
        var (longA, latA) = GetGeo(a);
        var (longB, latB) = GetGeo(b);

        const double RRR = 6378.388;
        var q1 = Math.Cos(longA - longB);
        var q2 = Math.Cos(latA - latB);
        var q3 = Math.Cos(latA + latB);
        return (int)(RRR * Math.Acos(0.5 * ((1.0 + q1) * q2 - (1.0 - q1) * q3)) + 1.0);
    }

    public static int ATT(Vector2 a, Vector2 b)
    {
        var r = MathF.Sqrt(Vector2.DistanceSquared(a, b) / 10.0f);
        var t = nint(r);
        return t < r ? t + 1 : t;
    }

    public static int CEIL_2D(Vector2 a, Vector2 b)
    => (int)Math.Ceiling(Vector2.Distance(a, b));

    public static Func<Vector2, Vector2, int>? GetFunc2D(EdgeWeightType type)
    {
        return type switch
        {
            EdgeWeightType.EUC_2D => EUC_2D,
            EdgeWeightType.MAX_2D => MAX_2D,
            EdgeWeightType.MAN_2D => MAN_2D,
            EdgeWeightType.CEIL_2D => CEIL_2D,
            EdgeWeightType.GEO => GEO,
            EdgeWeightType.ATT => ATT,
            _ => null,
        };
    }

    public static Func<Vector3, Vector3, int>? GetFunc3D(EdgeWeightType type)
    {
        return type switch
        {
            EdgeWeightType.EUC_3D => EUC_3D,
            EdgeWeightType.MAX_3D => MAX_3D,
            EdgeWeightType.MAN_3D => MAN_3D,
            _ => null,
        };
    }
}
