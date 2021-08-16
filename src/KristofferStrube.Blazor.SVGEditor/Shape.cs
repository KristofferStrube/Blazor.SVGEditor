using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public abstract class Shape : ISVGElement
    {
        public IElement Element { get; set; }

        public abstract Type Editor { get; }
        public SVG SVG { get; set; }
        public string Fill {
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
        public (double x, double y) Panner { get; set; }
        public int? CurrentAnchor { get; set; }
        public BoundingBox BoundingBox { get; set; } = new();
        public EditMode EditMode { get; set; }
        public IEnumerable<EditMode> AvailableEditModes { get; set; }
        public Action<ISVGElement> Changed { get; set; }
        public bool Selectable => SVG.CurrentShape == null;
        public bool Selected => SVG.CurrentShape == this;
        public string _StateRepresentation { get; set;}
        public string StateRepresentation => string.Join("-", Element.Attributes.Select(a => a.Value)) + Selected.ToString() + EditMode.ToString() + SVG.Scale + SVG.Translate.x + SVG.Translate.y;
        public abstract void HandleMouseMove(MouseEventArgs eventArgs);
        public abstract void HandleMouseUp(MouseEventArgs eventArgs);
        public abstract void HandleMouseOut(MouseEventArgs eventArgs);
        public abstract void Complete();
    }
}
