using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class IElementExtensions
{
    internal static double GetAttributeOrZero(this IElement element, string attribute)
    {
        string? attributeValue = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(attributeValue) ? 0 : attributeValue.ParseAsDouble();
    }

    internal static double GetAttributeOrOne(this IElement element, string attribute)
    {
        string? attributeValue = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(attributeValue) ? 1 : attributeValue.ParseAsDouble();
    }

    internal static string GetAttributeOrEmpty(this IElement element, string attribute)
    {
        string? attributeValue = element.GetAttribute(attribute);
        return string.IsNullOrWhiteSpace(attributeValue) ? string.Empty : attributeValue;
    }
}
