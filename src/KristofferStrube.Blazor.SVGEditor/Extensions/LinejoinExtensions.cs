namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class LinejoinExtensions
{
    public static Linejoin ParseAsLinejoin(this string value) => value switch
        {
            "miter-clip" => Linejoin.MiterClip,
            "round" => Linejoin.Round,
            "bevel" => Linejoin.Bevel,
            "arcs" => Linejoin.Arcs,
            _ => Linejoin.Miter,
        };

    public static string AsString(this Linejoin value) => value switch
    {
        Linejoin.MiterClip => "round",
        Linejoin.Round => "round",
        Linejoin.Bevel => "round",
        Linejoin.Arcs => "round",
        _ => "miter",
    };
}
