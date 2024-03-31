using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.GradientEditors;

namespace KristofferStrube.Blazor.SVGEditor;

public class Defs : ISVGElement
{
    private readonly Dictionary<string, Type> GradientTypes = new()
    {
        { "LINEARGRADIENT", typeof(LinearGradient) }
    };

    public Defs(IElement element, SVGEditor svg)
    {
        Element = element;
        SVG = svg;
        Children = Element.Children.Select(child =>
        {
            ISVGElement? sVGElement = SVG.SupportedElements.FirstOrDefault(se => se.CanHandle(child))?.ElementType is Type type
                ? Activator.CreateInstance(type, child, SVG) as Shape
                : GradientTypes.TryGetValue(child.TagName.ToUpper(), out Type? gradientType)
                    ? Activator.CreateInstance(gradientType, child, SVG) as ISVGElement
                    : throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
            if (sVGElement is not null)
            {
                sVGElement.Changed = UpdateInput;
            }
            return sVGElement;
        }).Select(element => element!).ToList();
    }

    public void UpdateInput(ISVGElement child)
    {
        child.UpdateHtml();
        Changed?.Invoke(this);
    }
    public string? Id { get; set; }

    public IElement Element { get; init; }

    public SVGEditor SVG { get; init; }

    public List<ISVGElement> Children { get; init; } = [];

    public Type Presenter => typeof(DefsPresenter);

    public string StateRepresentation => throw new NotImplementedException();

    public Action<ISVGElement>? Changed { get; set; }
    public string StoredHtml { get; set; } = string.Empty;

    public void Complete()
    {
    }

    public void Rerender()
    {
    }

    public void UpdateHtml()
    {
        Children.ForEach(c => c.UpdateHtml());
        StoredHtml = $"<defs{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>\n" + string.Join("", Children.Select(e => e.StoredHtml + "\n")) + "</defs>";
    }

    public void BeforeBeingRemoved()
    {
    }
}
