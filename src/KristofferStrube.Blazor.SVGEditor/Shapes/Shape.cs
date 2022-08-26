using AngleSharp;
using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor;

public abstract class Shape : ISVGElement
{
    private readonly Dictionary<string, Type> _animateTypes = new()
    {
        { "fill", typeof(AnimateFill) },
        { "stroke", typeof(AnimateStroke) },
        { "d", typeof(AnimateD) },
    };

    internal string _stateRepresentation;

    public Shape(IElement element, SVG svg)
    {
        Element = element;
        SVG = svg;

        AnimationElements = Element.Children
            .Where(child => child.TagName == "ANIMATE" && child.HasAttribute("attributename"))
            .Select(child =>
                {
                    string attributeName = child.GetAttribute("attributename");
                    if (_animateTypes.ContainsKey(attributeName))
                    {
                        BaseAnimate animation = (BaseAnimate)Activator.CreateInstance(_animateTypes[attributeName], child, SVG);
                        animation.Parent = this;
                        return animation;
                    }
                    else
                    {
                        return new AnimateDefault(child, SVG);
                    }
                }
            )
            .ToList();
    }

    public IElement Element { get; init; }
    public SVG SVG { get; init; }
    public abstract Type Editor { get; }

    public string Fill
    {
        get => Element.GetAttributeOrEmpty("fill");
        set { Element.SetAttribute("fill", value); Changed.Invoke(this); }
    }
    public string Stroke
    {
        get => Element.GetAttributeOrEmpty("stroke");
        set { Element.SetAttribute("stroke", value); Changed.Invoke(this); }
    }
    public string StrokeWidth
    {
        get => Element.GetAttributeOrEmpty("stroke-width");
        set { Element.SetAttribute("stroke-width", value); Changed.Invoke(this); }
    }
    public List<BaseAnimate> AnimationElements { get; set; }
    public bool HasAnimation => AnimationElements is { Count: > 0 };
    public Box BoundingBox { get; set; } = new();
    public Action<ISVGElement> Changed { get; set; }
    public bool Selected => SVG.VisibleSelectionShapes.Contains(this);
    public bool IsChildElement => Element.ParentElement?.TagName is "G" or null;
    public virtual string StateRepresentation =>
        string.Join("-", Element.Attributes.Select(a => a.Value)) +
        Selected.ToString() +
        SVG.EditMode.ToString() +
        SVG.Scale + SVG.Translate.x +
        SVG.Translate.y +
        Serialize(BoundingBox) +
        string.Join("-", AnimationElements.Select(a => a.StateRepresentation));

    public string StoredHtml { get; set; }
    public virtual void UpdateHtml()
    {
        AnimationElements.ForEach(a => a.UpdateHtml());
        StoredHtml = $"<{Element.LocalName}{string.Join("", Element.Attributes.Select(a => $" {a.Name}=\"{a.Value}\""))}>" + (AnimationElements.Count > 0 ? "\n" : "") + string.Join("", AnimationElements.Select(a => a.StoredHtml + "\n")) + $"</{Element.LocalName}>";
    }

    public virtual void Rerender()
    {
        _stateRepresentation = null;
    }

    public abstract void SnapToInteger();

    public abstract IEnumerable<(double x, double y)> SelectionPoints { get; }

    public abstract void HandlePointerMove(PointerEventArgs eventArgs);
    public abstract void HandlePointerUp(PointerEventArgs eventArgs);
    public abstract void HandlePointerOut(PointerEventArgs eventArgs);
    public abstract void Complete();
}
