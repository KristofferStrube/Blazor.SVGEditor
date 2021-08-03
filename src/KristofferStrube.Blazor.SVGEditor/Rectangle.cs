using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KristofferStrube.Blazor.SVGEditor
{
    public class Rectangle : Shape
    {
        public Rectangle(IElement element, SVG svg)
        {
            Element = element;
            SVG = svg;
        }

        public override Type Editor => typeof(RectangleEditor);

        public double x {
            get { return double.Parse(Element.GetAttribute("x") ?? string.Empty); }
            set { Element.SetAttribute("x", value.ToString()); Changed.Invoke(this); }
        }
        public double y
        {
            get { return double.Parse(Element.GetAttribute("y") ?? string.Empty); }
            set { Element.SetAttribute("y", value.ToString()); Changed.Invoke(this); }
        }
        public double width
        {
            get { return double.Parse(Element.GetAttribute("width") ?? string.Empty); }
            set { Element.SetAttribute("width", value.ToString()); Changed.Invoke(this); }
        }
        public double height
        {
            get { return double.Parse(Element.GetAttribute("height") ?? string.Empty); }
            set { Element.SetAttribute("height", value.ToString()); Changed.Invoke(this); }
        }

        public (double x, double y) Cursor { get; set; }

        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            var pos = SVG.LocalDetransform((eventArgs.OffsetX, eventArgs.OffsetY));
            switch (EditMode)
            {
                case EditMode.Add:
                    Cursor = (pos.x, pos.y);
                    break;
                case EditMode.Move:
                    var diff = (x: pos.x - Panner.x, y: pos.y - Panner.y);
                    Panner = (pos.x, y: pos.y);
                    x += diff.x;
                    y += diff.y;
                    break;
                case EditMode.MoveAnchor:
                    if (CurrentAnchor == null)
                    {
                        CurrentAnchor = 0;
                    }
                    switch (CurrentAnchor)
                    {
                        case 0:
                            width -= pos.x - x;
                            height -= pos.y - y;
                            x = pos.x;
                            y = pos.y;
                            break;
                        case 1:
                            width = pos.x - x;
                            height -= pos.y - y;
                            y = pos.y;
                            break;
                        case 2:
                            width = pos.x - x;
                            height = pos.y - y;
                            break;
                        case 3:
                            width -= pos.x - x;
                            height = pos.y - y;
                            x = pos.x;
                            break;
                    }
                    break;
            }
        }

        public override void HandleMouseUp(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.Move or EditMode.MoveAnchor:
                    EditMode = EditMode.None;
                    break;
            }
        }

        public override void HandleMouseOut(MouseEventArgs eventArgs)
        {
        }

        public new static Action<SVG> AddNew = SVG =>
        {
        };

        public override void Complete()
        {
        }
    }
}
