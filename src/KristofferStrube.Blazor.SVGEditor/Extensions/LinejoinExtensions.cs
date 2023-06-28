namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class LinejoinExtensions
{
    public static Linejoin ParseAsLinejoin(this string value)
    {
        return value switch
        {
            "miter-clip" => Linejoin.MiterClip,
            "round" => Linejoin.Round,
            "bevel" => Linejoin.Bevel,
            "arcs" => Linejoin.Arcs,
            _ => Linejoin.Miter,
        };
    }

    public static string AsString(this Linejoin value)
    {
        return value switch
        {
            Linejoin.MiterClip => "miter-clip",
            Linejoin.Round => "round",
            Linejoin.Bevel => "bevel",
            Linejoin.Arcs => "arcs",
            Linejoin.Miter => "miter",
            _ => "miter",
        };
    }
}
