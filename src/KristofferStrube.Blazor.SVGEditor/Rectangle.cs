using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;
using KristofferStrube.Blazor.SVGEditor.Models;
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

        public void Select(MouseEventArgs eventArgs)
        {
            if (SVG.CurrentShape == null || SVG.CurrentShape.EditMode == EditMode.None)
            {
                SVG.CurrentShape = this;
                Panner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                EditMode = EditMode.Move;
            }
        }

        public void SelectAnchor(int anchor)
        {
            CurrentAnchor = anchor;
            EditMode = EditMode.MoveAnchor;
        }

        public (double x, double y) Cursor { get; set; }

        public int? CurrentAnchor { get; set; }

        public override void HandleMouseMove(MouseEventArgs eventArgs)
        {
            switch (EditMode)
            {
                case EditMode.Add:
                    Cursor = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
                    break;
                case EditMode.Move:
                    var diff = (x: eventArgs.OffsetX - Panner.x, y: eventArgs.OffsetY - Panner.y);
                    Panner = (x: eventArgs.OffsetX, y: eventArgs.OffsetY);
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
                            width -= eventArgs.OffsetX - x;
                            height -= eventArgs.OffsetY - y;
                            x = eventArgs.OffsetX;
                            y = eventArgs.OffsetY;
                            break;
                        case 1:
                            width = eventArgs.OffsetX - x;
                            height -= eventArgs.OffsetY - y;
                            y = eventArgs.OffsetY;
                            break;
                        case 2:
                            width = eventArgs.OffsetX - x;
                            height = eventArgs.OffsetY - y;
                            break;
                        case 3:
                            width -= eventArgs.OffsetX - x;
                            height = eventArgs.OffsetY - y;
                            x = eventArgs.OffsetX;
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
    }
}
