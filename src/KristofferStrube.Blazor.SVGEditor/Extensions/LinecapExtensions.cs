namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class LinecapExtensions
{
    public static Linecap ParseAsLinecap(this string value) => value switch
        {
            "round" => Linecap.Round,
            "square" => Linecap.Square,
            _ => Linecap.Butt,
        };

    public static string AsString(this Linecap value) => value switch
    {
        Linecap.Round => "round",
        Linecap.Square => "square",
        _ => "butt",
    };
}
