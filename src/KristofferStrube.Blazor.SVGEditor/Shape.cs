using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using static System.Text.Json.JsonSerializer;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class Shape : ISVGElement
    {
        public IElement Element { get; set; }

        public abstract Type Editor { get; }
        public SVG SVG { get; set; }
        public string Fill
        {
            get => Element.GetAttribute("fill") ?? string.Empty;
            set { Element.SetAttribute("fill", value); Changed.Invoke(this); }
        }
        public string Stroke
        {
            get => Element.GetAttribute("stroke") ?? string.Empty;
            set { Element.SetAttribute("stroke", value); Changed.Invoke(this); }
        }
        public string StrokeWidth
        {
            get => Element.GetAttribute("stroke-width") ?? string.Empty;
            set { Element.SetAttribute("stroke-width", value); Changed.Invoke(this); }
        }
        public BoundingBox BoundingBox { get; set; } = new();
        public Action<ISVGElement> Changed { get; set; }
        public bool Selectable => SVG.SelectedElements.Count == 0;
        public bool Selected => SVG.SelectedElements.Contains(this);
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
