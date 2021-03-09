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

        public bool Selected { get; }

        public bool Selectable { get; }

        public EditMode EditMode { get; set; }

        public string TagName { get; }

        public IEnumerable<EditMode> AvailableEditModes { get; set; }

        public (double x, double y) Panner { get; set; }

        public void HandleMouseMove(MouseEventArgs eventArgs);
        public void HandleMouseUp(MouseEventArgs eventArgs);
        public void HandleMouseOut(MouseEventArgs eventArgs);
        public Action Changed { get; set; }
    }

    public enum EditMode
    {
        None,
        Add,
        Move,
        MoveAnchor,
    }
}
