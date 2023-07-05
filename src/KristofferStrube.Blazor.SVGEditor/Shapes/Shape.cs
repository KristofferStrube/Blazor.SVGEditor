using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor;

public abstract class Shape : ISVGElement
{
    internal static readonly Dictionary<string, Type> animateTypes = new()
    {
        { "fill", typeof(AnimateFill) },
        { "stroke", typeof(AnimateStroke) },
        { "stroke-dashoffset", typeof(AnimateStrokeDashoffset) },
        { "d", typeof(AnimateD) },
    };

    internal string _stateRepresentation = string.Empty;

    public Shape(IElement element, SVGEditor svg)
    {
        Element = element;
        SVG = svg;

        AnimationElements = Element.Children
            .Where(child => child.TagName == "ANIMATE" && child.HasAttribute("attributename"))
            .Select(child =>
                {
                    string? attributeName = child.GetAttribute("attributename");
                    if (attributeName is not null && animateTypes.TryGetValue(attributeName, out Type? animateType))
                    {
                        var animation = Activator.CreateInstance(animateType, child, this, SVG) as BaseAnimate;
                        return animation ?? throw new NotImplementedException($"Tag not supported:\n {child.OuterHtml}");
                    }
                    else
                    {
                        return new AnimateDefault(child, this, SVG);
                    }
                }
            )
            .ToList();
    }

    public string? Id { get; set; }
    public IElement Element { get; init; }
    public SVGEditor SVG { get; init; }
    public abstract Type Presenter { get; }

    public string Fill
    {
        get => Element.GetAttributeOrEmpty("fill");
        set { Element.SetAttribute("fill", value); Changed?.Invoke(this); }
    }
    public string Stroke
    {
        get => Element.GetAttributeOrEmpty("stroke");
        set { Element.SetAttribute("stroke", value); Changed?.Invoke(this); }
    }
    public string StrokeWidth
    {
        get => Element.GetAttributeOrEmpty("stroke-width");
        set { Element.SetAttribute("stroke-width", value); Changed?.Invoke(this); }
    }
    public Linecap StrokeLinecap
    {
        get => Element.GetAttributeOrEmpty("stroke-linecap").ToLower().ParseAsLinecap();
        set { Element.SetAttribute("stroke-linecap", value.AsString()); Changed?.Invoke(this); }
    }
    public Linejoin StrokeLinejoin
    {
        get => Element.GetAttributeOrEmpty("stroke-linejoin").ToLower().ParseAsLinejoin();
        set { Element.SetAttribute("stroke-linejoin", value.AsString()); Changed?.Invoke(this); }
    }
    public string StrokeDasharray
    {
        get => Element.GetAttributeOrEmpty("stroke-dasharray");
        set { Element.SetAttribute("stroke-dasharray", value); Changed?.Invoke(this); }
    }
    public double StrokeDashoffset
    {
        get => Element.GetAttributeOrZero("stroke-dashoffset");
        set { Element.SetAttribute("stroke-dashoffset", value.AsString()); Changed?.Invoke(this); }
    }
    public List<BaseAnimate> AnimationElements { get; set; }
    public bool HasAnimation => AnimationElements is { Count: > 0 };
    public Box BoundingBox { get; set; } = new();
    public Action<ISVGElement>? Changed { get; set; }
    public bool Selected => SVG.VisibleSelectionShapes.Contains(this);
    public bool IsChildElement => Element.ParentElement?.TagName is "G" or null;
    public bool ShouldTriggerContextMenu => SVG.ShouldShowContextMenu(this);
    public virtual string StateRepresentation =>
        string.Join("-", Element.Attributes.Select(a => a.Value)) +
        Selected.ToString() +
        SVG.EditMode.ToString() +
        SVG.Scale + SVG.Translate.x +
        SVG.Translate.y +
        Serialize(BoundingBox) +
        string.Join("-", AnimationElements.Select(a => a.StateRepresentation));

    public string StoredHtml { get; set; } = string.Empty;
    public virtual void UpdateHtml()
    {
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<{Element.LocalName}{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>" + (AnimationElements.Count > 0 ? "\n" : "") + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + $"</{Element.LocalName}>";
    }

    public virtual void Rerender()
    {
        _stateRepresentation = string.Empty;
    }

    public abstract void SnapToInteger();

    public abstract IEnumerable<(double x, double y)> SelectionPoints { get; }

    public abstract void HandlePointerMove(PointerEventArgs eventArgs);
    public abstract void HandlePointerUp(PointerEventArgs eventArgs);
    public abstract void HandlePointerOut(PointerEventArgs eventArgs);
    public abstract void Complete();
}
