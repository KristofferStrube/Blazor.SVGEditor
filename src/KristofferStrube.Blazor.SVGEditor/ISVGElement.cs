using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor
{
    public interface ISVGElement
    {
        public IElement Element { get; set; }

        public Type Editor { get; }

        public bool Selected { get; }

        public bool Selectable { get; }

        public BoundingBox BoundingBox { get; set; }

        public EditMode EditMode { get; set; }

        public (double x, double y) Panner { get; set; }

        public string _StateRepresentation { get; set; }

        public void HandleMouseMove(MouseEventArgs eventArgs);
        public void HandleMouseUp(MouseEventArgs eventArgs);
        public void HandleMouseOut(MouseEventArgs eventArgs);
        public Action<ISVGElement> Changed { get; set; }
        public string ToHtml();
        public void Complete();
    }
}
