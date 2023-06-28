using static System.Math;

namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class PointExtensions
{
    internal static bool WithinRangeOf(this (double x, double y) origin, (double x, double y) point, double range)
    {
        return Sqrt(Pow(origin.x - point.x, 2) + Pow(origin.y - point.y, 2)) <= range;
    }
}
