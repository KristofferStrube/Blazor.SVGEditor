using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor.Extensions;

internal static class IElementExtensions
{
    internal static double GetAttributeOrZero(this IElement element, string attribute)
    {
        string attributeValue = element.GetAttribute(attribute);
        if (string.IsNullOrWhiteSpace(attributeValue))
        {
            return 0;
        }

        return attributeValue.ParseAsDouble();
    }
    internal static string GetAttributeOrEmpty(this IElement element, string attribute)
    {
        string attributeValue = element.GetAttribute(attribute);
        if (string.IsNullOrWhiteSpace(attributeValue))
        {
            return string.Empty;
        }

        return attributeValue;
    }
}
