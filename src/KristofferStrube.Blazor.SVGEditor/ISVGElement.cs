using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace KristofferStrube.Blazor.SVGEditor
{
    public interface ISVGElement
    {
        public IElement Element { get; set; }

        public Type Editor { get; }

        public bool Selected { get; }

        public bool Selectable { get; }

        public EditMode EditMode { get; set; }

        public IEnumerable<EditMode> AvailableEditModes { get; set; }

        public (double x, double y) Panner { get; set; }

        public string _StateRepresentation { get; set; }

        public void HandleMouseMove(MouseEventArgs eventArgs);
        public void HandleMouseUp(MouseEventArgs eventArgs);
        public void HandleMouseOut(MouseEventArgs eventArgs);
        public Action<ISVGElement> Changed { get; set; }

        public static Action<SVG> AddNew;
        public void Complete();
    }

    public enum EditMode
    {
        None,
        Add,
        Move,
        MoveAnchor,
    }
}
