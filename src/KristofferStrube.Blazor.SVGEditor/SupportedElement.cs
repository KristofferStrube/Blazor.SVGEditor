using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor;

public record SupportedElement(Type ElementType, Func<IElement, bool> CanHandle);