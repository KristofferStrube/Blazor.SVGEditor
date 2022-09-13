using AngleSharp.Dom;

namespace KristofferStrube.Blazor.SVGEditor;

public class Stop : ISVGElement
{
    public Stop(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;
        AnimationElements = new();
    }

    public string Id { get; set; }
    public IElement Element { get; init; }
    public SVG SVG { get; init; }

    public Type Editor => throw new NotImplementedException();

    public string StateRepresentation => throw new NotImplementedException();

    public List<BaseAnimate> AnimationElements { get; set; }

    public Action<ISVGElement> Changed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string StoredHtml { get; set; }

    public void Complete()
    {
        throw new NotImplementedException();
    }

    public void Rerender()
    {
        throw new NotImplementedException();
    }

    public void UpdateHtml()
    {
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<stop{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>\n" + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + "</stop>";
    }
}
