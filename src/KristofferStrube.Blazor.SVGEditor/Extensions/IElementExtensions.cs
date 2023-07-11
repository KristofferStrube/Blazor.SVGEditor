using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor.Extensions;

public static class IElementExtensions
{
    public static double GetAttributeOrZero(this IElement element, string attribute)
    {
        string? attributeValue = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(attributeValue) ? 0 : attributeValue.ParseAsDouble();
    }

    public static double GetAttributeOrOne(this IElement element, string attribute)
    {
        string? attributeValue = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(attributeValue) ? 1 : attributeValue.ParseAsDouble();
    }

    public static string GetAttributeOrEmpty(this IElement element, string attribute)
    {
        string? attributeValue = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(attributeValue) ? string.Empty : attributeValue;
    }
}
