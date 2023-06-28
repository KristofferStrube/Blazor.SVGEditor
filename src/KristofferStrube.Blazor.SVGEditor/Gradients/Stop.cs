using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;

namespace KristofferStrube.Blazor.SVGEditor;

public class Stop : ISVGElement
{
    public Stop(IElement element, LinearGradient parent, SVGEditor svg)
    {
        Element = element;
        Parent = parent;
        SVG = svg;
        AnimationElements = new();
        Key = Guid.NewGuid();
    }

    public Guid Key { get; set; }

    public string? Id { get; set; }
    public IElement Element { get; init; }
    public LinearGradient Parent { get; init; }
    public SVGEditor SVG { get; init; }

    public Type Presenter => throw new NotImplementedException();

    public string StateRepresentation => $"{Offset}{StopColor}{StopOpacity}";

    public double Offset
    {
        get
        {
            string? offset = Element.GetAttribute("offset");
            return offset is null ? 0 : offset.Trim().EndsWith("%") ? offset.Trim()[..^1].ParseAsDouble() / 100 : offset.Trim().ParseAsDouble();
        }
        set
        {
            Element.SetAttribute("offset", (value * 100).AsString() + "%");
            Changed?.Invoke(this);
        }
    }

    public string StopColor
    {
        get => Element.GetAttributeOrEmpty("stop-color");
        set { Element.SetAttribute("stop-color", value); Changed?.Invoke(this); }
    }

    public double StopOpacity
    {
        get => Element.GetAttributeOrOne("stop-opacity");
        set { if (value != 1) { Element.SetAttribute("stop-opacity", value.AsString()); } Changed?.Invoke(this); }
    }

    public List<BaseAnimate> AnimationElements { get; set; }

    public Action<ISVGElement>? Changed
    {
        get => Parent.Changed; set => Parent.Changed = value;
    }
    public string StoredHtml { get; set; } = string.Empty;

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
        StoredHtml = $"<stop{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>" + (AnimationElements.Count > 0 ? "\n" : "") + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + "</stop>";
    }
}
