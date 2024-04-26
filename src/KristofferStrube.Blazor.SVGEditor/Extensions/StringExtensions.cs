using System.Globalization;

namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class StringExtensions
{
    internal static double ParseAsDouble(this string s)
    {
        return double.Parse(s, CultureInfo.InvariantCulture);
    }
    internal static List<(double x, double y)> ToPoints(this string points)
    {
        try
        {
            return string.IsNullOrWhiteSpace(points)
                ? []
                : points.Trim().Split(" ").Select(p => (x: p.Split(",")[0].ParseAsDouble(), y: p.Split(",")[1].ParseAsDouble())).ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }
    internal static string ToUrl(this string id)
    {
        return $"url('#{id}')";
    }
}
