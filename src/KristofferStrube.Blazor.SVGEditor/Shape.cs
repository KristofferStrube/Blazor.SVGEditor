using AngleSharp;
using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class Shape : ISVGElement
    {
        public Shape(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;

            Element.Children
                .ToList()
                .ForEach(child =>
                {
                    if (child.GetAttribute("attributeName") is string attributeName)
                    {
                        var animate = new Animate(this, child);
                        switch (attributeName)
                        {
                            case "fill": { FillAnimate = animate; break; }
                            case "stroke": { StrokeAnimate = animate; break; }
                            case "stroke-width": { StrokeWidthAnimate = animate; break; }
                        }
                    }
                }
                );
        }

        public IElement Element { get; set; }

        public abstract Type Editor { get; }
        public SVG SVG { get; set; }
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
        public Animate FillAnimate { get; set; }
        public Animate StrokeAnimate { get; set; }
        public Animate StrokeWidthAnimate { get; set; }
        public bool Playing { get; set; }
        public bool HasAnimation => FillAnimate is not null || StrokeAnimate is not null || StrokeWidthAnimate is not null;
        
        public BoundingBox BoundingBox { get; set; } = new();
        public Action<ISVGElement> Changed { get; set; }
        public bool Selected => SVG.MarkedElements.Contains(this);
        public bool IsChildElement => Element.ParentElement?.TagName is "G" or null;
        public string _StateRepresentation { get; set; }
        public virtual string StateRepresentation => string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + SVG.EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y + Serialize(BoundingBox);
        public virtual void UpdateHtml() => StoredHtml = Element.ToHtml();
        public string StoredHtml { get; set; }
        public virtual void Rerender()
        {
            _StateRepresentation = null;
        }
        public abstract void HandleMouseMove(MouseEventArgs eventArgs);
        public abstract void HandleMouseUp(MouseEventArgs eventArgs);
        public abstract void HandleMouseOut(MouseEventArgs eventArgs);
        public abstract void Complete();
    }
}
