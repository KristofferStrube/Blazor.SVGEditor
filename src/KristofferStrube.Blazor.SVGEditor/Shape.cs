using AngleSharp.Dom;
using KristofferStrube.Blazor.SVGEditor.Models;
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
        public SVG SVG { get; set; }
        public string Fill => Element.GetAttribute("fill") ?? string.Empty;
        public string Stroke => Element.GetAttribute("stroke") ?? string.Empty;
        public string StrokeWidth => Element.GetAttribute("stroke-width") ?? string.Empty;
        public string TagName => Element.TagName;
        public (double x, double y) Panner { get; set; }
        public EditMode EditMode { get; set; }
        public IEnumerable<EditMode> AvailableEditModes { get; set; }
        public Action<ISVGElement> Changed { get; set; }
        public bool Selectable => SVG.CurrentShape == null;
        public bool Selected => SVG.CurrentShape == this;
        public abstract void HandleMouseMove(MouseEventArgs eventArgs);
        public abstract void HandleMouseUp(MouseEventArgs eventArgs);
        public abstract void HandleMouseOut(MouseEventArgs eventArgs);

    }
}
